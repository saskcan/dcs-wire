using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCSWireUtils
{
    static public class Utils
    {
        static public string arrayToStringUntil(char[] src, int srcStart, int srcMax, char delim)
        {
            int i = srcStart;
            while (i <= srcMax)
            {
                if (src[i] == delim)
                {
                    break;
                }
                i = i + 1;
            }
            char[] value = new char[i - srcStart];
            Array.Copy(src, srcStart, value, 0, i - srcStart);
            return new string(value);
        }
    }
}
