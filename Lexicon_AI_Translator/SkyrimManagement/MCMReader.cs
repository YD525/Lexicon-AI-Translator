using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Shapes;
using LexTranslator.SkyrimManagement;
using LexTranslator.SkyrimModManager;
using LexTranslator.TranslateManage;
using PhoenixEngine.Engine;
using PhoenixEngine.Translate;

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

        public string GetTextIfTrans(Translator TranslatorRef)
        {
            if (this.TransText.Trim().Length > 0)
            {
                return this.TransText;
            }
            string GetKey = SkyrimData.GenUniqueKey(this.EditorID, this.Type);

            var Link = TranslatorRef.GetLink();
            var GetStr = Link[GetKey];

            if (GetStr!=null)
            {
                this.TransText = GetStr.String;
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


        public string GetTextIfTransR(Translator TranslatorRef)
        {
            string GetKey = SkyrimData.GenUniqueKey(this.EditorID, this.Type);

            var Link = TranslatorRef.GetLink();

            var GetStr = Link[GetKey];

            if (GetStr != null)
            {
                this.TransText = GetStr.String;

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

        public Translator TranslatorRef = null;

        public MCMReader(Translator TranslatorRef)
        {
            this.TranslatorRef = TranslatorRef;
        }
        public bool CheckIsMCM()
        {
            int MaxCheckCount = 2;
            foreach (var Get in Lines)
            {
                if (MaxCheckCount > 0)
                {
                    if (Get.Trim().Length > 0)
                    {
                        MaxCheckCount--;
                        if ((Get.StartsWith("$") || Get.StartsWith("#")) && (Get.Contains("\t") || Get.Contains(" ")))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            return false;
        }

        public void Close()
        {
            Lines.Clear();
            MCMItems.Clear();
        }

        public void LoadMCM(string Path)
        {
            TranslatorRef.ClearCache();
            Lines.Clear();
            MCMItems.Clear();

            Encoding Encoder = DataHelper.GetFileEncodeType(Path);

            var GetData = DataHelper.ReadFile(Path);
            var FileStr = Encoder.GetString(GetData);

            foreach (var GetLine in FileStr.Split(new char[2] { '\r', '\n' }))
            {
                if (GetLine.Trim().Length > 0)
                {
                    //Remove invisible BOM character (U+FEFF) from the beginning of the line.
                    string Line = GetLine.TrimStart('\uFEFF').Trim();
                    // Convert escaped newline characters (\n) to real newline characters for display.
                    Line = Line.Replace("\\n", "\n");

                    this.Lines.Add(Line);
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
            if (File.Exists(OutPutPath))
            {
                File.Delete(OutPutPath);
            }

            StringBuilder RichText = new StringBuilder();

            foreach (var GetMCMItem in this.MCMItems)
            {
                string NewStr = GetMCMItem.GetTextIfTrans(TranslatorRef);
                new TranslationPreprocessor().NormalizePunctuation(ref NewStr);

                // Convert actual newline characters to escaped \n format used in MCM TXT files.
                NewStr = NewStr.Replace("\r\n", "\\n").Replace("\n", "\\n");

                RichText.Append('$')
                        .Append(GetMCMItem.EditorID)
                        .Append('\t')
                        .Append(NewStr)
                        .Append("\r\n");
            }

            // Save as UTF-8 with BOM.
            // Compatible with Papyrus Script MCM TXT files and ensures correct encoding detection by text editors.
            File.WriteAllText(OutPutPath,RichText.ToString(),new UTF8Encoding(true));

            Close();
        }
    }
}
