using LexTranslator.SkyrimManage;
using LexTranslator.UIManage;
using PhoenixEngine.TranslateManagement;
using PhoenixEngine.TranslateCore;
using PhoenixEngine.TranslateManage;
using PhoenixEngine.EngineManagement;
using LexTranslator.UIManagement;
using PhoenixEngine.DelegateManagement;
using PhoenixEngine.RequestManagement;
using static PhoenixEngine.EngineManagement.DataTransmission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LexTranslator.SkyrimManagement;
using static PhoenixEngine.DelegateManagement.DelegateHelper;
using System.Windows;

namespace LexTranslator.TranslateManage
{
    public class TranslatorExtend
    {
        public static void Init()
        {
            DelegateHelper.SetDataCall += Recv;
            DelegateHelper.SetTranslationUnitCallBack += TranslationUnitStartWorkCall;

            RegListener("PreLog", new List<int>() { 2 }, new Action<int, object>((Sign, Any) =>
            {
                if (Sign == 2)
                {
                    if (Any is PreTranslateCall)
                    {
                        PreTranslateCall GetCall = (PreTranslateCall)Any;

                        UIHelper.NodeCallCallback(0,GetCall.Platform);
                    }
                }
            }));

            RegListener("MainLog", new List<int>() { 0 }, new Action<int, object>((Sign, Any) =>
            {
                if (Sign == 0)
                {
                    if (Any is string)
                    {
                        LogHelper.SetMainLog((string)Any);
                    }
                }
            }));

            RegListener("InputOutputLog", new List<int>() { 3, 5 }, new Action<int, object>((Sign, Any) =>
            {
                if (Sign == 5 || Sign == 3)
                {
                    if (Any is AICall)
                    {
                        AICall GetCall = (AICall)Any;

                        UIHelper.NodeCallCallback(GetCall.CustomID,GetCall.Platform);

                        LogHelper.SetInputLog(GetCall.Platform.ToString() + "->\n" + GetCall.SendString);
                        LogHelper.SetOutputLog(GetCall.Platform.ToString() + "->\n" + GetCall.ReceiveString);

                        DashBoardService.TokenStatistics(GetCall.Platform, GetCall.SendString, GetCall.ReceiveString);
                    }
                    if (Any is PlatformCall)
                    {
                        PlatformCall GetCall = (PlatformCall)Any;

                        UIHelper.NodeCallCallback(GetCall.CustomID,GetCall.Platform);

                        LogHelper.SetInputLog(GetCall.Platform.ToString() + "->\n" + GetCall.SendString);
                        LogHelper.SetOutputLog(GetCall.Platform.ToString() + "->\n" + GetCall.ReceiveString);
                    }
                }
            }));
        }

