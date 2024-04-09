namespace Skyline.DataMiner.Utils.TableCleanup.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.Scripting;
    using Skyline.DataMiner.Utils.TableCleanup.Interfaces;
    using Skyline.DataMiner.Utils.TableCleanup.SubFilters;

    /// <summary>
    /// Initializes a new instance of the TableFilters class that contains the filters to apply to the TableData class.
    /// </summary>
    public class TableMaxRowCondition : IFilter
    {
        /// <summary>
        /// These Filters are to be used on the TableData class and can be either of CleanupMethod NA, Combo, RowAge or RowCount.
        /// </summary>
        internal readonly List<ISubFilter> Filters = new List<ISubFilter>();

        /// <summary>
        /// This is to register the cleanup parameters.
        /// </summary>
        /// <param name="cleanupMethodPid">This is the cleanup methods that will be used.</param>
        /// <param name="maxAlarmCountPid">This is the parameter ID for the maximum amount of rows in the table.</param>
        /// <param name="maxAlarmAgePid">This is the parameter ID for the maximum age of rows in the table. This parameter should be configured in seconds.</param>
        public TableMaxRowCondition(int cleanupMethodPid, int maxAlarmCountPid, int maxAlarmAgePid)
        {
            RemovedPrimaryKeys = new List<string>();
            CleanupMethodPid = cleanupMethodPid;
            MaxAlarmCountPid = maxAlarmCountPid;
            MaxAlarmAgePid = maxAlarmAgePid;
        }

        /// <summary>
        /// The primary keys that will be removed.
        /// </summary>
        public List<string> RemovedPrimaryKeys { get; set; }

        private int CleanupMethodPid { get; set; }

        private int MaxAlarmCountPid { get; set; }

        private int MaxAlarmAgePid { get; set; }

        private bool IsAgeFilterDefined { get; set; }

        /// <summary>
        /// Apply the condition on the rows to find the keys to remove.
        /// </summary>
        /// <param name="protocol">This is needed to get the settings of the filter.</param>
        /// <param name="rows">The rows that will be checked on.</param>
        public void Execute(SLProtocol protocol, List<CleanupRow> rows)
        {
            IsAgeFilterDefined = false;
            uint[] tableCleanupValuesPids = new uint[]
                {
                    Convert.ToUInt32(CleanupMethodPid),
                    Convert.ToUInt32(MaxAlarmCountPid),
                    Convert.ToUInt32(MaxAlarmAgePid),
                };
            object[] tableCleanupValues = (object[])protocol.GetParameters(tableCleanupValuesPids);
            CleanupMethod cleanupMethod = (CleanupMethod)Convert.ToInt32(tableCleanupValues[0]);
            int maxAlarmCount = Convert.ToInt32(tableCleanupValues[1]);
            int maxAlarmAge = Convert.ToInt32(tableCleanupValues[2]);
            int deletionAmountMaxAlarmCount = Convert.ToInt32((double)maxAlarmCount / 100 * 20); // Remove 20% of the data
            switch (cleanupMethod)
            {
                case CleanupMethod.RowAgeAndRowCount:
                    Filters.Add(new MaximumAgeCondition(maxAlarmAge));
                    Filters.Add(new MaximumRowCountCondition(maxAlarmCount, deletionAmountMaxAlarmCount));
                    IsAgeFilterDefined = true;
                    break;

                case CleanupMethod.RowAge:
                    Filters.Add(new MaximumAgeCondition(maxAlarmAge));
                    IsAgeFilterDefined = true;
                    break;

                case CleanupMethod.RowCount:
                    Filters.Add(new MaximumRowCountCondition(maxAlarmCount, deletionAmountMaxAlarmCount));
                    break;
            }

            Validate();
            if (IsAgeFilterDefined)
            {
                rows = rows.OrderBy(x => x.Timestamp).ToList();
            }

            HashSet<string> keysToDelete = new HashSet<string>();
            foreach (ISubFilter filter in Filters)
            {
                filter.Execute(rows);
                if (filter.RemovedPrimaryKeys != null)
                {
                    keysToDelete.UnionWith(filter.RemovedPrimaryKeys);
                }
            }

            RemovedPrimaryKeys.AddRange(keysToDelete);
        }

        private void Validate()
        {
            if (Filters.Count == 0)
            {
                throw new InvalidOperationException("No filter was registered.");
            }
        }
    }
}