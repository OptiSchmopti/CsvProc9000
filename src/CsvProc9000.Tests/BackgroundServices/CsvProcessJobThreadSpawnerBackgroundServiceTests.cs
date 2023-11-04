using ArrangeContext.Moq;
using CsvProc9000.BackgroundServices;
using CsvProc9000.Jobs.Contracts;
using CsvProc9000.Model.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CsvProc9000.Tests.BackgroundServices
{
    public class CsvProcessJobThreadSpawnerBackgroundServiceTests
    {
        [Fact]
        public void Be_Of_Needed_Base_Class()
        {
            var (context, _) = CreateContext();
            var sut = context.Build();

            sut
                .Should()
                .BeAssignableTo<BackgroundService>();
        }

        [Theory]
        [InlineData(-5)]
        [InlineData(-1)]
        [InlineData(-0)]
        public async Task Start_Fails_When_JobThreadCount_Is_Invalid(int jobThreadCount)
        {
            var (context, options) = CreateContext();
            var sut = context.Build();

            options.JobThreadCount = jobThreadCount;

            await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.StartAsync(CancellationToken.None));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(100)]
        public async Task Start_Spawns_Job_Threads(int jobThreadCount)
        {
            var (context, options) = CreateContext();
            var sut = context.Build();

            var jobThreads = new List<Mock<ICsvProcessJobThread>>();

            context
                .For<ICsvProcessJobThreadFactory>()
                .Setup(factory => factory.Create())
                .Returns(() =>
                {
                    var jobThreadMock = new Mock<ICsvProcessJobThread>();
                    jobThreads.Add(jobThreadMock);
                    return jobThreadMock.Object;
                });

            options.JobThreadCount = jobThreadCount;

            await sut.StartAsync(CancellationToken.None);

            jobThreads
                .Should()
                .HaveCount(jobThreadCount);

            foreach (var jobThread in jobThreads)
                jobThread
                    .Verify(thread =>
                        thread.Start(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Disposes_Threads()
        {
            var (context, options) = CreateContext();
            var sut = context.Build();

            var jobThread = new Mock<ICsvProcessJobThread>();
            context
                .For<ICsvProcessJobThreadFactory>()
                .Setup(factory => factory.Create())
                .Returns(jobThread.Object);

            options.JobThreadCount = 1;

            await sut.StartAsync(CancellationToken.None);

            sut.Dispose();

            jobThread.Verify(thread => thread.Dispose(), Times.Once);
        }

        private static (ArrangeContext<CsvProcessJobThreadSpawnerBackgroundService>, CsvProcessorOptions)
            CreateContext()
        {
            var options = new CsvProcessorOptions();
            var optionsMock = new Mock<IOptions<CsvProcessorOptions>>();
            optionsMock
                .SetupGet(opt => opt.Value)
                .Returns(options);

            var context = new ArrangeContext<CsvProcessJobThreadSpawnerBackgroundService>();
            context.Use(optionsMock);

            return (context, options);
        }
    }
}
