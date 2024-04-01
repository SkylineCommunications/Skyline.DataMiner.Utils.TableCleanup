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
        /// <param name="tablePid">The trap table which is being cleaned</param>
        /// <param name="input">Takes in CleanupData that will be filtered by the Filters initialized by the Builder class</param>
        public void DeleteFilteredTable(SLProtocol protocol, int tablePid, CleanupData input)
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

            protocol.DeleteRow(tablePid, keysToDelete.ToArray());
        }

        /// <summary>
        /// The builder class of the TableFilters. The cleanup method to be added needs to be a discreet in the form of: -1 = NA, 1 = Combo, 2 = TrapAge, 3 = RowCount.
        /// </summary>
        public class TableFiltersBuilder
        {
            private readonly TableFilters instance = new TableFilters();

            /// <summary>
            /// Builds the TableFilters with the added parameters and then validates it.
            /// </summary>
            /// <param name="cleanupMethodParam">The cleanup method must be an enum of the form, -1 = NA, 1 = Combo, 2 = TrapAge, 3 = RowCount.</param>
            /// <param name="maxAlarmCount">The max alarm count must be a positive number that determines the maximum number of rows allowed in the table.</param>
            /// <param name="deletionAmount">This number determines how many rows are deleted from the table when it reaches its max capacity</param>
            /// <param name="maxAlarmAge">This time is the duration allowed of the oldest trap in the table in seconds.</param>
            /// <returns></returns>
            public TableFilters Build(CleanupMethod cleanupMethodParam, int maxAlarmCount, int deletionAmount, int maxAlarmAge)
            {
                switch (cleanupMethodParam)
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