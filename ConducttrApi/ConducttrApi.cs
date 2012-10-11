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
        public string Execute()
        {
            string consumerKey = "a599823e633c8f0c096c3349f1854ac9050657264";
            string consumerSecret = "a82b2071d88b45d4154ee9c50ad1dfd0";
            string accessToken = "6acf422bf0ad6fe06ac45495f3830062050657267";
            string accessTokenSecret = "69803b3e2699460ae6d9f929e77a9949";
            string apiUrl = "https://api.tstoryteller.com/v1/project/226/";
            string apiMethod = "ping";
            HttpDeliveryMethods method = HttpDeliveryMethods.PostRequest;

            InMemoryTokenManager tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
            tokenManager.ExpireRequestTokenAndStoreNewAccessToken(consumerKey, "", accessToken, accessTokenSecret);
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

            string url = apiUrl + apiMethod;
            Uri uri = new Uri(url);

            HttpWebRequest request = conducttr.PrepareAuthorizedRequest(
                new MessageReceivingEndpoint(url, method),
                accessToken);
            var response = request.GetResponse();
            return (new StreamReader(response.GetResponseStream())).ReadToEnd();
        }
    }
}
