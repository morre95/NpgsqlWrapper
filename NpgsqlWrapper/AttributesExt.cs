using System.Reflection;

namespace NpgsqlWrapper
{
    public static class AttributesExt
    {
        public static IEnumerable<PropertyInfo> AllAttributes<T>(this object obj, string name)
        {
            var allProperties = obj.GetType().GetProperties()
            .Where(x => x.GetCustomAttributes(typeof(T), true).Length >= 1 && x.Name == name);
            return allProperties;
        }

        public static IEnumerable<PropertyInfo> GetMyAttr<T>(this object obj)
        {
            var allProperties = obj.GetType().GetProperties()
            .Where(x => x.GetCustomAttributes(typeof(T), true).Length >= 1);
            return allProperties;
        }

        public static T ReadAttribute<T>(this PropertyInfo propertyInfo)
        {
            var returnType = propertyInfo.GetCustomAttributes(typeof(T), true)
            .Cast<T>().FirstOrDefault();
            return returnType;
        }
    }
}
