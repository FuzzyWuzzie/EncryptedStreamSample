using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Threading.Tasks;

namespace ESSClient
{
    class Client
    {
        private MainViewModel ViewModel;
        private Hashtable certificateErrors = new Hashtable();

        private string name;
        private TcpClient tcpClient;
        private SslStream sslStream;

        private class ConnectionStateObject
        {
            public SslStream stream;

            public byte[] buffer;
            public string message;

            public ConnectionStateObject()
            {
                buffer = new byte[2048];
            }
        }

        public Client(MainViewModel viewModel, string name)
        {
            this.ViewModel = viewModel;
            this.name = name;
        }

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            // If there are errors in the certificate chain, look at each error to determine the cause.
            else if ((sslPolicyErrors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain != null && chain.ChainStatus != null)
                {
                    foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus)
                    {
                        if ((certificate.Subject == certificate.Issuer) &&
                           (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot))
                        {
                            // Self-signed certificates with an untrusted root are valid.
                            ViewModel.TS_LogMessage("WARNING: server's certificate is self-signed!");
                            return true;
                        }
                        else
                        {
                            if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError)
                            {
                                // If there are any other errors in the certificate chain, the certificate is invalid,
                                if(System.Windows.MessageBox.Show("The server certificate is not valid.\nAccept?", "Certificate Validation", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
                                    return true;
                                else
                                    return false;
                            }
                        }
                    }
                }
            }

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        public void Start()
        {
            // connect
            TcpClient client = new TcpClient();
            client.BeginConnect("localhost", 5005, new AsyncCallback(ConnectCallback), client);
        }

        public void Close()
        {
            this.tcpClient.Close();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            // grab the connection
            TcpClient client = (TcpClient)ar.AsyncState;
            if(!client.Connected)
            {
                ViewModel.TS_SetStatus("Couldn't connect to server!");
                return;
            }
            ViewModel.TS_SetStatus("Connected to server as '" + name + "'!");

            // set up ssl
            SslStream sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            try
            {
                sslStream.AuthenticateAsClient("localhost");
            }
            catch(AuthenticationException e)
            {
                ViewModel.TS_LogMessage("Authentication exception: " + e.Message);
                ViewModel.TS_SetStatus("Authentication failed!");
                client.Close();
                return;
            }

            this.tcpClient = client;
            this.sslStream = sslStream;
        }

        public void SendMessage(string message)
        {
            // make sure we're connected
            if (!tcpClient.Connected) return;

            // build the message
            ConnectionStateObject mo = new ConnectionStateObject();
            mo.message = message;
            mo.buffer = Encoding.UTF8.GetBytes(message + "<EOF>");
            mo.stream = this.sslStream;

            // send it off!
            this.sslStream.BeginWrite(mo.buffer, 0, mo.buffer.Length, new AsyncCallback(WriteCallback), mo);
        }

        private void WriteCallback(IAsyncResult ar)
        {
            // grab the message
            ConnectionStateObject state = (ConnectionStateObject)ar.AsyncState;
            ViewModel.TS_LogMessage("Sent message: " + (ar.AsyncState as ConnectionStateObject).message);

            // prepare for the next message
            state.stream.EndWrite(ar);
        }
    }
}
