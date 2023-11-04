using ArrangeContext.Moq;
using CsvProc9000.BackgroundServices;
using CsvProc9000.Jobs;
using CsvProc9000.Jobs.Contracts;
using CsvProc9000.Model.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CsvProc9000.Tests.BackgroundServices
{
    public class CsvFileWatcherBackgroundServiceTests
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

        [Fact]
        public async Task Make_Sure_That_Inbox_Exists_Before_Watching()
        {
            var (context, _) = CreateContext();
            var sut = context.Build();

            context
                .For<IFileSystem>()
                .Setup(fileSystem => fileSystem.Directory.Exists(It.IsAny<string>()))
                .Returns(false);

            await sut.StartAsync(CancellationToken.None);

            context
                .For<IFileSystem>()
                .Verify(fs => fs.Directory.CreateDirectory(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Start_Enables_Raising_Events()
        {
            var (context, fileSystemWatcher) = CreateContext();
            var sut = context.Build();

            await sut.StartAsync(CancellationToken.None);

            fileSystemWatcher.VerifyAdd(watcher => watcher.Created += It.IsAny<FileSystemEventHandler>());
            fileSystemWatcher.VerifySet(watcher => watcher.Filter = "*.csv");
            fileSystemWatcher.VerifySet(watcher => watcher.EnableRaisingEvents = true);
        }

        [Fact]
        public async Task Stop_Disables_Raising_Events()
        {
            var (context, fileSystemWatcher) = CreateContext();
            var sut = context.Build();

            await sut.StopAsync(CancellationToken.None);

            fileSystemWatcher.VerifySet(watcher => watcher.EnableRaisingEvents = false);
        }

        [Fact]
        public void Dispose_Disposes_FileSystemWatcher()
        {
            var (context, fileSystemWatcher) = CreateContext();
            var sut = context.Build();

            sut.Dispose();

            fileSystemWatcher.VerifyRemove(watcher => watcher.Created -= It.IsAny<FileSystemEventHandler>());
            fileSystemWatcher.VerifyRemove(watcher => watcher.Renamed -= It.IsAny<RenamedEventHandler>());
        }

        [Fact]
        public void Not_Add_Created_Files_To_Pool_When_Wrong_Change_Type()
        {
            var (context, fileSystemWatcher) = CreateContext();
            _ = context.Build();

            fileSystemWatcher
                .Raise(watcher => watcher.Created += null,
                    new FileSystemEventArgs(WatcherChangeTypes.Renamed, "something", "some file"));

            context
                .For<IJobPool>()
                .Verify(pool => pool.Add(It.IsAny<IJob>()), Times.Never);
        }

        [Fact]
        public void Add_Created_Files_To_Pool()
        {
            var (context, fileSystemWatcher) = CreateContext();
            _ = context.Build();

            var fileInfoMock = new Mock<IFileInfo>();

            context
                .For<IFileSystem>()
                .Setup(fileSystem => fileSystem.FileInfo.FromFileName(It.IsAny<string>()))
                .Returns(fileInfoMock.Object);

            fileSystemWatcher
                .Raise(watcher => watcher.Created += null,
                    new FileSystemEventArgs(WatcherChangeTypes.All, "something", "some file"));

            context
                .For<IJobPool>()
                .Verify(pool => pool.Add(It.Is<CsvProcessJob>(job =>
                    job.TargetFile == fileInfoMock.Object)), Times.Once);
        }

        [Fact]
        public void Not_Add_Renamed_Files_To_Pool_When_Wrong_Change_Type()
        {
            var (context, fileSystemWatcher) = CreateContext();
            _ = context.Build();

            fileSystemWatcher
                .Raise(watcher => watcher.Renamed += null,
                    new RenamedEventArgs(WatcherChangeTypes.Deleted, "something", "some name", "some old name"));

            context
                .For<IJobPool>()
                .Verify(pool => pool.Add(It.IsAny<IJob>()), Times.Never);
        }

        [Fact]
        public void Add_Renamed_Files_To_Pool()
        {
            var (context, fileSystemWatcher) = CreateContext();
            _ = context.Build();

            var fileInfoMock = new Mock<IFileInfo>();

            context
                .For<IFileSystem>()
                .Setup(fileSystem => fileSystem.FileInfo.FromFileName(It.IsAny<string>()))
                .Returns(fileInfoMock.Object);

            fileSystemWatcher
                .Raise(watcher => watcher.Renamed += null,
                    new RenamedEventArgs(WatcherChangeTypes.All, "something", "some file", "some old name"));

            context
                .For<IJobPool>()
                .Verify(pool => pool.Add(It.Is<CsvProcessJob>(job =>
                    job.TargetFile == fileInfoMock.Object)), Times.Once);
        }

        private static (ArrangeContext<CsvFileWatcherBackgroundService>, Mock<IFileSystemWatcher>) CreateContext()
        {
            var options = new CsvProcessorOptions();
            var optionsMock = new Mock<IOptions<CsvProcessorOptions>>();
            optionsMock
                .SetupGet(opt => opt.Value)
                .Returns(options);

            var fileSystemWatcherMock = new Mock<IFileSystemWatcher>();

            var context = new ArrangeContext<CsvFileWatcherBackgroundService>();
            context.Use(optionsMock);

            context
                .For<IFileSystem>()
                .Setup(fileSystem => fileSystem.FileSystemWatcher.CreateNew(It.IsAny<string>()))
                .Returns(fileSystemWatcherMock.Object);

            context
                .For<IFileSystem>()
                .Setup(fileSystem => fileSystem.Directory.Exists(It.IsAny<string>()))
                .Returns(true);

            return (context, fileSystemWatcherMock);
        }
    }
}
