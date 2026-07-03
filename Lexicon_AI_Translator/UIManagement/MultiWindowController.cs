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
                                List<ManagedDialNode> Dialogues = new List<ManagedDialNode>();

                                if (DialogueLink.Head != null)
                                {
                                    Dialogues.AddRange(DialogueLink.Links);
                                }

                                TrackingWin.LoadDialogueRecords(Dialogues);
                            }
                            else
                            {
                                TrackingWin.DialogueListPanel.Children.Clear();
                            }

                            //MatchRelated
                            List<RecordItem> Records = new List<RecordItem>();

                            Records.Add(GetRecord);

                            for (int i = 0; i < Mod.TranslateView.RealLines.Count; i++)
                            {
                                if (Mod.TranslateView.RealLines[i].Key != SelectKey)
                                {
                                    if (Mod.TranslateView.RealLines[i].RealSource.Contains(GetRecord.String)||
                                        Mod.TranslateView.RealLines[i].SourceText.Contains(GetRecord.String)
                                    )
                                    {
                                        Records.Add(Mod.EspReader.Records[Mod.TranslateView.RealLines[i].Key]);
                                    }
                                }
                            }

                            if (Records.Count > 0)
                            {
                                TrackingWin.LoadRelatedTextRecords(Records);
                            }
                            else
                            {
                                TrackingWin.RelatedTextListPanel.Children.Clear();
                            }
                             
                        }
                       
                    }
                break;
                case GameFileType.PEX:
                    {
                        if (TrackingWin != null)
                        {
                            TrackingWin.Close();
                            TrackingWin = null;
                        }
                    }
                break;
            }
        
        }
    }
}
