using System;
using System.Collections.Generic;

using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.Protocol.Extension;
using Skyline.DataMiner.Utils.TableCleanup.Interfaces;

namespace Skyline.DataMiner.Utils.TableCleanup
{
    /// <summary>
    /// The class containing the row data that needs to be cleaned and filtered.
    /// </summary>
    public class TableCleaner
    {
        private readonly SLProtocol protocol;

        /// <summary>
        /// This constructor should be used if you want to cleanup on all the options.
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="tablePid"></param>
        /// <param name="indexColumnIdx"></param>
        /// <param name="timeColumnIdx"></param>
        public TableCleaner(SLProtocol protocol, int tablePid, int indexColumnIdx, int? timeColumnIdx)
        {
            this.protocol = protocol;
            TablePid = tablePid;

            Filters = new List<ICondition>();

            Rows = new List<CleanupRow>();
            if (timeColumnIdx == null)
            {
                string[] keys = protocol.GetKeys(tablePid);
                for (int i = 0; i < keys.Length; i++)
                {
                    Rows.Add(new CleanupRow()
                    {
                        PrimaryKey = Convert.ToString(keys[i]),
                        Timestamp = null
                    });
                }
            }
            else
            {
                uint[] columnsToGetIdx = new uint[] { Convert.ToUInt32(indexColumnIdx), Convert.ToUInt32(timeColumnIdx) };
                object[] columnsData = protocol.GetColumns(tablePid, columnsToGetIdx);
                object[] keys = (object[])columnsData[0];
                object[] datetimes = (object[])columnsData[1];

                Validate(keys, datetimes);
                for (int i = 0; i < keys.Length; i++)
                {
                    var row = new CleanupRow
                    {
                        PrimaryKey = Convert.ToString(keys[i]),
                        Timestamp = DateTime.FromOADate(Convert.ToDouble(datetimes[i]))
                    };
                    Rows.Add(row);
                }
            }
        }

        internal List<CleanupRow> Rows { get; set; }

        internal int TablePid { get; private set; }

        internal List<ICondition> Filters { get; set; }

        private void Validate(object[] keys, object[] datetimes)
        {
            if (keys == null)
            {
                throw new InvalidOperationException("No primary keys were provided.");
            }

            if (datetimes != null && datetimes.Length != keys.Length)
            {
                throw new InvalidOperationException("The number of primary keys does not match the number of timestamps.");
            }
        }

        /// <summary>
        /// Add a condition that has to be applied on the rows.
        /// </summary>
        /// <param name="filter">The filter that will be applied.</param>
        /// <returns>The TableCleaner object.</returns>
        public TableCleaner WithCondition(ICondition filter)
        {
            Filters.Add(filter);
            return this;
        }

        /// <summary>
        /// The cleanup method will execute all the conditions that where added.
        /// </summary>
        /// <returns>The TableCleaner object.</returns>
        public TableCleaner Cleanup()
        {
            var keysToDelete = new List<string>();
            foreach (var filter in Filters)
            {
                filter.Execute(protocol, Rows);
                keysToDelete.AddRange(filter.RemovedPrimaryKeys);
            }

            if (keysToDelete.Count > 0)
            {
                protocol.DeleteRow(TablePid, keysToDelete.ToArray());
            }

            return this;
        }
    }
}