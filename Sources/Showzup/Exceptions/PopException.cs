using System;
using UnityEngine;

namespace Silphid.Showzup
{
    public class PopException : PresentException
    {
        public PopException(GameObject presenter, string message = null, Exception innerException = null)
            : base(presenter, null, null, message, innerException) {}
    }
}