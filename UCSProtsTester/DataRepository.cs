using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCSProtsTester
{
    class DataRepository
    {
        private readonly ExportService _exportService;
        private readonly BlockingCollection<LogicalPressurePairReport> _reports = new BlockingCollection<LogicalPressurePairReport>();
        private readonly BlockingCollection<CriticalErrorWriteTag> _criticalErrorWriteTags = new BlockingCollection<CriticalErrorWriteTag>();
        public DataRepository(ExportService exportService)
        {
            _exportService = exportService;
            Task.Run(RunReports);
            Task.Run(RunTags);
        }

        private void RunReports()
        {
            foreach (var report in _reports.GetConsumingEnumerable())
                _exportService.ExportReport(report);
        }

        private void RunTags()
        {
            foreach (var tag in _criticalErrorWriteTags.GetConsumingEnumerable())
                _exportService.ExportCriticalTag(tag);
        }

        public void Add(LogicalPressurePairReport report)
        {
            _reports.Add(report);
        }

        public void Add(CriticalErrorWriteTag tag)
        {
            _criticalErrorWriteTags.Add(tag);
        }
    }
}
