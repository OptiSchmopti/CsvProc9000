using ArrangeContext.Moq;
using CsvProc9000.Jobs;
using CsvProc9000.Jobs.Contracts;
using FluentAssertions;
using Moq;
using System.IO.Abstractions;
using Xunit;

namespace CsvProc9000.Tests.Jobs;

public class JobPoolTests
{
    [Fact]
    public void Implements_Needed_Interface()
    {
        var context = new ArrangeContext<JobPool>();
        var sut = context.Build();

        sut
            .Should()
            .BeAssignableTo<IJobPool>();
    }

    [Fact]
    public void TryGet_Does_Not_Find_Non_Existing_Jobs()
    {
        var context = new ArrangeContext<JobPool>();
        var sut = context.Build();

        var result = sut.TryGet<CsvProcessJob>(out _);

        result
            .Should()
            .BeFalse();
    }

    [Fact]
    public void Add_And_TryGet_Job()
    {
        var context = new ArrangeContext<JobPool>();
        var sut = context.Build();

        var job = new CsvProcessJob(Mock.Of<IFileInfo>());

        sut.Add(job);

        var result = sut.TryGet<CsvProcessJob>(out var foundJob);

        result
            .Should()
            .BeTrue();
        foundJob
            .Should()
            .Be(job);
    }

    [Fact]
    public void Add_And_TryGet_Does_First_In_First_Out()
    {
        var context = new ArrangeContext<JobPool>();
        var sut = context.Build();

        var job1 = new CsvProcessJob(Mock.Of<IFileInfo>());
        var job2 = new CsvProcessJob(Mock.Of<IFileInfo>());

        sut.Add(job1);
        sut.Add(job2);

        sut.TryGet<CsvProcessJob>(out var foundJob);

        foundJob
            .Should()
            .Be(job1);
    }
}