        /// <summary>
        /// Protect the translation object being entered by the user from being changed
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static bool TranslationUnitStartWorkCall(TranslationUnit Item, int State)
        {
            if (State == 1 || State == 2)
            {
                if (DeFine.WorkingWin != null)
                {
                    if (DeFine.WorkingWin.TransViewList != null)
                    {
                        FakeGrid QueryGrid = DeFine.WorkingWin.TransViewList.KeyToFakeGrid(Item.Key);

                        if (QueryGrid != null)
                        {
                            bool IsCloud = false;
                            QueryGrid.SyncData(ref IsCloud);

                            if (QueryGrid.TransText.Length > 0)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public class RecvListener
        {
            public string Key = "";
            public List<int> ActiveIDs = new List<int>();
            public Action<int, object> Method = null;

            public RecvListener(string Key, List<int> ActiveIDs, Action<int, object> Func)
            {
                this.Key = Key;
                this.ActiveIDs = ActiveIDs;
                this.Method = Func;
            }
        }

        private static ReaderWriterLockSlim ListenersLock = new ReaderWriterLockSlim();
        public static void RemoveListener(string Key)
        {
            ListenersLock.EnterWriteLock();
            try
            {
                for (int i = 0; i < RecvListeners.Count; i++)
                {
                    if (RecvListeners[i].Key.Equals(Key))
                    {
                        RecvListeners.RemoveAt(i);
                        break;
                    }
                }
            }
            finally
            {
                ListenersLock.ExitWriteLock();
            }
        }

        public static void RegListener(string Key, List<int> ActiveIDs, Action<int, object> Action)
        {
            ListenersLock.EnterWriteLock();
            try
            {
                foreach (var Get in RecvListeners)
                {
                    if (Get.Key.Equals(Key))
                    {
                        return;
                    }
                }

                RecvListeners.Add(new RecvListener(Key, ActiveIDs, Action));
            }
            finally
            {
                ListenersLock.ExitWriteLock();
            }
        }

        public static List<RecvListener> RecvListeners = new List<RecvListener>();

        //Null = 0, CacheCall = 1, PreTranslateCall = 2, PlatformCall = 3, AICall = 5
        public static void Recv(int Sign, object Any)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    for (int i = 0; i < RecvListeners.Count; i++)
                    {
                        if (RecvListeners[i].ActiveIDs.Contains(Sign))
                        {
                            RecvListeners[i].Method.Invoke(Sign, Any);
                        }
                    }
                }
                catch { }
            });
        }

        public static void LogCall(string Log)
        {
            if (DeFine.WorkingWin != null)
            {
                DeFine.WorkingWin.Dispatcher.Invoke(new Action(() =>
                {
                    DeFine.WorkingWin.MainLog.Text = Log;
                }));
            }
        }

        public static Dictionary<int, List<TranslatorHistoryCache>> TranslatorHistoryCaches = new Dictionary<int, List<TranslatorHistoryCache>>();

        public static BatchTranslationCore TranslationCore = null;
        public static void ClearTranslatorHistoryCache()
        {
            RowStyleWin.RecordModifyStates.Clear();
            TranslatorHistoryCaches.Clear();
        }

        public static void SetTranslatorHistoryCache(string Key, string Translated, bool IsCloud)
        {
            int GetKey = Key.GetHashCode();

            if (!TranslatorHistoryCaches.ContainsKey(GetKey))
            {
                TranslatorHistoryCaches.Add(GetKey, new List<TranslatorHistoryCache>());
            }

            if (!TranslatorHistoryCaches[GetKey].Any(C => C.Translated == Translated))
            {
                TranslatorHistoryCaches[GetKey].Add(new TranslatorHistoryCache(Translated, IsCloud));
            }
        }

        public static List<TranslatorHistoryCache> GetTranslatorCache(string Key)
        {
            int GetKey = Key.GetHashCode();
            if (TranslatorHistoryCaches.ContainsKey(GetKey))
            {
                return TranslatorHistoryCaches[GetKey];
            }

            return null;
        }

        public static void SetTransBarTittle(string Log)
        {
            if (DeFine.WorkingWin != null)
            {
                DeFine.WorkingWin.Dispatcher.Invoke(new Action(() =>
                {
                    DeFine.WorkingWin.TransProcess.Content = Log;
                }));
            }
        }

        public static bool WaitStopSign()
        {
            while (TranslationStatus == StateControl.Stop)
            {
                Thread.Sleep(1000);

                if (TranslationStatus == StateControl.Cancel)
                {
                    if (TranslationCore != null)
                    {
                        TranslationCore.Close();
                    }

                    return true;
                }
            }

            if (TranslationStatus == StateControl.Cancel)
            {
                if (TranslationCore != null)
                {
                    TranslationCore.Close();
                }

                return true;
            }

            return false;
        }

        public static void MakeReady()
        {
            Phoenix.Config.ProtectedPatterns.Clear();

            foreach (var GetStr in DeFine.GlobalLocalSetting.P_Placeholders.Split(','))
            {
                if (GetStr.Trim().Length > 0)
                {
                    Phoenix.Config.ProtectedPatterns.Add(GetStr);
                }
            }

            ProxyCenter.UsingProxy();
        }

        public static bool PreparingComplete = false;
        public static bool FristInit = false;
        public static Thread PreparingTrd = null;
        public static Thread MarkLeaderTrd = null;
        public static void PreparingTranslationUnits()
        {
            if (!FristInit)
            {
                FristInit = true;
                PreparingComplete = false;
            }
            else
            {
                return;
            }
          
            if (PreparingTrd != null)
            {
                try
                {
                    PreparingTrd.Abort();
                }
                catch { }
                PreparingTrd = null;
            }

            if (MarkLeaderTrd != null)
            {
                try
                {
                    MarkLeaderTrd.Abort();
                } catch { }
                MarkLeaderTrd = null;
            }

            PreparingTrd = new Thread(() =>
            {
                try
                {
                    while (DeFine.WorkingWin.DataLoading == true)
                    {
                        Thread.Sleep(1000);
                    }

                    SetTransBarTittle("Preparing Translation Units...");

                    YDListView GetListView = DeFine.WorkingWin.TransViewList;

                    List<TranslationUnit> TranslationUnits = new List<TranslationUnit>();

                    for (int i = 0; i < GetListView.Rows; i++)
                    {
                        var Row = GetListView.RealLines[i];
                        bool IsCloud = false;
                        Row.SyncData(ref IsCloud);

                        bool HasAddAIMemory = false;

                        if (!HasAddAIMemory && DeFine.GlobalLocalSetting.ForceTranslationConsistency)
                        {
                            if (!string.IsNullOrEmpty(Row.TransText))
                            {
                                Phoenix.AddAIMemory(Row.GetSource(), Row.TransText);
                            }
                        }

                        if (DeFine.GlobalLocalSetting.AutoUpdateStringsFileToDatabase)
                        {
                            if (EspReader.Records.ContainsKey(Row.Key))
                            {
                                var GetRecord = EspReader.Records[Row.Key];

                                if (GetRecord.StringID > 0 && Row.TransText.Length > 0)
                                {
                                    string AutoType = Row.Type;

                                    if (AutoType == "Papyrus" || AutoType == "MCM")
                                    {
                                        AutoType = string.Empty;
                                    }
                                    else
                                    if (AutoType != "NPC_" && AutoType != "WRLD" && AutoType != "CLAS" && AutoType != "ARMO" && AutoType != "AMMO")
                                    {
                                        AutoType = string.Empty;
                                    }

                                    AdvancedDictionaryItem NewItem = new AdvancedDictionaryItem(
                                        string.Empty,//The rule applies to all files.
                                        AutoType,//Automatically determine the type of the current term
                                        Row.GetSource(),//Get the source text corresponding to stringsfile id
                                        Row.TransText,//Get the translation content
                                        Phoenix.From,//Get source language
                                        Phoenix.To,//Get target language
                                        1,//Use full-word matching
                                        0,//Case sensitivity is not ignored
                                        string.Empty
                                        );
                                    if (!AdvancedDictionary.CheckSame(NewItem))
                                    {
                                        AdvancedDictionary.AddItem(NewItem);
                                    }
                                }

                            }
                        }

                        if (Row.TransText.Trim().Length == 0)
                        {
                            bool CanSet = true;

                            if (Row.Type.Equals("BOOK"))
                            {
                                if (Row.Key.EndsWith("DESC") && !DeFine.GlobalLocalSetting.CanTranslateBook)
                                {
                                    if (DelegateHelper.SetDataCall != null)
                                    {
                                        DelegateHelper.SetDataCall(0, "Skip Book fields:" + Row.Key);
                                    }

                                    CanSet = false;
                                }
                            }
                            else
                            if (Row.Score < 5)
                            {
                                if (DelegateHelper.SetDataCall != null)
                                {
                                    DelegateHelper.SetDataCall(0, "Skip Dangerous fields:" + Row.Key);
                                }

                                CanSet = false;
                            }

                            if (DeFine.WorkingWin?.CurrentTransType == 2)
                            {
                                var GetTrans = EspReader.ToStringsFile.QueryData(Row.Key);

                                if (GetTrans != null)
                                {
                                    //Added to context memory. Helps AI improve accuracy.
                                    Phoenix.AddAIMemory(Row.GetSource(), GetTrans.Value);
                                    HasAddAIMemory = true;

                                    if (!Translator.TransData.ContainsKey(Row.Key))
                                    {
                                        Translator.TransData.Add(Row.Key, GetTrans.Value);
                                    }
                                    else
                                    {
                                        Translator.TransData[Row.Key] = GetTrans.Value;
                                    }


                                    var GetFakeGrid = GetListView.KeyToFakeGrid(Row.Key);
                                    if (GetFakeGrid != null)
                                    {
                                        Row.TransText = GetTrans.Value;

                                        Row.SyncUI(GetListView);
                                    }

                                    if (DelegateHelper.SetDataCall != null)
                                    {
                                        DelegateHelper.SetDataCall(0, "Skip StringsFile(" + GetTrans.Type.ToString() + ") fields:" + Row.Key);
                                    }

                                    CanSet = false;
                                }
                                else
                                {
                                    if (EspReader.Records.ContainsKey(Row.Key))
                                    {
                                        if (EspReader.Records[Row.Key].StringID > 0)
                                        {
                                            if (DelegateHelper.SetDataCall != null)
                                            {
                                                DelegateHelper.SetDataCall(0, "Skip StringsFile(" + EspReader.Records[Row.Key].String + ") fields:" + Row.Key);
                                            }

                                            CanSet = false;
                                        }
                                    }
                                }
                            }

                            if (Phoenix.Config.EnableGlobalSearch)
                            {
                                var QueryData = CloudDBCache.MatchOtherCloudItem(-1, (int)Phoenix.To, Row.SourceText);

                                if (QueryData.Count > 0)
                                {
                                    var GetData = QueryData[QueryData.Count - 1];

                                    Phoenix.AddAIMemory(Row.GetSource(), GetData.Result);
                                    HasAddAIMemory = true;

                                    if (!Translator.TransData.ContainsKey(Row.Key))
                                    {
                                        Translator.TransData.Add(Row.Key, GetData.Result);
                                    }
                                    else
                                    {
                                        Translator.TransData[Row.Key] = GetData.Result;
                                    }

                                    var GetFakeGrid = GetListView.KeyToFakeGrid(Row.Key);
                                    if (GetFakeGrid != null)
                                    {
                                        Row.TransText = GetData.Result;

                                        Row.SyncUI(GetListView);
                                    }

                                    if (DelegateHelper.SetDataCall != null)
                                    {
                                        DelegateHelper.SetDataCall(0, $"Database information matched, filename:{UniqueKeyHelper.RowidToOriginalKey(GetData.FileUniqueKey)}, value:{GetData.Result}");
                                    }

                                    CanSet = false;
                                }
                            }

                            if (CanSet)
                            {
                                TranslationUnits.Add(new TranslationUnit(Phoenix.GetFileUniqueKey(),
                                Row.Key, Row.Type, Row.SourceText, Row.TransText, "", Phoenix.From, Phoenix.To, Row.Score));
                            }
                        }
                    }

                    TranslationCore = new BatchTranslationCore(Phoenix.From, Phoenix.To, TranslationUnits);

                    MarkLeaderTrd = new Thread(() =>
                    {
                        TranslationCore.MarkLeaders();
                        MarkLeaderTrd = null;
                    });

                    if (!DeFine.GlobalLocalSetting.EnableAnalyzingWords)
                    {
                        TranslationCore.SkipWordAnalysis = true;

                        MarkLeaderTrd.Start();
                    }
                    else
                    {
                        MarkLeaderTrd.Start();

                        while (TranslationCore.WorkState < 1)
                        {
                            Thread.Sleep(100);

                            SetTransBarTittle("Analyzing Words(" + TranslationCore.MarkLeadersPercent + "%)...");
                        }

                        Thread.Sleep(1000);

                        GetListView.Parent.Dispatcher.Invoke(new Action(() => {
                            for (int i = 0; i < TranslationCore.UnitsLeaderToTranslate.Count; i++)
                            {
                                string GetKey = TranslationCore.UnitsLeaderToTranslate.ElementAt(i).Key;

                                for (int ir = 0; ir < GetListView.VisibleRows.Count; ir++)
                                {
                                    if (RowStyleWin.GetKey(GetListView.VisibleRows[ir].View).Equals(GetKey))
                                    {
                                        RowStyleWin.MarkLeader(GetListView.VisibleRows[ir].View, true);
                                        break;
                                    }
                                }
                            }
                        }));
                    }

                    if (TranslationUnits.Count == 0)
                    {
                        NeedNextPreparing = true;
                    }
                    else
                    {
                        NeedNextPreparing = false;
                    }

                    PreparingComplete = true;

                    PreparingTrd = null;
                }
                catch
                {
                    PreparingComplete = false;
                }
            });

            PreparingTrd.Start();
        }

        public static bool NeedNextPreparing = false;

        public static bool SyncTransStateFreeze = false;

        public static StateControl TranslationStatus = StateControl.Null;

        public static void SyncTransState(Action EndAction, bool IsKeep = false)
        {
            if (SyncTransStateFreeze)
            {
                EndAction.Invoke();
                return;
            }

            if (DeFine.WorkingWin == null)
            {
                TranslationStatus = StateControl.Cancel;
                EndAction.Invoke();
                return;
            }

            if (!PreparingComplete)
            {
                TranslationStatus = StateControl.Cancel;
                EndAction.Invoke();
                return;
            }

            new Thread(() =>
            {
                if (TranslationStatus == StateControl.Run && !IsKeep)
                {
                    if (NeedNextPreparing)
                    {
                        FristInit = false;

                        PreparingTranslationUnits();

                        while (PreparingTrd != null)
                        {
                            Thread.Sleep(100);
                        }

                        if ((TranslationCore.UnitsLeaderToTranslate.Count + TranslationCore.UnitsToTranslate.Count) == 0)
                        {
                            TranslationStatus = StateControl.Cancel;
                            EndAction.Invoke();
                            return;
                        }
                    }

                    Phoenix.SyncTrdCount();

                    YDListView GetListView = DeFine.WorkingWin.TransViewList;

                    if (GetListView != null)
                    {
                        SyncTransStateFreeze = true;

                        MakeReady();                   

                        if (DeFine.GlobalLocalSetting.ForceTranslationConsistency)
                        {
                            SetTransBarTittle("Preparing Consistency...");

                            for (int i = 0; i < GetListView.Rows; i++)
                            {
                                var Row = GetListView.RealLines[i];
                                bool IsCloud = false;
                                Row.SyncData(ref IsCloud);

                                bool HasAddAIMemory = false;

                                if (!HasAddAIMemory && DeFine.GlobalLocalSetting.ForceTranslationConsistency)
                                {
                                    if (!string.IsNullOrEmpty(Row.TransText))
                                    {
                                        Phoenix.AddAIMemory(Row.GetSource(), Row.TransText);
                                    }
                                }
                            }
                        }

                        TranslationCore.Start();

                        SyncTransStateFreeze = false;

                        EndAction.Invoke();

                        int ModifyCount = Phoenix.TranslatedCount;

                        SetTransBarTittle(string.Format("STRINGS({0}/{1})", ModifyCount, GetListView.Rows));

                        Thread.Sleep(1000);

                        if (WaitStopSign())
                        {
                            EndAction.Invoke();
                            return;
                        }

                        bool IsEnd = false;

                        while (!IsEnd)
                        {
                            var GetGrid = TranslationCore.DequeueTranslated(out IsEnd);

                            if (GetGrid != null)
                            {
                                var GetFakeGrid = GetListView.KeyToFakeGrid(GetGrid.Key);
                                if (GetFakeGrid != null)
                                {
                                    GetFakeGrid.TransText = GetGrid.TransText;
                                    GetFakeGrid.SyncUI(GetListView);
                                    SetTranslatorHistoryCache(GetGrid.Key, GetGrid.TransText, true);

                                    Phoenix.TranslatedCount++;
                                    SetTransBarTittle(string.Format("STRINGS({0}/{1})", Phoenix.TranslatedCount, GetListView.Rows));
                                }
                            }

                            Thread.Sleep(20);

                            if (WaitStopSign())
                            {
                                return;
                            }
                        }

                        TranslationStatus = StateControl.Cancel;

                        DeFine.WorkingWin?.UPDateUI();

                        EndAction.Invoke();
                    }
                }
                else
                if (TranslationStatus == StateControl.Stop)
                {
                    SyncTransStateFreeze = true;

                    if (TranslationCore != null)
                    {
                        TranslationCore.Stop();
                    }

                    EndAction.Invoke();

                    SyncTransStateFreeze = false;
                }
                else
                if (TranslationStatus == StateControl.Cancel || TranslationStatus == StateControl.Null)
                {
                    SyncTransStateFreeze = true;

                    if (TranslationCore != null)
                    {
                        try
                        {
                            TranslationCore.Close();
                        }
                        catch { }
                    }

                    EndAction.Invoke();

                    SyncTransStateFreeze = false;
                }
                else
                {
                    SyncTransStateFreeze = true;

                    if (TranslationCore != null)
                    {
                        TranslationCore.Keep();
                    }

                    EndAction.Invoke();

                    SyncTransStateFreeze = false;
                }
            }).Start();
        }

        public static int WriteDictionary()
        {
            int ReplaceCount = 0;
            for (int i = 0; i < DeFine.WorkingWin.TransViewList.Rows; i++)
            {
                FakeGrid GetFakeGrid = DeFine.WorkingWin.TransViewList.RealLines[i];

                string GetKey = GetFakeGrid.Key;
                string GetSourceText = GetFakeGrid.SourceText;
                var TargetText = GetFakeGrid.TransText;

                YDDictionaryHelper.UPDateTransText(GetKey, GetSourceText);
            }

            return ReplaceCount;
        }

        public static void ClearLocalCache(int FileUniqueKey)
        {
            LocalDBCache.DeleteCacheByFileUniqueKey(FileUniqueKey, DeFine.GlobalLocalSetting.TargetLanguage);
            Translator.TransData.Clear();
        }

        public static bool ClearCloudCache(int FileUniqueKey)
        {
            return CloudDBCache.ClearCloudCache(FileUniqueKey);
        }

        public static void Close()
        {
            FristInit = false;
            NeedNextPreparing = false;
            TranslationCore = null;

            if (PreparingTrd != null)
            {
                try
                {
                    PreparingTrd.Abort();
                }
                catch { }
                PreparingTrd = null;
            }

            if (MarkLeaderTrd != null)
            {
                try
                {
                    MarkLeaderTrd.Abort();
                }
                catch { }
                MarkLeaderTrd = null;
            }
        }
    }

    public class TranslatorHistoryCache
    {
        public DateTime ChangeTime;
        public string Translated = "";
        public bool IsCloud = false;

        public TranslatorHistoryCache(string Translated, bool IsCloud)
        {
            this.ChangeTime = DateTime.Now;
            this.Translated = Translated;
            this.IsCloud = IsCloud;
        }
    }
    public enum StateControl
    {
        Null = 0, Run = 1, Stop = 2, Cancel = 3
    }
}