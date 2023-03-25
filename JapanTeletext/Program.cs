using DeckLinkAPI;
using JapanTeletext;

// 同期符号はデータを含んでいない場合にも送出する必要がある
// SIは8Fとし、誤り訂正符号化を行って送出することが望ましい
var dummyDataLine = new byte[]
{
    0x55, 0x55, 0xa7, 0x8f, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00,
};
CyclicCode.Calc272190(dummyDataLine);

var prci = new PRCI(
    Header: new DataGroupHeader(
        DGI: 0,
        DGR: 0,
        DGL: true, // 非一括の場合番組データと最初のページデータは連結する
        DGC: 0),
    MagazineNumber: 0,
    ProgramNumber: 000, // 総目次番組
    PLV: 0, // レベルA
    PTC: 0b11_0_1, // 字幕_非一括_反復
    PDVR: 0b0_1, // プリンタ_ディスプレー
    PLS: 1, // 自動進行
    ProgramContentUpdateFlag: 0,
    new List<DataUnit>()
);

var programData = new ProgramData(
    SI: 6,
    PRCIData: prci,
    PACIData: new List<PACI>
    {
        new PACI(
            Header: new DataGroupHeader(
                DGI: 0,
                DGR: 0,
                DGL: false,
                DGC: 1),
            MagazineNumber: 0,
            ProgramNumber: 000, // 総目次番組
            PageNumber: 0,
            PLV: 0, // レベルA
            PTC: 0b11_0_1,
            PDVR: 0b0_1, // プリンタ_ディスプレー
            DMC: 0b1_0_0_00, // スーパー
            PRC: 0b1_0_00_0_1, // 音初期化禁止_画初期化禁止_未定義_ページ内容更新(字幕番組でない場合)_表示書き換え
            IPC: 0b00_000, // 標準密度(図形)_標準密度横書
            HeaderRasterColor: 8,
            RasterColor: 8,
            DataUnits: new List<DataUnit>
            {
                new DataUnit(
                    Parameter: 0x24,
                    LinkFlag: false,
                    Data: new byte[]
                    {
                        0x09, 0x85, 0x1D, 0x61, 0x0E, 0x62, 0x38, 0x33, 0x79, 0x49, 0x09, 0x38,
                        0x43, 0x31, 0x73, 0x09, 0x1D, 0x60, 0x0E, 0x41, 0x09, 0x87, 0x30, 0x31, 0x30, 0x2D, 0x30, 0x31,
                    }),
                new DataUnit(
                    Parameter: 0x20,
                    LinkFlag: false,
                    Data: new byte[]
                    {
                        0x09, 0x1D, 0x61, 0x83, 0xB3, 0xEC, 0xCF, 0x0E, 0x46, 0x6C, 0x53, 0x0F,
                        0x4A, 0x38, 0x3B, 0x7A, 0x42, 0x3F, 0x3D, 0x45, 0x4A, 0x7C, 0x41, 0x77, 0xCE, 0x3C, 0x42, 0x38,
                        0x33, 0x4A, 0x7C, 0x41, 0x77, 0xC7, 0xB9, 0xFA, 0xB3, 0xCE, 0x3F, 0x37, 0xB7, 0xA4, 0x4A, 0x7C,
                        0x41, 0x77, 0xC7, 0xCF, 0x4A, 0x38, 0x3B, 0x7A, 0x3E, 0x70, 0x4A, 0x73, 0xAC, 0x0E, 0x46, 0x6C,
                        0x53, 0xCE, 0x0F, 0x33, 0x28, 0xC8, 0x33, 0x28, 0xCE, 0x34, 0x56, 0xCE, 0xB9, 0xAD, 0xDE, 0xF2,
                        0x4D, 0x78, 0x4D, 0x51, 0xB7, 0xC6, 0x41, 0x77, 0xE9, 0xEC, 0xDE, 0xB9, 0xFA, 0x0D, 0x3C, 0x75,
                        0x3F, 0x2E, 0x3C, 0x54, 0xCF, 0xFD, 0x4A, 0x38, 0x3B, 0x7A, 0x3C, 0x75, 0x3F, 0x2E, 0x35, 0x21,
                        0xF2, 0x4D, 0x51, 0x30, 0x55, 0xB9, 0xEC, 0xD0, 0xFD, 0x32, 0x3F, 0x3C, 0x6F, 0x4E, 0x60, 0xAB,
                        0xCE, 0x4A, 0x38, 0x3B, 0x7A, 0x4A, 0x7C, 0x41, 0x77, 0x48, 0x56, 0x41, 0x48, 0xAB, 0xE9, 0x23,
                        0x31, 0xC4, 0xF2, 0xFD, 0x39, 0x25, 0xAD, 0xCA, 0xC8, 0xAD, 0xCB, 0x41, 0x2A, 0xD3, 0x3D, 0x50,
                        0xB7, 0xFD, 0x3C, 0x75, 0x41, 0x7C, 0x34, 0x49, 0xCB, 0x31, 0x47, 0xB7, 0x3D, 0x50, 0xB9, 0xB3,
                        0xC8, 0xAC, 0x3D, 0x50, 0x4D, 0x68, 0xDE, 0xB9, 0x89, 0xFA,
                    }
                )
            })
    }
);
var dataLines = programData.ToDataLines();

