using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BlockM3.Imaging.Fingerprint.Channels
{
    public class SCbChannel
    {
        public const float y_r = 0.299f;
        public const float y_g = 0.587f;
        public const float y_d = 0.114f;

        public const float cb_r = -0.168736f;
        public const float cb_g = -0.331264f;
        public const float cb_b = 0.5f;
        
        public const float cr_r = 0.5f;
        public const float cr_g = -0.418688f;
        public const float cr_b = -0.081312f;
        
        public const float b_cb = 1.772f;
        public const float g_cb = -0.344136f;
        public const float g_cr = -0.714136f;


        private static float[] y_r_table=new float[256];
        private static float[] y_g_table=new float[256];
        private static float[] y_b_table=new float[256];
        
        private static float[] cb_r_table=new float[256];
        private static float[] cb_g_table=new float[256];
        private static float[] cb_b_table=new float[256];
        
        private static float[] cr_r_table=new float[256];
        private static float[] cr_g_table=new float[256];
        private static float[] cr_b_table=new float[256];

        static SCbChannel()
        {
            for (int x = 0; x < 256; x++)
            {
                y_r_table[x] = y_r * x;
                y_g_table[x] = y_g * x;
                y_b_table[x] = y_d * x;
                
                cb_r_table[x] = cb_r * x;
                cb_g_table[x] = cb_g * x;
                cb_b_table[x] = cb_b * x;

                cr_r_table[x] = cr_r * x;
                cr_g_table[x] = cr_g * x;
                cr_b_table[x] = cr_b * x;
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte Clamp(float d)
        {
            int val = (int)System.Math.Round(d);
            if (d < 0)
                return 0;
            if (d > 255)
                return 255;
            return (byte)val;
        }

        public static float[,] ExtractCbChannel(byte[] bgra, int width, int clipX, int clipY, int clipWidth, int clipHeight)
        {
            float[,] u=new float[clipWidth, clipHeight];
            int w8 = clipWidth >> 3;
            int r8 = clipWidth % 8;
            Parallel.For(0, clipHeight, (y) =>
            {
                //unroll
                int dx = 0;
                int sx = ((y + clipY) * width + clipX) << 2;
                for (int x = 0; x < w8; x++)
                {
                    u[dx++, y] = 128+cb_r_table[bgra[sx + 2]]+cb_g_table[bgra[sx + 1]]+cb_b_table[bgra[sx]];
                    sx += 4;
                    u[dx++, y] = 128+cb_r_table[bgra[sx + 2]]+cb_g_table[bgra[sx + 1]]+cb_b_table[bgra[sx]];
                    sx += 4;
                    u[dx++, y] = 128+cb_r_table[bgra[sx + 2]]+cb_g_table[bgra[sx + 1]]+cb_b_table[bgra[sx]];
                    sx += 4;
                    u[dx++, y] = 128+cb_r_table[bgra[sx + 2]]+cb_g_table[bgra[sx + 1]]+cb_b_table[bgra[sx]];
                    sx += 4;
                    u[dx++, y] = 128+cb_r_table[bgra[sx + 2]]+cb_g_table[bgra[sx + 1]]+cb_b_table[bgra[sx]];
                    sx += 4;
                    u[dx++, y] = 128+cb_r_table[bgra[sx + 2]]+cb_g_table[bgra[sx + 1]]+cb_b_table[bgra[sx]];
                    sx += 4;
                    u[dx++, y] = 128+cb_r_table[bgra[sx + 2]]+cb_g_table[bgra[sx + 1]]+cb_b_table[bgra[sx]];
                    sx += 4;
                    u[dx++, y] = 128+cb_r_table[bgra[sx + 2]]+cb_g_table[bgra[sx + 1]]+cb_b_table[bgra[sx]];
                    sx += 4;
                }

                for (int x = 0; x < r8; x++)
                {
                    u[dx++, y] = 128+cb_r_table[bgra[sx + 2]]+cb_g_table[bgra[sx + 1]]+cb_b_table[bgra[sx]];
                    sx += 4;
                }
            });
            return u;
        }
       
        public static void InsertCbChannel(byte[] bgra, float[,] u, int width, int clipX, int clipY, int clipWidth, int clipHeight)
        {
            int w8 = clipWidth >> 3;
            int r8 = clipWidth % 8;

            Parallel.For(0, clipHeight, (y) =>
            {
                //unroll
                int dx = 0;
                int sx = ((y + clipY) * width + clipX) << 2;
                float oy;
                float ocb;
                float ocr;
                for (int x = 0; x < w8; x++)
                {
                    oy = y_r_table[bgra[sx + 2]] + y_g_table[bgra[sx + 1]] + y_b_table[bgra[sx]];
                    ocr = cr_r_table[bgra[sx + 2]] + cr_g_table[bgra[sx + 1]] + cr_b_table[bgra[sx]];
                    ocb = u[dx++, y] - 128;
                    bgra[sx++] = Clamp(oy + b_cb * ocb);
                    bgra[sx] = Clamp(oy + g_cb * ocb + g_cr * ocr);
                    sx += 3;
                    oy = y_r_table[bgra[sx + 2]] + y_g_table[bgra[sx + 1]] + y_b_table[bgra[sx]];
                    ocr = cr_r_table[bgra[sx + 2]] + cr_g_table[bgra[sx + 1]] + cr_b_table[bgra[sx]];
                    ocb = u[dx++, y] - 128;
                    bgra[sx++] = Clamp(oy + b_cb * ocb);
                    bgra[sx] = Clamp(oy + g_cb * ocb + g_cr * ocr);
                    sx += 3;
                    oy = y_r_table[bgra[sx + 2]] + y_g_table[bgra[sx + 1]] + y_b_table[bgra[sx]];
                    ocr = cr_r_table[bgra[sx + 2]] + cr_g_table[bgra[sx + 1]] + cr_b_table[bgra[sx]];
                    ocb = u[dx++, y] - 128;
                    bgra[sx++] = Clamp(oy + b_cb * ocb);
                    bgra[sx] = Clamp(oy + g_cb * ocb + g_cr * ocr);
                    sx += 3;
                    oy = y_r_table[bgra[sx + 2]] + y_g_table[bgra[sx + 1]] + y_b_table[bgra[sx]];
                    ocr = cr_r_table[bgra[sx + 2]] + cr_g_table[bgra[sx + 1]] + cr_b_table[bgra[sx]];
                    ocb = u[dx++, y] - 128;
                    bgra[sx++] = Clamp(oy + b_cb * ocb);
                    bgra[sx] = Clamp(oy + g_cb * ocb + g_cr * ocr);
                    sx += 3;
                    oy = y_r_table[bgra[sx + 2]] + y_g_table[bgra[sx + 1]] + y_b_table[bgra[sx]];
                    ocr = cr_r_table[bgra[sx + 2]] + cr_g_table[bgra[sx + 1]] + cr_b_table[bgra[sx]];
                    ocb = u[dx++, y] - 128;
                    bgra[sx++] = Clamp(oy + b_cb * ocb);
                    bgra[sx] = Clamp(oy + g_cb * ocb + g_cr * ocr);
                    sx += 3;
                    oy = y_r_table[bgra[sx + 2]] + y_g_table[bgra[sx + 1]] + y_b_table[bgra[sx]];
                    ocr = cr_r_table[bgra[sx + 2]] + cr_g_table[bgra[sx + 1]] + cr_b_table[bgra[sx]];
                    ocb = u[dx++, y] - 128;
                    bgra[sx++] = Clamp(oy + b_cb * ocb);
                    bgra[sx] = Clamp(oy + g_cb * ocb + g_cr * ocr);
                    sx += 3;
                    oy = y_r_table[bgra[sx + 2]] + y_g_table[bgra[sx + 1]] + y_b_table[bgra[sx]];
                    ocr = cr_r_table[bgra[sx + 2]] + cr_g_table[bgra[sx + 1]] + cr_b_table[bgra[sx]];
                    ocb = u[dx++, y] - 128;
                    bgra[sx++] = Clamp(oy + b_cb * ocb);
                    bgra[sx] = Clamp(oy + g_cb * ocb + g_cr * ocr);
                    sx += 3;
                    oy = y_r_table[bgra[sx + 2]] + y_g_table[bgra[sx + 1]] + y_b_table[bgra[sx]];
                    ocr = cr_r_table[bgra[sx + 2]] + cr_g_table[bgra[sx + 1]] + cr_b_table[bgra[sx]];
                    ocb = u[dx++, y] - 128;
                    bgra[sx++] = Clamp(oy + b_cb * ocb);
                    bgra[sx] = Clamp(oy + g_cb * ocb + g_cr * ocr);
                    sx += 3;

                }

                for (int x = 0; x < r8; x++)
                {
                    oy = y_r_table[bgra[sx + 2]] + y_g_table[bgra[sx + 1]] + y_b_table[bgra[sx]];
                    ocr = cr_r_table[bgra[sx + 2]] + cr_g_table[bgra[sx + 1]] + cr_b_table[bgra[sx]];
                    ocb = u[dx++, y] - 128;
                    bgra[sx++] = Clamp(oy + b_cb * ocb);
                    bgra[sx] = Clamp(oy + g_cb * ocb + g_cr * ocr);
                    sx += 3;

                }
            });
        }
    }
}
