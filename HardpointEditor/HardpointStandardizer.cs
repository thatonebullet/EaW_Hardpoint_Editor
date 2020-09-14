using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HardpointEditor
{
    class HardpointStandardizer
    {
        /*
        private static HardpointStandardizer instance;
        public static HardpointStandardizer GetInstance()
        {
            if(instance != null)
            {
                instance = new HardpointStandardizer();
            }
            return instance;
        }
        */

        //Temp type to search for
        public string m_searchXMLName { get; private set; }
        public string m_XMLTagType { get; private set; }
        public static string VARIANT_EXISTING_TAG = "Variant_Of_Existing_Type";
        private uint elements_standardized = 0;
        private Dictionary<string, XElement> template_dict = new Dictionary<string, XElement>();
        private Dictionary<string, HardpointData> hardpointDataCollection = new Dictionary<string, HardpointData>();

        public HardpointStandardizer(string p_xmlSearchType, IEnumerable<XElement> p_templateElements)
        {
            m_searchXMLName = p_xmlSearchType;
            /*
             * Template search and log
             */
            foreach (XElement el_template in p_templateElements)
            {

                //Find template search type and value
                m_XMLTagType = el_template.Name.ToString();
                string xmlTagName = el_template.FirstAttribute.ToString();

                OutputText.Instance.WriteLine("Accessing first template XML element. Type: " + m_XMLTagType + ", " + xmlTagName);

                if (!el_template.HasElements)
                {
                    OutputText.Instance.WriteLine("Template " + el_template.Name + " has no elements!");
                    continue;
                }


                var child = el_template.Element(m_searchXMLName);
                if(child != null)
                {
                    string childType = child.Name.ToString();
                    string childValue = child.Value;
                    OutputText.Instance.WriteLine("Accessing element. Type: " + childType + ", Value: " + childValue);

                    if (!template_dict.ContainsKey(childValue))
                    {
                        var elementToInsert = el_template;
                        var variantChild = el_template.Element(VARIANT_EXISTING_TAG);
                        if(variantChild != null)
                        {
                            //If we found a variant of existing type tag...
                            OutputText.Instance.WriteLine("Found variant of existing type. Type: " + childType + ", Value: " + childValue);
                            var variantName = variantChild.Value;
                            if(template_dict.ContainsKey(variantName))
                            {
                                //Use the variant template as a base for a new element
                                elementToInsert = new XElement(template_dict[variantName]);
                                //Add back in the overwritten children into the new variant template
                                foreach(var template_child in el_template.Elements())
                                {
                                    //Ignore the variant tag
                                    if(template_child.Name == VARIANT_EXISTING_TAG)
                                    {
                                        continue;
                                    }

                                    var matching_template_element = elementToInsert.Element(template_child.Name);
                                    if(matching_template_element != null)
                                    {
                                        matching_template_element.ReplaceWith(template_child);
                                    }
                                    else
                                    {
                                        if (elementToInsert.Elements().Count() > 0)
                                        {
                                            elementToInsert.Elements().Last().AddAfterSelf(template_child);
                                        }
                                        else
                                        {
                                            elementToInsert.Elements().LastOrDefault().AddAfterSelf(template_child);
                                        }
                                        
                                    }
                                }
                            }
                        }

                        template_dict.Add(childValue, elementToInsert);
                        OutputText.Instance.WriteLine("Found search type/value. Type: " + childType + ", Value: " + childValue);
                    }
                }
                
                OutputText.Instance.WriteLine("First XML Template found, starting XML standardization on files with found data");
            }
        }




        public void LoadHardpointFiles(string[] fileNames, string path)
        {
            List<XmlParseData> currentxmlData = XmlParser.ReadXMLFiles(fileNames, path);
            OutputText.Instance.WriteLine($"Loading in hardpoints from files in path: {path}");
            foreach(var hardpointData in currentxmlData)
            {
                LoadHardpointsInFile(hardpointData.fileName);
            }
        }

        public void LoadHardpointsInFile(string fileName)
        {
            XmlParseData currentxmlData = XmlParser.ReadXMLFile(fileName);
            OutputText.Instance.WriteLine("Loading in hardpoints from file: " + currentxmlData.fileName);

            foreach (XElement el in currentxmlData.xmlData.Root.Elements())
            {
                //Skip hardpoint if it is not a Hardpoint xml type
                if (el.Name != m_XMLTagType)
                {
                    OutputText.Instance.WriteLine("Skipping element " + el.Name);
                    continue;
                }
                //If the hardpoint has no children, skip
                if (!el.HasElements)
                {
                    OutputText.Instance.WriteLine("Template " + el.Name + " has no elements!");
                    continue;
                }

                //Add hardpoint to data collection 
                HardpointData hpData = new HardpointData(el);
                if (!hardpointDataCollection.ContainsKey(hpData.hardpoint_name))
                {
                    hardpointDataCollection.Add(hpData.hardpoint_name, hpData);
                }
            }
        }

        public XmlParseData StandardizeHardpointsInFile(string fileName)
        {
            XmlParseData currentxmlData = XmlParser.ReadXMLFile(fileName);
            OutputText.Instance.WriteLine("Beginning standardization on file: " + currentxmlData.fileName);
            bool has_file_changed = false;
            XElement ignoreHardpoint = new XElement("Editor_Ignore");
            //Loop through each hardpoint
            foreach (XElement el in currentxmlData.xmlData.Root.Elements())
            {

                //Skip hardpoint if it is not a Hardpoint xml type
                if (el.Name != m_XMLTagType)
                {
                    OutputText.Instance.WriteLine("Skipping element " + el.Name);
                    continue;
                }
                //If the hardpoint has no children, skip
                if (!el.HasElements)
                {
                    OutputText.Instance.WriteLine("Template " + el.Name + " has no elements!");
                    continue;
                }

                //Add hardpoint to data collection 
                HardpointData hpData = new HardpointData(el);
                if (!hardpointDataCollection.ContainsKey(hpData.hardpoint_name))
                {
                    hardpointDataCollection.Add(hpData.hardpoint_name, hpData);
                }

                //If the hardpoint has an Editor_Ignore tag, skip standardization for this hardpoint
                if (el.Elements("Editor_Ignore").Any())
                {
                    OutputText.Instance.WriteLine("Skipping element " + el.Name + " since it was marked Editor_Ignore");
                    continue;
                }

                XElement template_element = null;
                bool elementIsMatching = false;
                foreach (XElement child in el.Elements())
                {

                    string childType = child.Name.ToString();
                    string childValue = child.Value;

                    //Find if this element root has a matching child type and value
                    if (childType == m_searchXMLName && template_dict.ContainsKey(childValue))
                    {
                        OutputText.Instance.WriteLine("Finding child " + childType + " , Value: " + childValue);
                        template_element = template_dict[childValue];
                        elementIsMatching = true;
                        break;
                    }
                }

                if (!elementIsMatching && template_element == null)
                    continue;


                uint children_standardized = 0;
                //Loop through each child in hardpoint and standardize the values
                
                foreach (XElement template_child in template_element.Elements())
                {
                    //Ignore this child if it is the xml tag we are using as a basis for standardization
                    if (template_child.Name.ToString() == m_searchXMLName)
                    {
                        continue;
                    }

                    

                    string templateChildType = template_child.Name.ToString().Replace(" ", "");
                    string templateChildValue = template_child.Value.Replace(" ", "");

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

                        if (!anyChildReplaced)
                        {
                            string replacementValue = ReplaceElement(childElement, template_child);
                            if (replacementValue != null && childElementValue != templateChildValue)
                            {
                                anyChildReplaced = true;
                                
                                string oldValue = childElement.Value.Replace(" ", string.Empty);
                                string newValue = replacementValue.Replace(" ", string.Empty);

                                childElement.Value = newValue;
                                if (children_standardized == 0)
                                    elements_standardized++;
                                children_standardized++;
                                OutputText.Instance.WriteLine("Edited value for child: " + childElement.Name.ToString() + " from value: " + oldValue + " to: " + childElement.Value);
                                

                            }
                            else if(childElementValue == templateChildValue)
                            {
                                anyChildReplaced = true;
                            }
                        }
                    }

                    //add any tags that are not in the hardpoint children
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

                        if (children_standardized == 0)
                            elements_standardized++;
                        children_standardized++;
                        OutputText.Instance.WriteLine("Added child: " + templateChildType + " value: " + templateChildValue);
                    }

                    //TODO: Mark a tag for deletion


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

                //Special cases for Innacuracy
                switch (e1Type)
                {
                    case "Fire_Inaccuracy_Distance":
                        string[] e1_inaccuracy_data = e1Value.Split(',');
                        string[] e2_inaccuracy_data = e2Value.Split(',');

                        string e1_target_type = e1_inaccuracy_data[0].Replace(" ", string.Empty);
                        string e1_accuracy = e1_inaccuracy_data[1].Replace(" ", string.Empty);

                        string e2_target_type = e2_inaccuracy_data[0].Replace(" ", string.Empty);
                        string e2_accuracy = e2_inaccuracy_data[1].Replace(" ", string.Empty);

                        if (e1_target_type == e2_target_type && e1_accuracy != e2_accuracy)
                        {
                            replacementValue = e1_inaccuracy_data[0] + "," + e2_inaccuracy_data[1];
                        }
                        break;
                    default:
                        replacementValue = e2Value;
                        break;
                }
            }
            return replacementValue;
        }

        private string AdjustNonTargetableValue(XElement child)
        {
            string replacementValue = null;

            string e1Type = child.Name.ToString();
            string e1Value = child.Value;

            float value;
            switch (e1Type)
            {
                case "Fire_Min_Recharge_Seconds":
                    value = float.Parse(e1Value);
                    value /= 2;
                    replacementValue = value.ToString();
                    break;
                case "Fire_Max_Recharge_Seconds":
                    value = float.Parse(e1Value);
                    value /= 2;
                    replacementValue = value.ToString();
                    break;
                case "Fire_Pulse_Delay_Seconds":
                    value = float.Parse(e1Value);
                    value /= 2;
                    replacementValue = value.ToString();
                    break;
                default:
                    break;
            }

            return replacementValue;
        }



    }
}
