using System;
using System.Collections.Generic;
using System.Text;

namespace ExpireBlobFunction.Utils
{

    public static class ChronologicalTime
    {
        /// <summary>
        /// Returns a value that can be used for Azure table storage (Partition- or Row Key)
        /// that stores the records in reverse chronological order to get the latest entries on top
        /// </summary>
        public static string ReverseChronologicalValue
        {
            get
            {
                var s = GetReverseChronologicalValue(DateTime.UtcNow);
                return s;
            }
        }

        /// <summary>
        /// Returns a value that can be used for Azure table storage (Partition- or Row Key)
        /// that stores the records in chronological order to get the oldest entries on top
        /// </summary>
        public static string ChronologicalValue
        {
            get
            {
                var s = GetChronologicalValue(DateTime.UtcNow);
                return s;
            }
        }

        public static string GetReverseChronologicalValue(DateTime dateTime)
        {
            var s = $"{DateTime.MaxValue.Ticks - dateTime.Ticks:D19}";
            return s;
        }

        public static string GetChronologicalValue(DateTime dateTime)
        {
            var s = $"{dateTime.Ticks:D19}";
            return s;
        }

        public static DateTime GetTimeFromChronologicalValue(long ticks)
        {
            var dt = new DateTime(ticks);
            return dt;
        }

        public static DateTime GetTimeFromReverseChronologicalValue(long ticks)
        {
            var dt = new DateTime(DateTime.MaxValue.Ticks - ticks);
            return dt;
        }
    }
}
