using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace UdpListenerLib
{
    public class UdpDataReceiveEventArgs : EventArgs
    {
        #region Private Fields

        private readonly IPEndPoint remoteEndPoint;

        #endregion

        #region Constructor

        public UdpDataReceiveEventArgs(byte[] data, IPEndPoint ep)
        {
            this.Data = data;
            this.remoteEndPoint = ep;
        }

        #endregion

        #region Properties

        public byte[] Data { get; }
        public IPAddress RemoteIP { get { return remoteEndPoint?.Address; } }

        #endregion
    }
}
