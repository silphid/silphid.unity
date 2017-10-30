using System;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup
{
    public class PresentException : Exception
    {
        public GameObject Presenter { get; }
        public object Input { get; }
        public Options Options { get; }

        public PresentException(GameObject presenter, object input, Options options, string message = null, Exception innerException = null)
            : base(message, innerException)
        {
            Presenter = presenter;
            Input = input;
            Options = options;
        }

        public override string Message =>
            $"{base.Message}\r\n" +
            $"Presenter: {Presenter.ToHierarchyPath()}\r\n" +
            $"InputType: {Input?.GetType().Name}\r\n" +
            $"Options: {Options}";
    }
}