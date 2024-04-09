namespace Skyline.DataMiner.Utils.TableCleanup.Interfaces
{
    using Skyline.DataMiner.Scripting;
    using System.Collections.Generic;

    /// <summary>
    /// An interface for the filters to be implemented to clean and filter the table.
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        /// Contains the primary keys that are removed by the cleaning and filtering methods from the table.
        /// </summary>
        List<string> RemovedPrimaryKeys { get; }

        /// <summary>
        /// This will apply the filter on the rows
        /// </summary>
        /// <param name="protocol">This is needed to retrieve the values of the settings of the filter.</param>
        /// <param name="rows">These are the rows where the filter will be executed on.</param>
        void Execute(SLProtocol protocol, List<CleanupRow> rows);
    }
}