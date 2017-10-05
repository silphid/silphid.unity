using System;
using System.Collections.Generic;
using System.Net;
using UniRx;
using UnityEngine.Networking;

namespace Silphid.Loadzup
{
    public class NetworkpException : Exception
    {

        public NetworkpException()
        {
        }

        public NetworkpException(string message) : base(message)
        {
        }

        public NetworkpException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}