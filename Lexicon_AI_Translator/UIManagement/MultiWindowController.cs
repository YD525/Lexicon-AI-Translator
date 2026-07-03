using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LexTranslator.SkyrimManagement;

namespace LexTranslator.UIManagement
{
    public class MultiWindowController
    {
        public RecordTracking TrackingWin = null;

        public void AttachMod(string SelectKey,Window CurrentWin,ModFile Mod)
        {
            switch (Mod.Type)
            {
                case GameFileType.ESP:
                    {
                        if (TrackingWin == null)
                        {
                            TrackingWin = new RecordTracking(CurrentWin);
                            
                        }
                        else
                        {
                            TrackingWin.Owner = CurrentWin;
                        }

                        if (Mod.EspReader.Records.ContainsKey(SelectKey))
                        {
                            var GetRecord = Mod.EspReader.Records[SelectKey];

                            //Find NPC
                            if (Mod.EspReader.GameCharacters.ContainsKey(SelectKey))
                            {
                                if (Mod.EspReader.GameCharacters[SelectKey].Count>1)
                                TrackingWin.LoadNpcRecord(GetRecord,
                                    Mod.EspReader.GameCharacters[SelectKey][0].Name,
                                       Mod.EspReader.GameCharacters[SelectKey][0].Gender.ToString());
                            }
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
