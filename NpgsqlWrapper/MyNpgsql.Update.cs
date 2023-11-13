using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlWrapper
{
    public partial class MyNpgsql : MyNpgsqlBase
    {
        /// <summary>
        /// Updates rows in a database table based on the provided object's properties.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
        /// 
        /// var editMe = new Teachers()
        ///  {
        ///      first_name = firstName,
        ///      last_name = lastName,
        ///      subject = subject,
        ///      salary = salary
        ///  };
        ///  
        /// DbParams p = new DbParams("id", 11);
        /// 
        /// int affectedRows = pgsql.Update(editMe, "id=@id", p);
        /// Console.WriteLine(affectedRows);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type of object representing the table to update.</typeparam>
        /// <param name="table">The object representing the table to update.</param>
        /// <param name="where">Optional WHERE clause to specify which rows to update.</param>
        /// <param name="whereParameters">Additional parameters to include in the SQL query.</param>
        /// <returns>The number of rows affected by the update operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when connection is null.</exception>
        public int Update<T>(T table, string? where = null, Dictionary<string, object>? whereParameters = null)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            PrepareUpdateSql(table, where, whereParameters, out string sql, out DbParams returnParameters);
            return ExecuteNonQuery(sql, returnParameters);
        }
    }
}
