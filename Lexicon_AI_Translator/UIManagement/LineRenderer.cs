using LexTranslator.UIManage;

namespace LexTranslator.UIManagement
{
    public class LineRenderer
    {
        public static FakeGrid CreateLine(string Type, string EditorID, string Key, string SourceText, string TransText, double Score)
        {
            return UIHelper.CreateFakeLine(Type,Key,SourceText,TransText,Score);
        }
    }
}
