using System;
using System.Linq.Expressions;

namespace CwLib.Extension
{
    public static class FileExtension
    {   
        /// <summary>
        /// 如果檔名重複則重新命名
        /// </summary>
        /// <param name="fileName">檔案名稱</param>
        /// <param name="expression">檢查條件式</param>
        /// <returns></returns>
        public static string IsFileExistThenRename(this string fileName, Expression<Func<string, bool>> expression)
        {
            var resultName = fileName;

            var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileName);
            var ext = System.IO.Path.GetExtension(fileName);

            var count = 0;

            var func = expression.Compile();
            while (func.Invoke(resultName))
            {
                resultName = fileNameWithoutExtension + "_" + count + ext;
            }

            return resultName;
        }
    }
}