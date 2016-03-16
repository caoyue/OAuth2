using Newtonsoft.Json.Linq;
using OAuth2.Configuration;
using OAuth2.Infrastructure;
using OAuth2.Models;

namespace OAuth2.Client.Impl
{
    public class QqClient : OAuth2Client
    {
        private readonly IRequestFactory _factory;

        public QqClient(IRequestFactory factory, IClientConfiguration configuration)
            : base(factory, configuration)
        {
            _factory = factory;
        }

        public override string Name => "QQ";

        protected override Endpoint AccessCodeServiceEndpoint => new Endpoint
        {
            BaseUri = "https://graph.qq.com",
            Resource = "/oauth2.0/authorize"
        };

        protected override Endpoint AccessTokenServiceEndpoint => new Endpoint
        {
            BaseUri = "https://graph.qq.com",
            Resource = "/oauth2.0/token"
        };

        protected override Endpoint UserInfoServiceEndpoint => new Endpoint
        {
            BaseUri = "https://graph.qq.com",
            Resource = "/oauth2.0/me"
        };

        protected override UserInfo ParseUserInfo(string content)
        {
            var response = JObject.Parse(content);
            return new UserInfo
            {
                Id = response["openid"].Value<string>()
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