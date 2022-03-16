using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCSProtsTester
{
    public class Settings
    {
        public string ServerName { get; set; } = "localhost";
        public string ServerAddress { get; set; } = "Elesy.DualSource";
        public int ProtTriggeringWaitingDelay { get; set; } = 5000;
        public PostfixSettings PostfixSettings { get; set; } = new PostfixSettings();
        public List<LogicalPressurePair> LogicalPressurePairs { get; set; } = new List<LogicalPressurePair>();

    }

    public class PostfixSettings
    {
        public string HighEng { get; set; } = ".HighEng";
        public string LowEng { get; set; } = ".LowEng";

        public string HiHiLimit { get; set; } = ".HiHi";
        public string HiLimit { get; set; } = ".Hi";

        public string HighEngTR { get; set; } = ".TR.HighEng.wvalue";
        public string LowEngTR { get; set; } = ".TR.LowEng.wvalue";

        public string SetTMA { get; set; } = ".TU.SetTMA.wvalue";
        public string SetHART { get; set; } = ".TU.SetHART.wvalue";

    }
}
