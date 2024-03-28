using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Skyline.DataMiner.Utils.TableCleanup
{
    public class MaximumRowCountFilter : IFilter<CleanupData>
    {
        public MaximumRowCountFilter(int maxRows, int deletionAmount)
        {
            if (maxRows < 0)
            {
                throw new ArgumentException("The provided value cannot be negative.", "maxRows");
            }

            if (deletionAmount < 0)
            {
                throw new ArgumentException("The provided value cannot be negative.", "deletionAmount");
            }

            MaxRowCount = maxRows;
            DeletionAmount = deletionAmount;
        }

        private MaximumRowCountFilter()
        {
        }

        public int DeletionAmount { get; private set; }

        public int MaxRowCount { get; private set; }

        public ReadOnlyCollection<string> RemovedPrimaryKeys { get; private set; }

        /// <summary>
        /// The rows will be handled in the way they are passed toward the class.
        /// </summary>
        /// <returns>Cleanup Data.</returns>
        /// <param name="input">The cleanup info input.</param>
        public CleanupData Execute(CleanupData input)
        {
            List<CleanupRow> availableRows = new List<CleanupRow>(input.Rows);

            int size = availableRows.Count;
            bool isRemovalRequired = size > MaxRowCount;

            if (isRemovalRequired)
            {
                // If a user enters deletionAmount value that is bigger than the actual amount of data, an error would occur.
                int threshold = DeletionAmount > size ? size : DeletionAmount;

                RemovedPrimaryKeys = new ReadOnlyCollection<string>(availableRows.Take(threshold).Select(r => r.PrimaryKey).ToList());
                availableRows.RemoveRange(0, threshold);

                return new CleanupData(availableRows);
            }
            else
            {
                return input;
            }
        }
    }
}
