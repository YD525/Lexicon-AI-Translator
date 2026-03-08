using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using LexTranslator.SkyrimModManager;
using LexTranslator.TranslateManage;

namespace LexTranslator.SkyrimManagement
{
    public class RamCacheReader
    {
        public List<FakeGrid> RamLines = new List<FakeGrid>();
        public void Load(string FilePath)
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string GetRamCache = Encoding.UTF8.GetString(DataHelper.ReadFile(FilePath));
                    this.RamLines = JsonConvert.DeserializeObject<List<FakeGrid>>(GetRamCache);

                    if (RamLines != null)
                    {
                        foreach (var Get in RamLines)
                        {
                            TranslatorInterface.Instance.GetLink().Add(Get.Key, Get.TransText);
                        }
                    }
                }
            }
            catch { }
        }
        public void Close()
        {
            RamLines?.Clear();
        }

        public bool Save(string OutPutPath)
        {
            try
            {
                if (RamLines != null)
                {
                    for (int i = 0; i < RamLines.Count; i++)
                    {
                        bool IsCloud = false;
                        RamLines[i].SyncData(ref IsCloud);
                    }

                    string GetJson = JsonConvert.SerializeObject(RamLines, Formatting.Indented);

                    if (OutPutPath != null)
                    {
                        if (OutPutPath.Trim().Length > 0)
                        {
                            if (File.Exists(OutPutPath))
                            {
                                File.Delete(OutPutPath);
                            }
                            DataHelper.WriteFile(OutPutPath, Encoding.UTF8.GetBytes(GetJson));
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
