using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ESSServer
{
    class ConnectionStateObject
    {
        public TcpClient client;
        public SslStream stream;

        public byte[] buffer;
        public StringBuilder sb;

        public ConnectionStateObject()
        {
            buffer = new byte[2048];
            sb = new StringBuilder(buffer.Length);
        }
    }
}
