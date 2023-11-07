namespace NpgsqlWrapper
{
    /// <summary>
    /// A helper class for the SQL parameters to bind to the SQL query.
    /// </summary>
    internal class DbParams : Dictionary<string, object>
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public DbParams() : base() { }

        /// <summary>
        /// Constructor that adds key and value to bind to the SQL query
        /// </summary>
        /// <param name="key">Parameter key</param>
        /// <param name="value">Parameter value</param>
        public DbParams(string key, object value)
        {
            Add(key, value);
        }
    }
}
