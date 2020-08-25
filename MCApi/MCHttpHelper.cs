using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MCApi
{
    public static class MCHttpHelper
    {
        private static HttpClient client;
        static MCHttpHelper()
        {
            ServicePointManager.DefaultConnectionLimit = 5;
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "MCApi/0.1a");
        }
        private static T Deserialize<T>(Stream s)
        {
            JsonSerializer js = new JsonSerializer();
            using (StreamReader sr = new StreamReader(s))
            using (JsonTextReader jr = new JsonTextReader(sr))
                return js.Deserialize<T>(jr);
        }
        #region GET
        public static Task<T> Get<T>(string url) => Get<T>(new Uri(url));

        public static async Task<T> Get<T>(Uri url)
        {
            using var holder = await VariableResourceManager.NetworkConnections.Wait();
            var reply = await client.GetAsync(url);
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
        {
            var holder = await VariableResourceManager.NetworkConnections.WrapWait<Stream>();
            holder.Value = await client.GetStreamAsync(url);
            return holder;
        }
        #endregion
        #region POST
        public static Task<TResponse> PostYggdrasil<TResponse, TRequest>(string url, TRequest data) => PostYggdrasil<TResponse, TRequest>(new Uri(url), data);

        public static async Task<TResponse> PostYggdrasil<TResponse, TRequest>(Uri url, TRequest data)
        {
            var srl = JsonConvert.SerializeObject(data);
            using var holder = await VariableResourceManager.NetworkConnections.Wait();
            var reply = await client.PostAsync(url, new StringContent(srl, Encoding.UTF8, "application/json"));
            var str = await reply.Content.ReadAsStreamAsync();
            if ((int)reply.StatusCode > 299 || (int)reply.StatusCode < 200)
            {
                // Handle Yggdrasil error
                MCLoginError err;
                try
                {
                    err = Deserialize<MCLoginError>(str);
                }
                catch
                {
                    throw new MCLoginException("Invalid error from Yggdrasil");
                }
                MCLoginException exc = new MCLoginException(err.Error + ": " + err.ErrorMessage);
                exc.Data["Error"] = err;
                throw exc;
            }
            return Deserialize<TResponse>(str);
        }
        public static Task PostYggdrasil<TRequest>(string url, TRequest data) => PostYggdrasil<TRequest>(new Uri(url), data);

        public static async Task PostYggdrasil<TRequest>(Uri url, TRequest data)
        {
            var srl = JsonConvert.SerializeObject(data);
            using var holder = await VariableResourceManager.NetworkConnections.Wait();
            var reply = await client.PostAsync(url, new StringContent(srl, Encoding.UTF8, "application/json"));
            var str = await reply.Content.ReadAsStreamAsync();
            if ((int)reply.StatusCode > 299 || (int)reply.StatusCode < 200)
            {
                // Handle Yggdrasil error
                MCLoginError err;
                try
                {
                    err = Deserialize<MCLoginError>(str);
                }
                catch
                {
                    throw new MCLoginException("Invalid error from Yggdrasil");
                }
                MCLoginException exc = new MCLoginException(err.Error + ": " + err.ErrorMessage);
                exc.Data["Error"] = err;
                throw exc;
            }

        }
        #endregion
        #region HEAD
        public static Task<HttpResponseMessage> Head(string url) => Head(new Uri(url));

        public static async Task<HttpResponseMessage> Head(Uri url)
        {
            using var holder = await VariableResourceManager.NetworkConnections.Wait();
            return await client.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Head,
                RequestUri = url
            });
        }
        #endregion
    }
}