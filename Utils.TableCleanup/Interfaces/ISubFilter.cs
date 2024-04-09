using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Skyline.DataMiner.Utils.TableCleanup.Interfaces
{
    internal interface ISubFilter
    {
        /// <summary>
        /// Contains the primary keys that are removed by the cleaning and filtering methods from the table.
        /// </summary>
        ReadOnlyCollection<string> RemovedPrimaryKeys { get; }

        void Execute(List<CleanupRow> rows);
    }
}
