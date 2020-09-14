using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HardpointEditor
{
    public struct HardpointData
    {
        public XElement raw_xmlData { get; private set; }
        //Hardpoint variables
        public string hardpoint_name { get; private set; }

        public HardpointData(XElement p_xmlData)
        {
            raw_xmlData = p_xmlData;
            //Find hardpoint name
            hardpoint_name = p_xmlData.FirstAttribute.ToString();
        }
    }

    public struct UnitData
    {
        public XElement raw_xmlData { get; private set; }
        //Hardpoint variables
        public string unit_name { get; private set; }

        public UnitData(XElement p_xmlData)
        {
            raw_xmlData = p_xmlData;
            //Find hardpoint name
            unit_name = p_xmlData.FirstAttribute?.Value;
        }
    }

}
