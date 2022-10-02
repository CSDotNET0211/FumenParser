using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fumen
{
    public class Page
    {
        public Page(int fieldHeight, int fieldWidth)
        {
            Field = new int[fieldHeight * fieldWidth];
        }



        public int[] Field { get; set; }


        /// <summary>
        /// ミノが選択されていない場合 kind=rotation=location=0
        /// </summary>
        public (int kind, int rotation, int location) Current { get; set; }
        /// <summary>
        /// 盛 鏡 色 コメント 接着
        /// 2ページ以降の色は必ずfalse
        /// 接着はオンのときfalse
        /// </summary>
        public (bool raise, bool mirror, bool color, bool comment, bool locked) Flag { get; set; }
        public string Comment { get; set; }
    }
}
