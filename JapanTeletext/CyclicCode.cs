namespace JapanTeletext
{
    class CyclicCode
    {
        public static ushort CRC16(Span<byte> data, ushort r)
        {
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 1; j < 256; j += j)
                {
                    // x^16+x^12+x^5+1
                    ushort p = (1 << 12) + (1 << 5) + 1;
                    var bit = (data[i] & j) != 0;
                    bit = (((r >> 15) != 0 ? 1 : 0) ^ (bit ? 1 : 0)) != 0;
                    r <<= 1;
                    if (bit)
                    {
                        r ^= p;
                    }
                }
            }
            return r;
        }
        /// <summary>
        /// (272,190)短縮化差集合巡回符号を求める
        /// <paramref name="dataLine"/>の同期符号(24-bit)の次の190-bitを生成多項式(x^82+x^77+x^76+x^71+x^67+x^66+x^56+x^52+x^48+x^40+x^36+x^34+x^24+x^22+x^18+x^10+x^4+1)で割った余りを求め次の82-bitに書き込む
        /// </summary>
        /// <param name="dataLine">データライン (同期符号24-bit+272-bit)</param>
        public static void Calc272190(byte[] dataLine)
        {
            // ビット同期符号: 16bit
            // バイト同期符号: 8bit
            ulong r1 = 0;
            uint r2 = 0;
            for (int i = 24; i < 24 + 190; i++)
            {
                // x^82+x^77+x^76+x^71+x^67+x^66+x^56+x^52+x^48+x^40+x^36+x^34+x^24+x^22+x^18+x^10+x^4+1
                var p1 = (1uL << 56) + (1uL << 52) + (1uL << 48) + (1uL << 40) + (1uL << 36) + (1uL << 34) + (1uL << 24) + (1uL << 22) + (1uL << 18) + (1uL << 10) + (1uL << 4) + 1;
                var p2 = (1u << (77 - 64)) + (1u << (76 - 64)) + (1u << (71 - 64)) + (1u << (67 - 64)) + (1u << (66 - 64));
                var r2High = r2 & (1u << (81 - 64));
                var bit = (dataLine[i / 8] & (1 << (i % 8))) != 0;
                bit = ((r2High != 0 ? 1 : 0) ^ (bit ? 1 : 0)) != 0;
                var r1High = (uint)(r1 >> 63);
                r1 <<= 1;
                r2 <<= 1;
                r2 |= r1High;
                if (bit)
                {
                    r1 ^= p1;
                    r2 ^= p2;
                }
            }
            for (int i = 24 + 190; i < 24 + 190 + 82 - 64; i++)
            {
                if ((r2 & (1uL << (24 + 190 + 82 - 64 - i - 1))) != 0)
                {
                    dataLine[i / 8] |= (byte)(1 << (i % 8));
                }
                else
                {
                    dataLine[i / 8] &= (byte)~(1 << (i % 8));
                }
            }
            for (int i = 24 + 190 + 82 - 64; i < 24 + 272; i++)
            {
                if ((r1 & (1uL << (24 + 272 - i - 1))) != 0)
                {
                    dataLine[i / 8] |= (byte)(1 << (i % 8));
                }
                else
                {
                    dataLine[i / 8] &= (byte)~(1 << (i % 8));
                }
            }
        }
    }
}
