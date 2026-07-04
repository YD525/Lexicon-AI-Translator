using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LexTranslator.SkyrimManage;
using LexTranslator.TranslateManage;
using LexTranslator.UIManagement;
using PexInterface;
using PhoenixEngine;
using PhoenixEngine.Engine;
using PhoenixEngine.Translate;
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

        public LexDictionary Lex_Dictionary = new LexDictionary();

        public Translator P_Translator = null;
        public YDListView ListView = null;

        public TranslateView Win = null;

        public GameFileState State = GameFileState.Null;

        public ModFile(string Path)
        {
            this.P_Translator = new Translator(DeFine.GlobalLocalSetting.SourceLanguage, DeFine.GlobalLocalSetting.TargetLanguage, true);

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

                    ListView.RealLines[i].SyncData(ref IsCloud);

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
                            if (!RamCacheReader.Save(this.Path))
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
    }
}
