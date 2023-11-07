namespace NpgsqlWrapper
{
    internal class DbParams : Dictionary<string, object>
    {
        public DbParams() : base() { }
        public DbParams(string key, object value)
        {
            Add(key, value);
        }
    }
}
