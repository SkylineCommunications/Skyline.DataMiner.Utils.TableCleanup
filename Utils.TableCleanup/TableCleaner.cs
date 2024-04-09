using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.TableCleanup.Interfaces;
using SLNetMessages = Skyline.DataMiner.Net.Messages;

namespace Skyline.DataMiner.Utils.TableCleanup
{
    /// <summary>
    /// The class containing the row data that needs to be cleaned and filtered.
    /// </summary>
    public class TableCleaner
    {
        /// <summary>
        /// This constructor should be used if you want to cleanup on all the options.
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="tablePid"></param>
        /// <param name="indexColumnIdx"></param>
        /// <param name="timeColumnIdx"></param>
        public TableCleaner(SLProtocol protocol, int tablePid, int indexColumnIdx, int? timeColumnIdx)
        {
            _Protocol = protocol;
            Filters = new List<ICondition>();
            Rows = new List<CleanupRow>();
            TablePid = tablePid;
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
                object indexAndTimeColumnIdx = new uint[] { Convert.ToUInt32(indexColumnIdx), Convert.ToUInt32(timeColumnIdx) };
                object[] indexAndTimeColumns = (object[])protocol.NotifyProtocol((int)SLNetMessages.NotifyType.NT_GET_TABLE_COLUMNS, tablePid, indexAndTimeColumnIdx);
                object[] keys = (object[])indexAndTimeColumns[0];
                object[] datetime = (object[])indexAndTimeColumns[1];
                Validate(keys, datetime);
                for (int i = 0; i < keys.Length; i++)
                {
                    CleanupRow row = new CleanupRow();
                    row.PrimaryKey = Convert.ToString(keys[i]);
                    row.Timestamp = DateTime.FromOADate(Convert.ToDouble(datetime[i]));
                    Rows.Add(row);
                }
            }
        }

        internal List<CleanupRow> Rows { get; set; }

        internal int TablePid { get; private set; }

        internal List<ICondition> Filters { get; set; }

        private SLProtocol _Protocol {  get; set; }

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
        /// <returns>The tablecleaner object.</returns>
        public TableCleaner WithCondition(ICondition filter)
        {
            Filters.Add(filter);
            return this;
        }

        /// <summary>
        /// The cleanup method will execute all the conditions that where added.
        /// </summary>
        /// <returns>The tablecleaner object.</returns>
        public TableCleaner Cleanup()
        {
            List<string> keysToDelete = new List<string>();
            foreach (var filter in Filters)
            {
                filter.Execute(_Protocol, Rows);
                keysToDelete.AddRange(filter.RemovedPrimaryKeys);
            }

            if (keysToDelete.Count > 0)
            {
                _Protocol.DeleteRow(TablePid, keysToDelete.ToArray());
            }

            return this;
        }
    }
}