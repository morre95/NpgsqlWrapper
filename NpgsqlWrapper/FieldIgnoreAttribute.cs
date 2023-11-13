namespace NpgsqlWrapper
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class FieldIgnoreAttribute : Attribute
    {
    }
}
