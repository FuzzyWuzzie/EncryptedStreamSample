using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace ESSServer
{
    class Server
    {
        private MainViewModel ViewModel;

        public Server(MainViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        // used as the validation certificate
        static X509Certificate2 ServerCertificate = null;

        public void Start()
        {
            // load the certificate
            // generated with:
            //  makecert.exe -r -pe -n "CN=localhost" -sky exchange -sv server.pvk server.cer
            //  pvk2pfx -pvk server.pvk -spc server.cer -pfx server.pfx
            // TODO: take as parameters
            ServerCertificate = new X509Certificate2("C:\\Users\\mfplab\\Documents\\Visual Studio 2015\\Projects\\TLSIntro01\\cert\\server.pfx", "derp");
            ViewModel.TS_LogMessage("SSL certificate loaded!");

            // start the server
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 5005);
            if (localEndPoint == null)
            {
                ViewModel.TS_SetStatus("ERROR setting up IP end point!");
            }
            else
            {
                TcpListener listener = new TcpListener(localEndPoint);
                if (listener == null)
                {
                    ViewModel.TS_SetStatus("ERROR setting up TcpListener!");
                }
                else
                {
                    listener.Start();
                    ViewModel.TS_SetStatus("TCP listener started");
                    listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), listener);
                }
            }
        }

        private void AcceptCallback(IAsyncResult result)
        {
            // accept the client
            TcpListener listener = (TcpListener)result.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(result);
            ViewModel.TS_LogMessage("Client connected!");

            // set up SSL
            SslStream sslStream = new SslStream(client.GetStream(), false);
            sslStream.AuthenticateAsServer(ServerCertificate, false, SslProtocols.Tls, true);

            // create the user session
            ConnectionStateObject state = new ConnectionStateObject();
            state.client = client;
            state.stream = sslStream;

            // begin reading from the client
            ViewModel.TS_LogMessage("Awaiting client message...");
            sslStream.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(ReceiveCallback), state);

            // start accepting again
            listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), listener);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.
            ConnectionStateObject state = (ConnectionStateObject)ar.AsyncState;
            int byteCount = -1;

            //ViewModel.TS_LogMessage("Reading data from the client.");
            byteCount = state.stream.EndRead(ar);

            // Use Decoder class to convert from bytes to UTF8
            // in case a character spans two buffers.
            Decoder decoder = Encoding.UTF8.GetDecoder();
            char[] chars = new char[decoder.GetCharCount(state.buffer, 0, byteCount)];
            decoder.GetChars(state.buffer, 0, byteCount, chars, 0);
            state.sb.Append(chars);

            // Check for EOF or an empty message.
            if (state.sb.ToString().IndexOf("<EOF>") == -1 && byteCount != 0)
            {
                // We are not finished reading.
                // Asynchronously read more message data from  the server.
                state.stream.BeginRead(state.buffer, 0, state.buffer.Length,
                    new AsyncCallback(ReceiveCallback),
                    state);
            }
            else if(byteCount == 0)
            {
                // the stream got shutdown
                state.stream.Close();
                ViewModel.TS_LogMessage("Client disconnected!");
            }
            else
            {
                // we have a whole message
                ViewModel.TS_LogMessage("Received: " + state.sb.ToString().Substring(0, state.sb.Length - 5));

                // read another message
                state.sb.Clear();
                state.stream.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(ReceiveCallback), state);
            }
        }
    }
}
