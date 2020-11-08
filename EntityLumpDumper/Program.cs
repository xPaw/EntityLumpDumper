using System;
using System.IO;
using System.Linq;
using LibBSP;
using SevenZip.Compression.LZMA;

namespace EntityLumpDumper
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine($"Provide path to the bsp file");
                Environment.Exit(1);
            }

            var map = args[0];

            if (!File.Exists(map))
            {
                Console.Error.WriteLine($"Map does not exist: {map}");
                Environment.Exit(1);
            }

            Console.WriteLine($"Reading {map}");

            var bsp = new BSP(map);
            var bytes = bsp.reader.ReadLump(bsp.reader.GetLumpInfo(Entity.GetIndexForLump(bsp.version), bsp.version));

            // Detect LZMA header
            if (bytes.Length > 4 && bytes[0] == 0x4C && bytes[1] == 0x5A && bytes[2] == 0x4D && bytes[3] == 0x41)
            {
                var uncompressedSize = BitConverter.ToInt32(bytes, 4);
                var compressedSize = BitConverter.ToInt32(bytes, 8);
                
                var decoder = new Decoder();

                // Skip LZMA + actual size + compressed size
                decoder.SetDecoderProperties(bytes.Skip(12).Take(5).ToArray());

                // Skip entire lzma header
                using var inputStream = new MemoryStream(bytes, 17, compressedSize);
                using var outStream = new MemoryStream(uncompressedSize);

                decoder.Code(inputStream, outStream, compressedSize, uncompressedSize, null);

                bytes = outStream.ToArray();
            }

            if (bytes.Length > 0 && bytes[^1] == 0x00)
            {
                bytes[^1] = 0x0A;
            }
            
            var dump = Path.ChangeExtension(map, "lmp");
            File.WriteAllBytes(dump, bytes);

            Console.WriteLine($"Written {bytes.Length} bytes to {Path.GetFileName(dump)}");
        }
    }
}
