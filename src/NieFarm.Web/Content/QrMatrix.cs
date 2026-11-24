using System.Text;
using QRCoder;

namespace NieFarm.Web.Content;

/// <summary>
/// A QR code as a plain module grid (no quiet zone), built locally with QRCoder, so the same code can be written as a
/// PNG, an SVG or drawn as vector rectangles in a PDF (R-26). Error correction is Q (about 25 percent), because the
/// code sits on a bag that gets creased and scuffed. The payload is always a URL the server built itself
/// (<c>PublicUrls</c>), never text taken from a request.
/// </summary>
public sealed class QrMatrix
{
    /// <summary>The quiet zone every renderer must leave around the modules, in modules (the QR standard asks for 4).</summary>
    public const int QuietZone = 4;

    private readonly bool[,] _modules;

    private QrMatrix(bool[,] modules, int size)
    {
        _modules = modules;
        Size = size;
    }

    /// <summary>Modules per side, without the quiet zone.</summary>
    public int Size { get; }

    public bool IsDark(int row, int column) => _modules[row, column];

    public static QrMatrix Create(string payload)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);

        // QRCoder's ModuleMatrix carries its own quiet zone; the three finder patterns put a dark module on every
        // edge of the real symbol, so the bounding box of the dark modules is the symbol itself.
        var raw = data.ModuleMatrix;
        int min = int.MaxValue, max = -1;
        for (var r = 0; r < raw.Count; r++)
        {
            for (var c = 0; c < raw.Count; c++)
            {
                if (!raw[r][c])
                    continue;

                min = Math.Min(min, Math.Min(r, c));
                max = Math.Max(max, Math.Max(r, c));
            }
        }

        var size = max - min + 1;
        var modules = new bool[size, size];
        for (var r = 0; r < size; r++)
            for (var c = 0; c < size; c++)
                modules[r, c] = raw[r + min][c + min];

        return new QrMatrix(modules, size);
    }

    /// <summary>Horizontal runs of dark modules, in module units: one rectangle each when drawn as vectors.</summary>
    public IEnumerable<(int Row, int Column, int Length)> Runs()
    {
        for (var r = 0; r < Size; r++)
        {
            var c = 0;
            while (c < Size)
            {
                if (!_modules[r, c])
                {
                    c++;
                    continue;
                }

                var start = c;
                while (c < Size && _modules[r, c])
                    c++;

                yield return (r, start, c - start);
            }
        }
    }

    /// <summary>
    /// A black-on-white PNG with the quiet zone, <paramref name="pixelsPerModule"/> pixels per module. It is drawn from
    /// this grid (not by QRCoder's renderer) so PNG, SVG and PDF are guaranteed to show the very same modules.
    /// </summary>
    public byte[] ToPng(int pixelsPerModule = 20)
    {
        var modulesWide = Size + QuietZone * 2;
        var width = modulesWide * pixelsPerModule;

        // 1-bit greyscale, one filter byte (0 = none) per scanline.
        var stride = (width + 7) / 8;
        var raw = new byte[(stride + 1) * width];
        for (var y = 0; y < width; y++)
        {
            var offset = y * (stride + 1);
            raw[offset] = 0;
            var row = y / pixelsPerModule - QuietZone;
            for (var x = 0; x < stride; x++)
                raw[offset + 1 + x] = 0xFF; // white

            if (row < 0 || row >= Size)
                continue;

            for (var x = 0; x < width; x++)
            {
                var column = x / pixelsPerModule - QuietZone;
                if (column < 0 || column >= Size || !_modules[row, column])
                    continue;

                raw[offset + 1 + x / 8] &= (byte)~(0x80 >> (x % 8)); // black = 0
            }
        }

        using var output = new MemoryStream();
        output.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        var header = new byte[13];
        WriteInt32(header, 0, width);
        WriteInt32(header, 4, width);
        header[8] = 1; // bit depth
        header[9] = 0; // greyscale
        WriteChunk(output, "IHDR", header);

        using (var compressed = new MemoryStream())
        {
            using (var zlib = new System.IO.Compression.ZLibStream(compressed, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
                zlib.Write(raw);

            WriteChunk(output, "IDAT", compressed.ToArray());
        }

        WriteChunk(output, "IEND", []);
        return output.ToArray();
    }

    /// <summary>
    /// A single-path SVG with the quiet zone. It contains no text and no external reference, which is why it is safe
    /// to serve from the application's own origin.
    /// </summary>
    public string ToSvg()
    {
        var total = Size + QuietZone * 2;
        var path = new StringBuilder();
        foreach (var (row, column, length) in Runs())
            path.Append(System.FormattableString.Invariant($"M{column + QuietZone} {row + QuietZone}h{length}v1h-{length}z"));

        return System.FormattableString.Invariant(
            $"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {total} {total}" width="{total * 10}" height="{total * 10}" shape-rendering="crispEdges"><rect width="{total}" height="{total}" fill="#fff"/><path d="{path}" fill="#000"/></svg>""");
    }

    public string ToPngDataUri(int pixelsPerModule = 8) => "data:image/png;base64," + Convert.ToBase64String(ToPng(pixelsPerModule));

    private static readonly uint[] CrcTable = BuildCrcTable();

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            var c = n;
            for (var k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            table[n] = c;
        }

        return table;
    }

    private static void WriteInt32(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }

    private static void WriteChunk(Stream output, string type, byte[] data)
    {
        var length = new byte[4];
        WriteInt32(length, 0, data.Length);
        output.Write(length);

        var typeBytes = Encoding.ASCII.GetBytes(type);
        output.Write(typeBytes);
        output.Write(data);

        var crc = 0xFFFFFFFFu;
        foreach (var b in typeBytes)
            crc = CrcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
        foreach (var b in data)
            crc = CrcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);

        var crcBytes = new byte[4];
        WriteInt32(crcBytes, 0, (int)(crc ^ 0xFFFFFFFFu));
        output.Write(crcBytes);
    }
}
