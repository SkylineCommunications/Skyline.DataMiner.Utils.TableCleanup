using System.Collections.Generic;
using System;
using System.Linq;

namespace Skyline.DataMiner.Utils.TableCleanup
{
    public class CleanupData
    {
        internal CleanupData(List<CleanupRow> rows)
        {
            Rows = rows;
            Keys = rows.Select(r => r.PrimaryKey);
            Timestamps = rows.Select(r => r.Timestamp);
        }

        public IEnumerable<string> Keys { get; private set; }

        public IEnumerable<DateTime?> Timestamps { get; private set; }

        internal IEnumerable<CleanupRow> Rows { get; private set; }

        public static TrapCleanupDataBuilder Builder()
        {
            return new TrapCleanupDataBuilder();
        }

        public class TrapCleanupDataBuilder
        {
            private string[] keys;
            private DateTime[] timestamps;

            public CleanupData Build()
            {
                Validate();

                List<CleanupRow> rows = new List<CleanupRow>(keys.Length);

                for (int i = 0; i < keys.Length; i++)
                {
                    if (timestamps != null)
                    {
                        rows.Add(new CleanupRow()
                        {
                            PrimaryKey = keys[i],
                            Timestamp = timestamps[i]
                        });
                    }
                    else
                    {
                        rows.Add(new CleanupRow()
                        {
                            PrimaryKey = keys[i],
                            Timestamp = null
                        });
                    }
                }

                return new CleanupData(rows);
            }

            public TrapCleanupDataBuilder WithKeys(IEnumerable<string> keys)
            {
                this.keys = keys.ToArray();
                return this;
            }

            public TrapCleanupDataBuilder WithOleTimestamps(IEnumerable<double?> timestamps)
            {
                this.timestamps = timestamps.Select(r => r.HasValue ? DateTime.FromOADate(r.Value) : DateTime.MinValue).ToArray();
                return this;
            }

            public TrapCleanupDataBuilder WithDateTimeTimestamps(IEnumerable<DateTime?> timestamps)
            {
                this.timestamps = timestamps.Select(r => r.HasValue ? r.Value : DateTime.MinValue).ToArray();
                return this;
            }

            public TrapCleanupDataBuilder WithStringTimestamps(IEnumerable<string> timestamps)
            {
                this.timestamps = timestamps.Select(r => DateTime.TryParse(r, out var parsedDate) ? parsedDate : DateTime.MinValue).ToArray();
                return this;
            }

            private void Validate()
            {
                if (keys == null)
                {
                    throw new InvalidOperationException("No primary keys were provided.");
                }

                if (timestamps != null && timestamps.Length != keys.Length)
                {
                    throw new InvalidOperationException("The number of primary keys does not match the number of timestamps.");
                }
            }
        }
    }
}