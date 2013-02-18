using System;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Windows;
using System.Windows.Controls;
using HandbrakeCluster.Common;
using System.IO;
using ProtoBuf;

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

            string queueName = ConfigurationManager.AppSettings["RabbitMQQueue"];
            string queueHost = ConfigurationManager.AppSettings["RabbitMQHost"];
            Producer.Producer prod = new Producer.Producer(queueHost, queueName);
            foreach (ListBoxItem itm in files.Items)
            {
               
                try
                {
                    string argument = "-i \"{0}\" -o \"{1}\" --preset \"" + ConfigurationManager.AppSettings["HandbrakePreset"] + "\"";

                    string destination = txtDestination.Text + "\\" + System.IO.Path.GetFileNameWithoutExtension(itm.ToolTip.ToString()) + ".m4v";

                    ProcessMessage p = new ProcessMessage() { CommandLine = argument, DestinationURL = destination, OrignalFileURL = itm.ToolTip.ToString() };

                    using (MemoryStream ms = new MemoryStream())
                    {
                        Serializer.Serialize(ms, p);
                        byte[] message = ms.ToArray();
                        prod.SendMessage(message);
                        results.Items.Insert(0, itm.ToolTip.ToString());
                    }

                }
                catch (Exception ex)
                {
                    results.Items.Insert(0, ex.Message);                    
                }
            }
        }
    }
}

