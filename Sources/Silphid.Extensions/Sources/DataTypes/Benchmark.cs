using System;
using log4net;

namespace Silphid.Benchmarking
{
    public class Benchmark : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Benchmark));
    
        private readonly string _message;
        private readonly DateTime _startTime;
    
        public Benchmark(string message)
        {
            _message = message;
            _startTime = DateTime.UtcNow;
            
            if (Log.IsDebugEnabled)
                Log.Debug($"Start - {_message}");
        }

        public void Dispose()
        {
            if (Log.IsDebugEnabled)
            {
                var elapse = DateTime.UtcNow - _startTime;
                Log.Debug($"Completed in {(int) elapse.TotalMilliseconds} ms - {_message}");
            }
        }
    }
}