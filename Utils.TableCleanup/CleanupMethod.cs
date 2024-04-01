namespace Skyline.DataMiner.Utils.TableCleanup
{
    /// <summary>
    /// The CleanupMethod enum must strictly follow the following forma: -1 = NA, 0 = Combo, 1 = TrapAge, 2 = RowCount.
    /// </summary>
    public enum CleanupMethod
    {
        /// <summary>
        /// NA cleanup method means no cleanup method is being used. This is not recommended.
        /// </summary>
        NA = -1,

        /// <summary>
        /// Combo cleanup method uses both trap age and row count to filter and clean the table.
        /// </summary>
        Combo = 0,

        /// <summary>
        /// TrapAge cleanup method will filter and clean traps based on how old they are.
        /// </summary>
        TrapAge = 1,

        /// <summary>
        /// RowCount cleanup method will filter and clean the oldest traps once the trap table reaches a threshold capacity.
        /// </summary>
        RowCount = 2
    }
}