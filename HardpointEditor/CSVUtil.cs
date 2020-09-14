using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HardpointEditor
{
    class CSVUtil
    {
        public static XElement ConvertCSVtoFOCXML(string fileLocation, string xmlRootType, string xmlUnitType)
        {

            // Read into an array of strings.  
            string[] source = File.ReadAllLines(fileLocation);
            if(source.Length <= 1) //If there's only a header of xml tag names
            {
                return null;
            }
            string xmlHeaders = source[0];
            string[] xmlTags = xmlHeaders.Split(',');
            string[] unitStats = source.Skip(1).ToArray(); //Skipping the header xml tags


            XElement cust = new XElement(xmlRootType);
            
            for(int i = 0; i < unitStats.Length; i++)
            {
                XElement unitType = new XElement(xmlUnitType);
                string[] fields = unitStats[i].Split(',');
                for (int j = 0; j < fields.Length && j < xmlTags.Length; j++)
                {
                    if(j == 0)
                    {
                        unitType.Add(new XAttribute(xmlTags[j], fields[j]));
                    }
                    else
                    {
                        unitType.Add(new XElement(xmlTags[j], fields[j]));
                    }
                    
                }
                cust.Add(unitType);
            }
            
            return cust;
        }

        public static string ConvertFOCXMLtoCSV(string[] unitStatsFileList)
        {
            // Create the text file.  
            HashSet<string> csvHeader = new HashSet<string>();
            StringBuilder sb = new StringBuilder();
            csvHeader.Add("Name");
            sb.Append("Name");

            Dictionary<UnitData, Dictionary<string, string>> unitList = new Dictionary<UnitData, Dictionary<string, string>>();
            foreach (string filePath in unitStatsFileList)
            {
                XmlParseData currentxmlData = XmlParser.ReadXMLFile(filePath);
                OutputText.Instance.WriteLine("Beginning unit data export on file: " + currentxmlData.fileName);
                //Loop through each unit
                
                foreach (XElement el in currentxmlData.xmlData.Root.Elements())
                {
                    //Unit Level
                    if(el.HasAttributes && el.Attribute("Name") != null)
                    {
                        UnitData unitData = new UnitData(el);
                        Dictionary<string, string> unitStats = new Dictionary<string, string>();
                        foreach (XElement child in unitData.raw_xmlData.Elements())
                        {
                            if(child.HasElements) //Don't support complex xml child types yet
                            {
                                continue;
                            }

                            string tagType = child.Name.ToString().ToLower();

                            if (tagType == "hardpoints") //Hardpoint editor will need to handle this case
                            {
                                continue;
                            }

                            string tagValue = child.Value;

                            if (!csvHeader.Contains(tagType))
                            {
                                csvHeader.Add(tagType);
                            }

                            unitStats[tagType] = child.Value;
                        }

                        unitList[unitData] = unitStats;
                    }
                }
            }

            
            csvHeader.Remove("Name");
            //List<string> sortedheader = csvHeader.ToList();
            //sortedheader.Sort();
            
            foreach(var key in csvHeader)
            {
                sb.Append(",");
                sb.Append(key);
            }



            foreach(var unit in unitList)
            {
                sb.Append(",");
                sb.AppendLine();
                sb.Append(unit.Key.unit_name);
                
                var unitStats = unit.Value;
                foreach(string key in csvHeader)
                {
                    sb.Append(",");
                    if (unitStats.ContainsKey(key))
                    {
                        string statValue = Regex.Replace(unitStats[key], @"\t|\n|\r", "");
                        statValue = Regex.Replace(statValue, @",", " ");
                        sb.Append(statValue);
                    }
                }
            }
            string csvString;
            csvString = sb.ToString();
            return csvString;
        }
    }
}
