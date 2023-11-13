
namespace NpgsqlWrapper
{
    public partial class MyNpgsqlAsync : MyNpgsqlBase
    {
        /// <summary>
        /// Updates rows in a database table asynchronously based on the provided object's properties.
        /// </summary>
        /// <example>
        /// Usage:
        /// Clarification:
        /// It is not necessary to use WHERE in the 'where' parameter. The funktion will work the same with it and without it.
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
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
        /// await pgsql.UpdateAsync(editMe, "id=@id", p);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type of object representing the table to update.</typeparam>
        /// <param name="table">The object representing the table to update.</param>
        /// <param name="where">Optional WHERE clause to specify which rows to update.</param>
        /// <param name="whereParameters">Additional parameters to include in the SQL query.</param>
        /// <returns>The number of rows affected by the update operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the 'table' parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when a duplicate parameter name is detected in 'whereParameters'.</exception>
        public async Task<int> UpdateAsync<T>(T table, string? where = null, Dictionary<string, object>? whereParameters = null)
        {
            PrepareUpdateSql(table, where, whereParameters, out string sql, out DbParams returnParams);

            return await ExecuteNonQueryAsync(sql, returnParams, CancellationToken.None);
        }
    }
}
