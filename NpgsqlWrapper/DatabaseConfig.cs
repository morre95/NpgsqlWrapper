using System.Text.Json;

namespace NpgsqlWrapper
{
    public class DatabaseConfig
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string? EncryptionKey { get; set; }

        public static DatabaseConfig Load(string configFile)
        {
            string json = File.ReadAllText(configFile);
            DatabaseConfig config = JsonSerializer.Deserialize<DatabaseConfig>(json)!;

            if (config.EncryptionKey == null)
            {
                throw new ArgumentNullException("config key");
            }

            config.Server = Encryption.DecryptString(config.EncryptionKey, config.Server);
            config.Username = Encryption.DecryptString(config.EncryptionKey, config.Username);
            config.Password = Encryption.DecryptString(config.EncryptionKey, config.Password);
            config.Database = Encryption.DecryptString(config.EncryptionKey, config.Database);

            return config;
        }

        public static void Save(string configFile, DatabaseConfig config)
        {
            if (config.EncryptionKey == null)
            {
                config.EncryptionKey = Encryption.GenerateKey();
            }

            config.Server = Encryption.EncryptString(config.EncryptionKey, config.Server);
            config.Username = Encryption.EncryptString(config.EncryptionKey, config.Username);
            config.Password = Encryption.EncryptString(config.EncryptionKey, config.Password);
            config.Database = Encryption.EncryptString(config.EncryptionKey, config.Database);

            string jsonString = JsonSerializer.Serialize(config);
            File.WriteAllText(configFile, jsonString);
        }
    }
}
