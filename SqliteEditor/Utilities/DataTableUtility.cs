using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteEditor.Utilities
{
    public static class DataTableUtility
    {
        public static bool AreTablesEqual(DataTable dt1, DataTable dt2)
        {
            if (dt1 == null && dt2 != null) { return false; }
            if (dt1 != null && dt2 == null) { return false; }
            if (dt1 == null && dt2 == null) { return true; }
            if (dt1!.Rows.Count != dt2!.Rows.Count) { return false; }

            for (int rowIndex = 0; rowIndex < dt2.Rows.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < dt2.Columns.Count; colIndex++)
                {
                    if (!dt1.Rows[rowIndex][colIndex].Equals(
                        dt2.Rows[rowIndex][colIndex]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
