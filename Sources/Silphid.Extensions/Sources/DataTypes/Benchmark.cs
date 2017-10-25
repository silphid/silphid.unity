using System;
using log4net;

namespace Silphid.Benchmarking
{
    public class Benchmark : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Benchmark));
    
        private readonly string _message;
        private readonly DateTime _startTime;
        private static TimeSpan _totalTime = TimeSpan.Zero;
    
        public Benchmark(string message)
        {
            _message = message;
            _startTime = DateTime.UtcNow;
            
            Log.Debug($"Start - {_message}");
        }

        public void Dispose()
        {
            var elapsed = DateTime.UtcNow - _startTime;
            _totalTime += elapsed;
            Log.Debug($"End - {_message} - " +
                      $"Elapsed: {(int) elapsed.TotalMilliseconds} ms - " +
                      $"Total: {(int) _totalTime.TotalMilliseconds} ms");
        }
    }
}