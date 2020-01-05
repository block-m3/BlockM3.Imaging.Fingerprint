// Accord Wavelet Library
// The Accord.NET Framework
// http://accord-framework.net
//
// Copyright © César Souza, 2009-2017
// cesarsouza at gmail.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
//


using System.Threading.Tasks;

namespace BlockM3.Imaging.Fingerprint.Math
{
    //Single Floating Point Haar wavelet Transform original from Accord 
    //  Optimizations :
    //* Inlining and less temporaries buffer copies.
    //* Changed Divisions for Multiplications.

    public static class SDWT
    {
        private const float w0 = 0.5f;
        private const float w1 = -0.5f;
        private const float s0 = 0.5f;
        private const float s1 = 0.5f;
        private const float iw0 = 2;
        private const float is0 = 2;


         public static void Forward(float[,] data, int iterations)
        {
            int width = data.GetLength(0);
            int height = data.GetLength(1);

            for (int k = 0; k < iterations; k++)
            {
                int levHeight = height >> k;
                int levWidth = width >> k;
                Parallel.For(0, levWidth, (x) =>
                {
                    int h = levHeight >> 1;
                    float[] tmp = new float[levHeight];
                    for (int y = 0; y < h; y++)
                    {
                        int n = (y << 1);
                        tmp[y] = data[x, n] * s0 + data[x, n + 1] * s1;
                        tmp[y + h] = data[x, n] * w0 + data[x, n + 1] * w1;
                    }
                    for (int y = 0; y < tmp.Length; y++)
                        data[x, y] = tmp[y];
                });
                Parallel.For(0, levHeight, (y) =>
                {
                    int h = levWidth>> 1;
                    float[] tmp = new float[levWidth];
                    for (int x = 0; x < h; x++)
                    {
                        int n = (x << 1);
                        tmp[x] = data[n, y] * s0 + data[n + 1, y] * s1;
                        tmp[x + h] = data[n, y] * w0 + data[n + 1, y] * w1;
                    }
                    for (int x = 0; x < tmp.Length; x++)
                        data[x, y] = tmp[x];
                });
            }
        }

        public static void Reverse(float[,] data, int iterations)
        {
            int width = data.GetLength(0);
            int height = data.GetLength(1);

            for (int k = iterations - 1; k >= 0; k--)
            {
                int levHeight = height >> k;
                int levWidth = width >> k;
                Parallel.For(0, levHeight, (y) =>
                {
                    int h = levWidth >> 1;
                    float[] tmp = new float[levWidth];
                    for (int x = 0; x < h; x++)
                    {
                        int n = (x << 1);
                        tmp[n] = (data[x, y] * s0 + data[x + h, y] * w0) * iw0;
                        tmp[n + 1] = (data[x, y] * s1 + data[x + h, y] * w1) * is0;
                    }

                    for (int x = 0; x < tmp.Length; x++)
                        data[x, y] = tmp[x];
                });
                Parallel.For(0, levWidth, (x) =>
                {
                    int h = levHeight >> 1;
                    float[] tmp = new float[levHeight];
                    for (int y = 0; y < h; y++)
                    {
                        int n = (y << 1);
                        tmp[n] = (data[x, y] * s0 + data[x, y + h] * w0) * iw0;
                        tmp[n + 1] = (data[x, y] * s1 + data[x, y + h] * w1) * is0;
                    }

                    for (int y = 0; y < tmp.Length; y++)
                        data[x, y] = tmp[y];
                });
            }
        }
    }
}
