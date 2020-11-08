using System;
using System.IO;
using LibBSP;

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
