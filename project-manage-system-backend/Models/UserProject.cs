namespace project_manage_system_backend.Models
{
    public class UserProject
    {
        public string Account { get; set; }
        public User User { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
