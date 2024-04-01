namespace Skyline.DataMiner.Utils.TableCleanup
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// An interface for the filters to be implemented to clean and filter the trap table.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFilter<T>
    {
        /// <summary>
        /// Contains the primary keys that are removed by the cleaning and filtering methods from the table.
        /// </summary>
        ReadOnlyCollection<string> RemovedPrimaryKeys { get; }

        /// <summary>
        /// The execution of the filtering and cleaning on the traps table.
        /// </summary>
        /// <param name="input">The cleanup data that has to be cleaned by the filters.</param>
        /// <returns>The cleanup data after if has been cleaned and filtered by the filters.</returns>
        CleanupData Execute(CleanupData input);
    }
}