namespace Skyline.DataMiner.Utils.TableCleanup
{
    using System.Collections.ObjectModel;
    public interface IFilter<T>
    {
        ReadOnlyCollection<string> RemovedPrimaryKeys { get; }

        CleanupData Execute(CleanupData input);
    }
}