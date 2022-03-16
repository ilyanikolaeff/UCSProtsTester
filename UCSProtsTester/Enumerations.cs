using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCSProtsTester
{
    enum PressurePointState
    {
        // Верхний аварийный уровень
        HiHi = 1,
        // Верхний предельный уровень
        Hi = 2,
        // Недостоверность
        Undef = 3,
        // Отсутствует
        None = 0
    }

    enum MeasurementChannelType
    {
        TMA,
        HART
    }
}
