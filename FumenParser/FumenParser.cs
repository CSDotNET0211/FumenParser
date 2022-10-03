
using System.Security.Cryptography.X509Certificates;

namespace Fumen
{
    public class FumenParser
    {
        public enum BlockKind
        {
            Empty,
            I,
            L,
            O,
            Z,
            T,
            J,
            S,
            Gray
        }

        public enum Rotation
        {
            South,
            East,
            North,
            West
        }

        public static int FIELD_HEIGHT { get; private set; } = 24;
        public const int FIELD_WIDTH = 10;
        static public int FIELD_SIZE
        {
            get { return FIELD_WIDTH * FIELD_HEIGHT; }
        }

        /// <summary>
        /// 渡されたURLパラメータを譜面データに変換
        /// </summary>
        /// <param name="URLParameter">URLパラメータ 文頭のhttpの有無は自動判別</param>
        /// <returns></returns>
        static public FumenData Decode(string URLParameter)
        {
            string urlParam;
            if (URLParameter.StartsWith("http"))
                urlParam = URLParameter.Substring(URLParameter.IndexOf('?') + 1).Replace("?", "");
            else
                urlParam = URLParameter.Replace("?", "");

            var urlOffset = 0;

            FumenData fumenData = new FumenData();
            fumenData.View = urlParam.Substring(urlOffset, 1);
            urlOffset += 1;
            fumenData.Version = urlParam.Substring(urlOffset, 3);
            urlOffset += 3;
            urlOffset += 1;//@  
            int vhCount = 0;

            if (fumenData.Version == "115")
                FIELD_HEIGHT = 24;
            else if (fumenData.Version == "110")
                FIELD_HEIGHT = 22;
            else
                throw new Exception("不明なバージョン");

            //ページループ
            while (true)
            {
                fumenData.Pages.Add(new Page(FIELD_HEIGHT, FIELD_WIDTH));
                var topPage = fumenData.Pages[fumenData.Pages.Count - 1];

                //盤面
                DecodeField(topPage);
                //ミノフラグ
                DecodeMinoFlag(topPage);
                //コメント
                if (topPage.Flag.comment)
                    topPage.Comment = DecodeComment(urlParam, ref urlOffset);

                if (urlOffset == urlParam.Length)
                    break;
            }

            return fumenData;


            void DecodeField(Page topPage)
            {
                int blockOffset = 0;

                //ブロック別取得ループ
                while (blockOffset != FIELD_SIZE)
                {
                    if (vhCount > 0)
                    {
                        vhCount--;
                        topPage.Field = (int[])fumenData.Pages[fumenData.Pages.Count - 2].Field.Clone();
                        break;
                    }

                    var tempdata_str = urlParam.Substring(urlOffset, 2);
                    urlOffset += 2;

                    if (tempdata_str == "vh")
                    {
                        vhCount = Poll(1, urlParam.Substring(urlOffset, 1));
                        urlOffset += 1;
                    }

                    var tempdata = Poll(2, tempdata_str);

                    BlockKind data_blockKind = (BlockKind)(tempdata / FIELD_SIZE) - 8;
                    int data_count = tempdata % FIELD_SIZE + 1;

                    for (int i = 0; i < data_count; i++)
                        topPage.Field[blockOffset + i] = (int)data_blockKind;

                    blockOffset += data_count;
                }
            }
            void DecodeMinoFlag(Page topPage)
            {
                var tempdata = Poll(3, urlParam.Substring(urlOffset, 3));
                urlOffset += 3;

                var kind = tempdata % 8;
                tempdata /= 8;
                var rotation = tempdata % 4;
                tempdata /= 4;
                var location = tempdata % FIELD_SIZE;
                tempdata /= FIELD_SIZE;
                var raise = tempdata % 2 == 1 ? true : false;
                tempdata /= 2;
                var mirror = tempdata % 2 == 1 ? true : false;
                tempdata /= 2;
                var color = tempdata % 2 == 1 ? true : false;
                tempdata /= 2;
                var comment = tempdata % 2 == 1 ? true : false;
                tempdata /= 2;
                var locked = tempdata % 2 == 0 ? true : false;

                topPage.Current = (kind, rotation, location);
                topPage.Flag = (raise, mirror, color, comment, locked);
            }
        }

