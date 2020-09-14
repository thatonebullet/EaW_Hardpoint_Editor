using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HardpointEditor
{
    class UnitStandardizer
    {
        
        private uint units_standardized = 0;
        private Dictionary<string, XElement> unit_stats_dict = new Dictionary<string, XElement>();
        private Dictionary<string, UnitData> unit_dictionary = new Dictionary<string, UnitData>();
        
        public UnitStandardizer(IEnumerable<XElement> p_unitStats)
        {
            OutputText.Instance.WriteLine("Loading unit stats template");
            foreach (XElement unit in p_unitStats)
            {
                if (!unit.HasElements)
                {
                    OutputText.Instance.WriteLine("Unit Stat Template " + unit.Name + " has no elements!");
                    continue;
                }

                string xmlTagName = unit.FirstAttribute?.Value.ToString();
                if (String.IsNullOrEmpty(xmlTagName))
                    continue;
                string xmlTagType = unit.Name.ToString();

                unit_stats_dict[xmlTagName] = unit;
                OutputText.Instance.WriteLine("Accessing first child element. Type: " + xmlTagType + ", Value: " + xmlTagName);
                
                OutputText.Instance.WriteLine("Unit stats loaded, ready for XML standardization on files with found data");
            }
        }

            
        public XmlParseData StandardizeUnitsInFile(string fileName)
        {
            XmlParseData currentxmlData = XmlParser.ReadXMLFile(fileName);
            OutputText.Instance.WriteLine("Beginning standardization on file: " + currentxmlData.fileName);
            bool has_file_changed = false;
            //Loop through each unit
            foreach (XElement el in currentxmlData.xmlData.Root.Elements())
            {
                UnitData unitData = new UnitData(el);
                //Skip unit if it is not in the unit stat list
                if (!unit_stats_dict.ContainsKey(unitData.unit_name))
                {
                    OutputText.Instance.WriteLine("Skipping unit " + unitData.unit_name);
                    continue;
                }
                //If the unit has no children, skip
                if (!el.HasElements)
                {
                    OutputText.Instance.WriteLine("Unit " + unitData.unit_name + " has no elements!");
                    continue;
                }
                //Add unit to data collection 
                unit_dictionary[unitData.unit_name] = unitData;
                OutputText.Instance.WriteLine("Finding unit stats for " + unitData.unit_name);
                XElement unit_stats_template_element = unit_stats_dict[unitData.unit_name];


                //Loop through each template child and add any tags that are not in the hardpoint children

                uint children_standardized = 0;
                //Loop through each child in hardpoint and standardize the values
                foreach (XElement template_child in unit_stats_template_element.Elements())
                {

                    string templateChildType = template_child.Name.ToString().Replace(" ", "");
                    string templateChildValue = template_child.Value.Replace(" ", "");

                    if (String.IsNullOrEmpty(templateChildValue) || templateChildValue == "#N/A")
                    {
                        continue;
                    }

                    IEnumerable<XElement> collection = el.Elements(templateChildType);
                    bool anyChildReplaced = false;
                    foreach (XElement childElement in collection)
                    {
                        if (childElement.FirstAttribute != null)
                        {
                            if (childElement.FirstAttribute.Name == "Editor_Ignore")
                            {
                                anyChildReplaced = true;
                                continue;
                            }
                        }

                        string childElementType = childElement.Name.ToString().Replace(" ", "");
                        string childElementValue = childElement.Value.Replace(" ", "");

                        
                        string replacementValue = ReplaceElement(childElement, template_child);
                        if (replacementValue != null && childElementValue != templateChildValue)
                        {
                            anyChildReplaced = true;
                            string oldValue = childElement.Value.Replace(" ", string.Empty);
                            string newValue = replacementValue.Replace(" ", string.Empty);

                            childElement.Value = newValue;
                            if (children_standardized == 0)
                                units_standardized++;
                            children_standardized++;
                            OutputText.Instance.WriteLine("Edited value for child: " + childElement.Name.ToString() + " from value: " + oldValue + " to: " + childElement.Value);
                        }
                        else if(childElementValue == templateChildValue)
                        {
                            anyChildReplaced = true;
                        }
                        
                    }

                    if (!anyChildReplaced)
                    {
                        if (collection.Count() > 0)
                        {
                            collection.Last().AddAfterSelf(template_child);
                        }
                        else
                        {
                            el.Elements().LastOrDefault().AddAfterSelf(template_child);
                        }
                        OutputText.Instance.WriteLine("Added child: " + templateChildType + " value: " + templateChildValue);
                        if (children_standardized == 0)
                            units_standardized++;
                        children_standardized++;
                    }

                    if (!has_file_changed && children_standardized > 0)
                    {
                        has_file_changed = true;
                        currentxmlData.hasFileChanged = true;
                    }
                }

                OutputText.Instance.WriteLine("Done with standardization on element: " + el.Name + ", " + el.FirstAttribute.Value);
                OutputText.Instance.WriteLine("Children changed: " + children_standardized);
                children_standardized = 0;

            }
            OutputText.Instance.WriteLine("Done with standardization on file: " + currentxmlData.fileName);
            return currentxmlData;
        }

        private string ReplaceElement(XElement e1, XElement e2)
        {
            string replacementValue = null;

            string e1Type = e1.Name.ToString().Replace(" ", "");
            string e1Value = e1.Value.Replace(" ", "");

            string e2Type = e2.Name.ToString().Replace(" ", "");
            string e2Value = e2.Value.Replace(" ", "");


            if (e1Type == e2Type && e1Value != e2Value)
            {

                //Special cases for tag types
                switch (e1Type)
                {
                    
                    default:
                        replacementValue = e2Value;
                        break;
                }
            }
            return replacementValue;
        }


    }
}
