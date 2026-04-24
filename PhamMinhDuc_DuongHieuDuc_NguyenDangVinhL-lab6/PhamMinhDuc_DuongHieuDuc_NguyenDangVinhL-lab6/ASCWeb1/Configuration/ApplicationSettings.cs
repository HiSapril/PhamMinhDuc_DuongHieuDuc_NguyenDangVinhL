namespace ASCWeb1.Configuration
{
    public class ApplicationSettings
    {
        public required string ApplicationTitle { get; set; }
        public required string AdminEmail { get; set; }
        public required string AdminName { get; set; }
        public required string AdminPassword { get; set; }
        public required string Roles { get; set; }
        public required string EngineerEmail { get; set; }
        public required string EngineerName { get; set; }
        public required string EngineerPassword { get; set; }
        public required string SMTPServer { get; set; }
        public int SMTPPort { get; set; }
        public required string SMTPAccount { get; set; }
        public required string SMTPPassword { get; set; }

    }
}
