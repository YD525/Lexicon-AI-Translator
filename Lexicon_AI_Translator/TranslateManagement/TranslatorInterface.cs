using LexTranslator.SkyrimManage;
using LexTranslator.UIManage;
using LexTranslator.UIManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LexTranslator.SkyrimManagement;
using PhoenixEngine.Translate;
using PhoenixEngine;
using PhoenixEngine.Events;
using PhoenixEngine.P_Delegate;
using PhoenixEngine.ADO;
using PhoenixEngine.Unit;
using PhoenixEngine.Engine;
using PhoenixEngine.Request;
using PhoenixEngine.Sequence;
using PhoenixEngine.Platform;
using static PhoenixEngine.Platform.HumanTranslationApi;
using System.Windows;

namespace LexTranslator.TranslateManage
{
    // Copyright 2026 YD525
    public class TranslatorInterface
    {
        public static Translator Instance = null;

        public static void Init()
        {
            HumanTranslationApi.WaitHumanInput += new AwaitHumanTranslationHandler((Send) => 
            {
                if (DeFine.WorkingWin == null)
                {
                    return string.Empty;
                }

                InteractiveView NInteractiveView = null;

                Application.Current.Dispatcher.Invoke(new Action(() => 
                {
                    NInteractiveView = new InteractiveView();
                    NInteractiveView.Owner = DeFine.WorkingWin;
                    NInteractiveView.SetSend(Send);
                }));

                while (!NInteractiveView.CanExit)
                {
                    Thread.Sleep(500);
                }

                string Received = NInteractiveView.Received;

                Application.Current.Dispatcher.Invoke(new Action(() => 
                {
                    NInteractiveView.CanClose = true;
                    NInteractiveView.Close();
                }));

                return Received;
            });

            Instance = new Translator(DeFine.GlobalLocalSetting.SourceLanguage, DeFine.GlobalLocalSetting.TargetLanguage, true);

            EngineEvents.SetDataCall += Recv;
            EngineEvents.SetBaseUnitStateChangedCallback += BaseUnitStateChanged;

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
                    long Length = 0;

                    if (Any is AICall)
                    {
                        AICall GetCall = (AICall)Any;

                        UIHelper.NodeCallCallback(GetCall.CustomID, GetCall.Platform);

                        if (GetCall.SendString != null)
                        {
                            Length = GetCall.SendString.Length;
                        }

                        LogHelper.SetInputLog(GetCall.Platform.ToString() + "->\n" + GetCall.SendString);
                        LogHelper.SetOutputLog(GetCall.Platform.ToString() + "->\n" + GetCall.ReceiveString);
                    }
                    if (Any is PlatformCall)
                    {
                        PlatformCall GetCall = (PlatformCall)Any;

                        UIHelper.NodeCallCallback(GetCall.CustomID, GetCall.Platform);

                        if (GetCall.SendString != null)
                        {
                            Length = GetCall.SendString.Length;
                        }

                        LogHelper.SetInputLog(GetCall.Platform.ToString() + "->\n" + GetCall.SendString);
                        LogHelper.SetOutputLog(GetCall.Platform.ToString() + "->\n" + GetCall.ReceiveString);
                    }

                    if (DeFine.ChartDataRef != null)
                    {
                        DeFine.ChartDataRef.SetCurrent(Length);
                    }
                }
            }));
        }

        /// <summary>
        /// Protect the translation object being entered by the user from being changed
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static UnitContext<BaseUnit> BaseUnitStateChanged(BaseUnit Item, UnitTranslationState State)
        {
            if (State == UnitTranslationState.Queued)
            {
                if (DeFine.WorkingWin != null)
                {
                    if (DeFine.WorkingWin.TransViewList != null)
                    {
                        FakeGrid QueryGrid = DeFine.WorkingWin.TransViewList.KeyToFakeGrid(Item.Key);

                        if (QueryGrid != null)
                        {
                            if (QueryGrid.TransText.Length == 0)
                            {
                                bool IsCloud = false;
                                QueryGrid.SyncData(ref IsCloud);
                            }
                        }
                    }
                }
            }

            return new UnitContext<BaseUnit>();
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
                        Instance.GetBatchCore()?.Close();
                    }

                    return true;
                }
            }

            if (TranslationStatus == StateControl.Cancel)
            {
                if (Instance != null)
                {
                    Instance.GetBatchCore()?.Close();
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

        public static List<BaseUnit> GetCanTransUnits()
        {
            YDListView GetListView = DeFine.WorkingWin.TransViewList;
            List<BaseUnit> BaseUnits = new List<BaseUnit>();

            for (int i = 0; i < GetListView.Rows; i++)
            {
                var Row = GetListView.RealLines[i];
                bool IsCloud = false;
                Row.SyncData(ref IsCloud);

                bool HasAddAIMemory = false;

                if (!HasAddAIMemory)
                {
                    if (!string.IsNullOrEmpty(Row.TransText))
                    {
                        TranslatorInterface.Instance.AddAIMemory(Row.GetSource(), Row.TransText);
                    }
                }

                EspReader EspInstance = null;
                if (DeFine.WorkingWin != null)
                {
                    if (DeFine.WorkingWin.CurrentTransType == 2)
                    {
                        //IsEspFile
                        EspInstance = DeFine.WorkingWin.GlobalEspReader;
                    }
                }

                if (DeFine.GlobalLocalSetting.AutoUpdateStringsFileToDatabase)
                {
                    if(EspInstance!=null)
                    if (EspInstance.Records.ContainsKey(Row.Key))
                    {
                        var GetRecord = EspInstance.Records[Row.Key];

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
                                Row.GetRealSource(),//Get the source text corresponding to stringsfile id
                                Row.TransText,//Get the translation content
                                TranslatorInterface.Instance.From,//Get source language
                                TranslatorInterface.Instance.To,//Get target language
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
                            if (EngineEvents.SetDataCall != null)
                            {
                                EngineEvents.SetDataCall(0, "Skip Book fields:" + Row.Key);
                            }

                            CanSet = false;
                        }
                    }
                    else
                    if (Row.Score <= 0)
                    {
                        if (EngineEvents.SetDataCall != null)
                        {
                            EngineEvents.SetDataCall(0, "Skip Dangerous fields:" + Row.Key);
                        }

                        CanSet = false;
                    }

                    if (EspInstance != null)
                    {
                        var GetTrans = EspInstance.ToStringsFile.QueryData(Row.Key);

                        if (GetTrans != null)
                        {
                            //Added to context memory. Helps AI improve accuracy.
                            TranslatorInterface.Instance.AddAIMemory(Row.GetSource(), GetTrans.Value);
                            HasAddAIMemory = true;

                            var Link = Instance.GetLink();
                            Link[Row.Key] = GetTrans.Value;

                            var GetFakeGrid = GetListView.KeyToFakeGrid(Row.Key);
                            if (GetFakeGrid != null)
                            {
                                Row.TransText = GetTrans.Value;

                                Row.SyncUI(GetListView);
                            }

                            if (EngineEvents.SetDataCall != null)
                            {
                                EngineEvents.SetDataCall(0, "Skip StringsFile(" + GetTrans.Type.ToString() + ") fields:" + Row.Key);
                            }

                            CanSet = false;
                        }
                        else
                        {
                            if (EspInstance.Records.ContainsKey(Row.Key))
                            {
                                if (EspInstance.Records[Row.Key].StringID > 0)
                                {
                                    if (EngineEvents.SetDataCall != null)
                                    {
                                        EngineEvents.SetDataCall(0, "Skip StringsFile(" + EspInstance.Records[Row.Key].String + ") fields:" + Row.Key);
                                    }

                                    CanSet = false;
                                }
                            }
                        }
                    }

                    if (CanSet)
                    {
                        var Link = Instance.GetLink();
                        if (Link[Row.Key] != null)
                        {
                            CanSet = false;
                        }

                        if (CanSet)
                        {
                            BaseUnits.Add(new BaseUnit(TranslatorInterface.Instance.GetFileUniqueKey(),
                           Row.Key, Row.Type, Row.SourceText, Row.TransText, Row.Score));
                        }
                    }
                }
            }

            return BaseUnits;
        }

        public static bool PreparingComplete = false;

        private static int _InitGuard = 0;
        public static bool FristInit
        {
            get => Interlocked.CompareExchange(ref _InitGuard, 0, 0) == 1;
            set => Interlocked.Exchange(ref _InitGuard, value ? 1 : 0);
        }

        public static Thread PreparingTrd = null;
        public static Thread InitTrd = null;
        public static void PreparingTranslationUnits()
        {
            if (Instance != null)
            {
                var BatchCore = Instance.GetBatchCore();
                if (BatchCore != null)
                {
                    BatchCore.Close();
                }
            }
           
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

                    List<BaseUnit> BaseUnits = GetCanTransUnits();
                    InitTrd = new Thread(() =>
                    {
                        Instance.Init(BaseUnits, AggregationMode.Aggregation,RowStyleWin.DictionaryKeys.Count);
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

            new Thread(() =>
            {
                var GetBatchCore = Instance.GetBatchCore();

                if (TranslationStatus == StateControl.Run && !IsKeep)
                {
                    DeFine.WorkingWin?.UPDateUI();

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

                    var BaseUnits = GetCanTransUnits();

                    if (BaseUnits.Count == 0)
                    {
                        TranslationStatus = StateControl.Cancel;
                        EndAction.Invoke();
                        return;
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

                        SetTransBarTittle("Preparing Consistency...");

                        for (int i = 0; i < GetListView.Rows; i++)
                        {
                            var Row = GetListView.RealLines[i];
                            bool IsCloud = false;
                            Row.SyncData(ref IsCloud);

                            if (!string.IsNullOrEmpty(Row.TransText))
                            {
                                TranslatorInterface.Instance.AddAIMemory(Row.GetSource(), Row.TransText);
                            }
                        }

                        int ModifyCount = 0;

                        if (GetBatchCore != null)
                        {
                            ModifyCount = GetBatchCore.TranslatedCount;
                            GetBatchCore.Close();

                            GetBatchCore.Init(BaseUnits, AggregationMode.Aggregation,RowStyleWin.DictionaryKeys.Count);
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
                        int TotalCount = 0;

                        DateTime StartTime = DateTime.Now;

                        while (!IsEnd)
                        {
                            try
                            {
                                if (TranslationStatus == StateControl.Cancel)
                                {
                                    break;
                                }

                                if (!GetBatchCore.IsWorking && GetBatchCore.ProcStage != 10)
                                {
                                    if ((DateTime.Now - StartTime).TotalSeconds > 30)
                                        break;
                                }
                                else
                                {
                                    StartTime = DateTime.Now;
                                }

                                var GetUnit = GetBatchCore.DequeueTranslated(out IsEnd);

                                if (GetUnit != null)
                                {
                                    TotalCount++;
                                    TranslatorInterface.Instance.SetLink(GetUnit.Key, GetUnit.Translated);
                                    SetTransBarTittle(string.Format("STRINGS({0}/{1})",
                                        GetBatchCore.TranslatedCount, GetListView.Rows));
                                }
                                else 
                                if (!IsEnd)
                                {
                                    Thread.Sleep(10);
                                }

                                if (WaitStopSign())
                                {
                                    return;
                                }
                            }
                            catch
                            {
                                Thread.Sleep(10);
                            }
                        }

                        while (!GetBatchCore.TranslatedQueue.IsEmpty)
                        {
                            if (GetBatchCore.TranslatedQueue.TryDequeue(out var TailUnit))
                            {
                                TranslatorInterface.Instance.SetLink(TailUnit.Key, TailUnit.Translated);
                            }
                        }


                        DeFine.WorkingWin.TransViewList.QuickRefresh();

                        var BatchCore = TranslatorInterface.Instance.GetBatchCore();

                        if (BatchCore != null)
                        {
                            Thread.Sleep(500);
                            BatchCore.Close();
                        }

                        TranslationStatus = StateControl.Cancel;
                        EndAction.Invoke();
                    }
                }
                else if (TranslationStatus == StateControl.Stop)
                {
                    SyncTransStateFreeze = true;

                    if (GetBatchCore != null)
                    {
                        GetBatchCore.Stop();
                    }

                    EndAction.Invoke();

                    SyncTransStateFreeze = false;
                }
                else if (TranslationStatus == StateControl.Cancel || TranslationStatus == StateControl.Null)
                {
                    SyncTransStateFreeze = true;

                    if (GetBatchCore != null)
                    {
                        try
                        {
                            GetBatchCore.Close();
                        }
                        catch { }
                    }

                    EndAction.Invoke();

                    SyncTransStateFreeze = false;

                    InteractiveView.CloseAll();
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

                    InteractiveView.CloseAll();
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
            RowStyleWin.RecordModifyStates.Clear();
            TranslatorInterface.TranslatorHistoryCaches.Clear();
            RowStyleWin.DictionaryKeys.Clear();

            FristInit = false;
            NeedNextPreparing = false;

            var GetBatchCore = Instance.GetBatchCore();
            if (GetBatchCore != null)
            {
                GetBatchCore.Close();
                SetTransBarTittle(string.Format("STRINGS({0}/{1})",0, 0));
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

            InteractiveView.CloseAll();
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