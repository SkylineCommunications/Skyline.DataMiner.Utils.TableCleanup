using Skyline.DataMiner.Utils.TableCleanup.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Skyline.DataMiner.Utils.TableCleanup.SubFilters
{
    /// <summary>
    /// The maximum age of the rows allowed on the table that implements the IFilter interface.
    /// </summary>
    internal class MaximumAgeCondition : ISubCondition
    {
        /// <summary>
        /// The maximum age allowed by the filter that cleans all rows with timestamps older than the given seconds provided.
        /// </summary>
        /// <param name="seconds">The threshold age allowed of the oldest rows given in seconds.</param>
        /// <exception cref="ArgumentException">An exception thrown if the threshold time is invalid.</exception>
        public MaximumAgeCondition(int seconds)
        {
            if (seconds <= 0)
            {
                throw new ArgumentException("Value cannot be smaller or equal to zero.", "seconds");
            }

            MinimumAllowed = DateTime.Now.AddSeconds(-1 * seconds);
        }

        /// <summary>
        /// The primary keys deleted and taken out of the table after filtering.
        /// </summary>
        public ReadOnlyCollection<string> RemovedPrimaryKeys { get; private set; }

        private DateTime MinimumAllowed { get; set; }

        /// <summary>
        /// This method will filter the given input data by the max age filter.
        /// </summary>
        /// <returns>The data after it has been cleaned and filtered.returns>
        /// <param name="input">The cleanup info input.</param>
        public void Execute(List<CleanupRow> rows)
        {
            List<string> removedKeys = new List<string>();
            List<CleanupRow> filtered = new List<CleanupRow>();
            foreach (CleanupRow row in rows)
            {
                if (row.Timestamp.HasValue)
                {
                    DateTime registered = row.Timestamp.Value;

                    if (registered > MinimumAllowed)
                    {
                        filtered.Add(row);
                    }
                    else
                    {
                        removedKeys.Add(row.PrimaryKey);
                    }
                }
                else
                {
                    filtered.Add(row);
                }
            }

            RemovedPrimaryKeys = new ReadOnlyCollection<string>(removedKeys);
            rows = filtered;
        }
    }
}