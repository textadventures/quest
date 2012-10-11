using System;
using System.Collections.Generic;
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

        public string Execute(string command, IList<string> parameters)
        {
            switch (command)
            {
                case "init":
                    m_consumerKey = parameters[0];
                    m_consumerSecret = parameters[1];
                    m_accessToken = parameters[2];
                    m_accessTokenSecret = parameters[3];
                    m_apiUrl = parameters[4];
                    return null;
                case "ping":
                    return CallAPI("ping");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string CallAPI(string apiMethod)
        {
            HttpDeliveryMethods method = HttpDeliveryMethods.PostRequest;

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
    }
}
