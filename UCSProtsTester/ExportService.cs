using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCSProtsTester
{
    class ExportService
    {
        public void ExportReport(LogicalPressurePairReport report)
        {
            using (FileStream fileStream = new FileStream("_Sivkox_.txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter exportedFile = new StreamWriter(fileStream))
                {
                    foreach (var line in report.GetStringRepresentation())
                        exportedFile.WriteLine(line);
                }
            }
        }

        public void ExportCriticalTag(CriticalErrorWriteTag criticalErrorWriteTag)
        {
            using (FileStream fileStream = new FileStream("_Sivkox_FIX_PLS.txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter exportedFile = new StreamWriter(fileStream))
                {
                    exportedFile.WriteLine(criticalErrorWriteTag.ToString());
                }
            }
        }
    }
}
