﻿using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Messaging;
using HandbrakeCluster.Common;
using ProtoBuf;


namespace HandbrakeCluster.Adder
{
    class Program
    {
        static void Main(string[] args)
        {
            string queueName = ConfigurationManager.AppSettings["RabbitMQQueue"];
            string queueHost = ConfigurationManager.AppSettings["RabbitMQHost"];
            
            if (args.Count() != 4)
            {
                Console.WriteLine("needs 4 params: sourcedir sourcefiletypes dstdir dstfileformat");
                return;
            }

            try
            {
                Producer.Producer prod = new Producer.Producer(queueHost, queueName);

                string[] files = Directory.GetFiles(args[0], args[1], SearchOption.AllDirectories);
                int count = 0;
                foreach (string s in files)
                {                    
                    try
                    {
                        string argument = "-i \"{0}\" -o \"{1}\" --preset \"" + ConfigurationManager.AppSettings["HandbrakePreset"] + "\"";

                        string destination = args[2] + "\\" + Path.GetFileNameWithoutExtension(s) + args[3];

                        ProcessMessage p = new ProcessMessage() { CommandLine = argument, DestinationUrl = destination, OrignalFileUrl = s };

                        using (MemoryStream ms = new MemoryStream())
                        {
                            Serializer.Serialize(ms, p);
                            byte[] message = ms.ToArray();
                            prod.SendMessage(message);
                        }

                        Console.WriteLine("Adding message for {0} to queue", s);
                        count++;
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        
                    }
                }
                Console.WriteLine("added {0} items to queue.", count);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}


