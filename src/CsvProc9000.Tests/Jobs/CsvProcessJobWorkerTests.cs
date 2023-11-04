using ArrangeContext.Moq;
using CsvProc9000.Csv.Contracts;
using CsvProc9000.Jobs;
using CsvProc9000.Jobs.Contracts;
using CsvProc9000.Model;
using CsvProc9000.Model.Configuration;
using CsvProc9000.Model.Csv;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Xunit;

namespace CsvProc9000.Tests.Jobs
{
    public class CsvProcessJobWorkerTests
    {
        [Fact]
        public void Implements_Needed_Interface()
        {
            var (context, _) = CreateContext();
            var sut = context.Build();

            sut
                .Should()
                .BeAssignableTo<ICsvProcessJobWorker>();
        }

        [Fact]
        public async Task WorkOn_Does_Not_Process_When_File_Does_Not_Exist()
        {
            var (context, _) = CreateContext();
            var sut = context.Build();

            var file = new Mock<IFileInfo>();
            file
                .SetupGet(f => f.Exists)
                .Returns(false);

            var job = new CsvProcessJob(file.Object);

            await sut.WorkOnAsync(Guid.NewGuid(), job);

            context
                .For<ICsvImporter>()
                .Verify(importer => importer.ImportAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task WorkOn_Does_Not_Process_When_File_Is_Not_Csv()
        {
            var (context, _) = CreateContext();
            var sut = context.Build();

            var file = new Mock<IFileInfo>();
            file
                .SetupGet(f => f.Exists)
                .Returns(true);

            file
                .SetupGet(f => f.Extension)
                .Returns(".exe");

            var job = new CsvProcessJob(file.Object);

            await sut.WorkOnAsync(Guid.NewGuid(), job);

            context
                .For<ICsvImporter>()
                .Verify(importer => importer.ImportAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task WorkOn_Processes_Correct_File(bool shouldDeleteFile)
        {
            var (context, options) = CreateContext();
            var sut = context.Build();

            options.DeleteInboxFile = shouldDeleteFile;

            var file = new Mock<IFileInfo>();
            file
                .SetupGet(f => f.Exists)
                .Returns(true);

            file
                .SetupGet(f => f.Extension)
                .Returns(".csv");

            var job = new CsvProcessJob(file.Object);

            context
                .For<ICsvImporter>()
                .Setup(importer => importer.ImportAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new Result<CsvFile>(true, new CsvFile("something"))));

            context
                .For<IFileSystem>()
                .Setup(fileSystem => fileSystem.Path.Combine(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("some combined file path");

            await sut.WorkOnAsync(Guid.NewGuid(), job);

            context
                .For<ICsvImporter>()
                .Verify(importer => importer.ImportAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            context
                .For<IApplyRulesToCsvFile>()
                .Verify(applyRules => applyRules.Apply(It.IsAny<CsvFile>(), It.IsAny<Guid>(), It.IsAny<Guid>()),
                    Times.Once);

            context
                .For<ICsvExporter>()
                .Verify(
                    exporter => exporter.ExportAsync(
                        It.IsAny<CsvFile>(), 
                        It.IsAny<string>(), 
                        It.IsAny<string>(), 
                        It.IsAny<string>(),
                        It.IsAny<bool>()), 
                    Times.Once);


            if (shouldDeleteFile)
                file.Verify(f => f.Delete(), Times.Once);
            else
                file.Verify(f => f.Delete(), Times.Never);
        }

        [Fact]
        public async Task WorkOn_Cleans_Up_When_Export_Fails()
        {
            var (context, _) = CreateContext();
            var sut = context.Build();

            var file = new Mock<IFileInfo>();
            file
                .SetupGet(f => f.Exists)
                .Returns(true);

            file
                .SetupGet(f => f.Extension)
                .Returns(".csv");

            var job = new CsvProcessJob(file.Object);

            context
                .For<ICsvImporter>()
                .Setup(importer => importer.ImportAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new Result<CsvFile>(true, new CsvFile("something"))));

            context
                .For<IFileSystem>()
                .Setup(fileSystem => fileSystem.Path.Combine(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("some combined file path");

            context
                .For<ICsvExporter>()
                .Setup(exporter => exporter.ExportAsync(
                    It.IsAny<CsvFile>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Throws<Exception>();

            context
                .For<IFileSystem>()
                .Setup(fileSystem => fileSystem.File.Exists(It.IsAny<string>()))
                .Returns(true);

            await sut.WorkOnAsync(Guid.NewGuid(), job);

            context
                .For<IFileSystem>()
                .Verify(fileSystem => fileSystem.File.Delete(It.IsAny<string>()), Times.Once);
        }

        private static (ArrangeContext<CsvProcessJobWorker>, CsvProcessorOptions) CreateContext()
        {
            var options = new CsvProcessorOptions();
            var optionsMock = new Mock<IOptions<CsvProcessorOptions>>();
            optionsMock
                .SetupGet(opt => opt.Value)
                .Returns(options);

            var context = new ArrangeContext<CsvProcessJobWorker>();
            context.Use(optionsMock);

            return (context, options);
        }
    }
}
