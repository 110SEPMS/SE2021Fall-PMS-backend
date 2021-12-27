using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using System.Linq;
using System.Threading.Tasks;

namespace project_manage_system_backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RepoInfoController : ControllerBase
    {
        private readonly RepoInfoService _repoInfoService;
        public RepoInfoController(PMSContext dbContext)
        {
            _repoInfoService = new RepoInfoService(dbContext);
        }

        [Authorize]
        [HttpGet("commit/{repoId}")]
        public async Task<IActionResult> GetCommitInfo(int repoId)
        {
            string oauth_token = User.Claims.FirstOrDefault(c => c.Type.Equals("oauth")).Value;
            return Ok(await _repoInfoService.RequestCommitInfo(repoId, oauth_token));
        }

        [Authorize]
        [HttpGet("codebase/{repoId}")]
        public async Task<IActionResult> GetCodebase(int repoId)
        {
            return Ok(await _repoInfoService.RequestCodebase(repoId));
        }

        [Authorize]
        [HttpGet("contribute/{repoId}")]
        public async Task<IActionResult> GetContributorsActtivity(int repoId)
        {
            string oauth_token = User.Claims.FirstOrDefault(c => c.Type.Equals("oauth")).Value;
            return Ok(await _repoInfoService.RequestContributorsActivity(repoId, oauth_token));
        }

        [Authorize]
        [HttpGet("issue/{repoId}")]
        public async Task<IActionResult> GetIssueInfo(int repoId)
        {
            string token = User.Claims.FirstOrDefault(c => c.Type.Equals("oauth")).Value;
            return Ok(await _repoInfoService.RequestIssueInfo(repoId, token));
        }

        [Authorize]
        [HttpGet("sonarqube/{repoId}")]
        public async Task<IActionResult> GetSonarqube(int repoid)
        {
            return Ok(await _repoInfoService.GetSonarqubeInfo(repoid));
        }

        [Authorize]
        [HttpGet("ishavesonarqube/{repoId}")]
        public async Task<IActionResult> IsHaveSonarqube(int repoid)
        {
            return Ok(await _repoInfoService.IsHaveSonarqube(repoid));
        }
    }
}
