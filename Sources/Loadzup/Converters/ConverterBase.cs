using System;
using System.Linq;
using System.Text;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Loadzup
{
    public abstract class ConverterBase : IConverter
    {
        #region Private fields

        private Type[] _inputTypes;
        private Type[] _outputTypes;
        private string[] _mediaTypes;

        #endregion

        #region Input/Output/MediaType setters

        protected void SetInput<T>()
        {
            SetInputs(typeof(T));
        }

        protected void SetInputs<T1, T2>()
        {
            SetInputs(typeof(T1), typeof(T2));
        }

        protected void SetInputs(params Type[] inputTypes)
        {
            if (_inputTypes != null)
                throw new InvalidOperationException($"Input types already set for converter {GetType().Name}");

            _inputTypes = inputTypes;
        }

        protected void SetOutput<T>()
        {
            SetOutputs(typeof(T));
        }

        protected void SetOutputs<T1, T2>()
        {
            SetOutputs(typeof(T1), typeof(T2));
        }

        protected void SetOutputs(params Type[] outputTypes)
        {
            if (_outputTypes != null)
                throw new InvalidOperationException($"Output types already set for converter {GetType().Name}");

            _outputTypes = outputTypes;
        }

        protected void SetMediaTypes(params string[] mediaTypes)
        {
            if (_mediaTypes != null)
                throw new InvalidOperationException($"Media types already set for converter {GetType().Name}");

            _mediaTypes = mediaTypes;
        }

        #endregion

        #region IConverter members

        public bool Supports<T>(object input, string mediaType) =>
            input != null && SupportsInputType(input.GetType()) && SupportsOutputType(typeof(T)) &&
            SupportsMediaType(mediaType) && SupportsInternal<T>(input, mediaType);

        private bool SupportsInputType(Type inputType) =>
            _inputTypes?.Any(inputType.IsAssignableTo) ?? true;

        private bool SupportsOutputType(Type outputType) =>
            _outputTypes?.Any(outputType.IsAssignableFrom) ?? true;

        private bool SupportsMediaType(string mediaType) =>
            _mediaTypes == null || _mediaTypes.Any(x => x.Equals(mediaType, StringComparison.OrdinalIgnoreCase));

        IObservable<T> IConverter.Convert<T>(object input, string mediaType, Encoding encoding)
        {
            if (!((IConverter) this).Supports<T>(input, mediaType))
                return ThrowNotSupportedConversionException<T>(input, mediaType, encoding);

            try
            {
                return ConvertAsync<T>(input, mediaType, encoding)
                      .Cast<object, T>()
                      .Catch<T, Exception>(ex => ThrowFailedConversionException<T>(ex, input, mediaType, encoding));
            }
            catch (Exception ex)
            {
                return ThrowFailedConversionException<T>(ex, input, mediaType, encoding);
            }
        }

        #endregion

        #region Virtuals

        protected virtual bool SupportsInternal<T>(object input, string mediaType) => true;

        protected virtual IObservable<object> ConvertAsync<T>(object input, string mediaType, Encoding encoding) =>
            Observable.Return(ConvertSync<T>(input, mediaType, encoding));

        protected virtual object ConvertSync<T>(object input, string mediaType, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Throw helpers

        protected IObservable<T> ThrowNotSupportedConversionException<T>(
            object input,
            string mediaType,
            Encoding encoding) =>
            Observable.Throw<T>(
                new ConversionException("Conversion not supported", GetType(), input, typeof(T), mediaType, encoding));

        protected IObservable<T> ThrowFailedConversionException<T>(Exception exception,
                                                                   object input,
                                                                   string mediaType,
                                                                   Encoding encoding) =>
            Observable.Throw<T>(
                new ConversionException(
                    "Conversion failed",
                    exception,
                    GetType(),
                    input,
                    typeof(T),
                    mediaType,
                    encoding));

        #endregion
    }

    public abstract class ConverterBase<TInput> : ConverterBase
    {
        protected ConverterBase()
        {
            SetInput<TInput>();
        }

        protected sealed override bool SupportsInternal<T>(object input, string mediaType) =>
            SupportsInternal<T>((TInput) input, mediaType);

        protected sealed override IObservable<object>
            ConvertAsync<T>(object input, string mediaType, Encoding encoding) =>
            ConvertAsync<T>((TInput) input, mediaType, encoding);

        protected sealed override object ConvertSync<T>(object input, string mediaType, Encoding encoding) =>
            ConvertSync<T>((TInput) input, mediaType, encoding);

        protected virtual bool SupportsInternal<T>(TInput input, string mediaType) => true;

        protected virtual IObservable<object> ConvertAsync<T>(TInput input, string mediaType, Encoding encoding) =>
            Observable.Return(ConvertSync<T>(input, mediaType, encoding));

        protected virtual object ConvertSync<T>(TInput input, string mediaType, Encoding encoding)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class ConverterBase<TInput1, TInput2> : ConverterBase
    {
        protected ConverterBase()
        {
            SetInputs<TInput1, TInput2>();
        }

        protected sealed override IObservable<object> ConvertAsync<T>(object input, string mediaType, Encoding encoding)
        {
            if (input is TInput1)
                return ConvertAsync<T>((TInput1) input, mediaType, encoding);

            return ConvertAsync<T>((TInput2) input, mediaType, encoding);
        }

        protected sealed override object ConvertSync<T>(object input, string mediaType, Encoding encoding)
        {
            if (input is TInput1)
                return ConvertSync<T>((TInput1) input, mediaType, encoding);

            return ConvertSync<T>((TInput2) input, mediaType, encoding);
        }

        protected virtual IObservable<object> ConvertAsync<T>(TInput1 input, string mediaType, Encoding encoding) =>
            Observable.Return(ConvertSync<T>(input, mediaType, encoding));

        protected virtual IObservable<object> ConvertAsync<T>(TInput2 input, string mediaType, Encoding encoding) =>
            Observable.Return(ConvertSync<T>(input, mediaType, encoding));

        protected virtual object ConvertSync<T>(TInput1 input, string mediaType, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        protected virtual object ConvertSync<T>(TInput2 input, string mediaType, Encoding encoding)
        {
            throw new NotImplementedException();
        }
    }
}