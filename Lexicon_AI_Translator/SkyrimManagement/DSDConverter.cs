using System.Collections.Generic;
using System.Linq;
using LexTranslator.TranslateManage;
using PhoenixEngine.Translate;

namespace LexTranslator.SkyrimManagement
{
    public class DSDConverter
    {
        public static string Version = "1.0";
        public class DSDItem
        {
            public string editor_id { get; set; } = "";
            public string form_id { get; set; } = "";
            public int index { get; set; } = 1;
            public string type { get; set; } = "";
            public string original { get; set; } = "";
            public string @string { get; set; } = "";
        }

        public class DSDFile
        {
            public List<DSDItem> DSDItems = new List<DSDItem>();
        }

        public static DSDFile RecordsToDSDFile()
        {
            DSDFile NewDSDFile = new DSDFile();
            List <DSDItem> DSDItems = new List<DSDItem>();

            for (int i = 0; i < EspReader.Records.Count; i++)
            {
                string GetKey = EspReader.Records.ElementAt(i).Key;
                var Record = EspReader.Records[GetKey];

                var Link = TranslatorInterface.Instance.GetLink();

                string GetTransData = null;

                if (Link.ContainsKey(Record.UniqueKey))
                {
                    GetTransData = TranslatorInterface.Instance.GetLink(Record.UniqueKey);
                }

                if (GetTransData != null)
                {
                    if (GetTransData.Length > 0 && GetTransData != Record.String)
                    {
                        DSDItem NDSDItem = new DSDItem();
                        NDSDItem.editor_id = Record.EditorID;
                        NDSDItem.form_id = Record.FormID;
                        NDSDItem.type = Record.ParentSig + " " + Record.ChildSig;
                        NDSDItem.original = Record.String;
                        NDSDItem.@string = GetTransData;
                        DSDItems.Add(NDSDItem);
                    }
                }
            }

            NewDSDFile.DSDItems = DSDItems;

            return NewDSDFile;
        }
    }
}
