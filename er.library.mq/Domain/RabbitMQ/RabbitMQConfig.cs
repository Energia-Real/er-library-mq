using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace er.library.mq.Domain.RabbitMQ
{
    public class RabbitMQConfig
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ExchangeName { get; set; } = "default.exchange";
        public string QueueName { get; set; } = "default.queue";
        public string VirtualHost { get; set; }
        public string RoutingKey { get; set; }
    }
}
