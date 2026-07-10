using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using LexTranslator.SkyrimManage;
using LexTranslator.TranslateManage;
using LexTranslator.UIManagement;
using PexInterface;
using PhoenixEngine;
using PhoenixEngine.ADO;
using PhoenixEngine.Engine;
using PhoenixEngine.Events;
using PhoenixEngine.Request;
using PhoenixEngine.Translate;
using PhoenixEngine.Unit;
using static PexInterface.PexHeuristicAnalysis;

namespace LexTranslator.SkyrimManagement
{
    public enum GameFileType
    { 
       Null = 0,ESP = 1,PEX = 2,MCM = 3,XML = 5,JSON = 6
    }
    public enum GameFileState
    { 
       Null = 0,Load = 1,Save = 2
    }
    public class ModFile
    {
        public string Path = "";
        public string FileName = "";
        public GameFileType Type = GameFileType.Null;
        public RamCacheReader RamCacheReader = null;
        public EspReader EspReader = null;
        public MCMReader MCMReader = null;
        public string PSCCode = "";
        public PexHeuristicAnalysis PexReader = null;
        public R_XmlReader XmlReader = null;
        public Dictionary<string, int> PexLinks = new Dictionary<string, int>();
        public Dictionary<string, ManagedDialContext> DialNodeCache = new Dictionary<string, ManagedDialContext>();

        public LexDictionary Lex_Dictionary = new LexDictionary();

        public Translator P_Translator = null;
        public YDListView ListView = null;

        public TranslateView Win = null;

        public GameFileState State = GameFileState.Null;

