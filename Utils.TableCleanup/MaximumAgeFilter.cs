using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Skyline.DataMiner.Utils.TableCleanup
{   
    public class MaximumAgeFilter : IFilter<CleanupData>
    {
        public MaximumAgeFilter(int seconds)
        {
            if (seconds <= 0)
            {
                throw new ArgumentException("Value cannot be smaller or equal to zero.", "seconds");
            }

            MinimumAllowed = DateTime.Now.AddSeconds(-1 * seconds);
        }

        public ReadOnlyCollection<string> RemovedPrimaryKeys { get; private set; }

        private DateTime MinimumAllowed { get; set; }

        public static MaximumAgeFilter FromDays(int days)
        {
            return new MaximumAgeFilter((int)TimeSpan.FromDays(days).TotalSeconds);
        }

        public static MaximumAgeFilter FromHours(int hours)
        {
            return new MaximumAgeFilter((int)TimeSpan.FromHours(hours).TotalSeconds);
        }

        public static MaximumAgeFilter FromMinutes(int minutes)
        {
            return new MaximumAgeFilter((int)TimeSpan.FromMinutes(minutes).TotalSeconds);
        }

        public static MaximumAgeFilter FromSeconds(int seconds)
        {
            return new MaximumAgeFilter(seconds);
        }

        public static MaximumAgeFilter FromTimespan(TimeSpan time)
        {
            return new MaximumAgeFilter((int)time.TotalSeconds);
        }

        public CleanupData Execute(CleanupData input)
        {
            List<string> removedKeys = new List<string>();
            List<CleanupRow> filtered = new List<CleanupRow>();

            if (input.Timestamps != null)
            {
                foreach (CleanupRow row in input.Rows)
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
                return new CleanupData(filtered);
            }
            else
            {
                return input;
            }
        }
    }
}