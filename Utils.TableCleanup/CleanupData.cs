using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Scripting;
using SLNetMessages = Skyline.DataMiner.Net.Messages;

namespace Skyline.DataMiner.Utils.TableCleanup
{
    /// <summary>
    /// The class containing the row data that needs to be cleaned and filtered.
    /// </summary>
    public class CleanupData
    {
        internal CleanupData(List<CleanupRow> rows)
        {
            Rows = rows;
            Keys = rows.Select(r => r.PrimaryKey);
            Timestamps = rows.Select(r => r.Timestamp);
        }

        /// <summary>
        /// The keys of the rows that are remaining in the table after being filtered.
        /// </summary>
        public IEnumerable<string> Keys { get; private set; }

        /// <summary>
        /// The timestamps of the rows that need to be referenced when filtering by the age of the rows.
        /// </summary>
        public IEnumerable<DateTime?> Timestamps { get; private set; }

        internal IEnumerable<CleanupRow> Rows { get; private set; }

        internal int TablePid { get; private set; }

        /// <summary>
        /// The builder constructor that builds the table with the data that is filtered.
        /// </summary>
        /// <returns></returns>
        public static TableCleanupDataBuilder Builder()
        {
            return new TableCleanupDataBuilder();
        }

        /// <summary>
        /// The builder class for the filtered tables data that needs to initialize with the timestamps of the data before they can be filtered.
        /// </summary>
        public class TableCleanupDataBuilder
        {
            private string[] keys;
            private DateTime[] timestamps;

            /// <summary>
            /// This method builds the CleanupData that is used with the TableFilters to clean the Table.
            /// </summary>
            /// <param name="protocol">The SLProtocol process that the element is running on.</param>
            /// <param name="tablePid"> The Parameter ID of the table that needs to be cleaned.</param>
            /// <param name="indexColumnIdx">The Index column index position in the table being cleaned.</param>
            /// <param name="timeColumnIdx">The Time column index position in the table being cleaned.</param>
            /// <returns>A CleanupData class that contains data that has yet to be filtered.</returns>
            public CleanupData Build(SLProtocol protocol, int tablePid, int indexColumnIdx, int timeColumnIdx)
            {
                object indexAndTimeColumnIdx = new int[] { indexColumnIdx, timeColumnIdx };
                object[] indexAndTimeColumns = (object[])protocol.NotifyProtocol((int)SLNetMessages.NotifyType.NT_GET_TABLE_COLUMNS, tablePid, indexAndTimeColumnIdx);
                string[] keys = Array.ConvertAll((object[])indexAndTimeColumns[0], Convert.ToString);
                double[] rowAge = Array.ConvertAll((object[])indexAndTimeColumns[1], Convert.ToDouble);
                this.keys = keys.ToArray();
                this.timestamps = rowAge.Select(r => DateTime.FromOADate(r)).ToArray();
                Validate();

                List<CleanupRow> rows = new List<CleanupRow>(this.keys.Length);

                for (int i = 0; i < this.keys.Length; i++)
                {
                    if (timestamps != null)
                    {
                        rows.Add(new CleanupRow()
                        {
                            PrimaryKey = this.keys[i],
                            Timestamp = timestamps[i]
                        });
                    }
                    else
                    {
                        rows.Add(new CleanupRow()
                        {
                            PrimaryKey = this.keys[i],
                            Timestamp = null
                        });
                    }
                }
                CleanupData cleanupData = new CleanupData(rows);
                cleanupData.TablePid = tablePid;
                return cleanupData;
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