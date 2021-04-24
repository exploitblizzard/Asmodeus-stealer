using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UploadAFile
{
    public class BrowserLoginData
    {
        public string FormSubmitUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Browser { get; set; }

        public BrowserLoginData(string url, string username, string password, string browser)
        {
            FormSubmitUrl = url;
            Username = username;
            Password = password;
            Browser = browser;
        }

    }
}
