using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using Cluster.Common;

namespace HandbrakeCluster
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MessageQueue rmTxnQ = new MessageQueue(ConfigurationManager.AppSettings["MSMQLocation"]);

                while (rmTxnQ.CanRead)
                {
                    rmTxnQ.Formatter = new XmlMessageFormatter(new Type[] { typeof(ProcessMessage) });

                    MessageQueueTransaction msgTx = new MessageQueueTransaction();
                    msgTx.Begin();
                    try
                    {
                        rmTxnQ.Formatter = new XmlMessageFormatter(new Type[] { typeof(ProcessMessage) });

                        System.Messaging.Message msgTxn = rmTxnQ.Receive(MessageQueueTransactionType.Single);

                        ProcessMessage msg = (ProcessMessage)msgTxn.Body;

                        Process p = new Process();

                        string cmdLine = string.Format(msg.CommandLine, msg.OrignalFileURL, msg.DestinationURL);
                        Console.WriteLine("Recieved message. Commandline: {0}", cmdLine);
                        Console.WriteLine("Starting at {0}", DateTime.Now);
                        Stopwatch st = new Stopwatch();
                        st.Start();
                        p.StartInfo = new ProcessStartInfo() { Arguments = cmdLine, UseShellExecute = true, FileName = ConfigurationManager.AppSettings["HandbrakeEXE"] };//, RedirectStandardOutput = true, RedirectStandardError = true,  };
                        p.Start();
                        p.WaitForExit();
                        Console.WriteLine("Finsihed at {0} - Total time: {1}", DateTime.Now, st.Elapsed);
                        msgTx.Commit();

                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Somethign went wrong... {0}", ex.Message);
                        msgTx.Abort();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
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