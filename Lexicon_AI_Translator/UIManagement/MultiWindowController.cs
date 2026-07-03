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

                        RecordItem GetRecord = null;

                        if (Mod.EspReader.Records.ContainsKey(SelectKey))
                        {
                            GetRecord = Mod.EspReader.Records[SelectKey];
                        }

                        if (GetRecord != null)
                        {
                            //Find NPC
                            if (Mod.EspReader.GameCharacters.ContainsKey(SelectKey))
                            {
                                if (Mod.EspReader.GameCharacters[SelectKey].Count > 1)
                                    TrackingWin.LoadNpcRecord(GetRecord,
                                        Mod.EspReader.GameCharacters[SelectKey][0].Name,
                                           Mod.EspReader.GameCharacters[SelectKey][0].Gender.ToString());
                            }
                            else
                            {
                                TrackingWin.NpcListPanel.Children.Clear();
                            }

                            //SearchDialogue
                            var DialogueLink = Mod.EspReader.GetDialContext(Mod.EspReader.Records[SelectKey].RealFormID);

                            if (DialogueLink != null)
                            {
                                List<DialogueRecordItem> Dialogues = new List<DialogueRecordItem>();

                                if (DialogueLink.Head != null)
                                {
                                    TrackingWin.LoadDialogueRecords(DialogueLink.Head);
                                }

                               
                             
                            }
                            else
                            {
                                TrackingWin.DialogueListPanel.Children.Clear();
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
