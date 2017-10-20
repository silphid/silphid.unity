using System;
using System.Linq;
using System.Text;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Loadzup
{
    public abstract class ConverterBase : IConverter
    {
        protected readonly Type[] _inputTypes;
        protected readonly string[] _mediaTypes;

        protected ConverterBase(Type inputType1, params string[] mediaTypes)
        {
            _inputTypes = new[] {inputType1};
            _mediaTypes = mediaTypes;
        }

        protected ConverterBase(Type inputType1, Type inputType2, params string[] mediaTypes)
        {
            _inputTypes = new[] {inputType1, inputType2};
            _mediaTypes = mediaTypes;
        }

        private bool SupportsContentType(ContentType contentType) =>
            contentType == null ||
            _mediaTypes.Length == 0 ||
            _mediaTypes.Any(x => x.Equals(contentType.MediaType, StringComparison.OrdinalIgnoreCase));

        public bool Supports<T>(object input, ContentType contentType) =>
            _inputTypes.Any(x => input.GetType().IsAssignableTo(x)) &&
            SupportsContentType(contentType) &&
            SupportsInternal<T>(input, contentType);

        IObservable<T> IConverter.Convert<T>(object input, ContentType contentType, Encoding encoding)
        {
            if (!((IConverter) this).Supports<T>(input, contentType))
                return ThrowNotSupportedConversionException<T>(input, contentType, encoding);

            try
            {
                return ConvertInternal<T>(input, contentType, encoding)
                    .Catch<T, Exception>(ex => ThrowFailedConversionException<T>(ex, input, contentType, encoding));
            }
            catch (Exception ex)
            {
                return ThrowFailedConversionException<T>(ex, input, contentType, encoding);
            }
        }

        protected virtual bool SupportsInternal<T>(object input, ContentType contentType) => true;

        protected abstract IObservable<T> ConvertInternal<T>(object input, ContentType contentType, Encoding encoding);

        protected IObservable<T> ThrowNotSupportedConversionException<T>(object input, ContentType contentType, Encoding encoding) =>
            Observable.Throw<T>(
                new ConversionException("Conversion not supported", GetType(), input, typeof(T), contentType, encoding));

        protected IObservable<T> ThrowFailedConversionException<T>(Exception exception, object input, ContentType contentType, Encoding encoding) =>
            Observable.Throw<T>(
                new ConversionException("Conversion failed", exception, GetType(), input, typeof(T), contentType, encoding));
    }

    public abstract class ConverterBase<TInput> : ConverterBase
    {
        protected ConverterBase(params string[] mediaTypes) : base(typeof(TInput), mediaTypes)
        {
        }

        protected sealed override bool SupportsInternal<T>(object input, ContentType contentType) =>
            SupportsInternal<T>((TInput) input, contentType);

        protected sealed override IObservable<T> ConvertInternal<T>(object input, ContentType contentType, Encoding encoding) =>
            ConvertInternal<T>((TInput) input, contentType, encoding);

        protected virtual bool SupportsInternal<T>(TInput input, ContentType contentType) => true;

        protected abstract IObservable<T> ConvertInternal<T>(TInput input, ContentType contentType, Encoding encoding);
    }

    public abstract class ConverterBase<TInput1, TInput2> : ConverterBase
    {
        protected ConverterBase(params string[] mediaTypes) : base(typeof(TInput1), typeof(TInput2), mediaTypes)
        {
        }

        protected sealed override IObservable<T> ConvertInternal<T>(object input, ContentType contentType, Encoding encoding)
        {
            if (input is TInput1)
                return ConvertInternal<T>((TInput1) input, contentType, encoding);
            
            return ConvertInternal<T>((TInput2) input, contentType, encoding);
        }

        protected abstract IObservable<T> ConvertInternal<T>(TInput1 input, ContentType contentType, Encoding encoding);
        protected abstract IObservable<T> ConvertInternal<T>(TInput2 input, ContentType contentType, Encoding encoding);
    }
}