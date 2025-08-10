using ArrangeContext.Moq;
using CsvProc9000.BackgroundServices;
using CsvProc9000.Jobs;
using CsvProc9000.Jobs.Contracts;
using CsvProc9000.Model.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CsvProc9000.Tests.BackgroundServices;

public class CsvExistingFileWatcherBackgroundServiceTests
{
    [Fact]
    public void Be_Of_Needed_Base_Class()
    {
        var context = CreateContext();
        var sut = context.Build();

        sut
            .Should()
            .BeAssignableTo<BackgroundService>();
    }

    [Fact]
    public async Task Add_Previously_Existing_Files_To_Pool()
    {
        var context = CreateContext();
        var sut = context.Build();

        const string file = "some file";
        var fileInfoMock = new Mock<IFileInfo>();

        context
            .For<IFileSystem>()
            .Setup(fileSystem => fileSystem.FileInfo.FromFileName(file))
            .Returns(fileInfoMock.Object);

        context
            .For<IFileSystem>()
            .Setup(fileSystem => fileSystem.Directory.GetFiles(It.IsAny<string>(), "*.csv"))
            .Returns(new[] { file });

        await sut.StartAsync(CancellationToken.None);

        context
            .For<IJobPool>()
            .Verify(pool =>
                pool.Add(It.Is<CsvProcessJob>(job =>
                    job.TargetFile == fileInfoMock.Object)), Times.Once);
    }

    private static ArrangeContext<CsvExistingFileWatcherBackgroundService> CreateContext()
    {
        var options = new CsvProcessorOptions();
        var optionsMock = new Mock<IOptions<CsvProcessorOptions>>();
        optionsMock
            .SetupGet(opt => opt.Value)
            .Returns(options);

        var context = new ArrangeContext<CsvExistingFileWatcherBackgroundService>();
        context.Use(optionsMock);

        return context;
    }
}