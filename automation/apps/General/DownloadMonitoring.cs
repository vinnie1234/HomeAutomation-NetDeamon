using System.Reactive.Concurrency;
using System.Text.RegularExpressions;
using Automation.Helpers;
using Automation.Models.DiscordNotificationModels;
using Automation.Models.Yts;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(DownloadMonitoring))]
[Focus]
public partial class DownloadMonitoring : BaseApp
{
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public DownloadMonitoring(
        IHaContext haContext,
        ILogger<DownloadMonitoring> logger,
        INotify notify,
        IScheduler scheduler,
        IDataRepository dataRepository)
        : base(haContext, logger, notify, scheduler)
    {
        YtsMonitoring(notify, dataRepository);
    }

    private void YtsMonitoring(INotify notify, IDataRepository dataRepository)
    {
        Entities.Sensor.YtsFeed.StateChanges().Subscribe(x =>
        {
            if (Entities.Sensor.YtsFeed.Attributes?.Entries != null)
            {
                var discordChannel = ConfigManager.GetValueFromConfigNested("Discord", "Logs") ?? "";

                var items = Entities.Sensor.YtsFeed.Attributes?.Entries!.Cast<JsonElement>()
                    .Select(o => o.Deserialize<Yts>()).ToList();

                if (items != null)
                {
                    var oldList = dataRepository.Get<List<Yts>>("yts");

                    foreach (var discordModel in from ytsItem in items
                             where ytsItem != null
                             where oldList == null || oldList.All(x => x.Id != ytsItem.Id)
                             let downloadLink = ytsItem.Links.First(x => x.Type == "application/x-bittorrent").Href
                             let image = GetTextFromHtmlRegex(ytsItem.Summary, ImgRegex())
                             let imbdRating = GetTextFromHtmlRegex(ytsItem.Summary, IMDBRatingRegex())
                             let genre = GetTextFromHtmlRegex(ytsItem.Summary, GenreRegex())
                             let size = GetTextFromHtmlRegex(ytsItem.Summary, SizeRegex())
                             let runtime = GetTextFromHtmlRegex(ytsItem.Summary, RuntimeRegex())
                             select new DiscordNotificationModel
                             {
                                 Embed = new Embed
                                 {
                                     Title = ytsItem.Title,
                                     Url = ytsItem.Link,
                                     Thumbnail = new Location(image),
                                     Fields = new[]
                                     {
                                         new Field { Name = @"Rating", Value = imbdRating },
                                         new Field { Name = @"Genre", Value = genre },
                                         new Field { Name = @"Size", Value = size },
                                         new Field { Name = @"Runtime", Value = runtime },
                                         new Field { Name = @"Direct Download", Value = downloadLink }
                                     }
                                 },
                                 Urls = new[] { downloadLink }
                             })
                        notify.NotifyDiscord("", new[] { discordChannel }, discordModel);

                    dataRepository.Save("yts", items);
                }
            }
        });
    }

    private string GetTextFromHtmlRegex(string htmlSource, Regex regex)
    {
        var matchesImgSrc = regex.Matches(htmlSource);
        var match = matchesImgSrc.First();
        return match.Groups.Count == 2 ? match.Groups[1].Value : match.Groups[2].Value;
    }


    [GeneratedRegex("<img[^>]*?src\\s*=\\s*[\"']?([^'\" >]+?)[ '\"][^>]*?>", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-NL")]
    private static partial Regex ImgRegex();

    [GeneratedRegex("(IMDB Rating:)(.+?)(?=<)", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-NL")]
    private static partial Regex IMDBRatingRegex();

    [GeneratedRegex("(Genre:)(.+?)(?=<)", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-NL")]
    private static partial Regex GenreRegex();

    [GeneratedRegex("(Size:)(.+?)(?=<)", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-NL")]
    private static partial Regex SizeRegex();

    [GeneratedRegex("(Runtime:)(.+?)(?=<)", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-NL")]
    private static partial Regex RuntimeRegex();
}