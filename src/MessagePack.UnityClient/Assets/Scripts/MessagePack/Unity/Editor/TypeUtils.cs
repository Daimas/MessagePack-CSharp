using System;
using System.Text;

namespace MessagePack.Unity
{
    public static class TypeUtils
    {
        public static string GetRealFullName(this Type t)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("global::");
            if (t.IsGenericType)
            {
                sb.Append(t.FullName.Substring(0, t.FullName.IndexOf('`')));
                sb.Append('<');
                bool appendComma = false;
                foreach (Type arg in t.GetGenericArguments())
                {
                    if (appendComma) sb.Append(',');
                    sb.Append(GetRealFullName(arg));
                    appendComma = true;
                }
                sb.Append('>');
            }
            else
            {
                sb.Append(t.FullName);
            }

            sb.Replace('+', '.');
            return sb.ToString();
        }
    }
}