        /// <summary>
        /// 渡された譜面データをURLパラメータ用に変換
        /// </summary>
        /// <param name="fumenData"></param>
        /// <returns></returns>
        static public string Encode(FumenData fumenData)
        {
            if (fumenData.Version == "115")
                FIELD_HEIGHT = 24;
            else if (fumenData.Version == "110")
                FIELD_HEIGHT = 22;
            else
                throw new Exception("不明なバージョン");

            string resultStr = string.Empty;

            resultStr += fumenData.View;
            resultStr += fumenData.Version;
            resultStr += "@";

            int vhCount = 0;

            //ページループ
            for (int pageIndex = 0; pageIndex < fumenData.Pages.Count; pageIndex++)
            {
                var currentPage = fumenData.Pages[pageIndex];

                if (vhCount == 0)
                    EncodeField(pageIndex, currentPage);
                else
                    vhCount--;

                EncodeMinoFlag(currentPage);

                if (currentPage.Flag.comment)
                    resultStr += EncodeComment(currentPage.Comment);

            }

            return resultStr;


            void EncodeField(int pageIndex, Page currentPage)
            {
                int blockKind = currentPage.Field[0];
                int blockCount = 1;

                for (int y = 0; y < FIELD_HEIGHT; y++)
                {
                    for (int x = 0; x < FIELD_WIDTH; x++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        //同じ種類ならまとめる、違ったら今までまとめた分をエンコード
                        if (blockKind == currentPage.Field[x + y * 10])
                        {
                            blockCount++;

                            //シメのまとめ
                            if (y == FIELD_HEIGHT - 1 && x == FIELD_WIDTH - 1)
                                EncodeFieldPartially();
                        }
                        else
                        {
                            EncodeFieldPartially();

                            blockKind = currentPage.Field[x + y * 10];
                            blockCount = 1;
                        }

                        //まとめた分をエンコード関数
                        void EncodeFieldPartially()
                        {
                            var encodedData = PollRevert(2, (currentPage.Field[x + y * 10 - 1] + 8) * FIELD_SIZE + (blockCount - 1));
                            resultStr += encodedData;

                            if (encodedData == "vh")
                            {
                                while (true)
                                {
                                    if (pageIndex + vhCount + 1 < fumenData.Pages.Count &&
                                        fumenData.Pages[pageIndex].Field.IsSameValue(fumenData.Pages[pageIndex + 1 + vhCount].Field))
                                        vhCount++;
                                    else
                                        break;
                                }

                                resultStr += PollRevert(1, vhCount);
                            }
                        }

                    }
                }

            }
            void EncodeMinoFlag(Page currentPage)
            {
                int minoFlagData = currentPage.Flag.locked ? 0 : 1;
                minoFlagData *= 2;
                minoFlagData += currentPage.Flag.comment ? 1 : 0;
                minoFlagData *= 2;
                minoFlagData += currentPage.Flag.color ? 1 : 0;
                minoFlagData *= 2;
                minoFlagData += currentPage.Flag.mirror ? 1 : 0;
                minoFlagData *= 2;
                minoFlagData += currentPage.Flag.raise ? 1 : 0;
                minoFlagData *= FIELD_SIZE;
                minoFlagData += currentPage.Current.location;
                minoFlagData *= 4;
                minoFlagData += currentPage.Current.rotation;
                minoFlagData *= 8;
                minoFlagData += currentPage.Current.kind;
                resultStr += PollRevert(3, minoFlagData);

            }

        }

