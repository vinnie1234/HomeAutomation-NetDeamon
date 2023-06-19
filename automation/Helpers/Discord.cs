using RestSharp;

namespace Automation.Helpers;

public static class Discord
{
    public static void SendMessage(string uri, string text)
    {
        var options = new RestClientOptions("https://discord.com")
        {
            MaxTimeout = -1
        };
        var client = new RestClient(options);
        var request = new RestRequest($"/api/webhooks/{uri}", Method.Post);
        request.AddHeader("Content-Type", "application/json");
        var body = $"{{\"content\": \" {text}\"}}";
        request.AddStringBody(body, DataFormat.Json);
        client.Execute(request);
    }
}