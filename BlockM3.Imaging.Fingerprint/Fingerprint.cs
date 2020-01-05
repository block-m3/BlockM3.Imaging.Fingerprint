using System;
using System.Runtime.InteropServices;
using BlockM3.Imaging.Fingerprint.Channels;
using BlockM3.Imaging.Fingerprint.Methods;
using BlockM3.Imaging.Fingerprint.ReedSolomon;
using SkiaSharp;

namespace BlockM3.Imaging.Fingerprint
{
    public class Fingerprint
    {

        public static SKBitmap Insert(SKBitmap input, byte[] data, SubBand band=SubBand.HL)
        {
            SKColorType colorType = input.ColorType;
            int codewords = data.Length >> 1;
            byte[] enc = ReedSolomonAlgorithm.Encode(data, codewords);
            byte[] total=new byte[data.Length+enc.Length];
            Array.Copy(data,0,total,0,data.Length);
            Array.Copy(enc,0,total,data.Length,enc.Length);
            if (colorType != SKColorType.Bgra8888)
                input = input.Copy(SKColorType.Bgra8888);
            (int clipX, int clipY, int clipWith, int clipHeight) = DWTDCT.GetBestClip(total.Length, input.Width, input.Height);
            byte[] bgra = input.Bytes;
            double[,] uChannel = DCbChannel.ExtractCbChannel(bgra, input.Width, clipX, clipY, clipWith, clipHeight);
            DWTDCT.Insert(uChannel, total, band);
            DCbChannel.InsertCbChannel(bgra,uChannel,input.Width, clipX, clipY, clipWith, clipHeight);
            var gcHandle = GCHandle.Alloc(bgra, GCHandleType.Pinned);
            var info=new SKImageInfo(input.Width, input.Height,SKColorType.Bgra8888,input.AlphaType);
            var bitmap = new SKBitmap();
            bitmap.InstallPixels(info,gcHandle.AddrOfPinnedObject(), info.RowBytes, delegate { gcHandle.Free(); });
            if (colorType != SKColorType.Bgra8888)
                bitmap = bitmap.Copy(colorType);
            return bitmap;
        }
        public static byte[] Extract(SKBitmap input, int dataLength, SubBand band=SubBand.HL)
        {
            SKColorType colorType = input.ColorType;
            if (colorType != SKColorType.Bgra8888)
                input = input.Copy(SKColorType.Bgra8888);
            return Extract(input.Bytes, input.Width, input.Height, dataLength, band);
        }
        public static byte[] Extract(byte[] bgra, int width, int height, int dataLength, SubBand band=SubBand.HL)
        {
            int codeWords = dataLength >> 1;
            int totalLength = dataLength + codeWords;
            (int clipX, int clipY, int clipWith, int clipHeight) = DWTDCT.GetBestClip(totalLength, width, height);
            double[,] uChannel = DCbChannel.ExtractCbChannel(bgra, width, clipX, clipY, clipWith, clipHeight);
            byte[] data = DWTDCT.Extract(uChannel, totalLength, band);
            byte[] code = new byte[dataLength];
            byte[] ecc=new byte[codeWords];
            Array.Copy(data, 0, code, 0, dataLength);
            Array.Copy(data, dataLength,ecc,0,codeWords);
            return ReedSolomonAlgorithm.Decode(code, ecc);
        }

        public static byte[] FastExtract(SKBitmap input, int dataLength, SubBand band=SubBand.HL)
        {
            SKColorType colorType = input.ColorType;
            if (colorType != SKColorType.Bgra8888)
                input = input.Copy(SKColorType.Bgra8888);
            return FastExtract(input.Bytes, input.Width, input.Height, dataLength, band);
        }
        public static byte[] FastExtract(byte[] bgra, int width, int height, int dataLength, SubBand band=SubBand.HL)
        {
            int codeWords = dataLength >> 1;
            int totalLength = dataLength + codeWords;
            (int clipX, int clipY, int clipWith, int clipHeight) = DWTDCT.GetBestClip(totalLength, width, height);
            float[,] uChannel = SCbChannel.ExtractCbChannel(bgra, width, clipX, clipY, clipWith, clipHeight);
            byte[] data = DWTDCT.Extract(uChannel, totalLength, band);
            byte[] code = new byte[dataLength];
            byte[] ecc=new byte[codeWords];
            Array.Copy(data, 0, code, 0, dataLength);
            Array.Copy(data, dataLength,ecc,0,codeWords);
            return ReedSolomonAlgorithm.Decode(code, ecc);
        }
    }
}
