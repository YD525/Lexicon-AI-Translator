using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PhoenixTranslator.SkyrimModManager;
using PhoenixEngine.Common;
using PhoenixEngine.Memory;
using PhoenixEngine.Translate;
using PhoenixTranslator.ApplicationLayer;

namespace PhoenixTranslator.SkyrimManagement
{
    public class XmlItem
    {
        public string Type = "";
        public string EditorID = "";
        public string REC = "";
        public int RECID = 0;
        public string Key = "";
        public string SourceText = "";
        public string TransText = "";

        public XmlItem(Translator TranslatorRef, XMLStringItem Item)
        {
            this.Type = "Xml";
            this.EditorID = Item.EDID;
            this.REC = Item.REC;

            if (Item.RECID != null)
            {
                this.RECID = P_Convert.ObjToInt(Item.RECID);
            }

            this.Key = this.RECID + "_" + Item.EDID + "_" + Item.REC;
            this.SourceText = Item.Source;

            if (Item.Source == Item.Dest)
            {
                this.TransText = string.Empty;
            }
            else
            {
                this.TransText = Item.Dest;
                TranslatorRef.GetLink().Add(this.Key, new P_String(Item.Dest,0));
            }
        }

        public string GetTextIfTrans(Translator TranslatorRef)
        {
            string GetKey = this.Key;
            var Link = TranslatorRef.GetLink();

            var GetResult = Link[GetKey];
            if (GetResult != null)
            {
                this.TransText = GetResult.String;
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
    public class XMLStringItem
    {
        public string EDID { get; set; }
        public string REC { get; set; }

        public object RECID { get; set; }
        public string Source { get; set; }
        public string Dest { get; set; }
    }

    public class R_XmlReader
    {
        /// <summary>
        /// Gets or sets whether invalid XML is reported to the caller instead of the legacy modal boundary.
        /// </summary>
        public bool ThrowOnInvalidFormat { get; set; }

        public Translator TranslatorRef = null;
        public R_XmlReader(Translator TranslatorRef)
        {
            this.TranslatorRef = TranslatorRef;
        }

        public List<XmlItem> XmlItems = new List<XmlItem>();
        public Encoding CurrentEncoding = null;
        public XDocument Instance = null;

        public HashSet<string> UniqueKeys = new HashSet<string>();
        public void Load(string Path)
        {
            UniqueKeys.Clear();
            Close();
            CurrentEncoding = DataHelper.GetFileEncodeType(Path);
            XDocument Doc = XDocument.Load(Path);
            Instance = Doc;
            try
            {
                foreach (var GetItem in
                  Doc.Descendants("String")
                  .Select(x => new XMLStringItem
                  {
                      EDID = (string)x.Element("EDID"),
                      REC = (string)x.Element("REC"),
                      RECID = x.Element("REC")?.Attribute("id")?.Value,
                      Source = (string)x.Element("Source"),
                      Dest = (string)x.Element("Dest")
                  })
                  .ToList())
                {
                    XmlItem SetItem = new XmlItem(TranslatorRef,GetItem);
                    if (!UniqueKeys.Contains(SetItem.Key))
                    {
                        UniqueKeys.Add(SetItem.Key);
                        XmlItems.Add(SetItem);
                    }
                }
            }
            catch (System.Exception exception)
            {
                if (ThrowOnInvalidFormat)
                {
                    throw new System.IO.InvalidDataException(
                        "The XML translation format is unsupported.",
                        exception);
                }

                MessageBoxExtend.Show(PhoenixApp.WorkWin, "Msg","This XML file format is not supported.", PreviewDialogSeverity.Error);
            }
        }

        public void Close()
        {
            UniqueKeys.Clear();
            this.XmlItems.Clear();
            Instance = null;
        }

        public void Save(string Path)
        {
            var ItemDict = XmlItems.ToDictionary(x =>x.RECID +"_" + x.EditorID + "_" + x.REC, x => x);

            foreach (var StringNode in Instance.Descendants("String"))
            {
                var EditorID = (string)StringNode.Element("EDID");
                var Rec = (string)StringNode.Element("REC");
                var DestNode = StringNode.Element("Dest");
                int RECID = 0;

                if ((StringNode.Element("REC")?.Attribute("id")) != null)
                {
                    RECID = P_Convert.ObjToInt((StringNode.Element("REC")?.Attribute("id")?.Value));
                }

                if (EditorID != null && Rec != null)
                {
                    string Key = RECID +"_"+ EditorID + "_" + Rec;
                    if (ItemDict.TryGetValue(Key, out XmlItem Item))
                    {
                        Item.GetTextIfTrans(TranslatorRef);
                        DestNode.Value = Item.TransText;
                    }
                }
            }

            Instance.Save(Path);

            Close();
        }
    }
}
