using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCSProtsTester
{
    /// <summary>
    /// Класс описывающий результат проверки текущей пары давлений
    /// </summary>
    class PressurePairReport
    {
        public string LeftPointName { get; set; }
        public string RightPointName { get; set; }

        public PressurePointState LeftPointState { get; set; }
        public PressurePointState RightPointState { get; set; }

        public bool Result { get; set; }
        public string Message { get; set; }

        public DateTime TestTime { get; set; }

        public List<string> GetStringRepresentation()
        {
            var list = new List<string>();

            string indent = "\t";
            list.Add($"{indent}>>>>> Результат проверки комбинации для пары");
            list.Add($"{indent}Время проверки: {TestTime:yyyy-MM-dd HH:mm:ss}");
            list.Add($"{indent}Результат проверки: {Result}");
            if (!string.IsNullOrEmpty(Message))
                list.Add($"{indent}Сообщение: {Message}");
            list.Add($"{indent}Левая точка давления: {LeftPointName}");
            list.Add($"{indent}Состояние левой точки давления: {LeftPointState}");
            list.Add($"{indent}Правая точка давления: {RightPointName}");
            list.Add($"{indent}Состояние правой точки давления: {RightPointState}");
            list.Add($"{indent}<<<<<<");
            return list;
        }
    }
}
