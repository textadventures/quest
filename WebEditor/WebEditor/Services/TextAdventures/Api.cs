using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TASessionManager
{
    public static class Api
    {
        private static Uri s_baseAddress = new Uri(ConfigurationManager.AppSettings["BaseURI"] ?? "http://textadventures.co.uk/");

        public static T GetData<T>(string api) where T : class
        {
            var client = new HttpClient { BaseAddress = s_baseAddress };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = client.GetAsync(api).Result;
            if (!response.IsSuccessStatusCode) return null;

            return response.Content.ReadAsAsync<T>().Result;
        }

        public static TOut PostData<TIn, TOut>(string api, TIn model)
            where TIn : class
            where TOut : class
        {
            var response = PostData(api, model);
            return response.Content.ReadAsAsync<TOut>().Result;
        }

        public static HttpResponseMessage PostData<T>(string api, T model) where T : class
        {
            var client = new HttpClient { BaseAddress = s_baseAddress };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client.PostAsJsonAsync(api, model).Result;
        }
    }
}
