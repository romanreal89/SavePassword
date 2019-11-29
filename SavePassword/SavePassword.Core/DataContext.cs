using SavePassword.Core.Entities;
using SavePassword.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SavePassword.Core
{
    public class DataContext
    {
        private static DataContext instance;

        private DataContext() { }

        public static DataContext GetInstance()
        {
            if (instance == null)
                instance = new DataContext();
            return instance;
        }

        private readonly static string AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string CurrentName;
        private static string CurrentPassword;
        public MainModel PasswordModel { get; private set; }

        public void LoadData(string name, string password)
        {
            CurrentName = name;
            CurrentPassword = password;
            string fileName = Path.Combine(AppPath, name);
            if (!File.Exists(fileName)) Save(password);
            var file = File.ReadAllText(fileName);
            var decrypt = EncryptionHelper.Decrypt(file, password);
            PasswordModel = JsonSerializer.Deserialize<MainModel>(decrypt);
            if (PasswordModel.PassRecords == null)
                PasswordModel.PassRecords = new List<PassRecord>();
        }

        public void Save(string password = null)
        {
            password = password ?? CurrentPassword ?? throw new Exception("Password is empty! Can't Encript!");
            if (string.IsNullOrEmpty(CurrentName)) throw new Exception("Current configuration is empty! Nothing to save!");
            if (PasswordModel == null) PasswordModel = new MainModel();
            PasswordModel.LastUpdate = DateTime.Now;
            string fileName = Path.Combine(AppPath, CurrentName);
            var serialize = JsonSerializer.Serialize(PasswordModel);
            var encrypt = EncryptionHelper.Encrypt(serialize, password ?? CurrentPassword);
            using (StreamWriter streamWriter = new StreamWriter(fileName, false))
            {
                streamWriter.WriteLine(encrypt);
            }
        }

        public bool IsSignedin => !string.IsNullOrEmpty(CurrentName) && !string.IsNullOrEmpty(CurrentPassword);

        public void Signout()
        {
            PasswordModel = null;
            CurrentName = null;
            CurrentPassword = null;
        }
    }
}
