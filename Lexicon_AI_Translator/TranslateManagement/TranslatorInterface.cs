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
        public static void Init()
        {
            HumanTranslationApi.WaitHumanInput += new AwaitHumanTranslationHandler((Send) => 
            {
                if (DeFine.WorkWin == null)
                {
                    return string.Empty;
                }

                InteractiveView NInteractiveView = null;

                Application.Current.Dispatcher.Invoke(new Action(() => 
                {
                    NInteractiveView = new InteractiveView();
                    NInteractiveView.Owner = DeFine.WorkWin;
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
        public static UnitContext<BaseUnit> BaseUnitStateChanged(string ID,BaseUnit Item, UnitTranslationState State)
        {
            if (State == UnitTranslationState.Queued)
            {
                if (DeFine.WorkWin != null)
                {
                    for (int i = 0; i < DeFine.WorkWin.TabViews.Children.Count; i++)
                    {
                        if (DeFine.WorkWin.TabViews.Children[i] is TranslateView)
                        {
                            var Mod = (DeFine.WorkWin.TabViews.Children[i] as TranslateView).Mod;

                            if (Mod.Path.Equals(ID))//The reason for using the file path as the primary key ID is that a user cannot translate two pieces of content with the same path at the same time, and there are also limitations in the outer layer I implemented..
                            {
                                //This way, you can determine which Translator the BaseUnit belongs to by its ID, and then retrieve the ListView control itself based on the ModFile ~.
                                var ListView = Mod.ListView;

                                FakeGrid QueryGrid = ListView.KeyToFakeGrid(Item.Key);

                                if (QueryGrid != null)
                                {
                                    if (QueryGrid.TransText.Length == 0)
                                    {
                                        bool IsCloud = false;
                                        QueryGrid.SyncData(Mod, ref IsCloud);
                                    }
                                }

                                break;
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
            if (DeFine.WorkWin != null)
            {
                DeFine.WorkWin.Dispatcher.Invoke(new Action(() =>
                {
                    DeFine.WorkWin.MainLog.Text = Log;
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