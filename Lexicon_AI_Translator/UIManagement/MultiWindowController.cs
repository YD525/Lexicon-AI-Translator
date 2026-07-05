using System.Collections.Generic;
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

        private static void OpenCodeWin(ModFile Mod, LexGui Win)
        {
            if (CodeWin == null)
            {
                CodeWin = new CodeView(Mod, Win);
                CodeWin.Show();
            }
            else
            {
                CodeWin.ModRef = Mod;
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

        private static void OpenTrackingWin(ModFile Mod, LexGui Win)
        {
            if (TrackingWin == null)
            {
                TrackingWin = new RecordTracking(Mod, Win);
                TrackingWin.Show();
            }
            else
            {
                TrackingWin.ModRef = Mod;
                TrackingWin.Owner = Win;
            }
        }


        public static void AttachMod(string SelectKey, LexGui CurrentWin, ModFile Mod)
        {
            CurrentWin.UI(() =>
            {
                switch (Mod.Type)
                {
                        case GameFileType.ESP:
                        {
                            CloseCodeWin();
                            OpenTrackingWin(Mod, CurrentWin);

                            try
                            {
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
                                        if (Mod.EspReader.GameCharacters[SelectKey].Count > 0)
                                            TrackingWin.LoadNpcRecord(GetRecord,
                                                Mod.EspReader.GameCharacters[SelectKey][0].Name,
                                                   Mod.EspReader.GameCharacters[SelectKey][0].Gender.ToString());
                                    }
                                    else
                                    {
                                        TrackingWin.NpcListPanel.Children.Clear();
                                    }

                                    var DialogueLink = Mod.EspReader.GetDialContext(0, Mod.EspReader.Records[SelectKey].ParentIndex, Mod.EspReader.Records[SelectKey].SubIndex);

                                    if (DialogueLink != null)
                                    {
                                        List<ManagedDialNode> TempLinks = new List<ManagedDialNode>();

                                        //Recreate the array and put the title first.

                                        if (DialogueLink.Head != null)
                                        {
                                            TempLinks.Add(DialogueLink.Head);
                                        }

                                        if (DialogueLink.Links != null)
                                        {
                                            TempLinks.AddRange(DialogueLink.Links);
                                        }

                                        if (TempLinks.Count > 0)
                                        {
                                            TrackingWin.LoadDialogueRecords(Mod, TempLinks);
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

                                    for (int i = 0; i < Mod.ListView.RealLines.Count; i++)
                                    {
                                        if (Mod.ListView.RealLines[i].Key != SelectKey)
                                        {
                                            if (Mod.ListView.RealLines[i].RealSource.Contains(GetRecord.String) ||
                                                Mod.ListView.RealLines[i].SourceText.Contains(GetRecord.String)
                                            )
                                            {
                                                Records.Add(Mod.EspReader.Records[Mod.ListView.RealLines[i].Key]);
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


                                    TrackingWin.UpdateAllSectionHeights();

                                    CurrentWin.Focus();
                                }
                            }
                            catch { }
                        }
                        break;
                    case GameFileType.PEX:
                        {
                            CloseTrackingWin();
                            OpenCodeWin(Mod, CurrentWin);
                        }
                        break;
                }
            });
        }

        public static void CloseMod(ModFile Mod)
        {
            if (TrackingWin != null)
            {
                if (TrackingWin.ModRef.Path == Mod.Path)
                {
                    TrackingWin.ModRef = null;
                    TrackingWin.Close();

                    TrackingWin = null;
                }
            }
            if (CodeWin != null)
            {
                if (CodeWin.ModRef.Path == Mod.Path)
                {
                    CodeWin.ModRef = null;
                    CodeWin.Close();

                    CodeWin = null;
                }
            }
        }

        public static void HideAll()
        {
            if (TrackingWin != null)
            {
                TrackingWin.Hide();
            }
            if (CodeWin != null)
            {
                CodeWin.Hide();
            }
        }

        public static void ShowAll()
        {
            if (TrackingWin != null)
            {
                TrackingWin.Show();
            }
            if (CodeWin != null)
            {
                CodeWin.Show();
            }
        }
    }
}
