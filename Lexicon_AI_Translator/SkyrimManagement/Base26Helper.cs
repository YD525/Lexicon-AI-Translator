using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexTranslator.SkyrimManagement
{
    public class Base26Helper
    {
        public static string EncodeBase26(int Value)
        {
            if (Value < 0)
                throw new ArgumentOutOfRangeException(nameof(Value), "Value must be non-negative.");

            const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (Value == 0)
                return "A";

            var NStringBuilder = new StringBuilder();
            while (Value > 0)
            {
                NStringBuilder.Insert(0, Chars[Value % 26]);
                Value /= 26;
            }
            return NStringBuilder.ToString();
        }

        public static int DecodeBase26(string Encoded)
        {
            if (string.IsNullOrEmpty(Encoded))
                throw new ArgumentException("Encoded string cannot be empty.");

            int Result = 0;
            foreach (char C in Encoded)
            {
                if (C < 'A' || C > 'Z')
                    throw new FormatException("Invalid character in encoded string.");
                Result = Result * 26 + (C - 'A');
            }
            return Result;
        }

    }
}
