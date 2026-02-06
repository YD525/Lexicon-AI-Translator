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
using PhoenixEngine.EngineManagement.Unit;

namespace LexTranslator.TranslateManage
{
    public class TranslatorInterface
    {
        public static Translator Instance = null;

        public static void Init()
        {
            Instance = new Translator(Phoenix.From,Phoenix.To,true);

            DelegateHelper.SetDataCall += Recv;
            DelegateHelper.SetTranslationUnitCallBack += TranslationUnitStartWorkCall;

            RegListener("PreLog", new List<int>() { 2 }, new Action<int, object>((Sign, Any) =>
            {
                if (Sign == 2)
                {
                    if (Any is PreTranslateCall)
                    {
                        PreTranslateCall GetCall = (PreTranslateCall)Any;

                        UIHelper.NodeCallCallback(0, GetCall.Platform);
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

                        UIHelper.NodeCallCallback(GetCall.CustomID, GetCall.Platform);

                        LogHelper.SetInputLog(GetCall.Platform.ToString() + "->\n" + GetCall.SendString);
                        LogHelper.SetOutputLog(GetCall.Platform.ToString() + "->\n" + GetCall.ReceiveString);

                        DashBoardService.TokenStatistics(GetCall.Platform, GetCall.SendString, GetCall.ReceiveString);
                    }
                    if (Any is PlatformCall)
                    {
                        PlatformCall GetCall = (PlatformCall)Any;

                        UIHelper.NodeCallCallback(GetCall.CustomID, GetCall.Platform);

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
        public static bool TranslationUnitStartWorkCall(UnitGroup Item, int State)
        {
            if (State == 1 || State == 2)
            {
                if (DeFine.WorkingWin != null)
                {
                    if (DeFine.WorkingWin.TransViewList != null)
                    {
                        for (int i = 0; i < Item.Units.Count; i++)
                        {
                            var GetUnit = Item.Units[i];
                            FakeGrid QueryGrid = DeFine.WorkingWin.TransViewList.KeyToFakeGrid(GetUnit.Key);

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
                    if (Instance != null)
                    {
                        Instance.GetBatchCore()?.Cancel();
                    }

                    return true;
                }
            }

            if (TranslationStatus == StateControl.Cancel)
            {
                if (Instance != null)
                {
                    Instance.GetBatchCore()?.Cancel();
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
        public static Thread InitTrd = null;
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

            if (InitTrd != null)
            {
                try
                {
                    InitTrd.Abort();
                }
                catch { }
                InitTrd = null;
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

                    List<BaseUnit> BaseUnits = new List<BaseUnit>();

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

                                    if (Instance.TranslatedLink.ContainsKey(Row.Key))
                                    {
                                        Instance.TranslatedLink[Row.Key] = GetTrans.Value;
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

                                    if (Instance.TranslatedLink.ContainsKey(Row.Key))
                                    {
                                        Instance.TranslatedLink[Row.Key] = GetData.Result;
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
                                if (Instance.TranslatedLink.ContainsKey(Row.Key))
                                {
                                    if (Instance.TranslatedLink[Row.Key].Length > 0)
                                    {
                                        CanSet = false;
                                    }
                                }

                                if (CanSet)
                                {
                                    BaseUnits.Add(new BaseUnit(Phoenix.GetFileUniqueKey(),
                                   Row.Key, Row.Type, Row.SourceText, Row.TransText, Row.Score));
                                }
                            }
                        }
                    }

                    InitTrd = new Thread(() =>
                    {
                        Instance.ReInit();
                        Instance.Init(BaseUnits, AggregationMode.Aggregation);
                        InitTrd = null;
                    });

                    if (!DeFine.GlobalLocalSetting.EnableAnalyzingWords)
                    {
                        InitTrd.Start();

                        while (InitTrd != null)
                        {
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        InitTrd.Start();

                        var GetBatchCore = Instance.GetBatchCore();

                        while (GetBatchCore.ProcStage < 2)
                        {
                            Thread.Sleep(100);

                            SetTransBarTittle("Analyzing Words(" + GetBatchCore.MarkLeadersPercent + "%)...");
                        }

                        Thread.Sleep(1000);

                        if (GetBatchCore.Content != null)
                        GetListView.Parent.Dispatcher.Invoke(new Action(() =>
                        {
                            for (int i = 0; i < GetBatchCore.Content.UnionData.Leaders.Count; i++)
                            {
                                string GetKey = GetBatchCore.Content.UnionData.Leaders.ElementAt(i).Key;

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

                    if (BaseUnits.Count == 0)
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
                var GetBatchCore = Instance.GetBatchCore();

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

                        if ((GetBatchCore.GetCount()) == 0)
                        {
                            TranslationStatus = StateControl.Cancel;
                            EndAction.Invoke();
                            return;
                        }
                    }

                    if (Phoenix.Config.AutoSetThreadLimit)
                    {
                        Phoenix.SyncTrdCount();
                    }

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

                        int GetLeaderCount = GetBatchCore.Content.UnionData.Leaders.Count;
                        int ModifyCount = 0;

                        if (GetBatchCore != null)
                        {
                            ModifyCount = GetBatchCore.TranslatedCount;
                            GetBatchCore.Cancel();
                            GetBatchCore.Start();
                        }

                        SyncTransStateFreeze = false;

                        EndAction.Invoke();

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
                            try
                            {
                                var GetUnitGroup = GetBatchCore.DequeueTranslated(out IsEnd);

                                if (GetUnitGroup != null)
                                {
                                    for (int i = 0; i < GetUnitGroup.Units.Count; i++)
                                    {
                                        var GetUnit = GetUnitGroup.Units[i];

                                        var GetFakeGrid = GetListView.KeyToFakeGrid(GetUnit.Key);
                                        if (GetFakeGrid != null)
                                        {
                                            GetFakeGrid.TransText = GetUnit.Translated;
                                            GetFakeGrid.SyncUI(GetListView);
                                            SetTranslatorHistoryCache(GetUnit.Key, GetUnit.Translated, true);

                                            if (GetBatchCore != null)
                                            {
                                                SetTransBarTittle(string.Format("STRINGS({0}/{1})", GetBatchCore.TranslatedCount, GetListView.Rows));
                                            }
                                        }
                                    }
                                }

                                Thread.Sleep(20);

                                if (WaitStopSign())
                                {
                                    return;
                                }
                            }
                            catch { }

                            if (GetBatchCore.ProcStage == 0)
                            {
                                break;
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

                    if (GetBatchCore != null)
                    {
                        GetBatchCore.Stop();
                    }

                    EndAction.Invoke();

                    SyncTransStateFreeze = false;
                }
                else
                if (TranslationStatus == StateControl.Cancel || TranslationStatus == StateControl.Null)
                {
                    SyncTransStateFreeze = true;

                    if (GetBatchCore != null)
                    {
                        try
                        {
                            GetBatchCore.Cancel();
                        }
                        catch { }
                    }

                    EndAction.Invoke();

                    SyncTransStateFreeze = false;
                }
                else
                {
                    SyncTransStateFreeze = true;

                    if (GetBatchCore != null)
                    {
                        GetBatchCore.Keep();
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

      
        public static void Close()
        {
            FristInit = false;
            NeedNextPreparing = false;

            var GetBatchCore = Instance.GetBatchCore();
            if (GetBatchCore != null)
            {
                GetBatchCore.Cancel();

                if (GetBatchCore.Content != null)
                {
                    GetBatchCore.Content.Clear();
                }
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

            if (InitTrd != null)
            {
                try
                {
                    InitTrd.Abort();
                }
                catch { }
                InitTrd = null;
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