using System;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace HardpointEditor
{
    class OutputText : Singleton<OutputText>
    {
        delegate void UpdateTextCallback(string message);
        TextBox textbox;

        public void Output(ref TextBox box)
        {
            textbox = box;
        }

        public void WriteLine(string _text)
        {

            textbox.Dispatcher.Invoke(
            new UpdateTextCallback(this.writeToTextBox),
            new object[] { _text }
            );
        


        }

        void writeToTextBox(string s)
        {
            textbox.AppendText(s + "\n");
        }

        public void LogData()
        {
            //TODO: write ErrorLog.txt (append ONLY)
            //      date/time
            //      full output
            //      end message
        }
    }
}
