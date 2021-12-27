using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using project_manage_system_backend;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using RichardSzalay.MockHttp;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PMS_test.ControllersTest
{
    public class BaseControllerTests
    {
        protected PMSContext _dbContext;
        protected readonly AuthorizeService _authorizeService;
        protected readonly HttpClient _client;
        protected readonly AuthorizeDto _authorizeDto;

        public BaseControllerTests()
        {
            var builder = new WebHostBuilder()
            .UseEnvironment("Testing")
            .UseConfiguration(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
            )
            .UseStartup<Startup>();

            var server = new TestServer(builder);


            _dbContext = server.Host.Services.GetService(typeof(PMSContext)) as PMSContext;
            _dbContext.Database.EnsureCreated();
            _client = server.CreateClient();

            _authorizeService = InitialAuthorizeService();

            _authorizeDto = GetTestToken();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _authorizeDto.Token);
        }

        /// <summary>
        /// 初始化權限，使外部能取得token
        /// userAccount:github_testuser
        /// </summary>
        /// <returns></returns>
        internal AuthorizeService InitialAuthorizeService()
        {
            var mockHttp = new MockHttpMessageHandler();
            var _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            string clientId = _configuration.GetValue<string>("githubConfig:client_id");
            string clientSecret = _configuration.GetValue<string>("githubConfig:client_secret");

            string content = "{\"client_id\":\"" + clientId + "\",\"client_secret\":\"" + clientSecret + "\",\"code\":\"testcode\"}";
            mockHttp.When(HttpMethod.Post, "https://github.com/login/oauth/access_token")
                    .WithContent(content)
                    .Respond("application/text",
                    "access_token=token&scope=&token_type=bearer");

            mockHttp.When(HttpMethod.Get, "https://api.github.com/user")
                    .Respond("application/json",
                    "{\"login\":\"testuser\",\"avatar_url\":\"test\"}");

            return new AuthorizeService(_dbContext, _configuration, new JwtHelper(_configuration), mockHttp.ToHttpClient());
        }

        private AuthorizeDto GetTestToken()
        {
            var task = _authorizeService.AuthenticateGithub(new GithubLoginDto { Code = "testcode" });
            task.Wait();
            return task.Result;
        }
    }
}
