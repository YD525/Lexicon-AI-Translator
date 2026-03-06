using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml.Linq;
using LexTranslator.SkyrimModManager;
using LexTranslator.TranslateManage;
using LexTranslator.TranslateManagement;
using PhoenixEngine.TranslateManagement;

namespace LexTranslator.SkyrimManagement
{
    public class R_XmlReader
    {
        public class XmlItem
        {
            public string Type = "";
            public string EditorID = "";
            public string REC = "";
            public string Key = "";
            public string SourceText = "";
            public string TransText = "";

            public XmlItem(StringItem Item)
            {
                this.Type = "Xml";
                this.EditorID = Item.EDID;
                this.REC = Item.REC;

                this.Key =Crc32Helper.ComputeCrc32(Item.EDID + "_" + Item.REC);
                this.SourceText = Item.Source;

                if (Item.Source == Item.Dest)
                {
                    this.TransText = string.Empty;
                }
                else 
                {
                    this.TransText = Item.Dest;
                    TranslatorInterface.Instance.SetLink(this.Key,Item.Dest);
                }
            }

            public string GetTextIfTrans()
            {
                string GetKey = this.Key;
                var GetResult = TranslatorInterface.Instance.GetLink(GetKey);
                if (GetResult != null)
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

                if (this.TransText.Trim().Length > 0)
                {
                    return this.TransText;
                }

                return this.SourceText;
            }
        }
        public class StringItem
        {
            public string EDID { get; set; }
            public string REC { get; set; }
            public string Source { get; set; }
            public string Dest { get; set; }
        }

        public List<XmlItem> XmlItems = new List<XmlItem>();
        public Encoding CurrentEncoding = null;
        public XDocument Instance = null;
        public void Load(string Path)
        {
            Close();
            CurrentEncoding = DataHelper.GetFileEncodeType(Path);
            XDocument Doc = XDocument.Load(Path);
            Instance = Doc;
            try
            {
                foreach (var GetItem in
                  Doc.Descendants("String")
                  .Select(x => new StringItem
                  {
                      EDID = (string)x.Element("EDID"),
                      REC = (string)x.Element("REC"),
                      Source = (string)x.Element("Source"),
                      Dest = (string)x.Element("Dest")
                  })
                  .ToList())
                {
                    XmlItem SetItem = new XmlItem(GetItem);
                    XmlItems.Add(SetItem);
                }
            }
            catch 
            {
                MessageBoxExtend.Show(DeFine.WorkingWin, "This XML file format is not supported.");
            }
        }

        public void Close()
        {
            this.XmlItems.Clear();
            Instance = null;
        }

        public void Save(string Path)
        {
            var ItemDict = XmlItems.ToDictionary(x => x.EditorID + "|" + x.REC, x => x);

            foreach (var StringNode in Instance.Descendants("String"))
            {
                var EditorID = (string)StringNode.Element("EDID");
                var Rec = (string)StringNode.Element("REC");
                var DestNode = StringNode.Element("Dest");
                if (EditorID != null && Rec != null)
                {
                    string Key = EditorID + "|" + Rec;
                    if (ItemDict.TryGetValue(Key, out XmlItem Item))
                    {
                        Item.GetTextIfTrans();
                        DestNode.Value = Item.TransText;
                    }
                }
            }

            Instance.Save(Path);

            Close();
        }
    }
}
