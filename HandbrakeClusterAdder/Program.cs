using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using Cluster.Common;

namespace HandbrakeClusterAdder
{
    class Program
    {
        static void Main(string[] args)
        {
            string queueName = "FormatName:Direct=TCP:192.168.0.101\\Private$\\HandbrakeCluster";
           
            try
            {
                MessageQueue rmTxnQ = new MessageQueue(queueName);

                rmTxnQ.Formatter = new XmlMessageFormatter(new Type[] { typeof(ProcessMessage) });

                string[] files = Directory.GetFiles(args[0], args[1], SearchOption.AllDirectories);

                foreach (string s in files)
                {
                    MessageQueueTransaction msgTx = new MessageQueueTransaction();
                    msgTx.Begin();
                    try
                    {
                        string argument = "-i \"{0}\" -o \"{1}\" --preset \"AppleTV 2\"";

                        string destination = args[2] + "\\" + Path.GetFileNameWithoutExtension(s) + args[3];

                        ProcessMessage p = new ProcessMessage() { CommandLine = argument, DestinationURL = destination, OrignalFileURL = s };

                        rmTxnQ.Send(p, msgTx);
                        msgTx.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        msgTx.Abort();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }


    
}

namespace Cluster.Common
{
    public class ProcessMessage
    {
        public string OrignalFileURL { get; set; }
        public string DestinationURL { get; set; }
        public string CommandLine { get; set; }
    }
}