var iterator = new CDeckLinkIterator();
iterator.Next(out var deckLink);
var deckLinkOutput = (IDeckLinkOutput)deckLink;
deckLinkOutput.EnableVideoOutput(_BMDDisplayMode.bmdModeNTSC, _BMDVideoOutputFlags.bmdVideoOutputVANC);
deckLinkOutput.SetScheduledFrameCompletionCallback(new FrameCompletionCallback());
deckLinkOutput.GetDisplayMode(_BMDDisplayMode.bmdModeNTSC, out var displayMode);
displayMode.GetFrameRate(out var frameDuration, out var timeScale);
var width = displayMode.GetWidth();
var height = displayMode.GetHeight();

// 二値NRZで符号化
static void EncodeDataLine(IntPtr line, int start, int end, byte[] dataLine)
{
    unsafe
    {
        var p = (uint*)line;
        var len = 24 + 272 + 6;
        for (int i = 0; i < 720; i += 3)
        {
            // Cr0| Y0|Cb0
            //  Y2|Cb2| Y1
            // Cb4| Y3|Cr2
            //  Y5|Cr4| Y4
            p[i / 3 * 2 + 0] &= ~0x000003ffu;
            p[i / 3 * 2 + 0] |= 512; // Cb0
            p[i / 3 * 2 + 0] &= ~0x000ffc00u;
            p[i / 3 * 2 + 0] |= 64 << 10; // Y0
            p[i / 3 * 2 + 0] &= ~0x3ff00000u;
            p[i / 3 * 2 + 0] |= 512 << 20; // Cr0
        }
        for (int i = start; i < end; i++)
        {
            var index = (i - start) * len / (end - start);
            if (index >= (24 + 272))
                break;
            var p1 = ((dataLine[index / 8] >> (index % 8)) & 1) == 1 ? 920u : 0u;
            switch (i % 3)
            {
                case 0:
                    p[i / 3 * 2 + 0] &= ~0x000ffc00u;
                    p[i / 3 * 2 + 0] |= p1 << 10;
                    break;
                case 1:
                    p[i / 3 * 2 + 1] &= ~0x000003ffu;
                    p[i / 3 * 2 + 1] |= p1;
                    break;
                case 2:
                    p[i / 3 * 2 + 1] &= ~0x3ff00000u;
                    p[i / 3 * 2 + 1] |= p1 << 20;
                    break;
            }
        }
    }
}

