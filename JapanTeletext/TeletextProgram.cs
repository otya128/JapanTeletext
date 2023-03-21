namespace JapanTeletext
{
    record ProgramData(byte SI, PRCI? PRCIData, IList<PACI> PACIData)
    {
        const byte CR = 0x55; // クロックランイン
        const byte FC = 0xa7; // フレーミングコード
        const byte SOH = 0x01; // ヘッディング開始符号
        const byte STX = 0x02; // テキスト開始符号
        const byte ETX = 0x03; // テキスト終結符号
        const byte EOT = 0x04; // 伝送終了符号
        const byte ETB = 0x17; // 伝送ブロック終結符号
        const byte RS = 0x1e; // 情報分離符号
        const byte US = 0x1f; // データユニット分離符号

        static (byte[] dataLine, byte[] dataBlock) DataBlockToDataLine(byte[] dataBlock, byte SI, byte CI, bool TF, bool IF)
        {
            if (dataBlock.Length != 22)
            {
                throw new ArgumentException(null, nameof(dataBlock));
            }
            var dataLine = new byte[37];
            // ビット同期符号
            dataLine[0] = CR;
            dataLine[1] = CR;
            // バイト同期符号
            dataLine[2] = FC;
            switch (SI)
            {
                // 逐次受信処理: 字幕など
                case 0b0010: // 送出第1モード (逐次受信処理)
                    dataLine[3] = 0b10010010;
                    break;
                case 0b1010: // 送出第2モード (逐次受信処理)
                    dataLine[3] = 0b00101010;
                    break;
                case 0b0110: // 送出第3モード (記録受信処理)
                    dataLine[3] = 0b01000110;
                    break;
                case 0b1110: // 送出第4モード (記録受信処理)
                    dataLine[3] = 0b11111110;
                    break;
                case 0b0111: // 補助信号 (番組索引データ)
                    dataLine[3] = 0b00110111;
                    break;
                case 0b1111: // 運用信号
                    dataLine[3] = 0b10001111;
                    break;
                default:
                    throw new Exception();
            }
            dataLine[4] = CI; // CI
            if (TF)
            {
                dataLine[4] |= 0b010000; // TF 伝送制御フラグ
            }
            if (IF)
            {
                dataLine[4] |= 0b100000; // IF 誤り検出符号化区間識別フラグ (SOH/STXを含データパケット)
            }
            for (int i = 0; i < 22; i++)
            {
                dataLine[4 + i] |= (byte)((dataBlock[i] & 0b11) << 6);
                dataLine[4 + i + 1] = (byte)(dataBlock[i] >> 2);
            }
            CyclicCode.Calc272190(dataLine);
            return (dataLine, dataBlock);
        }

        static void SetDataUnitCRC16(IList<byte[]> dataUnit)
        {
            ushort crc16 = 0;
            var lastDataBlock = dataUnit.Last();
            foreach (var dataBlock in dataUnit.SkipLast(1))
            {
                crc16 = CyclicCode.CRC16(dataBlock, crc16);
            }
            crc16 = CyclicCode.CRC16(new(lastDataBlock, 0, 20), crc16);
            for (int i = 0; i < 16; i++)
            {
                if ((crc16 & (1 << (16 - i - 1))) != 0)
                {
                    lastDataBlock[20 + i / 8] |= (byte)(1 << (i % 8));
                }
                else
                {
                    lastDataBlock[20 + i / 8] &= (byte)~(1 << (i % 8));
                }
            }
        }
        public static void FillDataBlock(DataGroupHeader header, byte[] dataBlock, int dataGroupSize)
        {
            dataBlock[0] = SOH;
            dataBlock[1] = (byte)(header.DGI << 4);
            dataBlock[1] |= (byte)(header.DGR << 0);
            dataBlock[2] = (byte)(header.DGL ? 0x80 : 0);
            dataBlock[2] |= (byte)(header.DGC << 0);
            dataBlock[3] = (byte)(dataGroupSize >> 8);
            dataBlock[4] = (byte)(dataGroupSize & 0xff);
        }
        static byte[] PRCIToDataBlock(PRCI prci, byte numberOfPages)
        {
            var dataBlock = new byte[22];
            FillDataBlock(prci.Header, dataBlock, dataGroupSize: 1);
            dataBlock[7] = RS;
            dataBlock[8] = 0x20; // データヘッダパラメータ
            dataBlock[9] = 9; // データヘッダデータ長 (HL)
            dataBlock[10] = (byte)(prci.MagazineNumber << 4);
            dataBlock[10] |= (byte)(prci.ProgramNumber / 100 % 10);
            dataBlock[11] = (byte)((prci.ProgramNumber / 10 % 10) << 4);
            dataBlock[11] |= (byte)(prci.ProgramNumber % 10);
            dataBlock[12] = numberOfPages;
            dataBlock[13] = (byte)(prci.PLV << 4);
            dataBlock[13] |= prci.PTC;
            dataBlock[14] = prci.PDVR;
            dataBlock[15] = prci.PLS;
            dataBlock[16] = prci.ProgramContentUpdateFlag;
            if (prci.DataUnits.Count != 0)
            {
                throw new NotImplementedException();
            }
            // 番組データ長
            dataBlock[17] = 0;
            dataBlock[18] = 0;
            dataBlock[19] = ETX;
            SetDataUnitCRC16(new[] { dataBlock });
            return dataBlock;
        }

        static byte[] PACIToDataBlock(PACI paci, int dataGroupSize)
        {
            var dataBlock = new byte[22];
            FillDataBlock(paci.Header, dataBlock, dataGroupSize);
            const byte RS = 0x1e; // 情報分離符号
            dataBlock[7] = RS;
            dataBlock[8] = 0x21; // データヘッダパラメータ
            dataBlock[9] = 9; // データヘッダデータ長 (HL)
            dataBlock[10] = (byte)(paci.MagazineNumber << 4);
            dataBlock[10] |= (byte)(paci.ProgramNumber / 100 % 10);
            dataBlock[11] = (byte)((paci.ProgramNumber / 10 % 10) << 4);
            dataBlock[11] |= (byte)(paci.ProgramNumber % 10);
            dataBlock[12] = (byte)((paci.PageNumber / 10 % 10) << 4);
            dataBlock[12] |= (byte)(paci.PageNumber % 10);
            dataBlock[13] = (byte)(paci.PLV << 4);
            dataBlock[13] |= paci.PTC;
            dataBlock[14] = paci.PDVR;
            dataBlock[15] = paci.DMC;
            dataBlock[16] = paci.PRC;
            dataBlock[17] = paci.IPC;
            dataBlock[18] = (byte)(paci.HeaderRasterColor << 4);
            dataBlock[18] |= paci.RasterColor;
            dataBlock[19] = ETB;
            SetDataUnitCRC16(new[] { dataBlock });
            return dataBlock;
        }

        static IEnumerable<byte[]> DataUnitToDataBlocks(DataUnit dataUnit, byte end)
        {
            var dataBlocks = new List<byte[]>();
            var headDataBlock = new byte[22];
            dataBlocks.Add(headDataBlock);
            headDataBlock[0] = STX;
            headDataBlock[1] = US;
            headDataBlock[2] = dataUnit.Parameter;
            headDataBlock[3] = (byte)(dataUnit.Data.Length >> 8);
            if (dataUnit.LinkFlag)
            {
                headDataBlock[3] |= 0x80;
            }
            headDataBlock[4] = (byte)(dataUnit.Data.Length & 0xff);
            var headerSize = 5;
            var trailerSize = 3; // 終端符号+CRC-16
            if (dataUnit.Data.Length <= headDataBlock.Length - headerSize - trailerSize)
            {
                Array.Copy(dataUnit.Data, 0, headDataBlock, headerSize, dataUnit.Data.Length);
                headDataBlock[19] = end; // 終端符号
            }
            else
            {
                var offset = Math.Min(headDataBlock.Length - headerSize, dataUnit.Data.Length);
                Array.Copy(dataUnit.Data, 0, headDataBlock, headerSize, offset);
                while (true)
                {
                    var dataBlock = new byte[22];
                    dataBlocks.Add(dataBlock);
                    if (dataUnit.Data.Length - offset <= dataBlock.Length - trailerSize)
                    {
                        Array.Copy(dataUnit.Data, offset, dataBlock, 0, dataUnit.Data.Length - offset);
                        dataBlock[19] = end;
                        break;
                    }
                    var copySize = Math.Min(dataBlock.Length, dataUnit.Data.Length - offset);
                    Array.Copy(dataUnit.Data, offset, dataBlock, 0, copySize);
                    offset += copySize;
                }
            }
            SetDataUnitCRC16(dataBlocks);
            return dataBlocks;
        }
        public List<(byte[] dataLine, byte[] dataBlock)> ToDataLines()
        {
            List<(byte[] dataLine, byte[] dataBlock)> dataLines = new();
            byte ci = 0;
            var tf = true;
            if (PRCIData != null)
            {
                dataLines.Add(DataBlockToDataLine(PRCIToDataBlock(PRCIData, (byte)PACIData.Count), SI, CI: ci, TF: tf, IF: true));
                ci = (byte)((ci + 1) & 0xf);
                tf = false;
            }
            foreach (var paci in PACIData)
            {
                var placeHolder = dataLines.Count;
                var placeHolderCI = ci;
                var placeHolderTF = tf;
                tf = false;
                dataLines.Add((Array.Empty<byte>(), Array.Empty<byte>()));
                ci = (byte)((ci + 1) & 0xf);
                var dataGroupSize = 0;
                foreach (var (dataUnit, index) in paci.DataUnits.Select((value, index) => (value, index)))
                {
                    bool @if = true;
                    foreach (var dataBlock in DataUnitToDataBlocks(dataUnit, end: index == paci.DataUnits.Count - 1 ? EOT : ETB))
                    {
                        dataLines.Add(DataBlockToDataLine(dataBlock, SI, CI: ci, TF: false, IF: @if));
                        dataGroupSize++;
                        ci = (byte)((ci + 1) & 0xf);
                        @if = false;
                    }
                }
                dataLines[placeHolder] = (DataBlockToDataLine(PACIToDataBlock(paci, dataGroupSize + 1), SI, CI: placeHolderCI, TF: placeHolderTF, IF: true));
            }
            return dataLines;
        }
    }

    // データグループヘッダ 
    record DataGroupHeader
    (
        // データグループ識別符号
        // 番組管理データまたはページデータ: 0
        // 番組索引データ: 15
        byte DGI,
        // データグループ再送符号
        byte DGR,
        // データグループリンク符号
        bool DGL,
        // データグループ連続番号
        byte DGC
    );
    // 番組データヘッダ
    record PRCI
    (
        DataGroupHeader Header,
        // マガジン番号
        byte MagazineNumber,
        // 番組番号
        ushort ProgramNumber,
        // (ページ総数)
        // 機能レベル
        byte PLV,
        // 番組形態
        byte PTC,
        // 番組提示デバイス
        byte PDVR,
        // ページ進行
        byte PLS,
        // 番組内容更新フラグ
        byte ProgramContentUpdateFlag,
        // (番組データ長)
        IList<DataUnit> DataUnits
    );
    /// <summary>
    /// ページデータヘッダ
    /// </summary>
    /// <param name="Header"></param>
    /// <param name="MagazineNumber">マガジン番号</param>
    /// <param name="ProgramNumber">番組番号</param>
    /// <param name="PageNumber">ページ番号</param>
    /// <param name="PLV">機能レベル</param>
    /// <param name="PTC">番組形態</param>
    /// <param name="PDVR">ページ提示デバイス</param>
    /// <param name="DMC">表示モード制御</param>
    /// <param name="PRC">提示更新制御</param>
    /// <param name="IPC">初期提示制御</param>
    /// <param name="HeaderRasterColor">ヘッダラスタ色</param>
    /// <param name="RasterColor">ラスタ色</param>
    /// <param name="DataUnits"></param>
    record PACI
    (
        DataGroupHeader Header,
        // マガジン番号
        byte MagazineNumber,
        // 番組番号
        ushort ProgramNumber,
        // ページ番号
        byte PageNumber,
        // 機能レベル
        byte PLV,
        // 番組形態
        byte PTC,
        // ページ提示デバイス
        byte PDVR,
        // 表示モード制御
        byte DMC,
        // 提示更新制御
        byte PRC,
        // 初期提示制御
        byte IPC,
        // ヘッダラスタ色
        byte HeaderRasterColor,
        // ラスタ色
        byte RasterColor,
        IList<DataUnit> DataUnits
    );

    record DataUnit(byte Parameter, bool LinkFlag, byte[] Data);
}
