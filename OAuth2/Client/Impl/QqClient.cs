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

        private Endpoint UserDetailServiceEndPoint => new Endpoint
        {
            BaseUri = "https://graph.qq.com",
            Resource = "/user/get_user_info"
        };

        protected override UserInfo ParseUserInfo(string content)
        {
            var json = content.Replace("callback(", "").Replace(");", "");
            var response = JObject.Parse(json);
            return new UserInfo
            {
                Id = response["openid"].Value<string>()
            };
        }

        private UserInfo ParseUserDetail(string content)
        {
            var response = JObject.Parse(content);
            if (response["ret"].Value<int>() != 0)
            {
                throw new UnexpectedResponseException($"GetUserInfoError:{response["msg"].Value<string>()}");
            }
            return new UserInfo
            {
                FirstName = response["nickname"].Value<string>(),
                LastName = string.Empty,
                AvatarUri =
                {
                    Large = response["figureurl_qq_2"].Value<string>(),
                    Normal = response["figureurl_qq_1"].Value<string>(),
                    Small = response["figureurl_qq_1"].Value<string>()
                }
            };
        }

        private UserInfo GetUserDetail(string id)
        {
            var detailClient = _factory.CreateClient(UserDetailServiceEndPoint);
            var detailRequest = _factory.CreateRequest(UserDetailServiceEndPoint)
                .AddParameter("access_token", AccessToken)
                .AddParameter("oauth_consumer_key", Configuration.ClientId)
                .AddParameter("openid", id);
            var detail = detailClient.ExecuteAndVerify(detailRequest);
            return ParseUserDetail(detail.Content);
        }

        protected override UserInfo GetUserInfo()
        {
            var client = _factory.CreateClient(UserInfoServiceEndpoint);
            var request = _factory.CreateRequest(UserInfoServiceEndpoint)
                .AddParameter("access_token", AccessToken);

            var response = client.ExecuteAndVerify(request);
            var result = ParseUserInfo(response.Content);

            var userInfo = GetUserDetail(result.Id);
            userInfo.ProviderName = Name;
            userInfo.Id = result.Id;

            return userInfo;
        }
    }
}