while (true)
{
    // 同じCIのパケットを連続して送ることによって信頼性を上げる (規格外、PRCIを正常に送信できなくなる)
    // 本来はレベルBであればデータグループ単位で再送すべき
    var frames = 1;
    // 単一のフィールドのみを使う
    var singleField = false;
    var currentFrame = 0;
    for (int i = 0; i < (dataLines.Count + 1) / 2 * 2 + 2; i += singleField ? 1 : 2)
    {
        deckLinkOutput.CreateVideoFrame(width, height, width * 2, _BMDPixelFormat.bmdFormat8BitYUV, _BMDFrameFlags.bmdFrameFlagDefault, out var dataLineFrame);
        deckLinkOutput.CreateAncillaryData(_BMDPixelFormat.bmdFormat10BitYUV, out var anc);
        dataLineFrame.GetBytes(out var bytes);
        GenerateTestPattern(bytes, dataLineFrame.GetRowBytes(), width, height);
        var dataLine21h = dataLines.Count <= i ? dummyDataLine : dataLines[i].dataLine;
        anc.GetBufferForVerticalBlankingLine(21, out var vbiBuffer21h);
        EncodeDataLine(vbiBuffer21h, 4, width - 4, dataLine21h);
        if (!singleField)
        {
            var dataLine284h = dataLines.Count <= i + 1 ? dummyDataLine : dataLines[i + 1].dataLine;
            anc.GetBufferForVerticalBlankingLine(284, out var vblBuffer284h);
            EncodeDataLine(vblBuffer284h, 4, width - 4, dataLine284h);
        }
        else
        {
            anc.GetBufferForVerticalBlankingLine(284, out var vblBuffer284h);
            EncodeDataLine(vblBuffer284h, 4, width - 4, dataLine21h);
        }
        dataLineFrame.SetAncillaryData(anc);
        deckLinkOutput.ScheduleVideoFrame(dataLineFrame, currentFrame * frameDuration, frameDuration * frames, timeScale);
        currentFrame += frames;
    }
    deckLinkOutput.StartScheduledPlayback(0, timeScale, 1);
    while (true)
    {
        Thread.Sleep(1000);
        deckLinkOutput.GetBufferedVideoFrameCount(out var bfc);
        if (bfc == 0)
            break;
    }
    deckLinkOutput.StopScheduledPlayback(0, out var _, timeScale);
}

static void GenerateTestPattern(IntPtr bytes, int stride, int width, int height)
{
    unsafe
    {
        var p = (byte*)bytes;
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < width; x += 2)
            {
                p[stride * y + x * 2] = 128;
                p[stride * y + x * 2 + 1] = 16;
                p[stride * y + x * 2 + 2] = 128;
                p[stride * y + x * 2 + 3] = 16;
            }
        }
        for (int y = height - 16; y < height; y++)
        {
            for (int x = 0; x < width; x += 2)
            {
                p[stride * y + x * 2] = 128;
                p[stride * y + x * 2 + 1] = 16;
                p[stride * y + x * 2 + 2] = 128;
                p[stride * y + x * 2 + 3] = 16;
            }
        }
        for (int y = 16; y < height - 16; y++)
        {
            for (int x = 0; x < width; x += 2)
            {
                switch (x * 7 / 720)
                {
                    case 0:
                        p[stride * y + x * 2] = 128;
                        p[stride * y + x * 2 + 1] = 235;
                        p[stride * y + x * 2 + 2] = 128;
                        p[stride * y + x * 2 + 3] = 235;
                        break;
                    case 1:
                        p[stride * y + x * 2] = 44;
                        p[stride * y + x * 2 + 1] = 162;
                        p[stride * y + x * 2 + 2] = 142;
                        p[stride * y + x * 2 + 3] = 162;
                        break;
                    case 2:
                        p[stride * y + x * 2] = 156;
                        p[stride * y + x * 2 + 1] = 131;
                        p[stride * y + x * 2 + 2] = 44;
                        p[stride * y + x * 2 + 3] = 131;
                        break;
                    case 3:
                        p[stride * y + x * 2] = 72;
                        p[stride * y + x * 2 + 1] = 113;
                        p[stride * y + x * 2 + 2] = 58;
                        p[stride * y + x * 2 + 3] = 113;
                        break;
                    case 4:
                        p[stride * y + x * 2] = 184;
                        p[stride * y + x * 2 + 1] = 84;
                        p[stride * y + x * 2 + 2] = 198;
                        p[stride * y + x * 2 + 3] = 84;
                        break;
                    case 5:
                        p[stride * y + x * 2] = 100;
                        p[stride * y + x * 2 + 1] = 65;
                        p[stride * y + x * 2 + 2] = 212;
                        p[stride * y + x * 2 + 3] = 65;
                        break;
                    case 6:
                        p[stride * y + x * 2] = 212;
                        p[stride * y + x * 2 + 1] = 35;
                        p[stride * y + x * 2 + 2] = 114;
                        p[stride * y + x * 2 + 3] = 35;
                        break;
                }
            }
        }
    }
}
class FrameCompletionCallback : IDeckLinkVideoOutputCallback
{
    public void ScheduledFrameCompleted(IDeckLinkVideoFrame completedFrame, _BMDOutputFrameCompletionResult result)
    {
        Console.WriteLine($"COMPLETED {result}");
    }

    public void ScheduledPlaybackHasStopped()
    {

    }
}
