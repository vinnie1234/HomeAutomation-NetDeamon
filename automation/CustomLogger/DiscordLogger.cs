using Automation.Helpers;
using Serilog.Core;
using Serilog.Events;
using Discord;
using Discord.Webhook;
using Color = Discord.Color;

namespace Automation.CustomLogger;

public class DiscordLogger : ILogEventSink
{
    private readonly IFormatProvider? _formatProvider;
    private readonly LogEventLevel _restrictedToMinimumLevel;

    private static readonly string WebhookUrlDebug = ConfigManager.GetValueFromConfigNested("NetDaemonLogging", "Debug") ?? throw new InvalidOperationException();
    private static readonly string WebhookUrlInformation = ConfigManager.GetValueFromConfigNested("NetDaemonLogging", "Information") ?? throw new InvalidOperationException();
    private static readonly string WebhookUrlWarning = ConfigManager.GetValueFromConfigNested("NetDaemonLogging", "Warning") ?? throw new InvalidOperationException();
    private static readonly string WebhookUrlError = ConfigManager.GetValueFromConfigNested("NetDaemonLogging", "Error") ?? throw new InvalidOperationException();
    private static readonly string WebhookUrlException = ConfigManager.GetValueFromConfigNested("NetDaemonLogging", "Exception") ?? throw new InvalidOperationException();

    public DiscordLogger(
        IFormatProvider? formatProvider,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information)
    {
        _formatProvider = formatProvider;
        _restrictedToMinimumLevel = restrictedToMinimumLevel;
    }

    public void Emit(LogEvent logEvent)
    {
        SendMessage(logEvent);
    }

    private void SendMessage(LogEvent logEvent)
    {
        if (!ShouldLogMessage(_restrictedToMinimumLevel, logEvent.Level))
            return;


        var embedBuilder = new EmbedBuilder();
            
        try
        {
            if (logEvent.Exception != null)
            {
                var webHook = new DiscordWebhookClient(GetWebhookUrl());
                embedBuilder.Color = Color.DarkRed;
                embedBuilder.WithTitle(":o: Exception");
                embedBuilder.AddField("Type:", $"```{logEvent.Exception.GetType().FullName}```");

                var message = FormatMessage(logEvent.Exception.Message, 240);
                embedBuilder.AddField("Message:", message);

                var stackTrace = FormatMessage(logEvent.Exception.StackTrace ?? string.Empty, 1024);
                embedBuilder.AddField("StackTrace:", stackTrace);

                webHook.SendMessageAsync(null, false, new[] { embedBuilder.Build() })
                    .GetAwaiter()
                    .GetResult();
            }
            else
            {
                var webHook = new DiscordWebhookClient(GetWebhookUrl(logEvent.Level));
                var message = logEvent.RenderMessage(_formatProvider);
                message = FormatMessage(message, 240);

                SpecifyEmbedLevel(logEvent.Level, embedBuilder);
                embedBuilder.Description = message;

                webHook.SendMessageAsync(
                        null, false, new[] { embedBuilder.Build() })
                    .GetAwaiter()
                    .GetResult();
            }
        }

        catch (Exception ex)
        {
            var webHook = new DiscordWebhookClient(GetWebhookUrl());
            webHook.SendMessageAsync(
                    $"ooo snap, {ex.Message}")
                .GetAwaiter()
                .GetResult();
        }
    }

    private static void SpecifyEmbedLevel(LogEventLevel level, EmbedBuilder  embed)
    {
        switch (level)
        {
            case LogEventLevel.Verbose:
                embed.Title = ":loud_sound: Verbose";
                embed.Color = Color.LightGrey;
                break;
            case LogEventLevel.Debug:
                embed.Title = ":mag: Debug";
                embed.Color = Color.LightGrey;
                break;
            case LogEventLevel.Information:
                embed.Title = ":information_source: Information";
                embed.Color = new Color(0, 186, 255);
                break;
            case LogEventLevel.Warning:
                embed.Title = ":warning: Warning";
                embed.Color = new Color(255, 204, 0);
                break;
            case LogEventLevel.Error:
                embed.Title = ":x: Error";
                embed.Color =  new Color(255, 0, 0);
                break;
            case LogEventLevel.Fatal:
                embed.Title = ":skull_crossbones: Fatal";
                embed.Color = Color.DarkRed;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }

    private static string FormatMessage(string message, int maxLength)
    {
        if (message.Length > maxLength)
            message = $"{message[..maxLength]} ...";

        if (!string.IsNullOrWhiteSpace(message))
            message = $"```{message}```";

        return message;
    }

    private static bool ShouldLogMessage(
        LogEventLevel minimumLogEventLevel,
        LogEventLevel messageLogEventLevel) =>
        (int)messageLogEventLevel >= (int)minimumLogEventLevel;

    private static string GetWebhookUrl(LogEventLevel? level = null)
    {
        return level switch
        {
            LogEventLevel.Verbose => WebhookUrlDebug,
            LogEventLevel.Debug => WebhookUrlDebug,
            LogEventLevel.Information => WebhookUrlInformation,
            LogEventLevel.Warning => WebhookUrlWarning,
            LogEventLevel.Error => WebhookUrlError,
            LogEventLevel.Fatal => WebhookUrlException,
            null => WebhookUrlException,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }
}