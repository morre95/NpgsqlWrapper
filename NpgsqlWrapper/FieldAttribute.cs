namespace NpgsqlWrapper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        public readonly string FieldName;
        public readonly string? FieldType;
        public readonly bool FieldNotNull = false;
        public readonly bool FieldPrimaryKey = false;


        private Dictionary<string, object?> properties = new();

        public FieldAttribute(string name)
        {
            FieldName = name;
        }

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

        public T? GetValue<T>(string propertyName)
        {
            if (properties.TryGetValue(propertyName, out var value))
            {
                return (T?)value;
            }
            return default;
        }

        public void SetValue(string propertyName, object? value)
        {
            if (FieldNotNull && value == null) 
            {
                throw new ArgumentNullException("value");
            } 
            properties[propertyName] = value;
        }

        public object? this[string propertyName]
        {
            get => GetValue<object?>(propertyName);
            set => SetValue(propertyName, value);
        }
    }
}
