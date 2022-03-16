using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UCSProtsTester
{
    public class PressurePoint
    {
        [XmlAttribute]
        public string TagName { get; set; }

        [XmlIgnore]
        public double LowEng { get; set; }
        [XmlIgnore]
        public double HighEng { get; set; }
        [XmlIgnore]
        public double HiHiLimit { get; set; }
        [XmlIgnore]
        public double HiLimit { get; set; }
        [XmlIgnore]
        public double CurrentValue { get; set; }
        [XmlIgnore]
        public string Name { get; set; }
    }
}
