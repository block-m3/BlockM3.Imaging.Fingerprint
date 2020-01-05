// Accord Math Library
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

namespace BlockM3.Imaging.Fingerprint.Math
{
    //Single Floating point Discreet Cosine Transformation optimized 
    public class SDCT
    {
        private float scale;
        private int size;
        private float[,] Cosines;
        private float isqrt2;

        public SDCT(int size)
        {
            isqrt2 = 1.0f / (float)System.Math.Sqrt(2);
            Cosines = new float[size,size];
            this.size = size;
            double c = (float)System.Math.PI / (size << 1);
            scale =(float) System.Math.Sqrt(2.0D/size);
            for (int x = 0; x < size; x++)
            {
                double pr = ((x << 1) + 1)*c;
                for (int y = 0; y < size; y++)
                {
                    Cosines[x, y] = (float)System.Math.Cos(pr * y);
                }
            }
        }

        public float[,] Extract(float[,] data, int posx, int posy)
        {
            float[,] ext=new float[size,size];
            float[] temp=new float[size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float sum = 0;
                    for (int c = 0; c < size; c++)
                        sum += data[x+posx, c+posy] * Cosines[c, y];
                    ext[x,y]=scale * sum;
                }
                ext[x, 0] *= isqrt2;
            }
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float sum = 0;
                    for (int c = 0; c < size; c++)
                        sum += ext[c,y] * Cosines[c, x];
                    temp[x] = scale * sum;
                }
                ext[0,y] = temp[0] * isqrt2;
                for (int x = 1; x < size; x++)
                    ext[x,y] = temp[x];
            }

            return ext;
        }
        public void Insert(float[,] data, float[,] dest, int posx, int posy)
        {
            float[] temp=new float[size];
            for (int y = 0; y < size; y++)
            {
                float s = data[0, y] * isqrt2;
                for (int x = 0; x < size; x++)
                {
                    float sum = s;
                    for (int c = 1; c < size; c++)
                        sum += data[c, y] * Cosines[x, c];
                    dest[x+posx, y+posy] = scale * sum;
                }
            }

            for (int x = 0; x < size; x++)
            {
                float s =dest[x+posx, posy] * isqrt2;
                for (int y = 0; y < size; y++)
                {
                    float sum = s;
                    for (int c = 1; c < size; c++)
                        sum += dest[x + posx, c + posy] * Cosines[y, c];
                    temp[y] = scale * sum;
                }

                for (int y = 0; y < size; y++)
                    dest[x + posx, y + posy] = temp[y];
            }
        }
    }
}