        public ModFile(string Path)
        {
            //Although each tag has its own independent translator, there's only one Node selection view on the interface. This means multiple instances use a single configuration file. Furthermore, the current thread count must be calculated by adding up the number of running instances, and so on. I suddenly realized, what about the thread limit in the settings interface? It limits the number of threads for a single instance. Therefore, to be on the safe side, this version will only allow one translation to run simultaneously for now.
            this.P_Translator = new Translator(Path,DeFine.GlobalLocalSetting.SourceLanguage, DeFine.GlobalLocalSetting.TargetLanguage, true);

            if (System.IO.File.Exists(Path))
            {
                this.Path = Path;
                this.FileName = Path.Substring(Path.LastIndexOf(@"\") + @"\".Length);

                Lex_Dictionary.ReadDictionary(this.FileName);

                if (Path.ToLower().EndsWith(".xml"))
                {
                    this.Type = GameFileType.XML;
                    XmlReader = new R_XmlReader(P_Translator);
                }
                else
                if (Path.ToLower().EndsWith(".json"))
                {
                    this.Type = GameFileType.JSON;
                    RamCacheReader = new RamCacheReader(P_Translator);
                }
                else
                if (Path.ToLower().EndsWith(".pex"))
                {
                    this.Type = GameFileType.PEX;
                    PexReader = new PexHeuristicAnalysis();
                }
                else
                if (Path.ToLower().EndsWith(".txt"))
                {
                    this.Type = GameFileType.MCM;
                    MCMReader = new MCMReader(P_Translator);
                }
                else
                if (Path.ToLower().EndsWith(".esp") || Path.ToLower().EndsWith(".esm") || Path.ToLower().EndsWith(".esl"))
                {
                    this.Type = GameFileType.ESP;
                    EspReader = new EspReader(P_Translator);
                }
            }
        }

        public void SetListView(YDListView ListView)
        { 
           this.ListView = ListView;
        }
        private void Backup()
        {
            if (File.Exists(this.Path))
            { 
            
            }
        }

        private void ClearBackup()
        { 
        
        }

        public void Load()
        {
            if (File.Exists(this.Path))
            {
                Close();

                this.P_Translator.LoadFile(this.Path);

                this.Lex_Dictionary.ReadDictionary(this.FileName);

                switch (this.Type)
                {
                    case GameFileType.XML:
                        {
                            XmlReader.Load(this.Path);
                            State = GameFileState.Load;
                        }
                        break;
                    case GameFileType.JSON:
                        {
                            RamCacheReader.Load(this.Path);
                            State = GameFileState.Load;
                        }
                        break;
                    case GameFileType.ESP:
                        {
                            EspReader.LoadEsp(this.Path);
                            State = GameFileState.Load;
                        }
                        break;
                    case GameFileType.PEX:
                        {
                            CodeGenStyle AutoStyle = CodeGenStyle.Papyrus;

                            if (DeFine.GlobalLocalSetting.GenCSharp)
                            {
                                AutoStyle = CodeGenStyle.CSharp;
                            }

                            PexReader.Core.LoadPex(this.Path).ReadStrings().GetPsc(out this.PSCCode, DeFine.GlobalLocalSetting.ShowAssembly, AutoStyle).AnalysisStrings();
                            State = GameFileState.Load;
                        }
                        break;
                    case GameFileType.MCM:
                        {
                            MCMReader.LoadMCM(this.Path);
                            State = GameFileState.Load;
                        }
                        break;
                }
            }
        }

        public void SyncListView(bool CanSetSource)
        {
            if (ListView != null)
            {
                for (int i = 0; i < ListView.Rows; i++)
                {
                    bool IsCloud = false;

                    ListView.RealLines[i].SyncData(this,ref IsCloud);

                    string GetKey = ListView.RealLines[i].Key;

                    string GetTransText = ListView.RealLines[i].TransText;

                    if (CanSetSource)
                    {
                        if (string.IsNullOrEmpty(GetTransText))
                        {
                            GetTransText = ListView.RealLines[i].SourceText;
                        }
                    }

                    var Link = this.P_Translator.GetLink();
                    Link[GetKey] = GetTransText;
                }
            }
        }


        public void Save()
        {
            Backup();

            if (File.Exists(this.Path))
            {
                var Link = this.P_Translator.GetLink();

                if (DeFine.GlobalLocalSetting.UseFullPunctuation)
                {
                    Link.CheckLinks(new Action<string, string, bool>((string Key, string Value, bool Unique) =>
                    {
                        if (Value.Length > 0)
                        {
                            Link[Key] = TranslationPreprocessor.ToFullWidthSymbols(Value);
                        }
                    }));
                }

                if (this.Type == GameFileType.JSON)
                {
                    SyncListView(false);
                }
                else
                {
                    SyncListView(true);
                }

                switch (this.Type)
                {
                    case GameFileType.XML:
                        {
                            XmlReader.Save(this.Path);
                            State = GameFileState.Save;
                        }
                        break;
                    case GameFileType.JSON:
                        {
                            if (!RamCacheReader.Save(this,this.Path))
                            {
                                ClearBackup();
                                MessageBox.Show("Build RamCache Error!");
                            }
                            State = GameFileState.Save;
                        }
                        break;
                    case GameFileType.ESP:
                        {
                            int ModifyCount = EspReader.SaveEsp(this.Path);
                            if (ModifyCount == 0)
                            {
                                ClearBackup();
                            }
                            State = GameFileState.Save;
                        }
                        break;
                    case GameFileType.PEX:
                        {
                            PexReader.Core.GetStrings(out List<PexStringItem> Strings);

                            int TranslateCount = 0;

                            for (int i = 0; i < Strings.Count; i++)
                            {
                                var StringItem = Strings[i];
                                StringItem.Translated = this.P_Translator.GetLink(StringItem.UniqueKey);
                                if (StringItem.Translated.Length > 0)
                                {
                                    TranslateCount++;
                                }
                            }

                            if (TranslateCount > 0)
                            {
                                PexReader.Core.SavePex(this.Path, out int SaveState).Close();

                                if (SaveState > 0 == false)
                                {
                                    ClearBackup();
                                    MessageBox.Show("Build Script Error!");
                                }
                            }
                            State = GameFileState.Save;
                        }
                        break;
                    case GameFileType.MCM:
                        {
                            MCMReader.SaveMCMConfig(this.Path);
                            State = GameFileState.Save;
                        }
                        break;
                }

                Lex_Dictionary.WriteDictionary(this.ListView);
                Lex_Dictionary.CreatDictionary();
            }
        }

        public void Close()
        {
            PexLinks.Clear();
            DialNodeCache.Clear();
            Lex_Dictionary.Close();

            switch (this.Type)
            {
                case GameFileType.XML:
                    {
                        XmlReader.Close();
                    }
                break;
                case GameFileType.JSON:
                    {
                        RamCacheReader.Close();
                    }
                break;
                case GameFileType.ESP:
                    {
                        EspReader.Close();
                    }
                break;
                case GameFileType.PEX:
                    {
                        PexReader.Core.Close();
                        PexLinks.Clear();
                    }
                break;
                case GameFileType.MCM:
                    {
                        MCMReader.Close();
                    }
                break;
            }
        }


        #region Translate Control
        public Thread PreparingTrd = null;
        public Thread InitTrd = null;
        public StateControl TranslationStatus = StateControl.Null;
        public bool SyncTransStateFreeze = false;

        public bool PreparingComplete = false;

        private int _InitGuard = 0;
        public bool FristInit
        {
            get => Interlocked.CompareExchange(ref _InitGuard, 0, 0) == 1;
            set => Interlocked.Exchange(ref _InitGuard, value ? 1 : 0);
        }

        public UnitContext<BaseUnit> BaseUnitStateChanged(BaseUnit Item, UnitTranslationState State)
        {
            if (State == UnitTranslationState.Queued)
            {
                if (ListView != null)
                {
                    FakeGrid QueryGrid = ListView.KeyToFakeGrid(Item.Key);

                    if (QueryGrid != null)
                    {
                        if (QueryGrid.TransText.Length == 0)
                        {
                            bool IsCloud = false;
                            QueryGrid.SyncData(this,ref IsCloud);
                        }
                    }
                }
            }

            return new UnitContext<BaseUnit>();
        }
        public void SetTransBarTittle(string Log)
        {
            if (Win != null)
            {
                Win.Dispatcher.Invoke(new Action(() =>
                {
                    Win.TransProcess.Content = Log;
                }));
            }
        }

        public int GetTranslateCount()
        {
            int TranslateCount = 0;
            for (int i = 0; i < ListView.Rows; i++)
            {
                var Row = ListView.RealLines[i];

                if (Row.TransText.Length > 0)
                {
                    TranslateCount++;
                }
            }

            return TranslateCount;
        }
       
        public void Prepare()
        {
            if (P_Translator != null)
            {
                var BatchCore = P_Translator.GetBatchCore();
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
                    while (Win.DataLoading == true)
                    {
                        Thread.Sleep(1000);
                    }

                    SetTransBarTittle("Preparing Translation Units...");

                    List<BaseUnit> BaseUnits = GetCanTransUnits();
                    InitTrd = new Thread(() =>
                    {
                        P_Translator.Init(BaseUnits,GetTranslateCount(),
                        new P_BucketContainer.CheckLinks((TempUnits,Unit) =>
                        {
                            return MultiWindowController.CheckLinks(this,TempUnits,Unit);
                        }));
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

                        var GetBatchCore = P_Translator.GetBatchCore();

                        while (GetBatchCore.ProcStage < 2)
                        {
                            Thread.Sleep(100);

                            SetTransBarTittle("Analyzing Words(" + GetBatchCore.Container.MarkHeadsPercent + "%)...");
                        }

                        Thread.Sleep(1000);

                        if (GetBatchCore.Container != null)
                            ListView.Parent.Dispatcher.Invoke(new Action(() =>
                            {
                                for (int i = 0; i < GetBatchCore.Container.Heads.Count; i++)
                                {
                                    string GetKey = GetBatchCore.Container.Heads.ElementAt(i).Key;

                                    for (int ir = 0; ir < ListView.VisibleRows.Count; ir++)
                                    {
                                        if (RowStyleWin.GetKey(ListView.VisibleRows[ir].View).Equals(GetKey))
                                        {
                                            RowStyleWin.MarkLeader(ListView.VisibleRows[ir].View, true);
                                            break;
                                        }
                                    }
                                }
                            }));
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

        public bool WaitStopSign()
        {
            while (TranslationStatus == StateControl.Stop)
            {
                Thread.Sleep(1000);

                if (TranslationStatus == StateControl.Cancel)
                {
                    if (P_Translator != null)
                    {
                        P_Translator.GetBatchCore()?.Close();
                    }

                    return true;
                }
            }

            if (TranslationStatus == StateControl.Cancel)
            {
                if (P_Translator != null)
                {
                    P_Translator.GetBatchCore()?.Close();
                }

                return true;
            }

            return false;
        }

        public List<BaseUnit> GetCanTransUnits()
        {
            List<BaseUnit> BaseUnits = new List<BaseUnit>();

            this.DialNodeCache.Clear();

            for (int i = 0; i < ListView.Rows; i++)
            {
                var Row = ListView.RealLines[i];
                bool IsCloud = false;
                Row.SyncData(this,ref IsCloud);

                bool HasAddAIMemory = false;

                if (!HasAddAIMemory)
                {
                    if (!string.IsNullOrEmpty(Row.TransText))
                    {
                        P_Translator.AddAIMemory(Row.GetSource(), Row.TransText);
                    }
                }

                bool IsEsp = false;
                if (Type == GameFileType.ESP)
                {
                    IsEsp = true;
                }

                if (DeFine.GlobalLocalSetting.AutoUpdateStringsFileToDatabase)
                {
                    if (IsEsp)
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
                                    Row.GetRealSource(),//Get the source text corresponding to stringsfile id
                                    Row.TransText,//Get the translation content
                                    P_Translator.From,//Get source language
                                    P_Translator.To,//Get target language
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

                    if (IsEsp)
                    {
                        var GetTrans = EspReader.ToStringsFile.QueryData(Row.Key);

                        if (GetTrans != null)
                        {
                            //Added to context memory. Helps AI improve accuracy.
                            P_Translator.AddAIMemory(Row.GetSource(), GetTrans.Value);
                            HasAddAIMemory = true;

                            var Link = P_Translator.GetLink();
                            Link[Row.Key] = GetTrans.Value;

                            var GetFakeGrid = ListView.KeyToFakeGrid(Row.Key);
                            if (GetFakeGrid != null)
                            {
                                Row.TransText = GetTrans.Value;

                                Row.SyncUI(ListView);
                            }

                            if (EngineEvents.SetDataCall != null)
                            {
                                EngineEvents.SetDataCall(0, "Skip StringsFile(" + GetTrans.Type.ToString() + ") fields:" + Row.Key);
                            }

                            CanSet = false;
                        }
                        else
                        {
                            if (EspReader.Records.ContainsKey(Row.Key))
                            {
                                if (EspReader.Records[Row.Key].StringID > 0)
                                {
                                    if (EngineEvents.SetDataCall != null)
                                    {
                                        EngineEvents.SetDataCall(0, "Skip StringsFile(" + EspReader.Records[Row.Key].String + ") fields:" + Row.Key);
                                    }

                                    CanSet = false;
                                }
                            }
                        }
                    }

                    if (CanSet)
                    {
                        var Link = P_Translator.GetLink();
                        if (Link[Row.Key] != null)
                        {
                            CanSet = false;
                        }

                        if (CanSet)
                        {
                            string Emotion = "";

                            if (this.Type == GameFileType.ESP)
                            {
                                var GetRecord = this.EspReader.Records[Row.Key];

                                if (GetRecord.ParentSig == "INFO" || GetRecord.ParentSig == "DIAL")
                                {
                                    var LinkData = this.EspReader.GetDialContext(GetRecord);

                                    if (LinkData != null)
                                    {
                                        this.DialNodeCache[GetRecord.UniqueKey] = LinkData;

                                        foreach (var Get in LinkData.Links)
                                        {
                                            if (Get.RecordOffset == GetRecord.ParentIndex && Get.SubOffset == GetRecord.SubIndex)
                                            {
                                                Emotion = EmotionTypeHelper.FromRaw(Get.EmotionType).ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            BaseUnits.Add(new BaseUnit(P_Translator.GetFileUniqueKey(),
                            Row.Key, Row.Type, Row.SourceText, Row.TransText, Emotion, Row.Score));
                        }
                    }
                }
            }

            return BaseUnits;
        }

       
        public void MakeReady()
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

        public void SyncTransState(Action EndAction, bool IsKeep = false)
        {
            if (SyncTransStateFreeze)
            {
                EndAction.Invoke();
                return;
            }

            if (Win == null)
            {
                TranslationStatus = StateControl.Cancel;
                EndAction.Invoke();
                return;
            }

            new Thread(() =>
            {
                var GetBatchCore = P_Translator.GetBatchCore();

                if (TranslationStatus == StateControl.Run && !IsKeep)
                {
                    Win?.UPDateUI();

                    FristInit = false;

                    bool NeedPrepare = false;

                    if (GetBatchCore.Container == null)
                    {
                        NeedPrepare = true;
                    }
                    else
                    {
                        if (GetBatchCore.Container.GetCount() == 0)
                        {
                            NeedPrepare = true;
                        }
                    }

                    if (NeedPrepare)
                    {
                        Prepare();

                        while (PreparingTrd != null)
                        {
                            Thread.Sleep(100);
                        }
                    }

                    if ((GetBatchCore.GetCount()) == 0)
                    {
                        TranslationStatus = StateControl.Cancel;
                        EndAction.Invoke();
                        return;
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

                    if (ListView != null)
                    {
                        SyncTransStateFreeze = true;

                        MakeReady();

                        SetTransBarTittle("Preparing Consistency...");

                        for (int i = 0; i < ListView.Rows; i++)
                        {
                            var Row = ListView.RealLines[i];
                            bool IsCloud = false;
                            Row.SyncData(this,ref IsCloud);

                            if (!string.IsNullOrEmpty(Row.TransText))
                            {
                                P_Translator.AddAIMemory(Row.GetSource(), Row.TransText);
                            }
                        }

                        int ModifyCount = 0;

                        ModifyCount = GetBatchCore.BaseTranslatedCount + GetBatchCore.TranslatedCount;

                        GetBatchCore.Start();

                        SyncTransStateFreeze = false;

                        EndAction.Invoke();

                        SetTransBarTittle(string.Format("STRINGS({0}/{1})", ModifyCount, ListView.Rows));

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
                                    P_Translator.SetLink(GetUnit.Key, GetUnit.Translated);
                                    SetTransBarTittle(string.Format("STRINGS({0}/{1})",
                                          GetBatchCore.BaseTranslatedCount + GetBatchCore.TranslatedCount, ListView.Rows));

                                    ListView.MainCanvas.Dispatcher.Invoke(new Action(() => 
                                    {
                                        bool IsCloud = false;
                                        ListView.KeyToFakeGrid(GetUnit.Key).SyncData(this,ref IsCloud);
                                    }));
                                   
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
                                P_Translator.SetLink(TailUnit.Key, TailUnit.Translated);
                            }
                        }

                        //DeFine.WorkWin.TransViewList.QuickRefresh();

                        var BatchCore = P_Translator.GetBatchCore();

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

        public void CancelTranslateWork()
        {
            RowStyleWin.RecordModifyStates.Clear();
            TranslatorInterface.TranslatorHistoryCaches.Clear();

            FristInit = false;

            var GetBatchCore = P_Translator.GetBatchCore();
            if (GetBatchCore != null)
            {
                GetBatchCore.Close();
                SetTransBarTittle(string.Format("STRINGS({0}/{1})", 0, 0));
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

        #endregion
    }
}
