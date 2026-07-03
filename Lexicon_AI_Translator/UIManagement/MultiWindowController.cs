using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LexTranslator.SkyrimManagement;

namespace LexTranslator.UIManagement
{
    public class MultiWindowController
    {
        public RecordTracking TrackingWin = null;

        public void AttachMod(Window CurrentWin,ModFile Mod)
        {
            switch (Mod.Type)
            {
                case GameFileType.ESP:
                    {
                        if (TrackingWin == null)
                        {
                            TrackingWin = new RecordTracking(CurrentWin);
                        }
                    }
                break;
                case GameFileType.PEX:
                    { 
                    }
                break;
            }
        
        }
    }
}
