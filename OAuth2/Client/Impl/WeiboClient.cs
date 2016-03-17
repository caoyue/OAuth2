using Newtonsoft.Json.Linq;
using OAuth2.Configuration;
using OAuth2.Infrastructure;
using OAuth2.Models;

namespace OAuth2.Client.Impl
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
            get { return "Weibo"; }
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

        private Endpoint UserDetailServiceEndPoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://api.weibo.com",
                    Resource = "/2/users/show.json"
                };
            }
        }

        protected override UserInfo ParseUserInfo(string content)
        {
            var response = JObject.Parse(content);
            return new UserInfo
            {
                Id = response["uid"].Value<string>()
            };
        }

        private UserInfo ParseUserDetail(string content)
        {
            var response = JObject.Parse(content);
            return new UserInfo
            {
                Id = response["id"].Value<string>(),
                FirstName = response["screen_name"].Value<string>(),
                LastName = response["name"].Value<string>(),
                AvatarUri =
                {
                    Large = response["avatar_large"].Value<string>(),
                    Normal = response["avatar_large"].Value<string>(),
                    Small = response["profile_image_url"].Value<string>()
                }
            };
        }

        private UserInfo GetUserDetail(string id)
        {
            var detailClient = _factory.CreateClient(UserDetailServiceEndPoint);
            var detailRequest = _factory.CreateRequest(UserDetailServiceEndPoint)
                .AddParameter("access_token", AccessToken)
                .AddParameter("uid", id);
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

            return userInfo;
        }
    }
}