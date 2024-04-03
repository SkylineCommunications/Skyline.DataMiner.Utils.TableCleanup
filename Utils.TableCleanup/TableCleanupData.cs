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
    public class TableCleanupData
    {
        internal TableCleanupData(List<CleanupRow> rows)
        {
            Rows = rows;
            Keys = rows.Select(r => r.PrimaryKey).ToList();
            Timestamps = rows.Select(r => r.Timestamp).ToList();
        }

        /// <summary>
        /// This constructor can be used if you only want to cleanup on row count.
        /// </summary>
        /// <param name="protocol">This is SLProtocol which is used to manipulate table data.</param>
        /// <param name="tablePid">This is the table that will be manipulated.</param>
        public TableCleanupData(SLProtocol protocol, int tablePid)
        {
            TablePid = tablePid;
            Keys = protocol.GetKeys(tablePid).ToList();
            ValidateKeys();
            for (int i = 0; i < Keys.Count; i++)
            {
                Rows.Add(new CleanupRow()
                {
                    PrimaryKey = Keys[i],
                    Timestamp = null
                });
            }
        }

        /// <summary>
        /// This constructor should be used if you want to cleanup on row age, row count and/or row age.
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="tablePid"></param>
        /// <param name="indexColumnIdx"></param>
        /// <param name="timeColumnIdx"></param>
        public TableCleanupData(SLProtocol protocol, int tablePid, int indexColumnIdx, int timeColumnIdx)
        {
            TablePid = tablePid;
            object indexAndTimeColumnIdx = new int[] { indexColumnIdx, timeColumnIdx };
            object[] indexAndTimeColumns = (object[])protocol.NotifyProtocol((int)SLNetMessages.NotifyType.NT_GET_TABLE_COLUMNS, tablePid, indexAndTimeColumnIdx);
            string[] keys = Array.ConvertAll((object[])indexAndTimeColumns[0], Convert.ToString);
            double?[] rowAge = Array.ConvertAll((object[])indexAndTimeColumns[1], x => ConvertObjectToNullableDouble(x));
            Keys = keys.ToList();
            Timestamps = rowAge.Select(r => ConvertNullableDoubleToNullableDateTime(r)).ToList();
            Validate();
            for (int i = 0; i < Keys.Count; i++)
            {
                if (Timestamps != null)
                {
                    Rows.Add(new CleanupRow()
                    {
                        PrimaryKey = Keys[i],
                        Timestamp = Timestamps[i]
                    });
                }
                else
                {
                    Rows.Add(new CleanupRow()
                    {
                        PrimaryKey = Keys[i],
                        Timestamp = null
                    });
                }
            }
        }

        /// <summary>
        /// The keys of the rows that are remaining in the table after being filtered.
        /// </summary>
        public List<string> Keys { get; private set; }

        /// <summary>
        /// The timestamps of the rows that need to be referenced when filtering by the age of the rows.
        /// </summary>
        public List<DateTime?> Timestamps { get; private set; }

        internal List<CleanupRow> Rows { get; private set; }

        internal int TablePid { get; private set; }

        private static double? ConvertObjectToNullableDouble(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return null; // Return null if the object is null or DBNull
            }
            else
            {
                // Use Convert.ToDouble() to convert the object to a double
                return Convert.ToDouble(obj);
            }
        }

        private static DateTime? ConvertNullableDoubleToNullableDateTime(double? nullableDouble)
        {
            if (nullableDouble.HasValue)
            {
                // Convert the double value to a DateTime using DateTime.FromOADate
                return DateTime.FromOADate(nullableDouble.Value);
            }
            else
            {
                return null; // Return null if the input is null
            }
        }

        private void Validate()
        {
            if (Keys == null)
            {
                throw new InvalidOperationException("No primary keys were provided.");
            }

            if (Timestamps != null && Timestamps.Count != Keys.Count)
            {
                throw new InvalidOperationException("The number of primary keys does not match the number of timestamps.");
            }
        }

        private void ValidateKeys()
        {
            if (Keys == null)
            {
                throw new InvalidOperationException("No primary keys were provided.");
            }
        }
    }
}