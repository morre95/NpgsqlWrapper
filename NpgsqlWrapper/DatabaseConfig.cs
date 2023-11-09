using System.Text.Json;

namespace NpgsqlWrapper
{
    public class DatabaseConfig
    {
        public string? Server { get; set; }
        public int? Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Database { get; set; }
        public byte[]? Key { get; set; }
        public byte[]? Vector { get; set; }
        public byte[]? Bytes { get; set; }

        /// <summary>
        /// Decrypts and mapps a DatabaseConfig object and return it to be used for login in to a database.
        /// </summary>
        /// <param name="configFile">File path where tha json is stored.</param>
        /// <returns>DatabaseConfig with mapped login information.</returns>
        /// <exception cref="ArgumentNullException">Throws if key or vector is set.</exception>
        public static DatabaseConfig Load(string configFile)
        {
            string json = File.ReadAllText(configFile);
            DatabaseConfig config = JsonSerializer.Deserialize<DatabaseConfig>(json)!;

            if (config.Key == null || config.Vector == null || config.Bytes == null)
            {
                throw new ArgumentNullException("config key");
            }

            json = Encryption.DecryptStringFromBytes(config.Bytes, config.Key, config.Vector);

            config = JsonSerializer.Deserialize<DatabaseConfig>(json)!;

            DatabaseConfig dbLogin = new();
            dbLogin.Server = config.Server;
            dbLogin.Port = config.Port;
            dbLogin.Username = config.Username;
            dbLogin.Password = config.Password;
            dbLogin.Database = config.Database;

            return dbLogin;
        }

        /// <summary>
        /// Encrypts and saves login information to a database.
        /// </summary>
        /// <param name="configFile">File path where tha json is stored.</param>
        /// <param name="config">DatabaseConfig with mapped login information.</param>
        /// <exception cref="ArgumentNullException">Throws if key or vector is set.</exception>
        public static void Save(string configFile, DatabaseConfig config)
        {
            if (config.Key != null && config.Vector != null)
            {
                throw new ArgumentNullException("config key pair");
            }

            string originalJson = JsonSerializer.Serialize(config);
            byte[] key = new byte[32];
            byte[] vector = new byte[32];
            Encryption.GenerateKeyPair(out key, out vector);

            config.Vector = vector;
            config.Key = key;

            config.Bytes = Encryption.EncryptStringToBytes(originalJson, config.Key, config.Vector);

            config.Server = null;
            config.Port = null;
            config.Username = null;
            config.Password = null;
            config.Database = null;

            string jsonString = JsonSerializer.Serialize(config);
            File.WriteAllText(configFile, jsonString);
        }
    }
}
