using System.Collections.Generic;

namespace project_manage_system_backend.Models
{
    public class Project
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public User Owner { get; set; }

        public List<Repo> Repositories { get; set; } = new List<Repo>();

        public List<UserProject> Users { get; set; } = new List<UserProject>();
    }
}