        /// <summary>
        /// データ文字列を本来の文字列に変換
        /// </summary>
        /// <param name="urlParam">変換対象の文字列</param>
        /// <param name="urlOffset">変換対象の文字列のオフセット</param>
        /// <returns></returns>
        static public string DecodeComment(string urlParam, ref int urlOffset)
        {
            var letterCount = Poll(2, urlParam.Substring(urlOffset, 2));
            urlOffset += 2;
            if (letterCount == 0)
                return "";

            string unicodeComment = string.Empty;

            while (true)
            {
                string unicodeCommentBuff = string.Empty;

                var letterRawData = Poll(5, urlParam.Substring(urlOffset, 5));
                urlOffset += 5;

                for (int i = 0; i < 4; i++)
                {
                    int temp = (int)Math.Pow(96, (3 - i));

                    var tempdata = letterRawData / temp;
                    char letterChar = (char)(tempdata);
                    letterRawData -= letterChar * temp;


                    if (tempdata != 0)
                    {
                        unicodeCommentBuff = ((char)(letterChar + 32)).ToString() + unicodeCommentBuff;
                        letterCount--;

                    }

                }

                unicodeComment += unicodeCommentBuff;

                if (letterCount == 0)
                    break;

            }

            // Console.WriteLine(unicodeComment);
            return ConvertUnicode(unicodeComment);
        }
        /// <summary>
        /// Unicode文字列を本来の文字列に変換
        /// </summary>
        /// <param name="str">Unicode文字列</param>
        /// <returns></returns>
        static public string ConvertUnicode(string str)
        {
            string result = string.Empty;
            List<int> unicodeBuff = new List<int>();

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '%' || unicodeBuff.Count != 0)
                {
                    unicodeBuff.Add(str[i]);

                    if (unicodeBuff.Count == 6 || (unicodeBuff.Count == 3 && unicodeBuff[1] != 'u'))
                    {
                        result += unicodeBuff.ConvertUnicode2Letter();
                        unicodeBuff.Clear();
                    }

                }
                else
                    result += str[i].ToString();
            }

            return result;
        }
        /// <summary>
        /// データ文字列を数値に変換
        /// </summary>
        /// <param name="n">桁数</param>
        /// <param name="value">変換対象の文字列</param>
        /// <returns></returns>
        public static int Poll(int n, string value)
        {
            int result = 0;
            int temp = 1;

            for (int i = 0; i < n; i++)
            {
                result += value[i].ToString().Convert2Integer() * temp;
                temp *= 64;
            }

            return result;
        }
        /// <summary>
        /// 数値をデータ文字列に変換
        /// </summary>
        /// <param name="n">桁数</param>
        /// <param name="value">変換対象の値</param>
        /// <returns></returns>
        public static string PollRevert(int n, int value)
        {
            string result = string.Empty;
            var tempPow = (int)Math.Pow(64, (n - 1));
            for (int i = 0; i < n; i++)
            {
                result = (value / tempPow).Convert2String() + result;
                value -= tempPow * (value / tempPow);
                tempPow /= 64;
            }

            return result;
        }
        /// <summary>
        /// 文字列をデータ文字列に変換
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns>データ文字列</returns>
        static string EncodeComment(string str)
        {
            string resultStr = string.Empty;

            string unicodeLetters = str.ConvertLetters2Unicode();
            //  Console.WriteLine(unicodeLetters);
            int letterCount = 0;
            string commentData = string.Empty;

            while (unicodeLetters.Length > letterCount)
            {
                long unicodeIntger = 0;
                long commentTablePow = 1;
                //１つの数字に変換
                var letterCount4Loop = letterCount;
                for (int i = letterCount4Loop; i < letterCount4Loop + 4; i++)
                {
                    if (i == unicodeLetters.Length)
                        break;

                    unicodeIntger += ((unicodeLetters[i] - 32) * commentTablePow);
                    commentTablePow *= 96;
                    letterCount++;
                }

                // TODO: ここら辺Poll作れるんじゃない
                //5文字データに変換
                long dataFormatPow = (int)Math.Pow(64, 4);
                string tempStr = string.Empty;
                for (int i = 0; i < 5; i++)
                {
                    tempStr = PollRevert(1, (int)(unicodeIntger / dataFormatPow)) + tempStr;
                    unicodeIntger -= dataFormatPow * (unicodeIntger / dataFormatPow);


                    dataFormatPow /= 64;
                }



                commentData += tempStr;
            }

            resultStr += PollRevert(2, letterCount);
            resultStr += commentData;

            return resultStr;

        }

        public static string Test(string str)
        {
            return str.ConvertLetters2Unicode();
        }
    }
}