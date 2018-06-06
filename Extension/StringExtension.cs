using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CwLib.Extension
{
    /// <summary>
    /// 擴充字串
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 移除HTML TAG
        /// </summary>
        /// <param name="htmlSource"></param>
        /// <returns></returns>
        public static string RemoveHtmlTag(this string htmlSource)
        {
            //移除  javascript code.
            htmlSource = Regex.Replace(htmlSource, @"<script[\d\D]*?>[\d\D]*?</script>", string.Empty);

            //移除html tag.
            htmlSource = Regex.Replace(htmlSource, @"<[^>]*>", string.Empty);

            return htmlSource;
        }

        /// <summary>
        /// 檢查是否為身分證字號或在台居留證號碼
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool CheckTaiwanOrResidentPermitId(this string id)
        {
            var idArray = id.ToUpper().Trim().ToCharArray();

            if (idArray.Length != 10)
            {
                return false;
            }

            switch (idArray[1])
            {
                case '1':
                case '2':
                    return CheckTaiwanId(id);
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                    return CheckTaiwanResidentPermitId(id);
            }

            return false;
        }

        /// <summary>
        /// 檢查身分證字號
        /// </summary>
        /// <param name="id">身分證字號</param>
        /// <returns>是否符合規則</returns>
        public static bool CheckTaiwanId(this string id)
        {
            const string charEng = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
            const string charNumber = "0123456789";

            var idArray = id.ToUpper().Trim().ToCharArray();

            if (idArray.Length != 10)
            {
                return false;
            }

            var isFirst = true;
            var intCount = 0;
            var sum = 8;
            foreach (var idChar in idArray)
            {
                if (isFirst)
                {
                    if (charEng.IndexOf(idChar) == -1)
                    {
                        return false;
                    }

                    var tempArray = Convert.ToString(charEng.IndexOf(idChar) + 10).ToCharArray();
                    intCount += Convert.ToInt32(Convert.ToString(tempArray[0]));
                    intCount += Convert.ToInt32(Convert.ToString(tempArray[1])) * 9;
                    isFirst = false;
                    continue;
                }

                if (charNumber.IndexOf(idChar) == -1)
                {
                    return false;
                }

                intCount += Convert.ToInt32(Convert.ToString(idChar)) * (sum <= 0 ? 1 : sum--);
            }

            return (intCount % 10) == 0;
        }

        /// <summary>
        /// 檢查在台居留證號碼
        /// </summary>
        /// <param name="id">在台居留證號碼</param>
        /// <returns>是否符合規則</returns>
        public static bool CheckTaiwanResidentPermitId(this string id)
        {
            const string charEng = "ABCD";
            const string charNumber = "0123456789";

            var idArray = id.ToUpper().Trim().ToCharArray();

            if (idArray.Length != 10 || (charEng.IndexOf(idArray[0]) == -1 | charEng.IndexOf(idArray[1]) == -1))
            {
                return false;
            }

            for (var i = 0 + 2; i <= 10 - 1; i++)
            {
                if (charNumber.IndexOf(idArray[i]) == -1)
                {
                    return false;
                }
            }

            var tempString = string.Empty;
            tempString += Convert.ToString(charEng.IndexOf(idArray[0]) + 10);
            tempString += Convert.ToString((charEng.IndexOf(idArray[1]) + 10) % 10);

            for (var i = 0 + 2; i <= 10 - 1; i++)
            {
                tempString += Convert.ToString(idArray[i]);
            }
            var intCount = 0;
            intCount += Convert.ToInt32(tempString.Substring(0, 1));
            intCount += Convert.ToInt32(tempString.Substring(1, 1)) * 9;
            intCount += Convert.ToInt32(tempString.Substring(2, 1)) * 8;
            intCount += Convert.ToInt32(tempString.Substring(3, 1)) * 7;
            intCount += Convert.ToInt32(tempString.Substring(4, 1)) * 6;
            intCount += Convert.ToInt32(tempString.Substring(5, 1)) * 5;
            intCount += Convert.ToInt32(tempString.Substring(6, 1)) * 4;
            intCount += Convert.ToInt32(tempString.Substring(7, 1)) * 3;
            intCount += Convert.ToInt32(tempString.Substring(8, 1)) * 2;
            intCount += Convert.ToInt32(tempString.Substring(9, 1)) * 1;
            intCount += Convert.ToInt32(tempString.Substring(10, 1));


            return (intCount % 10) == 0;
        }

        /// <summary>
        /// 字串轉SHA256
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string ToSha256(this string str)
        {
            var sha256 = SHA256.Create();//建立一個SHA256
            var source = Encoding.UTF8.GetBytes(str);//將字串轉為Byte[]
            var crypto = sha256.ComputeHash(source);//進行SHA256加密
            return Convert.ToBase64String(crypto);//把加密後的字串從Byte[]轉為字串
        }
    }
}