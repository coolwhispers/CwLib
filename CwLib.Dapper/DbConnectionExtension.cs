using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CwLib.Dapper
{
    /// <summary>
    /// 擴充DbConnection
    /// </summary>
    public static class DbConnectionExtension
    {
        /// <summary>
        /// 分頁查詢
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cnn">The CNN.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="buffered">if set to <c>true</c> [buffered].</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static IPagedList<T> PageQuery<T>(this IDbConnection cnn,
            int page,
            int pageSize,
            string sql,
            object param = null,
            IDbTransaction transaction = null,
            bool buffered = true,
            int? commandTimeout = default(int?),
            CommandType? commandType = default(CommandType?))
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            if (cnn is System.Data.SqlClient.SqlConnection)
            {
                return
                    new SqlQueryPagedList<T>(cnn, sql, param, page, pageSize, transaction, buffered, commandTimeout, commandType)
                    .Query();
            }

            throw new Exception($"\"{cnn.GetType().Name}\" Not Support.");
        }

        /// <summary>
        /// 自訂分頁查詢
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cnn">The CNN.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="dataCountSql">The data count SQL.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="buffered">if set to <c>true</c> [buffered].</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns></returns>
        public static IPagedList<T> CustomPageQuery<T>(this IDbConnection cnn,
            int page,
            int pageSize,
            string sql,
            string dataCountSql,
            object param = null,
            IDbTransaction transaction = null,
            bool buffered = true,
            int? commandTimeout = default(int?),
            CommandType? commandType = default(CommandType?))
        {
            return
                new PageList<T>(
                    cnn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType),
                    page, pageSize,
                    cnn.Query<int>(dataCountSql, param, transaction, buffered, commandTimeout, commandType).FirstOrDefault());
        }
    }
}
