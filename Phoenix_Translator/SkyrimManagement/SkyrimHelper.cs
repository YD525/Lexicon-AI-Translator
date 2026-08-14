namespace PhoenixTranslator.SkyrimManagement
{
    public class SkyrimData
    {
        public static string GenUniqueKey(string EditorID, string SetType)
        {
            return (EditorID + "(" + SetType + ")");
        }
    }
}
