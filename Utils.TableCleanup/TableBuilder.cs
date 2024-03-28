using System;
using System.Collections.Generic;

namespace Skyline.DataMiner.Utils.TableCleanup
{
    public class Table
    {
        private int maxAlarmCount;
        private int deletionAmount;
        private int maxAlarmAge;
        private CleanupMethod cleanupMethod;

        protected readonly List<IFilter<CleanupData>> Filters = new List<IFilter<CleanupData>>();

        private Table()
        {
        }

        public static TableBuilder Builder()
        {
            return new TableBuilder();
        }

        public IEnumerable<string> FindKeysToDelete(CleanupData input)
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

            return keysToDelete;
        }

        public class TableBuilder
        {
            private readonly Table instance = new Table();

            public Table Build()
            {
                // Check if the constructed object does not contain any issues.
                Validate();

                return instance;
            }

            public TableBuilder AddCleanupMethod(CleanupMethod cleanupMethodParam)
            {
                this.instance.cleanupMethod = cleanupMethodParam;
                return this;
            }

            public TableBuilder AddMaxAlarmCount(int maxAlarmCount, int deletionAmount)
            {
                this.instance.maxAlarmCount = maxAlarmCount;
                this.instance.deletionAmount = deletionAmount;
                return this;
            }

            public TableBuilder AddMaxAlarmAge(int maxAlarmAge)
            {
                this.instance.maxAlarmAge = maxAlarmAge;
                return this;
            }

            public TableBuilder RegisterFilters()
            {
                switch (this.instance.cleanupMethod)
                {
                    case CleanupMethod.NA:
                        // No cleaup has to be done.
                        break;

                    case CleanupMethod.Combo:
                        instance.Filters.Add(new MaximumRowCountFilter(Convert.ToInt32(this.instance.maxAlarmCount), this.instance.deletionAmount));
                        instance.Filters.Add(new MaximumAgeFilter(Convert.ToInt32(this.instance.maxAlarmAge)));
                        break;

                    case CleanupMethod.TrapAge:
                        instance.Filters.Add(new MaximumAgeFilter(Convert.ToInt32(this.instance.maxAlarmAge)));
                        break;

                    case CleanupMethod.RowCount:
                        instance.Filters.Add(new MaximumRowCountFilter(Convert.ToInt32(this.instance.maxAlarmCount), this.instance.deletionAmount));
                        break;
                }

                return this;
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