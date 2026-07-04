using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using LexTranslator.SkyrimManagement;

namespace LexTranslator.UIManagement
{
    public class MultiWindowController
    {
        public static RecordTracking TrackingWin = null;
        public static CodeView CodeWin = null;
        private static void CloseCodeWin()
        {
            if (CodeWin != null)
            {
                CodeWin.Close();
                CodeWin = null;
            }
        }

        private static void OpenCodeWin(LexGui Win)
        {
            if (CodeWin == null)
            {
                CodeWin = new CodeView(Win);
                CodeWin.Show();
            }
            else
            {
                CodeWin.Owner = Win;
            }
        }

        private static void CloseTrackingWin()
        {
            if (TrackingWin != null)
            {
                TrackingWin.Close();
                TrackingWin = null;
            }
        }

        private static void OpenTrackingWin(LexGui Win)
        {
            if (TrackingWin == null)
            {
                TrackingWin = new RecordTracking(Win);
                TrackingWin.Show();
            }
            else
            {
                TrackingWin.Owner = Win;
            }
        }

      
        public static void AttachMod(string SelectKey,LexGui CurrentWin,ModFile Mod)
        {
            CurrentWin.UI(() =>
            {
                switch (Mod.Type)
                {
                    case GameFileType.ESP:
                        {
                            CloseCodeWin();
                            OpenTrackingWin(CurrentWin);

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

                                var DialogueLink = Mod.EspReader.GetDialContext(0,Mod.EspReader.Records[SelectKey].ParentIndex, Mod.EspReader.Records[SelectKey].SubIndex);

                                if (DialogueLink != null)
                                {
                                    if (DialogueLink.Links != null)
                                    {
                                        TrackingWin.LoadDialogueRecords(Mod,DialogueLink.Links);
                                    }
                                    else
                                    {
                                        TrackingWin.DialogueListPanel.Children.Clear();
                                    }
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
                                        if (Mod.TranslateView.RealLines[i].RealSource.Contains(GetRecord.String) ||
                                            Mod.TranslateView.RealLines[i].SourceText.Contains(GetRecord.String)
                                        )
                                        {
                                            Records.Add(Mod.EspReader.Records[Mod.TranslateView.RealLines[i].Key]);
                                        }
                                    }
                                }

                                if (Records.Count > 1)
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
                            CloseTrackingWin();
                            OpenCodeWin(CurrentWin);
                        }
                        break;
                }
            });
        }
    }
}
