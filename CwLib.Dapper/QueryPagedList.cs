using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CwLib.Dapper
{
    /// <summary>
    /// 分頁基礎類別
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class QueryPagedList<T>
    {
        /// <summary>
        /// IDbConnection
        /// </summary>
        protected IDbConnection Cnn;
        /// <summary>
        /// 查詢字串
        /// </summary>
        protected string QueryString;
        private readonly object _param;
        private readonly IDbTransaction _transaction;
        private readonly bool _buffered;
        private readonly int? _commandTimeout;
        private readonly CommandType? _commandType;

        /// <summary>
        /// 頁碼
        /// </summary>
        protected readonly int Page;
        /// <summary>
        /// 每頁筆數
        /// </summary>
        protected readonly int PageSize;

        /// <summary>
        /// 建立查詢物件
        /// </summary>
        /// <param name="cnn">The CNN.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="buffered">if set to <c>true</c> [buffered].</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="commandType">Type of the command.</param>
        protected QueryPagedList(IDbConnection cnn, string sql, object param = null, int page = 1,
            int pageSize = 10, IDbTransaction transaction = null, bool buffered = true,
            int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
        {
            Cnn = cnn;
            QueryString = sql;
            _param = param;
            Page = page;
            PageSize = pageSize;
            _transaction = transaction;
            _buffered = buffered;
            _commandTimeout = commandTimeout;
            _commandType = commandType;
        }

        /// <summary>
        /// 計算分頁
        /// </summary>
        /// <returns></returns>
        protected abstract PagedResult ComputePaged();


        /// <summary>
        /// 計算分頁結果
        /// </summary>
        protected class PagedResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PagedResult"/> class.
            /// </summary>
            /// <param name="pagedQueryString">The paged query string.</param>
            /// <param name="dataCount">The data count.</param>
            /// <param name="pageSize">Size of the page.</param>
            public PagedResult(string pagedQueryString, int dataCount, int pageSize)
            {
                TotalDataCount = dataCount;

                PagedQueryString = pagedQueryString;
            }
            /// <summary>
            /// 資料總筆數
            /// </summary>
            public int TotalDataCount { get; set; }
            /// <summary>
            /// 分頁查詢字串
            /// </summary>
            public string PagedQueryString { get; set; }
        }

        /// <summary>
        /// 分頁查詢結果
        /// </summary>
        /// <returns></returns>
        public IPagedList<T> Query()
        {
            var result = ComputePaged();

            return new PageList<T>(
                Cnn.Query<T>(result.PagedQueryString,
                _param,
                _transaction,
                _buffered,
                _commandTimeout,
                _commandType),
                Page, PageSize,
                result.TotalDataCount);
        }
    }
}
