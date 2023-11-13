namespace NpgsqlWrapper
{
    /// <summary>
    /// Used to put custom names anf settings for fields
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class FieldAttribute : Attribute
    {
        public readonly string FieldName;
        public readonly string? FieldType = null;
        public readonly bool FieldNotNull = false;
        public readonly bool FieldPrimaryKey = false;

        private FieldValue _fieldValue = new();

        public object FieldValue { get { return _fieldValue[FieldName]; } private set { _fieldValue[FieldName] = value; } }

        /// <summary>
        /// Constructor to set field name.
        /// </summary>
        /// <param name="name">Field name.</param>
        public FieldAttribute(string name)
        {
            FieldName = name;
        }

        /// <summary>
        /// Constructor to set field name and field type.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <param name="type">Field type.</param>
        public FieldAttribute(string name, string type)
        {
            FieldName = name;
            FieldType = type;
        }

        /// <summary>
        /// Constructor to set field name, field type and NOT NULL.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <param name="type">Field type.</param>
        /// <param name="notNull">Bool to set a field NOT NULL.</param>
        public FieldAttribute(string name, string type, bool notNull)
        {
            FieldName = name;
            FieldType = type;
            FieldNotNull = notNull;
        }

        /// <summary>
        /// Constructor to set field name, field type, NOT NULL and primary key.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <param name="type">Field type.</param>
        /// <param name="notNull">Bool to set a field NOT NULL.</param>
        /// <param name="primaryKey">Bool to set a field as primery key.</param>
        public FieldAttribute(string name, string type, bool notNull, bool primaryKey)
        {
            FieldName = name;
            FieldType = type;
            FieldNotNull = notNull;
            FieldPrimaryKey = primaryKey;
        }

        /// <summary>
        /// Used to set default value internally
        /// </summary>
        /// <param name="value">Default value</param>
        public void SetValue(object value)
        {
            FieldValue = value;
        }
    }

    /// <summary>
    /// Helper class for FieldAttribute for storing values
    /// </summary>
    public class FieldValue
    {
        private Dictionary<string, object> properties = new Dictionary<string, object>();

        public object this[string propertyName]
        {
            get => GetValue<object>(propertyName);
            set => SetValue(propertyName, value);
        }

        /// <summary>
        /// Get value
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="propertyName">Property name</param>
        /// <returns>Object</returns>
        public T GetValue<T>(string propertyName)
        {
            if (properties.TryGetValue(propertyName, out var value))
            {
                return (T)value;
            }
            return default;
        }

        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="value">Value</param>
        public void SetValue(string propertyName, object value)
        {
            properties[propertyName] = value;
        }

        
    }
}
