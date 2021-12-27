using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace project_manage_system_backend.Models
{
    public class User
    {
        /// <summary>
        /// 第三方的帳號 Id
        /// </summary>
        [Key]
        public string Account { get; set; }

        /// <summary>
        /// 只有管理員有密碼
        /// </summary>
        public string Password { get; set; }

        public string Name { get; set; }

        public string AvatarUrl { get; set; }
        /// <summary>
        /// 值有：
        ///     User
        ///     Admin
        /// </summary>
        public string Authority { get; set; }

        public List<UserProject> Projects { get; set; } = new List<UserProject>();
    }
}
