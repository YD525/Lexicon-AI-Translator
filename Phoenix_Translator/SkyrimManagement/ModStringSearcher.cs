using System.Collections.Generic;
using System.IO;
using PhoenixEngine.Language;
using PhoenixEngine.Translate;
using PhoenixTranslator.SkyrimManage;

namespace PhoenixTranslator.SkyrimManagement
{
    public class SkyrimMod
    {
        public string ModName = "";
        public string ModPath = "";
        public List<string> AvailableFiles = new List<string>();

        public SkyrimMod(string ModName, string ModPath, List<string> AvailableFiles)
        {
            this.ModName = ModName;
            this.ModPath = ModPath;
            this.AvailableFiles = AvailableFiles;
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
                if (!IsMod(TargetPath, out string CModName, out List<string> CAvailableFiles))
                {
                    foreach (var GetChildPath in Directory.GetDirectories(TargetPath))
                    {
                        //To ensure performance, only one level of the directory is scanned.
                        if (IsMod(GetChildPath, out string ModName, out List<string> AvailableFiles))
                        {
                            Mods.Add(new SkyrimMod(ModName, GetChildPath, AvailableFiles));
                        }
                    }
                }
                else
                {
                    Mods.Add(new SkyrimMod(CModName, TargetPath, CAvailableFiles));
                }
            }

            return Mods;
        }

        public bool IsMod(string ModPath, out string ModName, out List<string> AvailableFiles)
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
                }
                IsMod = true;
            }

            if (Directory.Exists(Path.Combine(ModPath, "scripts")))
            {
                foreach (var GetFile in Directory.GetFiles(ModPath))
                {
                    if (GetFile.EndsWith(".pex"))
                    {
                        //Script
                        AvailableFiles.Add(GetFile);
                    }
                }
                IsMod = true;
            }

            if (Directory.Exists(Path.Combine(ModPath, "SKSE", "Plugins")))
            {
                //DLL 
                IsMod = true;
            }

            if (Directory.Exists(Path.Combine(ModPath, "Interface", "Translations")))
            {
                foreach (var GetFile in Directory.GetFiles(ModPath))
                {
                    if (GetFile.EndsWith(".txt"))
                    {
                        //Script MCM
                        AvailableFiles.Add(GetFile);
                    }
                }
                IsMod = true;
            }

            if (IsMod)
            {
                ModName = Path.GetFileName(ModPath);
            }

            return IsMod;
        }
    }
}
