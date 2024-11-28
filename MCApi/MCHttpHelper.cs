using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace MCApi;

public static class MCHttpHelper
{
    private static readonly HttpClient Client;
    
    static MCHttpHelper()
    {
        Client = new HttpClient(new HttpClientHandler()
        {
            MaxConnectionsPerServer = 5
        });
        Client.DefaultRequestHeaders.Add("User-Agent", "MCApi/0.1a");
    }

    private static T Deserialize<T>(Stream s)
    {
        var prev = s.Position;
        using var reader = new StreamReader(s);
        var str = reader.ReadToEnd();
        Debug.WriteLine(str);
        s.Position = prev;
        // yeah this will definitely not be null trust me bro
        return JsonSerializer.Deserialize<T>(s, CommonJsonOptions.Options)!;
    }
    #region GET
    public static Task<T> Get<T>(string url) => Get<T>(new Uri(url));

    public static async Task<T> Get<T>(Uri url)
    {
        using var holder = await VariableResourceManager.NetworkConnections.Wait();
        var reply = await Client.GetAsync(url);
        if ((int)reply.StatusCode >= 299)
        {
            if (reply.StatusCode == HttpStatusCode.NotFound)
            {
                throw new MCDownloadException("Did not find such file at " + url);
            }

            throw new MCDownloadException("Internal McApi error: Code " + reply.StatusCode.ToString() + " at " + url);
        }
        return Deserialize<T>(await reply.Content.ReadAsStreamAsync());

    }
    public static Task<WrappingResourceHolder<Stream>> Open(string url) => Open(new Uri(url));
    public static async Task<WrappingResourceHolder<Stream>> Open(Uri url)
     => await VariableResourceManager.NetworkConnections.WrapWait<Stream>(() => Client.GetStreamAsync(url));
    #endregion
    #region HEAD
    public static Task<HttpResponseMessage> Head(string url) => Head(new Uri(url));

    public static async Task<HttpResponseMessage> Head(Uri url)
    {
        using var holder = await VariableResourceManager.NetworkConnections.Wait();
        return await Client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Head,
            RequestUri = url
        });
    }
    #endregion
}