using System;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Windows;
using System.Windows.Controls;
using HandbrakeCluster.Common;

namespace HandbrakeCluster.GUITool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void files_Drop(object sender, DragEventArgs e)
        {
            string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            foreach (string droppedFilePath in droppedFilePaths)
            {
                ListBoxItem fileItem = new ListBoxItem();
                
                fileItem.Content = System.IO.Path.GetFileNameWithoutExtension(droppedFilePath);
                fileItem.ToolTip = droppedFilePath;

                files.Items.Add(fileItem);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            files.Items.Clear();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

            string queueName = ConfigurationManager.AppSettings["MSMQLocation"];

            MessageQueue rmTxnQ = new MessageQueue(queueName);

            rmTxnQ.Formatter = new XmlMessageFormatter(new Type[] { typeof(ProcessMessage) });

            foreach (ListBoxItem itm in files.Items)
            {
                MessageQueueTransaction msgTx = new MessageQueueTransaction();
                msgTx.Begin();
                try
                {
                    string argument = "-i \"{0}\" -o \"{1}\" --preset \"" + ConfigurationManager.AppSettings["HandbrakePreset"] + "\"";

                    string destination = txtDestination.Text + "\\" + System.IO.Path.GetFileNameWithoutExtension(itm.ToolTip.ToString()) + ".m4v";

                    ProcessMessage p = new ProcessMessage() { CommandLine = argument, DestinationURL = destination, OrignalFileURL = itm.ToolTip.ToString() };

                    rmTxnQ.Send(p, msgTx);
                    results.Items.Insert(0, string.Format("{0} added to queue", p.OrignalFileURL));
                   
                    msgTx.Commit();
                }
                catch (Exception ex)
                {
                    results.Items.Insert(0, ex.Message);
                    msgTx.Abort();
                }
            }
        }
    }
}

namespace HandbrakeCluster.Common
{
    public class ProcessMessage
    {
        public string OrignalFileURL { get; set; }
        public string DestinationURL { get; set; }
        public string CommandLine { get; set; }
    }
}
