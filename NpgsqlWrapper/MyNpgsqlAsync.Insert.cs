
namespace NpgsqlWrapper
{
    public partial class MyNpgsqlAsync : MyNpgsqlBase
    {
        /// <summary>
        /// Inserts asynchronously. 
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// var teacherToAdd = new Teachers()
        ///         {
        ///             first_name = firstName,
        ///             last_name = lastName,
        ///             subject = subject,
        ///             salary = salary
        ///         };
        /// await pgsql.InsertAsync(teacherToAdd);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="objToInsert">Objects with data.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        public async Task<int> InsertAsync<T>(T objToInsert)
        {
            PrepareInsertSql(objToInsert, out string sql, out DbParams parameters);
            return await ExecuteNonQueryAsync(sql, parameters, CancellationToken.None);
        }

        /// <summary>
        /// Inserts asynchronously with returning statement.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// var teacherToAdd = new Teachers()
        ///         {
        ///             first_name = firstName,
        ///             last_name = lastName,
        ///             subject = subject,
        ///             salary = salary
        ///         };
        /// var teacher = await pgsql.InsertReturningAsync(teacherToAdd); 
        /// Console.WriteLine(teacher.first_name);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type of objects to insert.</typeparam>
        /// <param name="objToInsert">The object with records to insert.</param>
        /// <returns>A list of objects containing the inserted records.</returns>
        public async Task<T?> InsertReturningAsync<T>(T objToInsert)
        {
            PrepareInsertSql(objToInsert, out string sql, out DbParams parameters);
            sql += " RETURNING *";
            return await ExecuteOneAsync<T>(sql, parameters, CancellationToken.None);
        }

        /// <summary>
        /// Inserts a list of objects into a database table asynchronously.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// var teacherToAdd1 = new Teachers()
        ///         {
        ///             first_name = firstName1,
        ///             last_name = lastName1,
        ///             subject = subject1,
        ///             salary = salary1
        ///         };
        /// var teacherToAdd2 = new Teachers()
        ///         {
        ///             first_name = firstName2,
        ///             last_name = lastName2,
        ///             subject = subject2,
        ///             salary = salary2
        ///         };
        /// 
        /// List<Teachers> addMe = new();
        /// addMe.Add(teacherToAdd1);
        /// addMe.Add(teacherToAdd2);
        /// 
        /// int affectedRows = await pgsql.InsertManyAsync(addMe);
        /// Console.WriteLine(affectedRows);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type of objects to insert.</typeparam>
        /// <param name="objToInsert">The object with records to insert.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        public async Task<int> InsertManyAsync<T>(List<T> objToInsert)
        {
            PrepareManyInsertSql(objToInsert, out string sql, out DbParams parameters);
            return await ExecuteNonQueryAsync(sql, parameters, CancellationToken.None);
        }

        /// <summary>
        /// Inserts a list of objects into a database table asynchronously and returns the inserted records.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// var teacherToAdd1 = new Teachers()
        ///         {
        ///             first_name = firstName1,
        ///             last_name = lastName1,
        ///             subject = subject1,
        ///             salary = salary1
        ///         };
        /// var teacherToAdd2 = new Teachers()
        ///         {
        ///             first_name = firstName2,
        ///             last_name = lastName2,
        ///             subject = subject2,
        ///             salary = salary2
        ///         };
        /// 
        /// List<Teachers> addMe = new();
        /// addMe.Add(teacherToAdd1);
        /// addMe.Add(teacherToAdd2);
        /// 
        /// var teachers = await pgsql.InsertReturningAsync(addMe); 
        /// 
        /// foreach (var teacher in teachers)
        ///     Console.WriteLine(teacher.first_name);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type of objects to insert.</typeparam>
        /// <param name="listToInsert">The list of objects to insert.</param>
        /// <returns>A list of objects containing the inserted records.</returns>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of arguments.</exception>
        public async Task<List<T>?> InsertManyReturningAsync<T>(List<T> listToInsert)
        {
            PrepareManyInsertSql(listToInsert, out string sql, out DbParams parameters);
            sql += " RETURNING *";
            return await ExecuteAsync<T>(sql, parameters, CancellationToken.None);
        }
    }
}
