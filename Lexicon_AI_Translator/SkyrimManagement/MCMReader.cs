using System.Collections.Generic;
using System.IO;
using System.Text;
using PhoenixEngine.TranslateManage;
using LexTranslator.SkyrimManagement;
using LexTranslator.SkyrimModManager;

namespace LexTranslator.SkyrimManage
{

    //string Type,string EditorID,string Key, string SourceText,string TransText
    public class MCMItem
    {
        public string Type = "";
        public string EditorID = "";
        public string Key = "";
        public string SourceText = "";
        public string TransText = "";

        public MCMItem(string EditorID,string SourceText)
        {
            this.Type = "MCM";
            this.EditorID = EditorID;
            if (this.EditorID.Contains("$"))
            {
                this.EditorID = EditorID.Replace("$","");
            }
            this.Key = SkyrimData.GenUniqueKey(this.EditorID, this.Type);
            this.SourceText = SourceText;
            this.TransText = string.Empty;
        }

        public string GetTextIfTrans()
        {
            if (this.TransText.Trim().Length > 0)
            {
                return this.TransText;
            }
            string GetKey = SkyrimData.GenUniqueKey(this.EditorID, this.Type);
            var GetResult = TranslatorBridge.GetTranslatorCache(GetKey);
            if (GetResult!=null)
            {
                this.TransText = GetResult;
                if (this.TransText.Length > 0)
                {
                    return this.TransText;
                }
                else
                {
                    return this.SourceText;
                }
            }

            return this.SourceText;
        }


        public string GetTextIfTransR()
        {
            string GetKey = SkyrimData.GenUniqueKey(this.EditorID, this.Type);
            var GetResult = TranslatorBridge.GetTranslatorCache(GetKey);
            if (GetResult != null)
            {
                this.TransText = GetResult;
                if (this.TransText.Length > 0)
                {
                    return this.TransText;
                }
                else
                {
                    return "";
                }
            }

            return "";
        }
    }
    public class MCMReader
    {
        public List<string> Lines = new List<string>();
        public List<MCMItem> MCMItems = new List<MCMItem>();
        public Encoding CurrentEncoding = null;
        public bool CheckIsMCM()
        {
            foreach (var Get in Lines)
            {
                if ((Get.StartsWith("$")|| Get.StartsWith("#")) && (Get.Contains("\t") || Get.Contains(" ")))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public void Close()
        {
            TranslatorExtend.ClearTranslatorHistoryCache();
            Lines.Clear();
            MCMItems.Clear();
        }

        public void LoadMCM(string Path)
        {
            TranslatorExtend.ClearTranslatorHistoryCache();
            TranslatorBridge.ClearCache();
            Lines.Clear();
            MCMItems.Clear();

            Encoding Encoder = DataHelper.GetFileEncodeType(Path);

            var GetData = DataHelper.ReadFile(Path);
            var FileStr = Encoder.GetString(GetData);

            foreach (var GetLine in FileStr.Split(new char[2] { '\r', '\n' }))
            {
                if (GetLine.Trim().Length > 0)
                { 
                   this.Lines.Add(GetLine.Trim());
                }
            }

            if (CheckIsMCM())
            {
                ReadMCMConfig();
            }
        }
        //SystemDataWriter.PreFormatStr

        public void ReadMCMConfig()
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                string GetLine = Lines[i];

                if (GetLine.StartsWith("$") && (GetLine.Contains("\t") || GetLine.Contains(" ")))
                {
                    string AutoSplictChar = "";

                    if (GetLine.Contains("\t"))
                    {
                        AutoSplictChar = "\t";
                    }
                    else
                    if (GetLine.Contains(" "))
                    {
                        AutoSplictChar = " ";
                    }

                    string GetEditorID = GetLine.Substring(0, GetLine.IndexOf(AutoSplictChar));
                    string GetSourceValue = GetLine.Substring(GetEditorID.Length);
                    GetSourceValue = GetSourceValue.Trim();

                    MCMItem NMCMItem = new MCMItem(GetEditorID,GetSourceValue);
                    this.MCMItems.Add(NMCMItem);
                }
            }
        }

        public void SaveMCMConfig(string OutPutPath)
        {
            TranslatorExtend.ClearTranslatorHistoryCache();
            if (File.Exists(OutPutPath))
            {
                return;
            }
            string RichText = "";
            foreach (var GetMCMItem in this.MCMItems)
            {
                string NewStr = GetMCMItem.GetTextIfTrans();
                TranslationPreprocessor.NormalizePunctuation(ref NewStr);
                RichText += string.Format("${0}\t{1}\n", GetMCMItem.EditorID, NewStr);
            }
            DataHelper.WriteFile(OutPutPath,Encoding.UTF8.GetBytes(RichText));

            Close();
        }
    }
}
