namespace project_manage_system_backend.Models
{
    public class Repo
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }
        /// <summary>
        /// 實際 repository (第三方) 的擁有者名稱
        /// </summary>
        public string Owner { get; set; }

        public Project Project { get; set; }
        /// <summary>
        /// 有無sonarqube
        /// </summary>
        public bool isSonarqube { get; set; }
        /// <summary>
        /// foramt: "帳號:密碼"
        /// encryption: base64
        /// </summary>
        public string accountColonPw { get; set; }

        /// <summary>
        /// Sonarqube url
        /// </summary>
        public string sonarqubeUrl { get; set; }

        /// <summary>
        /// sonarqube projectkey
        /// </summary>
        public string projectKey { get; set; }
    }
}
