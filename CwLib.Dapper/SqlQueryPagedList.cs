using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CwLib.Dapper
{
    internal class SqlQueryPagedList<T> : QueryPagedList<T>
    {
        private static readonly Dictionary<string, int> SqlVersions = new Dictionary<string, int>();
        private static readonly object CheckVersionLoch = new object();

        private void CheckVersion()
        {
            lock (CheckVersionLoch)
            {
                if (!SqlVersions.ContainsKey(Cnn.ConnectionString))
                {
                    var sqlVersion =
                     Cnn.Query<string>("SELECT SERVERPROPERTY('ProductVersion')")
                         .FirstOrDefault()?
                         .Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries)
                         .FirstOrDefault();

                    if (!string.IsNullOrEmpty(sqlVersion))
                    {
                        SqlVersions.Add(Cnn.ConnectionString, int.Parse(sqlVersion));
                    }

                }
            }

            if (!SqlVersions.ContainsKey(Cnn.ConnectionString) || SqlVersions[Cnn.ConnectionString] < 11)
            {
                throw new Exception("SQL Server is not 2012 or above ");
            }
        }

        /// <summary>
        /// 計算分頁
        /// </summary>
        /// <returns></returns>
        protected override PagedResult ComputePaged()
        {
            CheckVersion();

            var skipCount = Page <= 1 ? 0 : (Page - 1) * PageSize;

            var fristFrom = QueryString.ToUpper().IndexOf(" FROM ", StringComparison.Ordinal);

            var countString = QueryString.Substring(fristFrom, QueryString.Length - fristFrom);

            var lastOrderBy = countString.ToUpper().LastIndexOf(" ORDER BY ", StringComparison.Ordinal);

            return new PagedResult(
                QueryString + $" OFFSET {skipCount} ROWS FETCH FIRST {PageSize} ROWS ONLY ",
                Cnn.Query<int>("SELECT COUNT(*) " + countString.Substring(0, lastOrderBy)).FirstOrDefault(),
                PageSize
            );
        }

        public SqlQueryPagedList(IDbConnection cnn, string sql, object param = null, int page = 1, int pageSize = 10, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) 
            : base(cnn, sql, param, page, pageSize, transaction, buffered, commandTimeout, commandType)
        {
        }
    }
}