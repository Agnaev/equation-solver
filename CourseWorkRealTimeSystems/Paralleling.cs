using System;
using System.Threading;

namespace CourseWorkRealTimeSystems
{
    public class ThreadWithResult<T>
    {
        private Thread _thread { get; set; }
        public T result { get; private set; } = default;
        public ThreadWithResult(Thread thread)
        {
            this._thread = thread;
        }
        public void Run(object param) => this._thread.Start(param);
        public void Run() => this._thread.Start();
        public void Join() => this._thread.Join();
        public void SetResult(T result)
        {
            this.result = result;
        }
    }

    struct ThreadParams
    {
        public string quation;
        public double start, end;
        public int count;
        public Random rand;
        public int threadId;
    }
}
