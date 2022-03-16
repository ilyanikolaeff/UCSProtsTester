using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCSProtsTester
{
    class CriticalErrorWriteTag
    {
        public string TagName { get; set; }
        public object TagValue { get; set; }

        public override string ToString()
        {
            return $"TagName = {TagName}, TagValue = {TagValue}";
        }
    }
}
