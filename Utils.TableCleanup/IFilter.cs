namespace Skyline.DataMiner.Utils.TableCleanup
{
    using Skyline.DataMiner.Scripting;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// An interface for the filters to be implemented to clean and filter the table.
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Contains the primary keys that are removed by the cleaning and filtering methods from the table.
        /// </summary>
        List<string> RemovedPrimaryKeys { get; }

        void Execute(SLProtocol protocol, List<CleanupRow> rows);
    }
}