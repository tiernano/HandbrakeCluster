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
                p.StartInfo = new ProcessStartInfo() { 
                        Arguments = cmdLine, 
                        UseShellExecute = false, 
                        FileName = ConfigurationManager.AppSettings["HandbrakeEXE"],
                        RedirectStandardOutput = true, 
                        RedirectStandardError = true
                };
                p.OutputDataReceived += p_OutputDataReceived;

                p.ErrorDataReceived += p_ErrorDataReceived;

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
                Console.WriteLine("Finsihed at {0} - Total time: {1}", DateTime.Now, st.Elapsed);                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong... {0}", ex.Message);
                throw;
            }
        }

        static void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            //todo: write this back to a central place, possibly parse the info... but for now, just write to console...
            ShowOutput(e.Data, ConsoleColor.Green, "Info");

        }

        static void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //todo: write this back to a central place, possibly parse the info... but for now, just write to console...
            ShowOutput(e.Data, ConsoleColor.Red, "ERROR");
        }

        static void ShowOutput(string data, ConsoleColor color, string type)
        {
            if (data != null)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine("{1}: {0}", data, type);
                Console.ForegroundColor = oldColor;
            }
        }
    }


}
