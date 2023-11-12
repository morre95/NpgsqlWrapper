namespace NpgsqlWrapper
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class FieldAttribute : Attribute
    {
        public readonly string FieldName;
        public readonly string FieldType;
        public readonly bool FieldNotNull = false;
        public readonly bool FieldPrimaryKey = false;

        private FieldValue _fieldValue = new();

        public object FieldValue { get { return _fieldValue[FieldName]; } private set { _fieldValue[FieldName] = value; } }

        public FieldAttribute(string name, string type)
        {
            FieldName = name;
            FieldType = type;
        }

        public FieldAttribute(string name, string type, bool notNull)
        {
            FieldName = name;
            FieldType = type;
            FieldNotNull = notNull;
        }

        public FieldAttribute(string name, string type, bool notNull, bool primaryKey)
        {
            FieldName = name;
            FieldType = type;
            FieldNotNull = notNull;
            FieldPrimaryKey = primaryKey;
        }

        public void SetValue(object value)
        {
            FieldValue = value;
        }
    }

    public class FieldValue
    {
        private Dictionary<string, object> properties = new Dictionary<string, object>();

        public T GetValue<T>(string propertyName)
        {
            if (properties.TryGetValue(propertyName, out var value))
            {
                return (T)value;
            }
            return default(T);
        }

        public void SetValue(string propertyName, object value)
        {
            properties[propertyName] = value;
        }

        public object this[string propertyName]
        {
            get => GetValue<object>(propertyName);
            set => SetValue(propertyName, value);
        }
    }
}
