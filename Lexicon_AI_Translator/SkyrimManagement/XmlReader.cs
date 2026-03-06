using System;
using System.Linq;
using System.Xml.Linq;

namespace LexTranslator.SkyrimManagement
{
    public class XmlReader
    {
        public class MCMItem
        {
            public string Type = "";
            public string EditorID = "";
            public string Key = "";
            public string SourceText = "";
            public string TransText = "";

            public MCMItem(string EditorID, string SourceText)
            {
                this.Type = "Xml";
                this.EditorID = EditorID;

                this.Key = "";
                this.SourceText = SourceText;
                this.TransText = string.Empty;
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

        public void Load(string Path)
        {
            XDocument Doc = XDocument.Load(Path);

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

            }
        }
    }
}
