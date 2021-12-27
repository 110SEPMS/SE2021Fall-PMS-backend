namespace project_manage_system_backend.Dtos
{
    public class InvitationDto
    {
        public string ApplicantId { get; set; }
        public int ProjectId { get; set; }
    }
    public class ReplyToInvitationDto
    {
        public int InvitationId { get; set; }
        public bool IsAgreed { get; set; }
    }
}
