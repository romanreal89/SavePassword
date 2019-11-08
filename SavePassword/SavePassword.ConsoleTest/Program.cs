using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SavePassword.ConsoleTest {
    class Program {
        static void Main(string[] args) {
            List<Test> tests = new List<Test> {
                new Test{ Id =1, Name = "Test1", BirthDay = DateTime.Today, IsActive = true },
                new Test{ Id =2, Name = "Test2", BirthDay = DateTime.Today.AddDays(10), IsActive = false },
                new Test{ Id =3, Name = "Test3", BirthDay = DateTime.Today.AddMonths(1), IsActive = true },
            };
            string one = JsonSerializer.Serialize<Test>(tests.First());
            string list = JsonSerializer.Serialize<List<Test>>(tests);
            Console.WriteLine(one);
            Console.WriteLine();
            Console.WriteLine(list);
            Console.ReadKey();
            Test test = JsonSerializer.Deserialize<Test>(one);
            List<Test> test_d = JsonSerializer.Deserialize<List<Test>>(list);
            Console.ReadKey();

        }

        class Test {
            public long Id { get; set; }
            public string Name { get; set; }
            public DateTime BirthDay { get; set; }
            public bool IsActive { get; set; }
        }

        static void Main2(string[] args) {
            Console.WriteLine("Please enter a password to use:");
            string password = Console.ReadLine();
            Console.WriteLine("Please enter a string to encrypt:");
            string plaintext = Console.ReadLine();
            Console.WriteLine("");

            Console.WriteLine("Your encrypted string is:");
            string encryptedstring = EncryptionHelper.Encrypt(plaintext, password);
            Console.WriteLine(encryptedstring);
            Console.WriteLine("");

            Console.WriteLine("Your decrypted string is:");
            string decryptedstring = EncryptionHelper.Decrypt(encryptedstring, password);
            Console.WriteLine(decryptedstring);
            Console.WriteLine("");

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        static void Main1(string[] args) {
            var key = Console.ReadLine();// Guid.NewGuid().ToString("N");

            var original = "Hello World!";
            var encrypted = Encrypt(original, key);

            key = Console.ReadLine();
            var decrypted = Decrypt(encrypted, key);

            Console.WriteLine(original);
            Console.WriteLine(encrypted);
            Console.WriteLine(decrypted);

            Console.ReadKey();
        }

        static string Encrypt(string text, string key) {
            var _key = Encoding.UTF8.GetBytes(key);

            using (var aes = Aes.Create()) {
                using (var encryptor = aes.CreateEncryptor(_key, aes.IV)) {
                    using (var ms = new MemoryStream()) {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
                            using (var sw = new StreamWriter(cs)) {
                                sw.Write(text);
                            }
                        }

                        var iv = aes.IV;

                        var encrypted = ms.ToArray();

                        var result = new byte[iv.Length + encrypted.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        static string Decrypt(string encrypted, string key) {
            var b = Convert.FromBase64String(encrypted);

            var iv = new byte[16];
            var cipher = new byte[16];

            Buffer.BlockCopy(b, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(b, iv.Length, cipher, 0, iv.Length);

            var _key = Encoding.UTF8.GetBytes(key);

            using (var aes = Aes.Create()) {
                using (var decryptor = aes.CreateDecryptor(_key, iv)) {
                    var result = string.Empty;
                    using (var ms = new MemoryStream(cipher)) {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)) {
                            using (var sr = new StreamReader(cs)) {
                                result = sr.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }

        public static class StringCipher {
            // This constant is used to determine the keysize of the encryption algorithm in bits.
            // We divide this by 8 within the code below to get the equivalent number of bytes.
            private const int Keysize = 256;

            // This constant determines the number of iterations for the password bytes generation function.
            private const int DerivationIterations = 1000;

            public static string Encrypt(string plainText, string passPhrase) {
                // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
                // so that the same Salt and IV values can be used when decrypting.  
                var saltStringBytes = Generate256BitsOfRandomEntropy();
                var ivStringBytes = Generate256BitsOfRandomEntropy();
                var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations)) {
                    var keyBytes = password.GetBytes(Keysize / 8);
                    using (var symmetricKey = new RijndaelManaged()) {
                        symmetricKey.BlockSize = 256;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes)) {
                            using (var memoryStream = new MemoryStream()) {
                                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)) {
                                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                    cryptoStream.FlushFinalBlock();
                                    // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                    var cipherTextBytes = saltStringBytes;
                                    cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                    cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                    memoryStream.Close();
                                    cryptoStream.Close();
                                    return Convert.ToBase64String(cipherTextBytes);
                                }
                            }
                        }
                    }
                }
            }

            public static string Decrypt(string cipherText, string passPhrase) {
                // Get the complete stream of bytes that represent:
                // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
                var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
                // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
                var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
                // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
                var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
                // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
                var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

                using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations)) {
                    var keyBytes = password.GetBytes(Keysize / 8);
                    using (var symmetricKey = new RijndaelManaged()) {
                        symmetricKey.BlockSize = 256;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes)) {
                            using (var memoryStream = new MemoryStream(cipherTextBytes)) {
                                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)) {
                                    var plainTextBytes = new byte[cipherTextBytes.Length];
                                    var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                    memoryStream.Close();
                                    cryptoStream.Close();
                                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                                }
                            }
                        }
                    }
                }
            }

            private static byte[] Generate256BitsOfRandomEntropy() {
                var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
                using (var rngCsp = new RNGCryptoServiceProvider()) {
                    // Fill the array with cryptographically secure random bytes.
                    rngCsp.GetBytes(randomBytes);
                }
                return randomBytes;
            }
        }

        public static class EncryptionHelper {
            public static string Encrypt(string clearText, string EncryptionKey) {
                //string EncryptionKey = "abc123";
                byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
                using (Aes encryptor = Aes.Create()) {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream()) {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write)) {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        clearText = Convert.ToBase64String(ms.ToArray());
                    }
                }
                return clearText;
            }
            public static string Decrypt(string cipherText, string EncryptionKey) {
                //string EncryptionKey = "abc123";
                cipherText = cipherText.Replace(" ", "+");
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create()) {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream()) {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write)) {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return cipherText;
            }
        }
    }
}
