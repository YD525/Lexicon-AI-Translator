using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using PhoenixEngine.Language;
using PhoenixEngine.Translate;
using PhoenixTranslator.SkyrimManage;

namespace PhoenixTranslator.SkyrimManagement
{
    public class SkyrimMod
    {
        public string ModPath = "";
        public string ModName = "";
        public long ModID = 0;
       
        public List<string> AvailableFiles = new List<string>();
        public SkyrimMod()
        { 
        
        }
        public SkyrimMod(string ModPath, string ModName,  List<string> AvailableFiles, long ModID)
        {
            this.ModPath = ModPath;
            this.ModName = ModName;
            this.AvailableFiles = AvailableFiles;

            this.ModID = ModID;
        }
        public string GetUrl()
        {
            return string.Format("https://www.nexusmods.com/skyrimspecialedition/mods/{0}", this.ModID);
        }
    }

    public class ModReader
    {
        private Translator _Instance;

        public EspReader Esp = null;
        public PexReader Pex = null;
        public MCMReader MCM = null;

        public void Init()
        {
            Close();
            _Instance = new Translator("ModSearch", Languages.English, Languages.English, true);

            Esp = new EspReader(_Instance);
            Pex = new PexReader();
            MCM = new MCMReader(_Instance);
        }

        public bool Contains(string Path, string Str)
        {
            if (Path.EndsWith(".esp") || Path.EndsWith(".esm") || Path.EndsWith(".esl"))
            {
                Esp.LoadEsp(Path);

                foreach (var Get in Esp.SelectSig("ALL"))
                {
                    if (Get.Value.String.Contains(Str))
                    {
                        Esp.Close();
                        return true;
                    }
                }
            }
            else
            if (Path.EndsWith(".pex"))
            {
                Pex.LoadPex(Path);

                foreach (var Get in Pex.Records)
                {
                    if (Get.Value.Original.Contains(Str))
                    {
                        Pex.Close();
                        return true;
                    }
                }
            }
            else
            if (Path.EndsWith(".txt"))
            {
                MCM.LoadMCM(Path);

                foreach (var Get in MCM.Lines)
                {
                    if (Get.Contains(Str))
                    {
                        MCM.Close();
                        return true;
                    }
                }
            }

            return false;
        }

        public void Close()
        {
            _Instance?.Close();
            _Instance = null;

            Esp?.Close();
            Pex?.Close();
            MCM?.Close();
        }
    }
    public class ModStringSearcher
    {
        public List<string> SearchStr(List<SkyrimMod> Mods, string Str)
        {
            List<string> RetrievedFiles = new List<string>();

            ModReader NReader = new ModReader();
            NReader.Init();

            foreach (SkyrimMod Mod in Mods)
            {
                foreach (var GetFile in Mod.AvailableFiles)
                {
                    if (NReader.Contains(GetFile, Str))
                    {
                        RetrievedFiles.Add(GetFile);
                    }
                }
            }

            NReader.Close();
            return RetrievedFiles;
        }
        public List<SkyrimMod> ScanMods(string TargetPath)
        {
            List<SkyrimMod> Mods = new List<SkyrimMod>();
            if (Directory.Exists(TargetPath))
            {
                if (!IsMod(TargetPath, out string CModName, out List<string> CAvailableFiles,out long CModID))
                {
                    foreach (var GetChildPath in Directory.GetDirectories(TargetPath))
                    {
                        //To ensure performance, only one level of the directory is scanned.
                        if (IsMod(GetChildPath, out string ModName, out List<string> AvailableFiles,out long ModID))
                        {
                            Mods.Add(new SkyrimMod(GetChildPath,ModName,AvailableFiles,ModID));
                        }
                    }
                }
                else
                {
                    Mods.Add(new SkyrimMod(TargetPath,CModName,CAvailableFiles,CModID));
                }
            }

            return Mods;
        }

        public bool IsMod(string ModPath, out string ModName, out List<string> AvailableFiles,out long ModID)
        {
            AvailableFiles = new List<string>();
            ModName = string.Empty;

            bool IsMod = false;

            foreach (var GetFile in Directory.GetFiles(ModPath))
            {
                if (GetFile.EndsWith(".esp") || GetFile.EndsWith(".esm") || GetFile.EndsWith(".esl"))
                {
                    //Esp
                    AvailableFiles.Add(GetFile);
                    IsMod = true;
                }
            }

            string ScriptPath = Path.Combine(ModPath, "scripts");

            if (Directory.Exists(ScriptPath))
            {
                foreach (var GetFile in Directory.GetFiles(ScriptPath))
                {
                    if (GetFile.EndsWith(".pex"))
                    {
                        //Script
                        AvailableFiles.Add(GetFile);
                        IsMod = true;
                    }
                }
            }

            string SKSEPluginPath = Path.Combine(ModPath, "SKSE", "Plugins");

            if (Directory.Exists(SKSEPluginPath))
            {
                foreach (var GetFile in Directory.GetFiles(SKSEPluginPath))
                {
                    if (GetFile.EndsWith(".dll"))
                    {
                        //DLL 
                        IsMod = true;
                    }
                }
            }

            string InterfacePath = Path.Combine(ModPath, "Interface", "Translations");

            if (Directory.Exists(InterfacePath))
            {
                foreach (var GetFile in Directory.GetFiles(InterfacePath))
                {
                    if (GetFile.EndsWith(".txt"))
                    {
                        //Script MCM
                        AvailableFiles.Add(GetFile);

                        IsMod = true;
                    }
                }
            }

            if (IsMod)
            {
                ModName = Path.GetFileName(ModPath);

                string MetaPath = Path.Combine(ModPath, "meta.ini");

                if (File.Exists(MetaPath))
                {
                    string Content = File.ReadAllText(MetaPath);

                    Match Match = Regex.Match(Content, @"(?m)^\s*modid\s*=\s*(\d+)\s*$");

                    if (Match.Success)
                    {
                        ModID = int.Parse(Match.Groups[1].Value);
                    }
                    else
                    {
                        ModID = -1;
                    }
                }
                else
                {
                    ModID = -2;
                }
            }
            else
            {
                ModID = 0;
            }

            return IsMod;
        }
    }
}
