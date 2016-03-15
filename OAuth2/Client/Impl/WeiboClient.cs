using Newtonsoft.Json.Linq;
using OAuth2.Client;
using OAuth2.Configuration;
using OAuth2.Infrastructure;
using OAuth2.Models;

namespace ESports.LolMatrix.Web.OAuth
{
    public class WeiboClient : OAuth2Client
    {
        private readonly IRequestFactory _factory;

        public WeiboClient(IRequestFactory factory, IClientConfiguration configuration)
            : base(factory, configuration)
        {
            _factory = factory;
        }

        public override string Name
        {
            get
            {
                return "Weibo";
            }
        }

        protected override Endpoint AccessCodeServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://api.weibo.com",
                    Resource = "/oauth2/authorize"
                };
            }
        }

        protected override Endpoint AccessTokenServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://api.weibo.com",
                    Resource = "/oauth2/access_token"
                };
            }
        }

        protected override Endpoint UserInfoServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://api.weibo.com",
                    Resource = "/2/account/get_uid.json"
                };
            }
        }

        protected override UserInfo ParseUserInfo(string content)
        {
            var response = JObject.Parse(content);
            return new UserInfo()
            {
                Id = response["uid"].Value<string>()
            };
        }

        protected override UserInfo GetUserInfo()
        {
            var client = _factory.CreateClient(UserInfoServiceEndpoint);
            var request = _factory.CreateRequest(UserInfoServiceEndpoint)
                .AddParameter("access_token", AccessToken);

            var response = client.ExecuteAndVerify(request);

            var result = ParseUserInfo(response.Content);
            result.ProviderName = Name;

            return result;
        }
    }
}