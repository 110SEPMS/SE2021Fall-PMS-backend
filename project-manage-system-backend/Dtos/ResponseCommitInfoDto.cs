namespace project_manage_system_backend.Dtos
{
    public class CommitInfo
    {
        public int Week { get; set; }

        public int[] Days { get; set; }

        public int Total { get; set; }
    }

    public class ResponseCommitInfoDto
    {
        public int week { get; set; }

        public int[] days { get; set; }

        public int total { get; set; }
    }
}
