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
                MessageQueue rmTxnQ = new MessageQueue("FormatName:Direct=TCP:192.168.0.101\\Private$\\HandbrakeCluster");
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

                    p.StartInfo = new ProcessStartInfo() { Arguments = cmdLine, UseShellExecute = true, FileName = ConfigurationManager.AppSettings["HandbrakeEXE"]};//, RedirectStandardOutput = true, RedirectStandardError = true,  };
                    p.Start();
                    
                    //string err;                   
                    //Console.WriteLine("ERRORS!");
                    //p.BeginOutputReadLine();
                    
                    //while ((err = p.StandardError.ReadLine()) != null)
                    //{
                    //    Console.WriteLine(err);
                    //}                
                    
                    
                    //Console.WriteLine("Standard Output");
                    //string str;
                    //while ((str = p.StandardOutput.ReadLine()) != null)
                    //{
                    //    Console.WriteLine(str);
                    //}

                    
                    p.WaitForExit();
                    msgTx.Commit();
                    
                }
                catch
                {
                    msgTx.Abort();
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