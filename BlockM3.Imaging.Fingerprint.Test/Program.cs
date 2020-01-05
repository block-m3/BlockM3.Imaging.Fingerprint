using System;
using System.IO;
using System.Text;
using SkiaSharp;

namespace BlockM3.Imaging.Fingerprint.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2 || !File.Exists(args[0]))
            {
                Console.WriteLine("Usage: imagefile \"fingerprint\"");
                Console.WriteLine("Example:  image.png \"This text is going to be embedded\"");
                return;
            }

            string type = Path.GetExtension(args[0]);
            SKEncodedImageFormat format = SKEncodedImageFormat.Jpeg;

            switch (type)
            {
                case ".jpg":
                case ".jpeg":
                    format = SKEncodedImageFormat.Jpeg;
                    break;
                case ".png":
                    format = SKEncodedImageFormat.Png;
                    break;
                default:
                    Console.WriteLine("Invalid image format, supported formats: JPG, PNG");
                    return;
            }

            string destfilename = Path.Combine(Path.GetDirectoryName(args[0]), Path.GetFileNameWithoutExtension(args[0]) + "_fingerprinted" + type);

            SKBitmap bitmap = SKBitmap.Decode(args[0]);
            bool use7bits = true;
            foreach (char c in args[1])
            {
                if (c > 127)
                    use7bits = false;
            }

            byte[] data = use7bits ? args[1].Encode7Bits() : Encoding.UTF8.GetBytes(args[1]);
            SKBitmap dest = Fingerprint.Insert(bitmap, data);
            SKImage im = SKImage.FromBitmap(dest);
            SKData dt = im.Encode(format, 100);
            Stream s = File.OpenWrite(destfilename);
            dt.SaveTo(s);
            s.Close();
            SKBitmap bm2 = SKBitmap.Decode(destfilename);
            string result = use7bits ? Fingerprint.Extract(bm2, data.Length)?.Decode7Bits() : Encoding.UTF8.GetString(Fingerprint.Extract(bm2, data.Length) ?? new byte[0]);
            if (result == args[1])
            {
                Console.WriteLine("Fingerprint written sucesfully in " + destfilename);
            }
            else
            {
                Console.WriteLine("Unable to write fingerprint in " + destfilename);
            }
        }
    }
}