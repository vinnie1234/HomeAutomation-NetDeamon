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
        YtsMonitoring(notify, dataRepository, "yts2160p", Entities.Sensor.YtsFeed2160p);
        
        Entities.Sensor.YtsFeed1080.StateChanges()
            .Subscribe(_ =>
            {
                YtsMonitoring(notify, dataRepository, "yts1080", Entities.Sensor.YtsFeed1080);
            });
        Entities.Sensor.YtsFeed2160p.StateChanges()
            .Subscribe(_ =>
            {
                YtsMonitoring(notify, dataRepository, "yts2160p", Entities.Sensor.YtsFeed2160p);
            });
    }

    private static void YtsMonitoring(INotify notify, IDataRepository dataRepository, string saveId, SensorEntity feed )
    {
            if (feed.Attributes?.Entries != null)
            {
                var discordChannel = ConfigManager.GetValueFromConfigNested("Discord", "Yts") ?? "";
                var logChannel = ConfigManager.GetValueFromConfigNested("Discord", "Logs") ?? "";

                var items = feed.Attributes?.Entries!.Cast<JsonElement>()
                    .Select(o => o.Deserialize<Yts>()).ToList();

                var thisYear = DateTimeOffset.Now.Year;
                var lastYear = DateTimeOffset.Now.AddYears(-1).Year;

                if (items != null)
                {
                    var oldList = dataRepository.Get<List<Yts>>(saveId);

                    foreach (var discordModel in from ytsItem in items
                             where ytsItem != null
                             where oldList == null || oldList.TrueForAll(yts => yts.Id != ytsItem.Id)
                             where ytsItem.Title.Contains(thisYear.ToString()) ||
                                   ytsItem.Title.Contains(lastYear.ToString())
                             let downloadLink = ytsItem.Links.First(link => link.Type == "application/x-bittorrent")
                                 .Href
                             let image = GetTextFromHtmlRegex(ytsItem.Summary, ImgRegex())
                             let imbdRating = GetTextFromHtmlRegex(ytsItem.Summary, ImdbRatingRegex())
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
                                         new Field { Name = "Rating", Value = imbdRating },
                                         new Field { Name = "Genre", Value = genre },
                                         new Field { Name = "Size", Value = size },
                                         new Field { Name = "Runtime", Value = runtime },
                                         new Field { Name = "Direct Download", Value = downloadLink }
                                     }
                                 },
                                 Urls = new[] { downloadLink }
                             })
                    {
                        //Check Martin
                        if(discordModel?.Embed?.Title != null && (discordModel.Embed.Title.ToLower().Contains("a difficult year", StringComparison.CurrentCultureIgnoreCase) || 
                                                                  discordModel.Embed.Title.ToLower().Contains("Neem me mee", StringComparison.CurrentCultureIgnoreCase) || 
                                                                  discordModel.Embed.Title.Contains("une année difficile", StringComparison.CurrentCultureIgnoreCase) || 
                                                                  discordModel.Embed.Title.Contains("une annee difficile", StringComparison.CurrentCultureIgnoreCase)
                                                                  ))
                            notify.NotifyDiscord("Martin :) ", new[] { logChannel }, discordModel);
                        
                        notify.NotifyDiscord("", new[] { discordChannel }, discordModel);
                    }

                    dataRepository.Save(saveId, items);
                }
            }
    }

    private static string GetTextFromHtmlRegex(string htmlSource, Regex regex)
    {
        var matchesImgSrc = regex.Matches(htmlSource);
        var match = matchesImgSrc[0];
        return match.Groups.Count == 2 ? match.Groups[1].Value : match.Groups[2].Value;
    }


    [GeneratedRegex("<img[^>]*?src\\s*=\\s*[\"']?([^'\" >]+?)[ '\"][^>]*?>", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-NL")]
    private static partial Regex ImgRegex();

    [GeneratedRegex("(IMDB Rating:)(.+?)(?=<)", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-NL")]
    private static partial Regex ImdbRatingRegex();

    [GeneratedRegex("(Genre:)(.+?)(?=<)", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-NL")]
    private static partial Regex GenreRegex();

    [GeneratedRegex("(Size:)(.+?)(?=<)", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-NL")]
    private static partial Regex SizeRegex();

    [GeneratedRegex("(Runtime:)(.+?)(?=<)", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-NL")]
    private static partial Regex RuntimeRegex();
}
