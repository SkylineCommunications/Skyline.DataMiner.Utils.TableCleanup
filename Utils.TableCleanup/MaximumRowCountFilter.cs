﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Skyline.DataMiner.Utils.TableCleanup
{
    /// <summary>
    /// The maximum row count allowed on the table that implements the IFilter interface.
    /// </summary>
    public class MaximumRowCountFilter : IFilter<TableCleanupData>
    {
        /// <summary>
        /// The constructor class for the MaximumRowCountFilter.
        /// </summary>
        /// <param name="maxRows">The max rows that should be allowed for the table.</param>
        /// <param name="deletionAmount">The number of rows to be deleted when the table reaches its max capacity.</param>
        /// <exception cref="ArgumentException">Exception thrown if the given parameters are invalid.</exception>
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

        /// <summary>
        /// The parameter setting the number of rows to be deleted when the table reaches its max capacity.
        /// </summary>
        private int DeletionAmount { get; set; }

        /// <summary>
        /// The parameter setting the max rows that should be allowed for the table.
        /// </summary>
        private int MaxRowCount { get; set; }

        /// <summary>
        /// The parameter with the collection of primary keys of the rows that are removed from the table.
        /// </summary>
        public ReadOnlyCollection<string> RemovedPrimaryKeys { get; private set; }
        public int Threshold { get; set; }

        /// <summary>
        /// This method will filter the given input data by the max count filter.
        /// </summary>
        /// <returns>The data after it has been cleaned and filtered.returns>
        /// <param name="input">The cleanup info input.</param>
        public void Execute(TableCleanupData input)
        {
            List<CleanupRow> availableRows = new List<CleanupRow>(input.Rows);

            int size = availableRows.Count;
            bool isRemovalRequired = size > MaxRowCount;

            if (isRemovalRequired)
            {
                // If a user enters deletionAmount value that is bigger than the actual amount of data, an error would occur.
                int threshold = (DeletionAmount + MaxRowCount) > size ? size : size - (MaxRowCount - DeletionAmount);
                Threshold = threshold;
                RemovedPrimaryKeys = new ReadOnlyCollection<string>(availableRows.Take(threshold).Select(r => r.PrimaryKey).ToList());
            }
        }
    }
}