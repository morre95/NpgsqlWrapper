namespace NpgsqlWrapper
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TableNameAttribute : Attribute
    {
        public string TableName { get; set; }

        public TableNameAttribute(string tableName)
        {
            TableName = tableName.Replace(' ', '_');
        }

    }
}
