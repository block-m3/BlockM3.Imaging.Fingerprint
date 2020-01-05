using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BlockM3.Imaging.Fingerprint
{
    public static class Extensions
    {
        public static byte[] ToByteArray(this BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        public static byte[] Encode7Bits(this string data)
        {
            List<bool> b=new List<bool>();
            foreach (char d in data)
            {
                int c = d;
                b.Add((c&1)==1);
                c = c >> 1;
                b.Add((c&1)==1);
                c = c >> 1;
                b.Add((c&1)==1);
                c = c >> 1;
                b.Add((c&1)==1);
                c = c >> 1;
                b.Add((c&1)==1);
                c = c >> 1;
                b.Add((c&1)==1);
                c = c >> 1;
                b.Add((c&1)==1);
            }
            BitArray ba=new BitArray(b.ToArray());
            return ba.ToByteArray();
        }

        public static string Decode7Bits(this byte[] data)
        {
            BitArray ba = new BitArray(data);
            StringBuilder bld = new StringBuilder();
            for (int x = 0; x < ba.Length-6; x += 7)
            {
                int c = 0;
                if (ba[x+6])
                    c |= 1;
                c = c << 1;
                if (ba[x+5])
                    c |= 1;
                c = c << 1;
                if (ba[x+4])
                    c |= 1;
                c = c << 1;
                if (ba[x+3])
                    c |= 1;
                c = c << 1;
                if (ba[x+2])
                    c |= 1;
                c = c << 1;
                if (ba[x+1])
                    c |= 1;
                c = c << 1;
                if (ba[x])
                    c |= 1;
                bld.Append((char) c);
            }
            return bld.ToString();
        }

    }
}
