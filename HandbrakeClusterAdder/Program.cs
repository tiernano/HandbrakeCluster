using System;
using System.Collections.Generic;
using System.Configuration;
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
            string queueName = ConfigurationManager.AppSettings["MSMQLocation"];

            if (args.Count() != 4)
            {
                Console.WriteLine("needs 4 params: sourcedir sourcefiletypes dstdir dstfileformat");
                return;
            }

            try
            {
                MessageQueue rmTxnQ = new MessageQueue(queueName);

                rmTxnQ.Formatter = new XmlMessageFormatter(new Type[] { typeof(ProcessMessage) });

                string[] files = Directory.GetFiles(args[0], args[1], SearchOption.AllDirectories);
                int count = 0;
                foreach (string s in files)
                {
                    MessageQueueTransaction msgTx = new MessageQueueTransaction();
                    msgTx.Begin();
                    try
                    {
                        string argument = "-i \"{0}\" -o \"{1}\" --preset \"" + ConfigurationManager.AppSettings["HandbrakePreset"] + "\"";

                        string destination = args[2] + "\\" + Path.GetFileNameWithoutExtension(s) + args[3];

                        ProcessMessage p = new ProcessMessage() { CommandLine = argument, DestinationURL = destination, OrignalFileURL = s };

                        rmTxnQ.Send(p, msgTx);
                        Console.WriteLine("Adding message for {0} to queue", s);
                        count++;
                        msgTx.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        msgTx.Abort();
                    }
                }
                Console.WriteLine("added {0} items to queue. Queue count now {1}", count, rmTxnQ.GetAllMessages().Count());
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
