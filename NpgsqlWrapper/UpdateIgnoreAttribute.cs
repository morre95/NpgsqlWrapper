namespace NpgsqlWrapper
{
    /// <summary>
    /// Used to have the custom attribute to ignore a certen field in update operation by setting [UpdateIgnore] over the filed you want to ignore.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class UpdateIgnoreAttribute : Attribute
    {
    }
}
