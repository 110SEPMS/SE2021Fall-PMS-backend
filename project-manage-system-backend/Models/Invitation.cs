namespace project_manage_system_backend.Models
{
    public class Invitation
    {
        public int ID { get; set; }
        public User Inviter { get; set; }
        public User Applicant { get; set; }
        public Project InvitedProject { get; set; }
        public bool IsAgreed { get; set; }
    }
}
