using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexTranslator.TranslateManage;

namespace LexTranslator.UIManagement
{
    public class ChartData
    {
        public bool Paused = false;
        private double Current = 0;
        public long Total = 0;

        private long SingleUseLimit = 0;
        public double SetCurrent(long Current)
        {
            if (!Paused)
            {
                this.Current = Current;
                Total += Current;
                CheckLimit();
                return this.Current;
            }

            return 0;
        }

        public void ReSet()
        {
            this.Current = this.Total = 0;
        }

        public void CheckLimit()
        {
            if(this.SingleUseLimit != 0)
            if (this.Total > this.SingleUseLimit)
            {
                //If the token limit is exceeded, the fuse will trip, forcibly terminating the translation process.
                TranslatorInterface.TranslationStatus = StateControl.Cancel;

                TranslatorInterface.SyncTransState(new Action(() =>
                {
                    DeFine.WorkingWin.Dispatcher.Invoke(new Action(() =>
                    {
                        DeFine.WorkingWin.SyncTransStateUI();
                    }));
                }), false);
            }
        }

        public void SetTokenUseLimit(long MaxToken)
        { 
            this.SingleUseLimit = MaxToken;
        }
    }
}
