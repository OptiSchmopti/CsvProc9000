using CsvProc9000.Jobs.Contracts;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CsvProc9000.Jobs
{
    [ExcludeFromCodeCoverage] // simple DI-Factory
    internal sealed class CsvProcessJobThreadFactory : ICsvProcessJobThreadFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CsvProcessJobThreadFactory(
            [JetBrains.Annotations.NotNull] IServiceProvider serviceProvider)
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
