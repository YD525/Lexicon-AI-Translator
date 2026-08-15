using System;
using System.Collections.Generic;
using System.Linq;
using PexInterface;
using static PexInterface.PexHeuristicAnalysis;

namespace PhoenixTranslator.SkyrimManagement
{
    public class PexReader
    {
        public PexHeuristicAnalysis Interface = null;
        public Dictionary<string, int> PexLinks = new Dictionary<string, int>();
        public string PSCCode = "";

        public Dictionary<string, PexStringItem> Records = new Dictionary<string, PexStringItem>();

        public PexReader()
        {
            Interface = new PexHeuristicAnalysis();
        }

        public void LoadPex(string Path)
        {
            CodeGenStyle AutoStyle = CodeGenStyle.Papyrus;

            if (DeFine.GlobalLocalSetting.GenCSharp)
            {
                AutoStyle = CodeGenStyle.CSharp;
            }

            Interface.Core.LoadPex(Path).ReadStrings().GetPsc(out this.PSCCode, DeFine.GlobalLocalSetting.ShowAssembly, AutoStyle).AnalysisStrings();

            SelectStrings();
        }

        public void SelectStrings()
        {
            Interface.Core.GetStrings(out List<PexStringItem> Strings);

            try
            { 
                this.Records =  Strings.GroupBy(x => x.UniqueKey).ToDictionary(x => x.Key, x => x.First());
            }
            catch 
            {
                throw new Exception($"Warning: Duplicate key detected.");
            }
        }

        public void Close()
        {
            Interface.Core.Close();
            this.Records = null;
            this.PSCCode = string.Empty;

            this.PexLinks.Clear();
        }
    }
}
