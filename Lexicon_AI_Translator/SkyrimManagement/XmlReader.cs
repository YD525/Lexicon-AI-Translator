using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using LexTranslator.SkyrimModManager;
using LexTranslator.TranslateManagement;

namespace LexTranslator.SkyrimManagement
{
    public class R_XmlReader
    {
        public class XmlItem
        {
            public string Type = "";
            public string EditorID = "";
            public string Key = "";
            public string SourceText = "";
            public string TransText = "";

            public XmlItem(StringItem Item)
            {
                this.Type = "Xml";
                this.EditorID = Item.EDID;

                this.Key =Crc32Helper.ComputeCrc32(Item.EDID + "_" + Item.REC);
                this.SourceText = Item.Source;

                if (Item.Source == Item.Dest)
                {
                    this.TransText = string.Empty;
                }
                else 
                {
                    this.TransText = Item.Dest;
                }
            }

            public string GetTextIfTrans()
            {
                //if (this.TransText.Trim().Length > 0)
                //{
                //    return this.TransText;
                //}
                //string GetKey = this.Key;
                //var GetResult = TranslatorInterface.Instance.GetLink(GetKey);
                //if (GetResult != null)
                //{
                //    this.TransText = GetResult;
                //    if (this.TransText.Length > 0)
                //    {
                //        return this.TransText;
                //    }
                //    else
                //    {
                //        return this.SourceText;
                //    }
                //}

                //return this.SourceText;

                return string.Empty;
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

        public void Load(string Path)
        {
            Close();
            CurrentEncoding = DataHelper.GetFileEncodeType(Path);
            XDocument Doc = XDocument.Load(Path);
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
                    XmlItems.Add(new XmlItem(GetItem));
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
        }


        public void Save()
        { 
        
        }
    }
}
