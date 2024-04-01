using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Scripting;

namespace Skyline.DataMiner.Utils.TableCleanup
{
    /// <summary>
    /// Initializes a new instance of the TableFilters class that contains the filters to apply to the TableData class.
    /// </summary>
    public class TableFilters
    {
        /// <summary>
        /// These Filters are to be used on the TableData class and can be either of CleanupMethod NA, Combo, TrapAge or RowCount.
        /// </summary>
        protected readonly List<IFilter<CleanupData>> Filters = new List<IFilter<CleanupData>>();

        private TableFilters()
        {
        }

        /// <summary>
        /// This is the constructor for the Builder of the TableFilters class.
        /// </summary>
        /// <returns>A TableFiltersBuilder that can run the Build method with CleanupMethod parameters to create the TableFilters.</returns>
        public static TableFiltersBuilder Builder()
        {
            return new TableFiltersBuilder();
        }

        /// <summary>
        /// This will clean the table based on the filters provided on the provided tablePid in the protocol.
        /// </summary>
        /// <param name="protocol">The SLProtocol process that is running the delete function</param>
        /// <param name="input">Takes in CleanupData that will be filtered by the Filters initialized by the Builder class</param>
        public void DeleteFilteredTable(SLProtocol protocol, CleanupData input)
        {
            HashSet<string> keysToDelete = new HashSet<string>();

            foreach (IFilter<CleanupData> filter in this.Filters)
            {
                input = filter.Execute(input);

                if (filter.RemovedPrimaryKeys != null)
                {
                    keysToDelete.UnionWith(filter.RemovedPrimaryKeys);
                }
            }

            protocol.DeleteRow(input.TablePid, keysToDelete.ToArray());
        }

        /// <summary>
        /// The builder class of the TableFilters. The cleanup method to be added needs to be a discreet in the form of: -1 = NA, 1 = Combo, 2 = TrapAge, 3 = RowCount.
        /// </summary>
        public class TableFiltersBuilder
        {
            private readonly TableFilters instance = new TableFilters();

            /// <summary>
            /// Builds the TableFilters based on the given parameter IDs for the cleanup method, max alarm count, deletion amount and max alarm age and then validates it.
            /// </summary>
            /// <param name="protocol">The SLProtocol process to execute the clean up based on parameter IDs on.</param>
            /// <param name="cleanupMethodPid">The parameter ID for the cleanup method. The method must be an enum of the form, -1 = NA, 1 = Combo, 2 = TrapAge, 3 = RowCount.</param>
            /// <param name="maxAlarmCountPid">The parameter ID for the max alarm count. The count must be a positive number that determines the maximum number of rows allowed in the table.</param>
            /// <param name="deletionAmountPid">The parameter ID for the deletion amount. The deletion amount determines how many rows are deleted from the table when it reaches its max capacity</param>
            /// <param name="maxAlarmAgePid">The parameter ID for the max alarm age. This time is the duration allowed of the oldest trap in the table in seconds.</param>
            /// <returns>A built TableFilters class that can be used together with CleanupData to filter on that data.</returns>
            public TableFilters Build(SLProtocol protocol, int cleanupMethodPid, int maxAlarmCountPid, int deletionAmountPid, int maxAlarmAgePid)
            {
                object tableCleanupValuesPids = new int[]
                {
                    cleanupMethodPid,
                    maxAlarmCountPid,
                    deletionAmountPid,
                    maxAlarmAgePid,
                };
                object[] tableCleanupValues = (object[])protocol.GetParameters(tableCleanupValuesPids);
                CleanupMethod cleanupMethod = (CleanupMethod)Convert.ToInt32(tableCleanupValues[0]);
                int maxAlarmCount = Convert.ToInt32(tableCleanupValues[1]);
                int deletionAmount = Convert.ToInt32(tableCleanupValues[2]);
                int maxAlarmAge = Convert.ToInt32(tableCleanupValues[3]);
                switch (cleanupMethod)
                {
                    case CleanupMethod.NA:
                        // No cleanup has to be done.
                        break;

                    case CleanupMethod.Combo:
                        instance.Filters.Add(new MaximumRowCountFilter(maxAlarmCount, deletionAmount));
                        instance.Filters.Add(new MaximumAgeFilter(maxAlarmAge));
                        break;

                    case CleanupMethod.TrapAge:
                        instance.Filters.Add(new MaximumAgeFilter(maxAlarmAge));
                        break;

                    case CleanupMethod.RowCount:
                        instance.Filters.Add(new MaximumRowCountFilter(maxAlarmCount, deletionAmount));
                        break;
                }
                // Check if the constructed object does not contain any issues.
                Validate();

                return instance;
            }

            private void Validate()
            {
                if (instance.Filters.Count == 0)
                {
                    throw new InvalidOperationException("No filter was registered.");
                }
            }
        }
    }
}