using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net.Http;
using Xunit;

namespace PMS_test.ControllersTest
{
    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class RepoServiceTests
    {
        private readonly PMSContext _dbContext;
        private readonly HttpClient _client;
        private readonly RepoService _repoService;

        private const string _owner = "shark";
        private const string _name = "a";
        private const string _failFakeRepository = "https://github.com/" + _owner + "/" + _name;
        private const string _successFakeRepository = "https://github.com/" + _owner + "/testRepo";


        public RepoServiceTests()
        {
            _dbContext = new PMSContext(new DbContextOptionsBuilder<PMSContext>()
               .UseSqlite(CreateInMemoryDatabase())
               .Options);
            _dbContext.Database.EnsureCreated();
            _client = CreateMockClient();
            _repoService = new RepoService(_dbContext, _client);
            InitialDatabase();
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("DataSource=:memory:");

            connection.Open();

            return connection;
        }

        private HttpClient CreateMockClient()
        {
            var mockHttp = new MockHttpMessageHandler();

            ResponseGithubRepoInfoDto dto = new ResponseGithubRepoInfoDto
            {
                IsSucess = true,
                html_url = $"https://github.com/{_owner}",
                message = "",
                name = _name,
                owner = new Owner { login = _owner },
                url = _failFakeRepository
            };

            string response = JsonConvert.SerializeObject(dto);
            mockHttp.When(HttpMethod.Get, _failFakeRepository.Replace("github.com", "api.github.com/repos"))
                    .Respond("application/json", response);

            dto.url = _successFakeRepository;
            dto.name = "testRepo";
            response = JsonConvert.SerializeObject(dto);
            mockHttp.When(HttpMethod.Get, _successFakeRepository.Replace("github.com", "api.github.com/repos"))
                    .Respond("application/json", response);
            return mockHttp.ToHttpClient();
        }

        private void InitialDatabase()
        {
            var user = new User
            {
                Account = $"github_{_owner}",
                Authority = "User",
                AvatarUrl = null,
                Name = _owner,
            };

            var repo = new Repo
            {
                Name = _name,
                Owner = _owner,
                Url = $"https://github.com/{_owner}/{_name}"
            };

            var project = new Project
            {
                Name = "AAAA",
                Repositories = new List<Repo>() { repo },
            };
            user.Projects.Add(new UserProject { Project = project });
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }

        [Fact]
        public void TestGetRepositoryByProjectId()
        {
            var repo = _repoService.GetRepositoryByProjectId(1);
            Assert.Single(repo);
            Assert.Equal(_name, repo[0].Name);
            Assert.Equal(_owner, repo[0].Owner);
        }

        [Fact]
        async public void TestCreateRepoFali()
        {
            var response = await _repoService.CheckRepoExist(_failFakeRepository);

            Repo repo = new Repo
            {
                Name = response.name,
                Owner = response.owner.login,
                Url = response.url,
                Project = _repoService.GetProjectByProjectId(1),
            };
            var actual = Assert.Throws<Exception>(() => _repoService.CreateRepo(repo));
            Assert.Equal("Duplicate repo!", actual.Message);
        }

        [Fact]
        async public void TestCreateRepoSuccess()
        {
            var response = await _repoService.CheckRepoExist(_successFakeRepository);

            Repo repo = new Repo
            {
                Name = response.name,
                Owner = response.owner.login,
                Url = response.url,
                Project = _repoService.GetProjectByProjectId(1),
            };
            _repoService.CreateRepo(repo);
            var actual = _dbContext.Repositories.Find(2);
            Assert.Equal("testRepo", actual.Name);
            Assert.Equal(2, actual.ID);
            Assert.Equal(_owner, actual.Owner);
            Assert.Equal(_successFakeRepository, actual.Url);
            Assert.Equal(1, actual.Project.ID);
        }

        [Fact]
        public void TestDeleteRepoSuccess()
        {
            bool result = _repoService.DeleteRepo(1, 1);
            Assert.True(result);
        }

        [Fact]
        public void TestDeleteRepoFail()
        {
            bool result = _repoService.DeleteRepo(2, 1);
            Assert.False(result);
        }
    }
}
