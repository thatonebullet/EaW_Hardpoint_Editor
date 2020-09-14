using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Xml.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace HardpointEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] m_hardpointFileList;
        string[] m_unitFileList;
        XmlParseData m_templateFileData;
        XmlParseData m_processedUnitStats;
        uint generateButtonActive = 0;
        uint generateButtonActiveTarget = 2;
        string m_tempFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "Temp\\";
        string m_tempHardpointFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "Temp\\Hardpoints\\";
        string m_tempUnitFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "Temp\\Units\\";
        string m_OutputFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "Output\\";
        string m_hardpointOutputFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "Output\\Hardpoints\\";
        string m_unitOutputFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "Output\\Units\\";
        XmlParser xmlParser = new XmlParser();
        BackgroundWorker hardpointStandardizer = new BackgroundWorker();
        BackgroundWorker unitExporter = new BackgroundWorker();

        BackgroundWorker currentBackgroundWorker = null;
        public MainWindow()
        {
            //Initialize window
            InitializeComponent();

            //Initialize output text
            OutputText.Instance.Output(ref _ErrorLog);

            //Initialize hardpoint standardizer worker
            hardpointStandardizer.WorkerReportsProgress = true;
            hardpointStandardizer.DoWork += new System.ComponentModel.DoWorkEventHandler(StandardizeHardpointXMLFiles);
            hardpointStandardizer.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(ReportStandardization);
            hardpointStandardizer.WorkerSupportsCancellation = true;

            //TODO: Initialize unitExporter worker
            unitExporter.WorkerReportsProgress = true;
            unitExporter.DoWork += new System.ComponentModel.DoWorkEventHandler(StandardizeUnitXMLFiles);
            unitExporter.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(ReportStandardization);
            unitExporter.WorkerSupportsCancellation = true;

            //Clean up output file path
            if (Directory.Exists(m_hardpointOutputFilePath))
            {
                Directory.Delete(m_hardpointOutputFilePath, true);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

        }
        //
        // Loads the hardpoint template files.
        //
        private void Load_Template_Click(object sender, RoutedEventArgs e)
        {
            Load_Hardpoint_Templates();
        }

        private bool Load_Hardpoint_Templates()
        {
            string fileLocation = "", fullFilePath = "", fileName = "";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML Files (hardpoints_template.xml)|hardpoints_template.xml";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                fullFilePath = dlg.FileName;
                fileLocation = System.IO.Path.GetDirectoryName(fullFilePath);
                fileName = System.IO.Path.GetFileName(fullFilePath);
                OutputText.Instance.WriteLine("Loaded the template file: " + fullFilePath);
            }
            else
            {
                OutputText.Instance.WriteLine("Could not load hardpoint template!");
                return false;
            }

            m_templateFileData = XmlParser.ReadXMLFile(fullFilePath);
            if (m_templateFileData.xmlData != null)
            {
                _TemplateFile.Text = fullFilePath;
                return true;
            }
            return false;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Import_Hardpoints();
        }

        private void Import_Hardpoints()
        {
            string fileLocation = "", fullFilePath = "";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML Files (hardpointdatafiles.xml)|hardpointdatafiles.xml";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                fullFilePath = dlg.FileName;
                fileLocation = System.IO.Path.GetDirectoryName(fullFilePath);
            }
            else
            {
                OutputText.Instance.WriteLine("Failed to get template hardpoint from user.");
                return;
            }

            m_hardpointFileList = XmlParser.ReadXMLDataFiles(fullFilePath, m_tempHardpointFilePath);
            OutputText.Instance.WriteLine("Parsed " + fullFilePath);


            if (m_hardpointFileList.Length == 0)
            {
                OutputText.Instance.WriteLine("Hardpoint list is empty.");
            }
            else
            {
                OutputText.Instance.WriteLine("Hardpoint list is stored.");
                _HardpointFiles.Text = fullFilePath;
            }
        }

        
        private void Import_Units_Stats_Click(object sender, RoutedEventArgs e)
        {
            Import_Unit_Stats();
        }

        private void Import_Unit_Stats()
        {
            string fileLocation = "", fullFilePath = "";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                fullFilePath = dlg.FileName;
                fileLocation = System.IO.Path.GetDirectoryName(fullFilePath);
            }
            else
            {
                OutputText.Instance.WriteLine("Failed to get unit stats csv from user.");
                return;
            }
            
            XElement rawUnitStats = CSVUtil.ConvertCSVtoFOCXML(fullFilePath, "SpaceUnits", "SpaceUnit");
            if (rawUnitStats == null || !rawUnitStats.HasElements)
            {
                OutputText.Instance.WriteLine("Unit Stats File is empty.");
            }
            else
            {
                OutputText.Instance.WriteLine("Unit Stats File is stored.");
                _UnitStats.Text = fullFilePath;
            }
            m_processedUnitStats = new XmlParseData(fullFilePath, XDocument.Parse(rawUnitStats.ToString()));

            if (m_processedUnitStats.xmlData != null)
            {
                OutputText.Instance.WriteLine($"Parsed Unit Stats { fullFilePath }" );
            }
                
        }

        private void Import_Units_Click(object sender, RoutedEventArgs e)
        {
            Import_Units();
        }

        private void Import_Units()
        {
            string fileLocation = "", fullFilePath = "";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML Files (uniteditorfiles.xml)|uniteditorfiles.xml"; 


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                fullFilePath = dlg.FileName;
                fileLocation = System.IO.Path.GetDirectoryName(fullFilePath);
            }
            else
            {
                OutputText.Instance.WriteLine("Failed to get unit file list from user.");
                return;
            }

            m_unitFileList = XmlParser.ReadXMLDataFiles(fullFilePath, m_tempUnitFilePath);
            OutputText.Instance.WriteLine("Parsed " + fullFilePath);


            if (m_unitFileList.Length == 0)
            {
                OutputText.Instance.WriteLine("Unit list is empty.");
            }
            else
            {
                OutputText.Instance.WriteLine("Unit list is stored.");
                _UnitFiles.Text = fullFilePath;
            }
        }


        private void Export_Unit_Stats()
        {

            string rawUnitStats = CSVUtil.ConvertFOCXMLtoCSV(m_unitFileList);

            System.IO.FileInfo file = new System.IO.FileInfo(m_unitOutputFilePath + "rawUnitStats.csv");
            file.Directory.Create(); // If the directory already exists, this method does nothing.
            System.IO.File.WriteAllText(file.FullName, rawUnitStats);

        }



        private void Standardize_Hardpoints_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(_TemplateFile.Text))
            {
                Load_Hardpoint_Templates();
            }

            if(String.IsNullOrEmpty(_HardpointFiles.Text))
            {
                Import_Hardpoints();
            }

            if(String.IsNullOrEmpty(_TemplateFile.Text) || String.IsNullOrEmpty(_HardpointFiles.Text))
            {
                return;
            }
            

            OutputText.Instance.WriteLine("Starting XML Standardization on loaded files");

            //Clean up output file path
            if (Directory.Exists(m_hardpointOutputFilePath))
            {
                Directory.Delete(m_hardpointOutputFilePath, true);
            }

            currentBackgroundWorker?.CancelAsync();
            currentBackgroundWorker = hardpointStandardizer;
            currentBackgroundWorker.RunWorkerAsync();
        }

        private void Standardize_Units_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(_UnitStats.Text))
            {
                Import_Unit_Stats();
            }

            if (String.IsNullOrEmpty(_UnitFiles.Text))
            {
                Import_Units();
            }

            if (String.IsNullOrEmpty(_UnitFiles.Text) || String.IsNullOrEmpty(_UnitStats.Text))
            {
                return;
            }

            OutputText.Instance.WriteLine("Starting Unit Standardization on loaded files");

            //Clean up output file path
            if (Directory.Exists(m_unitOutputFilePath))
            {
                Directory.Delete(m_unitOutputFilePath, true);
            }
            currentBackgroundWorker?.CancelAsync();
            currentBackgroundWorker = unitExporter;
            currentBackgroundWorker.RunWorkerAsync();

            
        }

        private void Export_Units_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(_UnitFiles.Text))
            {
                Import_Units();
            }

            if (String.IsNullOrEmpty(_UnitFiles.Text))
            {
                OutputText.Instance.WriteLine("Not All Dependencies were successfully loaded to begin operation.");
                return;
            }

            OutputText.Instance.WriteLine("Starting Unit Standardization on loaded files");

            //Clean up output file path
            if (Directory.Exists(m_unitOutputFilePath))
            {
                Directory.Delete(m_unitOutputFilePath, true);
            }

            Export_Unit_Stats();

            //currentBackgroundWorker?.CancelAsync();
            //currentBackgroundWorker = unitExporter;
            //currentBackgroundWorker.RunWorkerAsync();


        }


        private void StandardizeHardpointXMLFiles(object sender, DoWorkEventArgs e)
        {
            HardpointStandardizer hpStandardizer = new HardpointStandardizer("Fire_Projectile_Type", m_templateFileData.xmlData.Root.Elements());
            
            
            XmlParseData currentxmlData;
            //Begin going through each file and standardizing them based on found template data
            uint files_changed = 0;
            foreach (string fileName in m_hardpointFileList)
            {
                currentxmlData = hpStandardizer.StandardizeHardpointsInFile(fileName);
                if (currentxmlData.hasFileChanged)
                {
                    if (!Directory.Exists(m_hardpointOutputFilePath))
                       Directory.CreateDirectory(m_hardpointOutputFilePath);
                    //Save edited file
                    currentxmlData.xmlData.Save(m_hardpointOutputFilePath + currentxmlData.fileName);
                    files_changed++;
                }
            }
            OutputText.Instance.WriteLine("Done with full file standardization!");
            //OutputText.Instance.WriteLine("Elements standardized: " + elements_standardized);
            OutputText.Instance.WriteLine("Files edited: " + files_changed);
        }

        private void StandardizeUnitXMLFiles(object sender, DoWorkEventArgs e)
        {
            //HardpointStandardizer hpStandardizer = new HardpointStandardizer("Fire_Projectile_Type", m_templateFileData.xmlData.Root.Elements());
            UnitStandardizer unitStandardizer = new UnitStandardizer(m_processedUnitStats.xmlData.Root.Elements());

            XmlParseData currentxmlData;
            //Begin going through each file and standardizing them based on found template data
            uint files_changed = 0;
            foreach (string fileName in m_unitFileList)
            {
                currentxmlData = unitStandardizer.StandardizeUnitsInFile(fileName);
                if (currentxmlData.hasFileChanged)
                {
                    if (!Directory.Exists(m_unitOutputFilePath))
                        Directory.CreateDirectory(m_unitOutputFilePath);
                    //Save edited file
                    currentxmlData.xmlData.Save(m_unitOutputFilePath + currentxmlData.fileName);
                    files_changed++;
                }
            }
            OutputText.Instance.WriteLine("Done with full file standardization!");
            //OutputText.Instance.WriteLine("Elements standardized: " + elements_standardized);
            OutputText.Instance.WriteLine("Files edited: " + files_changed);
        }




        private void ReportStandardization(object sender, ProgressChangedEventArgs e)
        {
            OutputText.Instance.WriteLine((string)e.UserState);
        }

        private void ErrorLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            _ErrorLog.ScrollToEnd();
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            currentBackgroundWorker?.CancelAsync();
            // Delete any pre-existing TEMP directory
            if (Directory.Exists(m_tempFilePath))
            {
                Directory.Delete(m_tempFilePath, true);
            }

        }

        
    }
}
