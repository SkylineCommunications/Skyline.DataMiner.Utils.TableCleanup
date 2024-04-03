namespace Skyline.DataMiner.Utils.TableCleanup
{
    /// <summary>
    /// The CleanupMethod enum must strictly follow the following format: 0 = RowAgeOrRowCount, 1 = RowAgeAndRowCount, 1 = RowAge, 2 = RowCount.
    /// </summary>
    public enum CleanupMethod
    {
        /// <summary>
        /// Combo cleanup method uses both row age and row count to filter and clean the table.
        /// </summary>
        RowAgeOrRowCount = 0,

        /// <summary>
        /// Combo cleanup method uses both row age and row count to filter and clean the table.
        /// </summary>
        RowAgeAndRowCount = 1,

        /// <summary>
        /// RowAge cleanup method will filter and clean rows based on how old they are.
        /// </summary>
        RowAge = 2,

        /// <summary>
        /// RowCount cleanup method will filter and clean the oldest rows once the table reaches a threshold capacity.
        /// </summary>
        RowCount = 3
    }
}