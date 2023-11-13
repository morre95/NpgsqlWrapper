namespace NpgsqlWrapper
{
    /// <summary>
    /// Used to set custom namns of tables with [TableName("my_table_name)]. Place it over the class to used to map data from database.
    /// </summary>
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
