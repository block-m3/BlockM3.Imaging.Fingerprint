using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BlockM3.Imaging.Fingerprint.Channels
{
    public class DCbChannel
    {
        public const double y_r = 0.299;
        public const double y_g = 0.587;
        public const double y_d = 0.114;

        public const double cb_r = -0.168736;
        public const double cb_g = -0.331264;
        public const double cb_b = 0.5;
        
        public const double cr_r = 0.5;
        public const double cr_g = -0.418688;
        public const double cr_b = -0.081312;
        
        public const double b_cb = 1.772;
        public const double g_cb = -0.344136;
        public const double g_cr = -0.714136;


        private static double[] y_r_table=new double[256];
        private static double[] y_g_table=new double[256];
        private static double[] y_b_table=new double[256];
        
        private static double[] cb_r_table=new double[256];
        private static double[] cb_g_table=new double[256];
        private static double[] cb_b_table=new double[256];
        
        private static double[] cr_r_table=new double[256];
        private static double[] cr_g_table=new double[256];
        private static double[] cr_b_table=new double[256];

        static DCbChannel()
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
        private static byte Clamp(double d)
        {
            int val = (int)System.Math.Round(d);
            if (d < 0)
                return 0;
            if (d > 255)
                return 255;
            return (byte)val;
        }

        public static double[,] ExtractCbChannel(byte[] bgra, int width, int clipX, int clipY, int clipWidth, int clipHeight)
        {
            double[,] u=new double[clipWidth, clipHeight];
            Parallel.For(0, clipHeight, (y) =>
            {
                int startY = ((y + clipY) * width + clipX) << 2;
                //unroll
                int w8 = clipWidth >> 3;
                int r8 = clipWidth % 8;
                int dx = 0;
                int sx = startY;
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
       
        public static void InsertCbChannel(byte[] bgra, double[,] u, int width, int clipX, int clipY, int clipWidth, int clipHeight)
        {
            Parallel.For(0, clipHeight, (y) =>
            {
                int startY = ((y + clipY) * width + clipX) << 2;


                //unroll
                int w8 = clipWidth >> 3;
                int r8 = clipWidth % 8;
                int dx = 0;
                int sx = startY;
                double oy;
                double ocb;
                double ocr;
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
