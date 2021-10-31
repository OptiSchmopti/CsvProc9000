using CsvProc9000.Jobs.Contracts;
using JetBrains.Annotations;
using System;

namespace CsvProc9000.Jobs
{
    internal sealed class CsvProcessJobThreadFactory : ICsvProcessJobThreadFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CsvProcessJobThreadFactory(
            [NotNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        
        public ICsvProcessJobThread Create()
        {
            var thread = (ICsvProcessJobThread) _serviceProvider.GetService(typeof(ICsvProcessJobThread));
            return thread;
        }
    }
}
