using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fumen
{
    public  class FumenData
    {
        public FumenData()
        {
            Pages=new List<Page>();
        }

        public string View { get; set; }
        public string Version { get; set; }
       public List<Page> Pages { get; set; }
      



    }
}
