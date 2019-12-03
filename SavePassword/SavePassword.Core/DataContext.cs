using SavePassword.Core.Entities;
using SavePassword.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private string EncryptedData { get; set; }
        public MainModel PasswordModel
        {
            get
            {
                if (string.IsNullOrEmpty(EncryptedData)) return new MainModel { PassRecords = new List<PassRecord>() };
                var decrypt = EncryptionHelper.Decrypt(EncryptedData, CurrentPassword);
                return JsonSerializer.Deserialize<MainModel>(decrypt);
            }
        }

        public void AddOrReplaceKey(PassRecord passRecord)
        {
            var model = PasswordModel;
            model.LastUpdate = DateTime.Now;
            if (passRecord != null)
            {
                var record = model.PassRecords.FirstOrDefault(x => x.Id == passRecord.Id);
                if (record == null)
                    model.PassRecords.Add(passRecord);
                else
                {
                    record.Details = passRecord.Details;
                    record.Login = passRecord.Login;
                    record.Password = passRecord.Password;
                    record.URL = passRecord.URL;
                    record.Group = passRecord.Group;
                    record.Name = passRecord.Name;
                }
            }
            var serialize = JsonSerializer.Serialize(model);
            EncryptedData = EncryptionHelper.Encrypt(serialize, CurrentPassword);
        }

        public void LoadData(string name, string password)
        {
            CurrentName = name;
            CurrentPassword = password;
            string fileName = Path.Combine(AppPath, name);
            if (!File.Exists(fileName)) SaveToFile(password);
            EncryptedData = File.ReadAllText(fileName);
        }

        public void SaveToFile(string password = null)
        {
            password = password ?? CurrentPassword ?? throw new Exception("Password is empty! Can't Encript!");
            if (string.IsNullOrEmpty(CurrentName)) throw new Exception("Current configuration is empty! Nothing to save!");
            string fileName = Path.Combine(AppPath, CurrentName);
            if (string.IsNullOrEmpty(EncryptedData)) AddOrReplaceKey(null);//init
            using (StreamWriter streamWriter = new StreamWriter(fileName, false))
            {
                streamWriter.WriteLine(EncryptedData);
            }
        }

        public bool IsSignedin => !string.IsNullOrEmpty(CurrentName) && !string.IsNullOrEmpty(CurrentPassword);

        public void Signout()
        {
            EncryptedData = null;
            CurrentName = null;
            CurrentPassword = null;
        }
    }
}
