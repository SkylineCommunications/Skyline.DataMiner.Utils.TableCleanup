namespace Skyline.DataMiner.Utils.TableCleanup.Interfaces
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal interface ISubFilter
    {
        ReadOnlyCollection<string> RemovedPrimaryKeys { get; }

        void Execute(List<CleanupRow> rows);
    }
}
