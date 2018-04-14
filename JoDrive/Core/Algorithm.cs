using System;

namespace JoDrive.Core
{
    public static class Algorithm
    {
        const uint Adler32_Base = 65521U;
        const int Adler32_MaxN = 4096;


        public static uint Adler32(ByteBuffer buffer, int len)
        {
            byte[] bytes = buffer.GetByteBuffer(out int start);
            return Adler32(bytes, start, start + len);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="end">不包括end位置上的数据</param>
        /// <returns></returns>
        public static uint Adler32(byte[] data, int start, int end)
        {
            uint a = 1, b = 0;
            int pos = start;
            int l = (end - start) & 3;
            for (int s = 0; s < l; s++)
            {
                a += data[pos++];
                b += a;
            }
            int n = 0;
            while ((n = Math.Min(end - pos, Adler32_MaxN)) > 0)
            {
                adler32_sum(data, pos, pos + n, ref a, ref b);
                pos += n;
            }
            return b << 16 | a;
        }
        public static uint Adler32Rolling(uint adler32, int len, byte last, byte next)
        {
            uint a = adler32 & 0xFFFFU;
            uint b = adler32 >> 16;
            if (last > a + next)
                a += Adler32_Base;
            a = (a - last + next) % Adler32_Base;

            uint t = (uint)(last * len - a + 1);
            if (last * len + 1 > a + b)
                b += (t / Adler32_Base + 1) * Adler32_Base;

            b = (b - t) % Adler32_Base;
            return b << 16 | a;
        }

        private static void adler32_sum(byte[] data, int start, int end, ref uint a, ref uint b)
        {
            while (start < end)
            {
                a += data[start]; b += a;
                a += data[start + 1]; b += a;
                a += data[start + 2]; b += a;
                a += data[start + 3]; b += a;
                start += 4;
            }
            a %= Adler32_Base;
            b %= Adler32_Base;
        }
    }
}
