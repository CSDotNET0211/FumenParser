using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fumen
{
    static public class Extension
    {
        const string STRING_TABLE = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        /// <summary>
        /// 文字をテト譜のデータテーブルに対応する数字に変換
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static public int Convert2Integer(this string value)
        {

            return STRING_TABLE.IndexOf(value);
        }

        static public string Convert2String(this int value)
        {
            return (STRING_TABLE[value]).ToString();
        }

        static public string ConvertUnicode2Letter(this List<int> list)
        {
            string result;

            if (!(list.Count == 3 || list.Count == 6))
                throw new Exception("正式なフォーマットではありません。");

            if (list[0] == '%')
            {
                if (list[1] == 'u')
                {
                    var tempstr = "\\u";
                    for (int i = 2; i < 6; i++)
                        tempstr += ((char)list[i]).ToString();

                    result = System.Text.RegularExpressions.Regex.Unescape(tempstr);
                }
                else
                {
                    var tempstr = "\\u";
                    tempstr+="00";
                    for (int i = 1; i < 3; i++)
                        tempstr += ((char)list[i]).ToString();

                    result = System.Text.RegularExpressions.Regex.Unescape(tempstr);

                }
            }
            else
                throw new Exception("正式なフォーマットではありません。");

            return result;
        }

        /// <summary>
        /// ASCIIにない文字はUnicodeに変換
        /// </summary>
        /// <param name="str">変換文字列</param>
        /// <returns>Unicode文字列</returns>
        static public string ConvertLetters2Unicode(this string str)
        {
            var result = string.Empty;
            UnicodeEncoding encoder = new UnicodeEncoding(true, false);

            for (int i = 0; i < str.Length; i++)
            {
                //Unicode:3
                if ((' ' <= str[i] && str[i] <= ')') ||
                    str[i] == ',' ||
                    (':' <= str[i] && str[i] <= '?') ||
                     ('[' <= str[i] && str[i] <= '^') ||
                       str[i] == '`' ||
                        ('{' <= str[i] && str[i] <= '~'))
                {
                    byte[] encodedBytes = encoder.GetBytes(str[i].ToString());
                    result += "%";
                    result += string.Format("{0:X2}", encodedBytes[1]);
                }
                else if (str[i] > 127)//Unicode:6
                {
                    byte[] encodedBytes = encoder.GetBytes(str[i].ToString());
                    result += "%u";
                    result += string.Format("{0:X2}", encodedBytes[0]);
                    result += string.Format("{0:X2}", encodedBytes[1]);
                }
                else//ASCII
                {
                    result += str[i].ToString();
                }
            }

            return result;
        }

        static public bool IsSameValue(this int[] array, int[] array2)
        {
            if (array.Length != array2.Length)
                return false;

            for (int i = 0; i < array.Length; i++)
                if (array[i] != array2[i])
                    return false;

            return true;
        }
    }
}
