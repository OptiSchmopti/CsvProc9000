using ArrangeContext.Moq;
using CsvProc9000.Jobs;
using CsvProc9000.Jobs.Contracts;
using FluentAssertions;
using Moq;
using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CsvProc9000.Tests.Jobs
{
    public class CsvProcessJobThreadTests
    {
        [Fact]
        public void Implements_Needed_Interfaces()
        {
            var context = new ArrangeContext<CsvProcessJobThread>();
            var sut = context.Build();

            sut
                .Should()
                .BeAssignableTo<ICsvProcessJobThread>()
                .And
                .BeAssignableTo<IJobThread>();

            sut
                .ThreadId
                .Should()
                .NotBe(Guid.Empty);
        }

        [Fact]
        public void Start_Stops_When_Cancellation_Token_Is_Cancelled()
        {
            var context = new ArrangeContext<CsvProcessJobThread>();
            var sut = context.Build();

            var job = new CsvProcessJob(Mock.Of<IFileInfo>());
            context
                .For<IJobPool>()
                .Setup(pool => pool.TryGet(out job))
                .Returns(false);
            
            var tokenSource = new CancellationTokenSource();
            
            sut.Start(tokenSource.Token);

            tokenSource.Cancel();
            
            context
                .For<IJobPool>()
                .Verify(pool => pool.TryGet(out job), Times.AtMost(5));
        }

        [Fact]
        public async Task Tells_The_Worker_To_Work()
        {
            var context = new ArrangeContext<CsvProcessJobThread>();
            var sut = context.Build();
            
            var job = new CsvProcessJob(Mock.Of<IFileInfo>());
            context
                .For<IJobPool>()
                .Setup(pool => pool.TryGet(out job))
                .Returns(true);
            
            var tokenSource = new CancellationTokenSource();
            
            sut.Start(tokenSource.Token);

            // yeah well this is timely process unfortunately ...
            await Task.Delay(500, tokenSource.Token);

            tokenSource.Cancel();
            
            context
                .For<ICsvProcessJobWorker>()
                .Verify(worker => worker.WorkOnAsync(It.IsAny<Guid>(), job));

            job
                .ExecutionStart
                .Should()
                .NotBe(DateTimeOffset.MinValue);

            job
                .ExecutionFinish
                .Should()
                .NotBe(DateTimeOffset.MinValue);
        }
    }
}
