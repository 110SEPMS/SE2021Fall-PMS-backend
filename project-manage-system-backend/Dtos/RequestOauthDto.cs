namespace project_manage_system_backend.Dtos
{
    public class RequestOauthDto
    {
        public string Client_id { get; set; }

        public string Client_secret { get; set; }

        public string Code { get; set; }
    }
}
