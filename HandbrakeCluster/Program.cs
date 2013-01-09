using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using HandbrakeCluster.Common;
using ProtoBuf;
using System.IO;

namespace HandbrakeCluster
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string queueName = ConfigurationManager.AppSettings["RabbitMQQueue"];
                string queueHost = ConfigurationManager.AppSettings["RabbitMQHost"];

                Consumer.Consumer con = new Consumer.Consumer(queueHost, queueName);
                {
                    con.onMessageReceived += con_onMessageReceived;
                    con.StartConsuming();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        static void con_onMessageReceived(byte[] message)
        {
            try
            {
                MemoryStream ms = new MemoryStream(message);
                var msg = Serializer.Deserialize<ProcessMessage>(ms);
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong... {0}", ex.Message);
                throw;
            }
        }
    }


}
