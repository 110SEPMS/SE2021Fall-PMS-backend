using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using project_manage_system_backend.Models;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using System.Collections.Generic;
using System.Data.Common;
using Xunit;

namespace PMS_test.ControllersTest
{
    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class InvitationServiceTests
    {
        private readonly PMSContext _dbContext;
        private readonly InvitationService _InvitationService;

        private const string _inviter = "shark", _applicant = "ina";
        private const string _inviterName = "a", _applicantName = "waaa";

        public InvitationServiceTests()
        {
            _dbContext = new PMSContext(new DbContextOptionsBuilder<PMSContext>()
               .UseSqlite(CreateInMemoryDatabase())
               .Options);
            _dbContext.Database.EnsureCreated();
            _InvitationService = new InvitationService(_dbContext);
            InitialDatabase();
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("DataSource=:memory:");

            connection.Open();

            return connection;
        }
        private void InitialDatabase()
        {
            var inviter = new User
            {
                Account = _inviter,
                Name = _inviterName,
                Projects = new List<UserProject>(),
            };
            var applicant = new User
            {
                Account = _applicant,
                Name = _applicantName,
            };
            var project1 = new Project
            {
                Name = "AAAA",
                Owner = inviter,
            };
            var project2 = new Project
            {
                Name = "BBBB",
                Owner = inviter,
            };
            var userProject = new List<UserProject>()
            {
                new UserProject
                {
                    User = inviter,
                    Project = project1,
                },
                new UserProject
                {
                    User = inviter,
                    Project = project2,
                },
            };
            inviter.Projects = userProject;

            _dbContext.Projects.Add(project1);
            _dbContext.Projects.Add(project2);
            _dbContext.Users.Add(inviter);
            _dbContext.Users.Add(applicant);
            _dbContext.SaveChanges();
        }

        [Fact]
        public void TestCreateInvitation()
        {
            var inviter = _dbContext.Users.Find("shark");
            var applicant = _dbContext.Users.Find("ina");
            var project = _dbContext.Projects.Find(1);
            var invitation = _InvitationService.CreateInvitation(inviter, applicant, project);
            Assert.Equal(inviter, invitation.Inviter);
            Assert.Equal(applicant, invitation.Applicant);
            Assert.Equal(project, invitation.InvitedProject);
            Assert.False(invitation.IsAgreed);
        }

        [Fact]
        public void TestGetInvitations()
        {
            var inviter = _dbContext.Users.Find("shark");
            var applicant = _dbContext.Users.Find("ina");
            var project1 = _dbContext.Projects.Find(1);
            var project2 = _dbContext.Projects.Find(2);
            _InvitationService.AddInvitation(_InvitationService.CreateInvitation(inviter, applicant, project1));
            _InvitationService.AddInvitation(_InvitationService.CreateInvitation(inviter, applicant, project2));
            var invitations = _InvitationService.GetInvitations(applicant);
            var invitation = _InvitationService.GetInvitation(2);

            Assert.Equal(2, invitations.Count);
            Assert.Equal(inviter, invitations[0].Inviter);
            Assert.Equal(applicant, invitations[0].Applicant);
            Assert.Equal(project1, invitations[1].InvitedProject);
            Assert.Equal(project2, invitations[0].InvitedProject);
            Assert.Equal(invitations[0], invitation);
            Assert.False(invitations[1].IsAgreed);
        }

        [Fact]
        public void TestIsInvitationExist()
        {
            var inviter = _dbContext.Users.Find("shark");
            var applicant = _dbContext.Users.Find("ina");
            var project1 = _dbContext.Projects.Find(1);
            var project2 = _dbContext.Projects.Find(2);
            _InvitationService.AddInvitation(_InvitationService.CreateInvitation(inviter, applicant, project1));
            var invitation1 = _InvitationService.CreateInvitation(inviter, applicant, project1);
            var invitation2 = _InvitationService.CreateInvitation(inviter, applicant, project2);

            Assert.True(_InvitationService.IsInvitationExist(invitation1));
            Assert.False(_InvitationService.IsInvitationExist(invitation2));
        }

        [Fact]
        public void TestIsUserInProject()
        {
            var inviter = _dbContext.Users.Find("shark");
            var applicant = _dbContext.Users.Find("ina");
            var project1 = _dbContext.Projects.Find(1);
            var project2 = _dbContext.Projects.Find(2);
            _InvitationService.AddInvitation(_InvitationService.CreateInvitation(inviter, applicant, project1));
            var userService = new UserService(_dbContext);
            userService.AddProject(_dbContext.Invitations.Find(1));

            Assert.True(_InvitationService.IsUserInProject(applicant, project1));
            Assert.False(_InvitationService.IsUserInProject(applicant, project2));
        }

        [Fact]
        public void TestDeleteInvitation()
        {
            var inviter = _dbContext.Users.Find("shark");
            var applicant = _dbContext.Users.Find("ina");
            var project1 = _dbContext.Projects.Find(1);
            var project2 = _dbContext.Projects.Find(2);
            var invitation1 = _InvitationService.CreateInvitation(inviter, applicant, project1);
            var invitation2 = _InvitationService.CreateInvitation(inviter, applicant, project1);
            _InvitationService.AddInvitation(invitation1);
            _InvitationService.AddInvitation(invitation2);
            _InvitationService.DeleteInvitation(invitation1);

            Assert.Single(_InvitationService.GetInvitations(applicant));
            Assert.Equal(invitation2, _InvitationService.GetInvitation(2));
        }
    }
}
