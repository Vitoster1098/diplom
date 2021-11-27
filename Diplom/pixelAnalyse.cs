using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace Diplom
{
    class pixelAnalyse
    {
        public Dictionary<string, Bitmap> map = new Dictionary<string, Bitmap>();
        public int[] avR = new int[256];
        public int[] avG = new int[256];
        public int[] avB = new int[256];

        public void setAvgColor(Color avg)
        {
            avR[avg.R]++;
            avG[avg.G]++;
            avB[avg.B]++;
        }

        public void clearAvg()
        {
            for (int i = 0; i < avR.Length - 1; avR[i++] = 0) { }
            for (int i = 0; i < avG.Length - 1; avG[i++] = 0) { }
            for (int i = 0; i < avB.Length - 1; avB[i++] = 0) { }
        }

        public void addPictures(string path, Bitmap picture)
        {
            map.Add(path, picture);
        }

        public Bitmap getPicture(string path)
        {
            return map[path];
        }

        /*public static UInt32 Brightness(UInt32 point, int poz, int lenght) //poz - значение ползунка, length - максимальное значение ползунка
        {
            int R;
            int G;
            int B;

            int N = (100 / lenght) * poz; //кол-во процентов

            R = (int)(((point & 0x00FF0000) >> 16) + N * 127.5 / 100);
            G = (int)(((point & 0x0000FF00) >> 8) + N * 127.5 / 100);
            B = (int)((point & 0x000000FF) + N * 127.5 / 100);

            //контролируем переполнение переменных
            if (R < 0) R = 0;
            if (R > 255) R = 255;
            if (G < 0) G = 0;
            if (G > 255) G = 255;
            if (B < 0) B = 0;
            if (B > 255) B = 255;

            point = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

            return point;
        }*/

        public double getAverageBrightness(Bitmap bitmap)
        {
            double lab = 0;

            BitmapData bpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr intPtr = bpdata.Scan0;

            byte[] da = new byte[(bitmap.Width * bitmap.Height) * 3];
            Marshal.Copy(intPtr, da, 0, da.Length);

            for (int i = 0; i < da.Length-1; i += 3)
            {
                //lab += (int)(da[i] * 0.299 + da[i+1] * 0.587 + da[i+2] * 0.114);
                lab += (int)(da[i] + da[i + 1] + da[i + 2]);
            }
            bitmap.UnlockBits(bpdata);

            return lab / ((bitmap.Width * bitmap.Height)*3);
        }

        public void setRGB(Chart r, Chart g, Chart b)
        {
            r.Series[0].Points.Clear();
            g.Series[0].Points.Clear();
            b.Series[0].Points.Clear();

            for (int i = 0; i < avR.Length - 1; ++i)
            {
                r.Series[0].Points.AddXY(i, avR[i]);
                g.Series[0].Points.AddXY(i, avG[i]);
                b.Series[0].Points.AddXY(i, avB[i]);
            }            
        }

        public Color newBrightness(Color point, double q)
        {
            byte R, G, B;
            q = 127.5 / q;
            R = (byte)(point.R + q);
            G = (byte)(point.G + q);
            B = (byte)(point.B + q);

            //контролируем переполнение переменных
            if (R < 0) R = 0;
            if (R > 255) R = 255;
            if (G < 0) G = 0;
            if (G > 255) G = 255;
            if (B < 0) B = 0;
            if (B > 255) B = 255;

            Color newColor = Color.FromArgb(255, R, G, B);
            return newColor;
        }

        /*public Color newBrightness(Color point, double koef, double length)
        {
            byte R, G, B;

            double N = (100 / length) * koef;

            R = (byte)(point.R + N * 127.5 / 100);
            G = (byte)(point.G + N * 127.5 / 100);
            B = (byte)(point.B + N * 127.5 / 100);

            //контролируем переполнение переменных
            if (R < 0) R = 0;
            if (R > 255) R = 255;
            if (G < 0) G = 0;
            if (G > 255) G = 255;
            if (B < 0) B = 0;
            if (B > 255) B = 255;

            Color newColor = Color.FromArgb(255, R, G, B);
            return newColor;
        }*/
    }
}