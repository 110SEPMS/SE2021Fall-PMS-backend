using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using System;
using System.Collections.Generic;
using System.Data.Common;
using Xunit;

namespace PMS_test
{
    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class UserServiceTests
    {
        private readonly PMSContext _dbContext;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _dbContext = new PMSContext(new DbContextOptionsBuilder<PMSContext>()
                                            .UseSqlite(CreateInMemoryDatabase())
                                            .Options);
            _dbContext.Database.EnsureCreated();
            _userService = new UserService(_dbContext);
            InitialDatabase();
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("DataSource=:memory:");

            connection.Open();

            return connection;
        }

        internal void InitialDatabase()
        {
            _dbContext.Users.Add(new User
            {
                Account = "admin",
                Password = Argon2.Hash("password"),
                Authority = "Admin",
                Name = "管理員"
            });

            _dbContext.Users.Add(new User
            {
                Account = "github_testDeleteUser",
                Password = Argon2.Hash("password"),
                Authority = "User",
                Name = "一般使用者"
            });
            _dbContext.SaveChanges();
        }

        [Fact]
        public void TestCreateAdminUser()
        {
            _userService.CreateUser(new LocalAccountDto
            {
                Account = "testAdmin",
                Password = "password"
            });

            var actualUser = _dbContext.Users.Find("testAdmin");
            Assert.Equal("testAdmin", actualUser.Account);
            Assert.Equal("Admin", actualUser.Authority);
        }

        [Fact]
        public void TestDeleteUserSuccess()
        {
            string account = "github_testDeleteUser";
            _userService.DeleteUserByAccount(account);
            var actual = Assert.Throws<Exception>(() => { _userService.GetUser(account); });
            Assert.Equal("User not found!", actual.Message);
        }

        [Fact]
        public void TestDeleteUserWithProject()
        {
            string account = "github_testDeleteUser";
            var user = _dbContext.Users.Find(account);
            user.Projects.Add(new UserProject { Account = user.Account, Project = new Project { Name = "project01", Owner = user }, User = user });
            _dbContext.SaveChanges();

            _userService.DeleteUserByAccount(account);
            var actual = Assert.Throws<Exception>(() => { _userService.GetUser(account); });
            Assert.Equal("User not found!", actual.Message);
        }

        [Fact]
        public void TestDeleteUserFail()
        {
            string account = "github_testDeleteUser";
            _userService.DeleteUserByAccount(account);
            var actual = Assert.Throws<Exception>(() => { _userService.DeleteUserByAccount(account); });
            Assert.Equal("User not found!", actual.Message);
        }

        [Fact]
        public void TestGetAllUserNotInclude()
        {
            string account = "github_testDeleteUser";
            var users = _userService.GetAllUserNotInclude(account);

            Assert.Single(users);
            Assert.Equal("admin", users[0].Account);
            Assert.Equal("Admin", users[0].Authority);
            Assert.Equal("管理員", users[0].Name);
        }

        [Fact]
        public void TestIsAdmin()
        {
            Assert.True(_userService.IsAdmin(_dbContext.Users.Find("admin").Account));
            Assert.False(_userService.IsAdmin(_dbContext.Users.Find("github_testDeleteUser").Account));
        }

        [Fact]
        public void TestEditUserInfo()
        {
            const string editedName = "改名惹";
            List<Operation<User>> operations = new List<Operation<User>>
            {
                new Operation<User>()
                {
                    op = "replace",
                    path = "/name",
                    value = editedName
                } ,
            };
            JsonPatchDocument<User> jsonPatchDocument = new JsonPatchDocument<User>(operations, new DefaultContractResolver());

            Assert.True(_userService.EditUserInfo("admin", jsonPatchDocument));
            var user = _userService.GetUserModel("admin");
            Assert.Equal(editedName, user.Name);
        }
    }
}
