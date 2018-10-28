using System;
using System.Collections.Generic;
using System.Text;

namespace UdpListenerLib
{
    public class UdpDataReceiveErrorEventArgs : EventArgs
    {
        #region Constructor

        public UdpDataReceiveErrorEventArgs(Exception ex)
        {
            this.Exception = ex;
        }

        #endregion

        #region Properties

        public Exception Exception { get; }

        #endregion
    }
}
