﻿using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using project_manage_system_backend.Shares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace project_manage_system_backend.Services
{
    public class RepoInfoService : BaseService
    {
        private readonly HttpClient _httpClient;
        public RepoInfoService(PMSContext dbContext, HttpClient client = null) : base(dbContext)
        {
            _httpClient = client ?? new HttpClient();
        }

        public async Task<CommitInfoDto> RequestCommitInfo(int repoId, string oauth_token)
        {
            Repo repo = _dbContext.Repositories.Find(repoId);
            string url = "https://api.github.com/repos/" + repo.Owner + "/" + repo.Name + "/stats/commit_activity";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", oauth_token);

            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                await Task.Delay(1000);
                response = await _httpClient.GetAsync(url);
            }

            string content = await response.Content.ReadAsStringAsync();
            var commitInfos = JsonSerializer.Deserialize<List<ResponseCommitInfoDto>>(content);

            List<WeekTotalData> weekChartDatas = new List<WeekTotalData>();
            List<DayOfWeekData> detailChartDatas = new List<DayOfWeekData>();

            foreach (var commitInfo in commitInfos)
            {
                weekChartDatas.Add(ConvertToWeekChartData(commitInfo));
                detailChartDatas.Add(ConvertToDetailChartData(commitInfo));
            }

            return new CommitInfoDto
            {
                WeekTotalData = weekChartDatas,
                DayOfWeekData = detailChartDatas
            };
        }

        public async Task<List<CodebaseDto>> RequestCodebase(int repoId)
        {
            Repo repo = _dbContext.Repositories.Find(repoId);
            string repoURL = "https://api.github.com/repos/" + repo.Owner + "/" + repo.Name + "/stats/code_frequency";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await _httpClient.GetAsync(repoURL);
            var contents = await response.Content.ReadAsStringAsync();
            var codebases = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int[]>>(contents);

            List<CodebaseDto> codebaseSet = new List<CodebaseDto>();

            foreach (var codebase in codebases)
            {
                CodebaseDto codebaseDto = new CodebaseDto()
                {
                    Date = DateHandler.ConvertToDateString(codebase[0]),
                    NumberOfRowsAdded = Convert.ToInt32(codebase[1]),
                    NumberOfRowsDeleted = Convert.ToInt32(codebase[2]),
                    NumberOfRows = Convert.ToInt32(codebase[1]) + Convert.ToInt32(codebase[2])
                };

                codebaseSet.Add(codebaseDto);
            }

            int thisWeekRows = 0;

            foreach (var codebase in codebaseSet)
            {
                codebase.NumberOfRows += thisWeekRows;
                thisWeekRows = codebase.NumberOfRows;
            }

            return codebaseSet;
        }

        public async Task<RepoIssuesDto> RequestIssueInfo(int repoId, string token)
        {
            RepoIssuesDto result = new RepoIssuesDto();
            Repo repo = _dbContext.Repositories.Find(repoId);
            List<double> closedTime = new List<double>();
            string url = repo.Url.Replace("github.com/", "api.github.com/repos/");


            result.closeIssues = await GetRepoIssues("closed", url, 100, token);
            result.openIssues = await GetRepoIssues("open", url, 100, token);

            foreach (var item in result.closeIssues)
            {
                DateTime closed = Convert.ToDateTime(item.closed_at);
                DateTime created = Convert.ToDateTime(item.created_at);

                item.closed_at = closed.ToString("yyyy-MM-dd HH:mm:ss");
                item.created_at = created.ToString("yyyy-MM-dd HH:mm:ss");

                closedTime.Add((closed - created).TotalSeconds);
            }
            foreach (var item in result.openIssues)
            {
                item.created_at = Convert.ToDateTime(item.created_at).ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (closedTime.Count != 0)
            {
                result.averageDealwithIssueTime = TimeSpan.FromSeconds(closedTime.Average()).ToString(@"dd\.hh\:mm\:\:ss\.\.");
                result.averageDealwithIssueTime = result.averageDealwithIssueTime.Replace("..", "Seconds").Replace("::", "Minute(s) ").Replace(":", "Hour(s) ").Replace(".", "Day(s) ");
            }
            else
            {
                result.averageDealwithIssueTime = "No Data";
            }
            return result;
        }

        private async Task<List<ResponseGithubRepoIssuesDto>> GetRepoIssues(string state, string url, int perPage, string token)
        {
            List<ResponseGithubRepoIssuesDto> result = new List<ResponseGithubRepoIssuesDto>();

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            int page = 1;


            var response = await _httpClient.GetAsync(url + $"/issues?state={state}&per_page=100&page={page}&sort=created");
            string content = await response.Content.ReadAsStringAsync();
            var tempList = JsonSerializer.Deserialize<List<ResponseGithubRepoIssuesDto>>(content);
            result.AddRange(tempList);

            return result;
        }

        private WeekTotalData ConvertToWeekChartData(ResponseCommitInfoDto commitInfo)
        {
            return new WeekTotalData
            {
                Total = commitInfo.total,
                Week = DateHandler.ConvertToDateString(commitInfo.week)
            };
        }

        private DayOfWeekData ConvertToDetailChartData(ResponseCommitInfoDto commitInfo)
        {
            List<DayCommit> detailDatas = new List<DayCommit>();

            int dayOfWeekCount = 0;
            foreach (var day in commitInfo.days)
            {
                detailDatas.Add(new DayCommit
                {
                    Day = DateHandler.ConvertToDayOfWeek(dayOfWeekCount++),
                    Commit = day
                });
            }

            return new DayOfWeekData
            {
                Week = DateHandler.ConvertToDateString(commitInfo.week),
                DetailDatas = detailDatas
            };
        }

        public async Task<List<ContributorsCommitActivityDto>> RequestContributorsActivity(int repoId, string oauth_token)
        {
            Repo repo = _dbContext.Repositories.Find(repoId);
            string url = "https://api.github.com/repos/" + repo.Owner + "/" + repo.Name + "/stats/contributors";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", oauth_token);
            var response = await _httpClient.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<ContributorsCommitActivityDto>>(content);
            // sort by commit 
            result.Sort((r1, r2) => r2.total.CompareTo(r1.total));

            foreach (var item in result)
            {
                item.commitsHtmlUrl = $"https://github.com/{repo.Owner}/{repo.Name}/commits?author={item.author.login}";
                foreach (var week in item.weeks)
                {
                    week.ws = DateHandler.ConvertToDateString(week.w);
                    item.totalAdditions += week.a;
                    item.totalDeletions += week.d;
                }
            }
            return result;
        }

        public async Task<SonarqubeInfoDto> GetSonarqubeInfo(int repoId)
        {
            Repo repo = _dbContext.Repositories.Find(repoId);
            string sonarqubeHostUrl = repo.sonarqubeUrl;
            string apiUrl = "api/measures/search?";
            string projectKey = repo.projectKey;
            string query = "&metricKeys=bugs,vulnerabilities,code_smells,duplicated_lines_density,coverage";
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {repo.accountColonPw}");
            var response = await _httpClient.GetAsync($"{sonarqubeHostUrl}{apiUrl}projectKeys={projectKey}{query}");
            string content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SonarqubeInfoDto>(content);
            result.projectName = projectKey;
            return result;
        }

        public async Task<bool> IsHaveSonarqube(int repoId)
        {
            Repo repo = await _dbContext.Repositories.FindAsync(repoId);
            return repo.isSonarqube;
        }
    }
}
