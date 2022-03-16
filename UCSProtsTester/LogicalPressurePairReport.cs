using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCSProtsTester
{
    class LogicalPressurePairReport
    {
        public DateTime TestTime { get; set; }
        public string Name { get; set; }
        public bool Result { get; set; }
        public string Message { get; set; }

        private List<PressurePairReport> _pairsReports { get; }

        public LogicalPressurePairReport()
        {
            _pairsReports = new List<PressurePairReport>();
            TestTime = DateTime.Now;
        }

        public void AddPairsReport(PressurePoint leftPoint, PressurePoint rightPoint, 
                                   PressurePointState leftPointState, PressurePointState rightPointState, 
                                   bool result, string message = "")
        {
            _pairsReports.Add(new PressurePairReport()
            {
                LeftPointName = leftPoint.Name,
                RightPointName = rightPoint.Name,
                LeftPointState = leftPointState,
                RightPointState = rightPointState,
                Result = result,
                Message = message,
                TestTime = DateTime.Now
            });

            if (!result)
                Result = result;
        }

        public IEnumerable<string> GetStringRepresentation()
        {
            var list = new List<string>
            {
                $"=====================================================",
                $"Проверяемая логическая пара: {Name}",
                $"Время начала проверки: {TestTime:yyyy-MM-dd HH:mm:ss}",
                $"Результат проверки: {Result}"
            };

            if (!string.IsNullOrEmpty(Message))
                list.Add($"Сообщение: {Result}");

            foreach (var pairReport in _pairsReports)
            {
                list.AddRange(pairReport.GetStringRepresentation());
                list.Add("\n");
            }
            list.Add($"=====================================================\n");

            return list;
        }
    }
}
