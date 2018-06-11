using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CwLib.Dapper
{
    /// <summary>
    /// 分頁
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    public interface IPagedList<out T> : IEnumerable<T>
    {
        /// <summary>
        /// 頁碼
        /// </summary>
        int Page { get; }
        /// <summary>
        /// 每頁筆數
        /// </summary>
        int PageSize { get; }
        /// <summary>
        /// 總頁數
        /// </summary>
        int TotalPage { get; }
        /// <summary>
        /// 總筆數
        /// </summary>
        int TotalCount { get; }
        /// <summary>
        /// 此頁內容
        /// </summary>
        IEnumerable<T> Data { get; }
    }

    internal class PageList<T> : IPagedList<T>
    {
        public PageList(IEnumerable<T> data, int page, int pageSize, int totalCount)
        {
            Data = data;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPage = TotalCount / pageSize + (TotalCount / pageSize != 0 ? 1 : 0);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        public int Page { get; }
        public int PageSize { get; }
        public int TotalPage { get; }
        public int TotalCount { get; }
        public IEnumerable<T> Data { get; }
    }
}
