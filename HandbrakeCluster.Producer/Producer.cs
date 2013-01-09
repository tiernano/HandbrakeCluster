using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandbrakeCluster.Producer
{
    public class Producer : IDisposable
    {
        protected IModel model;
        protected IConnection connection;
        protected string QueueName;
        public Producer(string hostname, string queueName)
        {
            QueueName = queueName;
            var connectionFact = new ConnectionFactory();
            connectionFact.HostName = hostname;
            connection = connectionFact.CreateConnection();
            model = connection.CreateModel();
            model.QueueDeclare(queueName, true, false, false, null);
        }

        public void SendMessage(byte[] message)
        {
            IBasicProperties basicProp = model.CreateBasicProperties();
            basicProp.SetPersistent(true);
            model.BasicPublish("", QueueName, basicProp, message);
        }


        public void Dispose()
        {
            if (connection != null)
            {
                connection.Close();
            }
            if (model != null)
            {
                model.Abort();
            }
            
        }
    }
}
