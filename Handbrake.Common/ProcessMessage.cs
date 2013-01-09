using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandbrakeCluster.Common
{

    
        [ProtoContract]
        public class ProcessMessage
        {
            [ProtoMember(1)]
            public string OrignalFileURL { get; set; }
            [ProtoMember(2)]
            public string DestinationURL { get; set; }
            [ProtoMember(3)]
            public string CommandLine { get; set; }
        }
    }

