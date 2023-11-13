using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlWrapper
{
    public partial class MyNpgsql :MyNpgsqlBase
    {

        /// <summary>
        /// Inserts. 
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
        /// 
        /// var teacherToAdd = new Teachers()
        ///         {
        ///             first_name = firstName,
        ///             last_name = lastName,
        ///             subject = subject,
        ///             salary = salary
        ///         };
        /// int numOfAffectedRows = pgsql.Insert(teacherToAdd);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="objToInsert">Objects with data.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        public int Insert<T>(T objToInsert)
        {
            PrepareInsertSql(objToInsert, out string sql, out DbParams parameters);
            return ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// Inserts with returning statement. 
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
        /// 
        /// var teacherToAdd = new Teachers()
        ///         {
        ///             first_name = firstName,
        ///             last_name = lastName,
        ///             subject = subject,
        ///             salary = salary
        ///         };
        /// Teachers teacher = pgsql.InsertReturning(teacherToAdd);
        /// Console.WriteLine(teacher.first_name);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="objToInsert">Objects with data.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        public T? InsertReturning<T>(T objToInsert)
        {
            PrepareInsertSql(objToInsert, out string sql, out DbParams parameters);
            sql += " RETURNING *";
            Debug.WriteLine(sql);
            return ExecuteOne<T>(sql, parameters);
        }

        /// <summary>
        /// Inserts many. 
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
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
        /// int affectedRows = pgsql.InsertMany(addMe);
        /// Console.WriteLine(affectedRows);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="objToInsert">List of 'T' with data to insert.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        public int InsertMany<T>(List<T> objToInsert)
        {
            // TODO: bör vara InsertMany<T>(Enumerable<T> objToInsert)
            PrepareManyInsertSql(objToInsert, out string sql, out DbParams parameters);
            return ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// Inserts many with returning statement.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
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
        /// Enumerable<Teachers> enumerable = pgsql.InsertManyReturning(addMe);
        /// List<Teachers> teachers = enumerable.ToList()
        /// Console.WriteLine(teachers[0].first_name);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="listToInsert">Objects with data.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> objects containing the inserted records.</returns>
        /// <exception cref="ArgumentNullException">Thrown when connection is null.</exception>
        public IEnumerable<T> InsertManyReturning<T>(List<T> listToInsert)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            PrepareManyInsertSql(listToInsert, out string sql, out DbParams parameters);
            sql += " RETURNING *";
            return Execute<T>(sql, parameters);
        }
    }
}
