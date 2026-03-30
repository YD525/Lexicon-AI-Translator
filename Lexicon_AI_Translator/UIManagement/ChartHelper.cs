using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexTranslator.UIManagement
{
    public class ChartData
    {
        public bool Paused = false;
        private double Current = 0;
        public long Total = 0;

        public double SetCurrent(long Current)
        {
            if (!Paused)
            {
                this.Current = Current;
                Total += Current;
                return this.Current;
            }

            return 0;
        }

        public void ReSet()
        {
            this.Current = this.Total = 0;
        }
    }
}
