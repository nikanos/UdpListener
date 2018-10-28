using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UdpListenerLib
{
    public class UdpListener
    {
        #region Constants

        private class Constants
        {
            public const int RECEIVE_TIMEOUT = 3000;
            public const int DATAGRAM_DATA_SIZE = 65507;

        }

        #endregion

        #region Private Memebers

        private Thread listenThread;
        private Socket socket;
        private readonly IPEndPoint endPoint;
        private volatile bool shouldStop;

        #endregion

        #region Constructors

        public UdpListener(int port)
            : this(IPAddress.Any, port)
        {
        }

        public UdpListener(IPAddress addr, int port)
        {
            endPoint = new IPEndPoint(addr, port);
        }

        #endregion

        #region Properties

        public bool IsListenerRunning
        {
            get { return listenThread != null && listenThread.IsAlive; }
        }

        public int Port => endPoint.Port;
        public IPAddress SourceAddress => endPoint.Address;

        #endregion

        #region Event Handling Stuff

        public event EventHandler<UdpDataReceiveEventArgs> DataReceive;
        public event EventHandler<UdpDataReceiveErrorEventArgs> DataReceiveError;

        protected virtual void OnDataReceive(UdpDataReceiveEventArgs e)
        {
            DataReceive?.Invoke(this, e);
        }

        protected virtual void OnDataReceiveError(UdpDataReceiveErrorEventArgs e)
        {
            DataReceiveError?.Invoke(this, e);
        }

        #endregion

        #region Methods

        public void StartListening()
        {
            if (socket == null)
            {
                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.Bind(endPoint);
                    socket.ReceiveTimeout = Constants.RECEIVE_TIMEOUT;

                    shouldStop = false;
                    this.listenThread = new Thread(new ThreadStart(ListenForClients));
                    this.listenThread.Start();
                }
                catch (Exception)
                {
                    if (socket != null)
                        socket.Close();
                    socket = null;
                    throw;
                }
            }
        }

        public void StopListening()
        {
            if (IsListenerRunning)
            {
                shouldStop = true;
                listenThread.Join();
            }
        }

        private void ListenForClients()
        {
            while (!shouldStop)
            {
                try
                {
                    byte[] buffer = new byte[Constants.DATAGRAM_DATA_SIZE];
                    EndPoint remoteEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
                    int bytesRead = socket.ReceiveFrom(buffer, ref remoteEndPoint);
                    byte[] data = new byte[bytesRead];
                    Array.Copy(sourceArray: buffer, destinationArray: data, length: bytesRead);

                    //we have the message and the address of the remote endpoint so pass it to anyone interested
                    UdpDataReceiveEventArgs args = new UdpDataReceiveEventArgs(data, (IPEndPoint)remoteEndPoint);
                    OnDataReceive(args);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.TimedOut)
                    {
                        //if a socket error other than timeout occurs, pass the error to anyone interested - do not break the server loop!
                        UdpDataReceiveErrorEventArgs args = new UdpDataReceiveErrorEventArgs(ex);
                        OnDataReceiveError(args);
                    }
                }
                catch (Exception)
                {
                    //close socket
                    if (socket != null)
                    {
                        socket.Close();
                        socket = null;
                    }
                    //and rethrow
                    throw;
                }
                Thread.Sleep(1);
            }//while (!shouldStop)
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }
    
        #endregion
    }
}
