using System;

namespace Skyline.DataMiner.Utils.TableCleanup
{
    /// <summary>
    /// This references a generic row that has to be cleaned up. It contains a primary key and a time stamp.
    /// </summary>
    public class CleanupRow
    {
        /// <summary>
        /// The primary key of a given row.
        /// </summary>
        public string PrimaryKey { get; set; }

        /// <summary>
        /// The time stamp of a given row.
        /// </summary>
        public DateTime? Timestamp { get; set; }
    }
}