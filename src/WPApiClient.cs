﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BCC.WPProxy
{
    public class WPApiClient
    {
        public WPApiClient(WPProxySettings settings, WPProxySiteSettingsAccessor siteSettings, IHttpClientFactory clientFactory)
        {
            SiteSettings = siteSettings;
            ClientFactory = clientFactory;
            Settings = settings;
        }

        WPProxySettings Settings { get; }
        WPProxySiteSettingsAccessor SiteSettings { get; }
        IHttpClientFactory ClientFactory { get; }

        public async Task<T> GetAsync<T>( string relativePath)
        {
            if (!SiteSettings.Current.UseProxyPlugin)
            {
                return default(T);
            }
            var uri = $"{SiteSettings.Current.SourceAddress}/wp-json/bcc-wp-proxy/v1/{relativePath}";

            var client = ClientFactory.CreateClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uri),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("X-Wp-Proxy-Key", Settings.ProxyKey);

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    var result = JsonConvert.DeserializeObject<T>(content);
                    return result;
                } 
                catch (Exception ex)
                {
                    throw;
                }
                
            }
            else
            {
                throw new Exception($"API request to destination server failed with status code {response.StatusCode}. Reason: {response.ReasonPhrase}");
            }
        }

    }
}
