using System;
using System.Threading;

namespace Api.Services.Counter
{
    public class CounterService : ICounterService
    {
        private long counterValue; 

        public CounterService() {
            counterValue = 0;
        }

        public long Increment()
        {
            return Interlocked.Increment(ref this.counterValue);
        }
    }
}
