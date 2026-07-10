using System.Collections.Generic;
using System.IO;
using System.Text;
using LexTranslator.SkyrimModManager;
using LexTranslator.TranslateManage;
using Newtonsoft.Json;

namespace LexTranslator.SkyrimManage
{
    public class YDDictionaryFile
    {
        public string ModName { get; set; } = "";
        public List<YDDictionary> Dictionarys { get; set; } = new List<YDDictionary>();
    }
    public class YDDictionary
    {
        public string Key { get; set; } = "";
        public string OriginalText { get; set; } = "";

        public YDDictionary()
        { 
        
        }

        public YDDictionary(string Key, string OriginalText)
        { 
           this.Key = Key;
           this.OriginalText = OriginalText;
        }

        public YDDictionary(YDDictionary Item)
        {
            this.Key = Item.Key;
            this.OriginalText = Item.OriginalText;
        }
    }
    public class LexDictionary
    {
        public YDDictionaryFile CurrentFile = null;
        public Dictionary<string, YDDictionary> Dictionarys = new Dictionary<string, YDDictionary>();

        public void Close()
        {
            CurrentFile = new YDDictionaryFile();
            Dictionarys.Clear();
            CurrentModName = string.Empty;
        }

        public bool CheckDictionary()
        {
            string ModName = CurrentModName;
            string SetPath = DeFine.GetFullPath(@"\Library\" + ModName + ".Json");
            if (File.Exists(SetPath))
            {
                return true;
            }
            return false;
        }

        public int WriteDictionary(YDListView View)
        {
            int ReplaceCount = 0;

            View.MainCanvas.Dispatcher.Invoke(new System.Action(() => {
               
                for (int i = 0; i < View.Rows; i++)
                {
                    FakeGrid GetFakeGrid = View.RealLines[i];

                    string GetKey = GetFakeGrid.Key;
                    string GetSourceText = GetFakeGrid.SourceText;
                    var TargetText = GetFakeGrid.TransText;

                    this.UPDateTransText(GetKey, GetSourceText);

                    ReplaceCount++;
                }
            }));

            return ReplaceCount;
        }
        public void CreateDictionary()
        {
            string ModName = CurrentModName;
            string SetPath = DeFine.GetFullPath(@"\Library\" + ModName) + ".Json";

            CurrentFile = new YDDictionaryFile();

            foreach (var Get in Dictionarys)
            {
                CurrentFile.ModName = ModName;
                CurrentFile.Dictionarys.Add(Get.Value);
            }

            if (File.Exists(SetPath))
            {
                File.Delete(SetPath);
            }

            string GetJson = JsonConvert.SerializeObject(CurrentFile, Formatting.Indented);

            DataHelper.WriteFile(SetPath,Encoding.UTF8.GetBytes(GetJson));
        }

        public string CurrentModName = string.Empty;
        public void ReadDictionary(string ModName)
        {
            CurrentModName = ModName;
            Dictionarys.Clear();

            string SetPath = DeFine.GetFullPath(@"\Library\" + ModName) + ".Json";
            if (File.Exists(SetPath))
            {
                string GetData = Encoding.UTF8.GetString(DataHelper.ReadFile(SetPath));
                var GetClass = JsonConvert.DeserializeObject<YDDictionaryFile>(GetData);
                if (GetClass != null)
                {
                    CurrentFile = GetClass;

                    foreach (var Get in CurrentFile.Dictionarys)
                    {
                        Dictionarys.Add(Get.Key,new YDDictionary(Get));
                    }
                }
            }
        }

        public YDDictionary CheckDictionary(string Key)
        {
            if (Dictionarys.ContainsKey(Key))
            { 
               return Dictionarys[Key];
            }

            return null;
        }

        public int UPDateTransText(string Key,string OriginalText)
        {   
            if (Dictionarys.ContainsKey(Key))
            {
                Dictionarys[Key].OriginalText = OriginalText;
                return 1;
            }
            else
            {
                Dictionarys.Add(Key,new YDDictionary(Key,OriginalText));
                return 2;
            }
        }
    }
}
