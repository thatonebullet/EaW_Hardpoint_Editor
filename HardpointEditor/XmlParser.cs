using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.IO;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace HardpointEditor
{
    struct XmlParseData
    {
        public XDocument xmlData;
        public string fileName;
        public bool hasFileChanged;

        public XmlParseData(string p_fileName, XDocument p_xmlData)
        {
            this.fileName = p_fileName;
            this.xmlData = p_xmlData;
            this.hasFileChanged = false;
        }


    }

    class XmlParser
    {
        
        public static List<XmlParseData> ReadXMLFiles(string[] fileNames, string filepath)
        {
            List<XmlParseData> hardpointFileData = new List<XmlParseData>(fileNames.Length);
            foreach(string fileName in fileNames)
            {
                hardpointFileData.Add ( ReadXMLFile(fileName, filepath) );
            }
            return hardpointFileData;
        }


        public static XmlParseData ReadXMLFile(string completeFileName)
        {
            return ReadXMLFile(System.IO.Path.GetFileName(completeFileName), System.IO.Path.GetDirectoryName(completeFileName));
        }

        public static XmlParseData ReadXMLFile(string fileName, string filepath)
        {
            string fullFilePathAndFileName = filepath + "\\" + fileName;
            XDocument doc = XDocument.Load(fullFilePathAndFileName);

            if (doc != null)
            {
                OutputText.Instance.WriteLine("Loaded the file: " + fullFilePathAndFileName);
            }
            else
            {
                string fileNotFoundName = Path.GetFileName(fileName) + " was not found in directory " + filepath;
                OutputText.Instance.WriteLine(fileNotFoundName);
            }

            XmlParseData hardpointFileData = new XmlParseData(fileName, doc);
            return hardpointFileData;
        }



        public static string[] ReadXMLDataFiles(string fileName, string newfilepath)
        {
            
            XDocument doc = XDocument.Load(fileName);
            string filePath = System.IO.Path.GetDirectoryName(fileName);
            List<string> fileNames = new List<string>();
            
            foreach (XElement el in doc.Root.Elements())
            {
                if(el.Name == "File")
                {
                    string fullfilepath = filePath + "\\" + el.Value;
                    string fullnewfilepath = newfilepath + el.Value;
                    CopyXmlDocument(fullfilepath, fullnewfilepath);
                    fileNames.Add(fullnewfilepath);
                    OutputText.Instance.WriteLine("Added " + el.Value + " to filename list");
                }
            }
            return fileNames.ToArray();
        }


        public static void CopyXmlDocument(string oldfile, string newfile)
        {
            XDocument doc = XDocument.Load(oldfile);

            string newdocpath = Path.GetDirectoryName(newfile);
            string olddocname = Path.GetFileName(oldfile);

            OutputText.Instance.WriteLine("Moved " + oldfile + " to " + newdocpath);
            if (File.Exists(newfile))
                File.Delete(newfile);

            if(!Directory.Exists(newdocpath))
                Directory.CreateDirectory(newdocpath);
            

            // Create fresh TEMP directory
            DirectoryInfo tempDirectory = Directory.CreateDirectory(newdocpath);
            FileAttributes attributes = tempDirectory.Attributes;
            attributes |= FileAttributes.Hidden;
            tempDirectory.Attributes = attributes;


            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                DirectorySecurity security = tempDirectory.GetAccessControl();

            security.AddAccessRule(new FileSystemAccessRule(@userName,
                                    FileSystemRights.Modify,
                                    AccessControlType.Allow));

            tempDirectory.SetAccessControl(security);


            doc.Save(newfile);
        }



    }
}
