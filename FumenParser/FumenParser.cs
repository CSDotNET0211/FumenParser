
using Microsoft.VisualBasic;

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

        public const int FIELD_HEIGHT = 24;
        public const int FIELD_WIDTH = 10;
        public const int FIELD_SIZE = FIELD_HEIGHT * FIELD_WIDTH;


        //https://fumen.zui.jp/?v110@ReC3jbF3gbA3ibH3gbE3pbAoUkAlvs2A1sDfET4p9B?lPZOBToDfEVZi9Alvs2AUDEfETYOVB7eDuKPkAlvs2A1sDf?ETY9KBlvs2A1yDfETo/AClPxRBTuDfEVekRBFxqnAUNKSA1?dkRByXHDBQRsRA1dEEBEYHDBQEFSAVvT3AwXPNBVD5AANRP?PAU9rSA1dEEBCYHDBQ04AAaHPVAUNuSA1dkRByX3JBzXOSA?Vl0ACkAAAA
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

            while (true)
            {
                fumenData.Pages.Add(new Page(FIELD_HEIGHT, FIELD_WIDTH));
                var topFumenPage = fumenData.Pages[fumenData.Pages.Count - 1];

                //盤面
                {
                    int blockOffset = 0;

                    //ブロック別取得ループ
                    while (blockOffset != FIELD_SIZE)
                    {
                        if (vhCount > 0)
                        {
                            vhCount--;
                            topFumenPage.Field = (int[])fumenData.Pages[fumenData.Pages.Count - 2].Field.Clone();
                            break;
                        }

                        var tempdata_str = urlParam.Substring(urlOffset, 2);
                        urlOffset += 2;

                        if (tempdata_str == "vh")
                        {
                            vhCount = Poll(1, urlParam.Substring(urlOffset, 1));
                            urlOffset += 1;

                            //初回vh
                            // if (fumenData.Pages.Count - 1 == 0)
                            //     vhCount--;

                        }

                        var tempdata = Poll(2, tempdata_str);

                        BlockKind data_blockDiff = (BlockKind)(tempdata / FIELD_SIZE) - 8;
                        int data_count = tempdata % FIELD_SIZE + 1;

                        int blockKind;

                        if (fumenData.Pages.Count == 1)
                            blockKind = (int)data_blockDiff;
                        else
                            blockKind = fumenData.Pages[fumenData.Pages.Count - 2].Field[blockOffset] - (int)data_blockDiff + 8;

                        // TODO: 変換調整
                        blockKind = (int)data_blockDiff;
                        for (int i = 0; i < data_count; i++)
                            topFumenPage.Field[blockOffset + i] = blockKind;

                        blockOffset += data_count;
                    }
                }

                //ミノフラグ
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

                    topFumenPage.Current = (kind, rotation, location);
                    topFumenPage.Flag = (raise, mirror, color, comment, locked);
                }

                // TODO: 更新フラグが立ってない間はコメントコピー
                //コメント
                if (topFumenPage.Flag.comment)
                    topFumenPage.Comment = ConvertComment2Letters(urlParam, ref urlOffset);


                if (urlOffset == urlParam.Length)
                    break;
            }

            return fumenData;
        }

        public static string Test(string str)
        {
            var value=0;
            return ConvertComment2Letters(str,ref value);
        }

        static public string Encode(FumenData fumenData)
        {
            string resultStr = string.Empty;

            resultStr += fumenData.View;
            resultStr += fumenData.Version;
            resultStr += "@";

            int vhCount = 0;

            for (int pageIndex = 0; pageIndex < fumenData.Pages.Count; pageIndex++)
            {
                var currentPage = fumenData.Pages[pageIndex];

                if (vhCount == 0)
                {
                    int blockKind = currentPage.Field[0];
                    int blockCount = 1;


                    for (int y = 0; y < FIELD_HEIGHT; y++)
                    {
                        for (int x = 0; x < FIELD_WIDTH; x++)
                        {
                            if (x == 0 && y == 0)
                                continue;

                            if (blockKind == currentPage.Field[x + y * 10])
                            {
                                blockCount++;

                                if (y == FIELD_HEIGHT - 1 && x == FIELD_WIDTH - 1)
                                {
                                    Update();
                                }
                            }
                            else
                            {
                                Update();



                                blockKind = currentPage.Field[x + y * 10];
                                blockCount = 1;
                            }

                            void Update()
                            {
                                //全て同じ盤面でも、最初がvhだったら
                                //

                                // TODO: リアルな値にするので後はよろしく


                                int blockDiff;
                                if (pageIndex - 1 >= 0)
                                    blockDiff = currentPage.Field[x + y * 10 - 1] - currentPage.Field[x + y * 10 - 1 - 1];
                                else
                                    blockDiff = currentPage.Field[x + y * 10 - 1];

                                blockDiff = currentPage.Field[x + y * 10 - 1];

#if DEBUG
                                int a = 3;
#endif
                                var encodedData = PollRevert(2, (blockDiff + 8) * 240 + (blockCount - 1));
                                resultStr += encodedData;

                                if (encodedData == "vh")
                                {
                                    while (true)
                                    {
                                        if (pageIndex + vhCount + 1 < fumenData.Pages.Count &&
                                            fumenData.Pages[pageIndex].Field.IsSameValue(fumenData.Pages[pageIndex + 1 + vhCount].Field))
                                        {
                                            vhCount++;
                                        }
                                        else
                                            break;
                                    }

                                    resultStr += PollRevert(1, vhCount);
                                }
                            }

                        }
                    }
                }
                else
                {
                    vhCount--;
                }


                // TODO: ミノ情報を追加した後、盤面リピートを先読み調査して追加、vhCountを作っておく
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


                //コメントをUnicodeに変換して、コメントテーブルで変換（char）、一つの数値に変換
                if (currentPage.Flag.comment)
                    resultStr += EncodeComment(currentPage.Comment);

            }

            return resultStr;

        }

        static public string ConvertComment2Letters(string urlParam, ref int urlOffset)
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

            return ConvertUnicode(unicodeComment);
        }

        static public string ConvertUnicode(string str)
        {
            string result = string.Empty;
            List<int> unicodeBuff = new List<int>();

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '%' || unicodeBuff.Count != 0)
                {
                    unicodeBuff.Add(str[i]);

                    if (unicodeBuff.Count == 6)
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

        static string EncodeComment(string str)
        {
            string resultStr = string.Empty;

            string unicodeLetters = str.ConvertLetters2Unicode();
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
        //左上が0、右下に向かって++


    }
}