using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BlockM3.Imaging.Fingerprint.Math;

namespace BlockM3.Imaging.Fingerprint.Methods
{
    public class DWTDCT
    {
        private static (int, int, int) CalcLevel(int length, int width, int height)
        {
            int w = width;
            int h = height;
            int level = 0;
            int totalArea = length << 7; //DCT area size * 8 bits * length
            w = (w >> 2) << 2;
            h = (h >> 2) << 2;
            int area = w * h;
            while(area>totalArea)
            {
                int nw = w >> 1;
                nw = (nw >> 2) << 2;
                int nh = h >> 1;
                nh = (nh >> 2) << 2;
                area = nw * nh;
                if (area > totalArea)
                {
                    w = nw;
                    h = nh;
                    level++;
                }
            }
            return (w, h, level);
        }

        public static (int, int, int, int) GetBestClip(int length, int width, int height)
        {
            (int clipWidth, int clipHeight, int level) = CalcLevel(length, width / 16 * 15, height / 16 * 15);
            if (level==0)
                throw new ArgumentException("Unable to embed, data is too long");
            clipWidth <<= level;
            clipHeight <<= level;
            int clipX = (width - clipWidth) >> 1;
            int clipY = (height - clipHeight) >> 1;
            return (clipX, clipY, clipWidth, clipHeight);
        }
        private static (int,int) GetSubBandStart(int width, int height, int level, SubBand band)
        {
            int posx = 0;
            int posy = 0;
            width >>= level;
            height >>= level;
            switch (band)
            {
                case SubBand.LH:
                    posy += height;
                    break;
                case SubBand.HL:
                    posx += width;
                    break;
                case SubBand.HH:
                    posx += width;
                    posy += height;
                    break;
            }

            return (posx, posy);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ApplyFeature(double[,] dct, double k)
        {
            var delta = System.Math.Max(System.Math.Abs(dct[1, 2] + dct[2, 0] + dct[2, 1] + dct[2, 2]), 2) * k;
            dct[1, 2] += delta;
            dct[2, 0] += delta;
            dct[2, 1] += delta;
            dct[2, 2] += delta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetFeature(double[,] dct)
        {
            return (dct[1, 2] + dct[2, 0] + dct[2, 1] + dct[2, 2]) > 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetFeature(float[,] dct)
        {
            return (dct[1, 2] + dct[2, 0] + dct[2, 1] + dct[2, 2]) > 0;
        }
        public static void Insert(double[,] channel, byte[] insert, SubBand band)
        {
            int width = channel.GetUpperBound(0) + 1;
            int height = channel.GetUpperBound(1) + 1;
            (int maxW, int maxH, int level) = CalcLevel(insert.Length, width, height);
            double sigma = 6 / (double)(1<<(level-1));
            BitArray b=new BitArray(insert);
            DDWT.Forward(channel,level);
            (int posX, int posY)=GetSubBandStart(width, height, level, band);
            DDCT dct=new DDCT(4);
            int orgW = maxW>>2;
            maxH >>=2;
            maxW += posX;
            Parallel.For(0, maxH, (z) =>
            { 
                int opos = z * orgW;
                int y = z << 2 + posY;
                for (int x = posX; x < maxW; x += 4)
                {
                    int pos = opos + ((x-posX) >> 2);
                    if (pos < b.Length)
                    {
                        double[,] dctData = dct.Extract(channel, x, y);
                        ApplyFeature(dctData, b[pos] ? sigma : -sigma);
                        dct.Insert(dctData, channel, x, y);
                    }

                }
            });

            DDWT.Reverse(channel,level);
        }

        public static byte[] Extract(double[,] channel, int totalLength, SubBand band)
        {
            int width = channel.GetUpperBound(0) + 1;
            int height = channel.GetUpperBound(1) + 1;
            (int maxW, int maxH, int level) = CalcLevel(totalLength, width, height);
            (int posX, int posY) = GetSubBandStart(width, height, level, band);
            DDWT.Forward(channel, level);
            DDCT dct = new DDCT(4);
            int orgW = maxW>>2;
            maxW += posX;
            maxH >>= 2;
            bool[] res = new bool[maxH * orgW];
            Parallel.For(0, maxH, (z) =>
            {
                int opos = z * orgW;
                int y = z << 2 + posY;
                for (int x = posX; x < maxW; x += 4)
                {
                    int pos = opos + ((x - posX) >> 2);
                    res[pos] = GetFeature(dct.Extract(channel, x, y));
                }
            });
            BitArray bi=new BitArray(res);
            byte[] array = bi.ToByteArray();
            Array.Resize(ref array, totalLength);
            return array;
        }
        public static byte[] Extract(float[,] channel, int totalLength, SubBand band)
        {
            int width = channel.GetUpperBound(0) + 1;
            int height = channel.GetUpperBound(1) + 1;
            (int maxW, int maxH, int level) = CalcLevel(totalLength, width, height);
            (int posX, int posY)=GetSubBandStart(width, height, level, band);
            SDWT.Forward(channel, level);
            List<bool> b=new List<bool>();
            SDCT dct=new SDCT(4);
            maxW += posX;
            maxH >>=2;
            Parallel.For(0, maxH, (z) =>
            {
                int y = z << 2 + posY;
                for (int x = posX; x < maxW; x += 4)
                    b.Add(GetFeature(dct.Extract(channel, x, y)));
            });
            BitArray bi=new BitArray(b.ToArray());
            byte[] array = bi.ToByteArray();
            Array.Resize(ref array, totalLength);
            return array;
        }
    }
}
