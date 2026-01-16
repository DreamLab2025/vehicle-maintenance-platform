namespace Verender.Common.Shared
{
    public static class ColorCode
    {
        public static bool IsHex(string hexCode)
        {
            if (string.IsNullOrEmpty(hexCode) || hexCode.Length != 7 || hexCode[0] != '#')
            {
                return false;
            }
            for (int i = 1; i < hexCode.Length; i++)
            {
                char c = hexCode[i];
                bool isHexDigit = (c >= '0' && c <= '9') ||
                                  (c >= 'A' && c <= 'F') ||
                                  (c >= 'a' && c <= 'f');
                if (!isHexDigit)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
