using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UCSProtsTester
{
    public class LogicalPressurePair
    {
        [XmlIgnore]
        public string Name { get; set; }

        [XmlAttribute]
        public string ProtSourceInput { get; set; }
        [XmlAttribute]
        public string ProtSourceOutput { get; set; }

        public PressurePoint LeftPoint { get; set; }
        public PressurePoint RightPoint { get; set; }
    }
}
