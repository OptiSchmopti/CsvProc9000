using ArrangeContext.Moq;
using CsvHelper;
using CsvProc9000.Csv;
using CsvProc9000.Csv.Contracts;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CsvProc9000.Tests.Csv
{
    public class CsvImporterTests
    {
        [Fact]
        public void Implements_Needed_Interface()
        {
            var (context, _) = CreateContext();
            var sut = context.Build();

            sut
                .Should()
                .BeAssignableTo<ICsvImporter>();
        }

        [Fact]
        public async Task Import_Does_Not_Accept_Invalid_Parameters()
        {
            var (context, _) = CreateContext();
            var sut = context.Build();

            await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.ImportAsync(null!, ","));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.ImportAsync(string.Empty, ","));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.ImportAsync(" ", ","));

            await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.ImportAsync("something", null!));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.ImportAsync("something", string.Empty));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.ImportAsync("something", " "));
        }

        [Fact]
        public async Task Import_Rejects_Files_That_Have_No_Header()
        {
            var (context, reader) = CreateContext();
            var sut = context.Build();

            reader
                .Setup(r => r.ReadHeader())
                .Returns(false);

            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => sut.ImportAsync("f", ","));

            reader
                .Setup(r => r.ReadHeader())
                .Returns(true);
            reader
                .SetupGet(r => r.HeaderRecord)
                .Returns(Array.Empty<string>());

            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => sut.ImportAsync("f", ","));
        }

        [Fact]
        public async Task Import()
        {
            var (context, reader) = CreateContext();
            var sut = context.Build();

            var headers = new[] { "h1", "h2" };

            reader
                .Setup(r => r.ReadHeader())
                .Returns(true);
            reader
                .SetupGet(r => r.HeaderRecord)
                .Returns(headers);

            // this construct is to only return true for 'ReadAsync' twice
            var readCalled = 0;
            reader
                .Setup(r => r.ReadAsync())
                .Returns(() => Task.FromResult(readCalled < 2))
                .Callback(() => readCalled++);

            var expectedValueField1 = "f1";
            reader
                .Setup(r => r.TryGetField(0, out expectedValueField1))
                .Returns(true);

            var expectedValueField2 = "f2";
            reader
                .Setup(r => r.TryGetField(1, out expectedValueField2))
                .Returns(true);

            var result = await sut.ImportAsync("f", ",");

            result
                .IsSuccess
                .Should()
                .BeTrue();

            var file = result.Value;

            file
                .Should()
                .NotBeNull();

            file
                .Rows
                .Should()
                .HaveCount(1);

            file
                .Rows
                .ElementAt(0)
                .Fields
                .ElementAt(0)
                .Column
                .Name
                .Should()
                .Be(headers[0]);

            file
                .Rows
                .ElementAt(0)
                .Fields
                .ElementAt(0)
                .Value
                .Should()
                .Be(expectedValueField1);

            file
                .Rows
                .ElementAt(0)
                .Fields
                .ElementAt(1)
                .Column
                .Name
                .Should()
                .Be(headers[1]);

            file
                .Rows
                .ElementAt(0)
                .Fields
                .ElementAt(1)
                .Value
                .Should()
                .Be(expectedValueField2);
        }

        private static (ArrangeContext<CsvImporter>, Mock<IReader>) CreateContext()
        {
            var context = new ArrangeContext<CsvImporter>();

            var reader = new Mock<IReader>();
            context
                .For<ICsvReaderFactory>()
                .Setup(rf => rf.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(reader.Object);

            return (context, reader);
        }
    }
}
