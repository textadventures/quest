using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;

namespace TextAdventures.Quest
{
    public class ConducttrApi
    {
        private string m_consumerKey;
        private string m_consumerSecret;
        private string m_accessToken;
        private string m_accessTokenSecret;
        private string m_apiUrl;

        public object Execute(string command, object parameters)
        {
            if (command == "init")
            {
                var parameterList = (IList<string>)parameters;
                m_consumerKey = parameterList[0];
                m_consumerSecret = parameterList[1];
                m_accessToken = parameterList[2];
                m_accessTokenSecret = parameterList[3];
                m_apiUrl = parameterList[4];
                return null;
            }

            string apiMethod;
            string[] data = command.Split(new[] { ' ' }, 2);
            HttpDeliveryMethods method = (data[0] == "get") ? HttpDeliveryMethods.GetRequest : HttpDeliveryMethods.PostRequest;

            var parameterDictionary = parameters as IDictionary<string, string>;

            if (parameterDictionary != null)
            {
                NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
                foreach (var item in parameterDictionary)
                {
                    queryString.Add(item.Key, item.Value);
                }

                apiMethod = data[1] + "?" + queryString.ToString();
            }
            else
            {
                apiMethod = data[1];
            }

            string result = CallAPI(apiMethod, method);

            return ParseResult(result);
        }

        private string CallAPI(string apiMethod, HttpDeliveryMethods method)
        {
            InMemoryTokenManager tokenManager = new InMemoryTokenManager(m_consumerKey, m_consumerSecret);
            tokenManager.ExpireRequestTokenAndStoreNewAccessToken(m_consumerKey, "", m_accessToken, m_accessTokenSecret);
            var conducttr = new WebConsumer(
                new ServiceProviderDescription
                {
                    RequestTokenEndpoint = new MessageReceivingEndpoint("https://api.tstoryteller.com/oauth/request_token", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
                    UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://api.tstoryteller.com/oauth/authorize", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
                    AccessTokenEndpoint = new MessageReceivingEndpoint("https://api.tstoryteller.com/oauth/access_token", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
                    TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
                    ProtocolVersion = ProtocolVersion.V10a,
                }
                , tokenManager);

            string url = m_apiUrl + apiMethod;
            Uri uri = new Uri(url);

            HttpWebRequest request = conducttr.PrepareAuthorizedRequest(
                new MessageReceivingEndpoint(url, method),
                m_accessToken);
            var response = request.GetResponse();
            return (new StreamReader(response.GetResponseStream())).ReadToEnd();
        }

        private class ConducttrApiResult
        {
            public List<Dictionary<string, string>> results { get; set; }
            public Dictionary<string, string> response { get; set; }
        }

        private object ParseResult(string resultString)
        {
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ConducttrApiResult>(resultString);
            return (result.results == null) ? null : result.results.FirstOrDefault();
        }
    }
}
