namespace CsvProc9000.Jobs.Contracts;

internal interface IJobPool
{
    void Add(IJob job);
    bool TryGet<T>(out T job) where T : IJob;
}