namespace UploadAFile

{
    public class FirefoxLoginsJSON
    {

        public class Rootobject
        {
            public int NextId { get; set; }
            public Login[] Logins { get; set; }
            public int Version { get; set; }
            public object[] PotentiallyVulnerablePasswords { get; set; }
            public Dismissedbreachalertsbyloginguid DismissedBreachAlertsByLoginGUID { get; set; }
        }

        public class Dismissedbreachalertsbyloginguid
        {
        }

        public class Login
        {
            public int Id { get; set; }
            public string Hostname { get; set; }
            public string HttpRealm { get; set; }
            public string FormSubmitURL { get; set; }
            public string UsernameField { get; set; }
            public string PasswordField { get; set; }
            public string EncryptedUsername { get; set; }
            public string EncryptedPassword { get; set; }
            public string Guid { get; set; }
            public int EncType { get; set; }
            public long TimeCreated { get; set; }
            public long TimeLastUsed { get; set; }
            public long TimePasswordChanged { get; set; }
            public int TimesUsed { get; set; }
        }
    }
}
