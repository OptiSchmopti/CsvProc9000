using CsvProc9000.Jobs.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace CsvProc9000.Jobs
{
    internal sealed class JobPool : IJobPool
    {
        private readonly List<IJob> _jobs = new();

        public void Add(IJob job)
        {
            lock (_jobs)
                _jobs.Add(job);
        }

        public bool TryGet<T>(out T job) where T : IJob
        {
            lock (_jobs)
            {
                job = _jobs
                    .OfType<T>()
                    .FirstOrDefault();

                var found = job != null;
                if (found)
                    _jobs.Remove(job);

                return found;
            }
        }
    }
}
