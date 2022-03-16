using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCSProtsTester
{
    class LinearScaleCalculator
    {
        private readonly double _k;
        private readonly double _b;
        private readonly double _x1;
        private readonly double _x2;
        private readonly double _y1;
        private readonly double _y2;
        public LinearScaleCalculator(double x1, double y1, double x2, double y2)
        {
            _x1 = x1;
            _x2 = x2;
            _y1 = y1;
            _y2 = y2;

            _k = (y2 - y1) / (x2 - x1);
            _b = (x2 * y1 - x1 * y2) / (x2 - x1);
        }

        public double GetYValueByX(double x)
        {
            return _k * x + _b;
        }

        public double GetXValueByY(double y)
        {
            return (y - _b) / _k;
        }

        /// <summary>
        /// Поиск такого х1, чтобы Х полученный по известному значению Y был максимально к нему близкий
        /// </summary>
        /// <param name="y">Известное Y</param>
        /// <param name="searchX">Искомый Х</param>
        /// <returns>Найденный X1 или double.NaN если найти не удалось</returns>
        public double FindX1ByYValue(double y, double searchX)
        {
            var x1 = _x1;
            var x2 = _x2;
            while (x1 < x2)
            {
                // создаем новое уравнение прямой
                var linearCalc = new LinearScaleCalculator(x1, _y1, x2, _y2);
                var x = linearCalc.GetXValueByY(y);
                if (Math.Abs(x - searchX) <= 0.01)
                    return x1;
                x1 += 0.01;
            }

            return double.NaN;
        }

        public double GetLowEng(double searchValue, double currValue, double highEng)
        {
            return (searchValue - currValue) / (1 - ((currValue) / highEng));
        }
    }
}
