using ArrangeContext.Moq;
using CsvHelper;
using CsvProc9000.Csv;
using CsvProc9000.Csv.Contracts;
using CsvProc9000.Model.Csv;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CsvProc9000.Tests.Csv
{
    public class CsvExporterTests
    {
        [Fact]
        public void Implements_Needed_Interface()
        {
            var (context, _) = CreateContext();
            var sut = context.Build();

            sut
                .Should()
                .BeAssignableTo<ICsvExporter>();
        }

        [Fact]
        public async Task Export_Does_Not_Accept_Invalid_Parameters()
        {
            var (context, _) = CreateContext();
            var sut = context.Build();

            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => sut.ExportAsync(null!, "something", ","));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.ExportAsync(new CsvFile("something"), null!, ","));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.ExportAsync(new CsvFile("something"), string.Empty, ","));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.ExportAsync(new CsvFile("something"), "something", null!));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.ExportAsync(new CsvFile("something"), "something", string.Empty));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.ExportAsync(new CsvFile("something"), "something", " "));
        }

        [Fact]
        public async Task Export_Makes_Sure_That_Destination_Directory_Exists()
        {
            var (context, _) = CreateContext();
            var sut = context.Build();

            var directoryInfo = new Mock<IDirectoryInfo>();
            directoryInfo
                .SetupGet(di => di.Exists)
                .Returns(false);

            var fileInfo = new Mock<IFileInfo>();
            fileInfo
                .SetupGet(fi => fi.Directory)
                .Returns(directoryInfo.Object);

            context
                .For<IFileSystem>()
                .Setup(fs => fs.FileInfo.FromFileName(It.IsAny<string>()))
                .Returns(fileInfo.Object);

            var file = new CsvFile("something");
            await sut.ExportAsync(file, "some file", ",");
            
            directoryInfo.Verify(di => di.Create(), Times.Once);
        }

        [Fact]
        public async Task Export_Writes_Data_To_File()
        {
            var (context, writer) = CreateContext();
            var sut = context.Build();

            var file = new CsvFile("something");
            var column1 = new CsvColumn(0, "name1");
            var column2 = new CsvColumn(1, "name2");

            var row1 = new CsvRow();
            var row2 = new CsvRow();
            
            row1.AddField(column1, "field11");
            row1.AddField(column2, "field12");
            
            row2.AddField(column1, "field21");
            row2.AddField(column2, "field22");
            
            file.AddRow(row1);
            file.AddRow(row2);
            
            await sut.ExportAsync(file, "something", ",");
            
            // 3 = 1 header, 2 rows
            writer.Verify(w => w.NextRecordAsync(), Times.Exactly(3));

            var valuesThatShouldHaveBeenWritten = new List<string> { column1.Name, column2.Name };
            valuesThatShouldHaveBeenWritten.AddRange(row1.Fields.Select(f => f.Value));
            valuesThatShouldHaveBeenWritten.AddRange(row2.Fields.Select(f => f.Value));
            
            foreach (var valueThatShouldHaveBeenWritten in valuesThatShouldHaveBeenWritten)
                writer.Verify(w => w.WriteField(valueThatShouldHaveBeenWritten, It.IsAny<bool>()), Times.Once);

            writer.Verify(w => w.FlushAsync(), Times.Once);
        }

        private static (ArrangeContext<CsvExporter>, Mock<IWriter>) CreateContext()
        {
            var context = new ArrangeContext<CsvExporter>();

            var writer = new Mock<IWriter>();
            context
                .For<ICsvWriterFactory>()
                .Setup(wf => wf.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(writer.Object);
            
            var directoryInfo = new Mock<IDirectoryInfo>();
            directoryInfo
                .SetupGet(di => di.Exists)
                .Returns(false);

            var fileInfo = new Mock<IFileInfo>();
            fileInfo
                .SetupGet(fi => fi.Directory)
                .Returns(directoryInfo.Object);

            context
                .For<IFileSystem>()
                .Setup(fs => fs.FileInfo.FromFileName(It.IsAny<string>()))
                .Returns(fileInfo.Object);

            return (context, writer);
        }
    }
}
