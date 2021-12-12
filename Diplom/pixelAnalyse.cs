using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Diplom
{
    class pixelAnalyse
    {
        private Bitmap bitmap = new Bitmap(100, 100);
        private bool[,] matrix;
        private double avgBrightness = 0;
        public int[] avR = new int[256];
        public int[] avG = new int[256];
        public int[] avB = new int[256];

        public pixelAnalyse()
        {

        }

        public void setBitmap(Bitmap picture)
        {
            Bitmap temp = new Bitmap(picture.Width, picture.Height);
            Graphics graphics = Graphics.FromImage(temp);
            graphics.Clear(Color.White);
            bitmap = new Bitmap(temp);
            bitmap = new Bitmap(picture);
            matrix = new bool[bitmap.Width, bitmap.Height];
        }

        public void setMatrix(bool[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); ++i)
            {
                for (int j = 0; j < matrix.GetLength(1); ++j)
                {
                    this.matrix[i, j] = matrix[i, j];
                }
            }
        }

        public Bitmap getBitmap()
        {
            return bitmap;
        }

        public pixelAnalyse(Bitmap bitmap)
        {
            Bitmap temp = new Bitmap(bitmap.Width, bitmap.Height);
            Graphics graphics = Graphics.FromImage(temp);
            graphics.Clear(Color.White);
            this.bitmap = new Bitmap(temp);
            this.bitmap = bitmap;
            avgBrightness = getAverageBrightness(bitmap);
            matrix = new bool[bitmap.Width, bitmap.Height];
        }

        public void setAvgBrightness(double avgBrightness)
        {
            this.avgBrightness = avgBrightness;
        }
        public double getAvgBrightness()
        {
            return avgBrightness;
        }

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

            int whiteCounter = 0;
            for (int i = 0; i < da.Length-1; i += 3)
            {
                if (da[i] != 255 && da[i + 1] != 255 && da[i + 2] != 255)
                    lab += (int)(da[i] + da[i + 1] + da[i + 2]);
                else
                    whiteCounter++;
            }
            bitmap.UnlockBits(bpdata);

            return lab / (((bitmap.Width * bitmap.Height) * 3) - whiteCounter);
        }

        public void changeBrightness()
        {
            avgBrightness = getAverageBrightness(bitmap);
            BitmapData bpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr intPtr = bpdata.Scan0;

            byte[] da = new byte[(bitmap.Width * bitmap.Height) * 3];
            Marshal.Copy(intPtr, da, 0, da.Length);

            clearAvg();

            //Здесь перевожу двумерный массив в одномерный с информацией о доступности изменения цвета
            int[] changedPix = new int[da.Length];
            int counter = 0;

            for(int i = 0; i < bitmap.Width; ++i)
            {
                for(int j=0; j < bitmap.Height; ++j)
                {
                    if (matrix[i, j])
                    {
                        changedPix[counter] = 1;
                        changedPix[counter + 1] = 1;
                        changedPix[counter + 2] = 1;
                    }
                    else
                    {
                        changedPix[counter] = 0;
                        changedPix[counter + 1] = 0;
                        changedPix[counter + 2] = 0;
                    }
                    counter++;
                }
            }

            for (int i = 0; i < da.Length - 1; i += 3)
            {
                if(changedPix[i] == 1 && da[i] != 255 && da[i + 1] != 255 && da[i + 2] != 255)
                {
                    Color clr = Color.FromArgb(255, da[i], da[i + 1], da[i + 2]);
                    Color newclr = new Color();
                    newclr = newBrightness(clr, avgBrightness);
                    setAvgColor(newclr);

                    da[i] = newclr.R;
                    da[i + 1] = newclr.G;
                    da[i + 2] = newclr.B;
                }                
            }            

            Marshal.Copy(da, 0, intPtr, da.Length);
            bitmap.UnlockBits(bpdata);
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
            double R = 0, G = 0, B = 0;
            int[] mx = { point.R, point.G, point.B };
            int max_color = mx.Max();            
            double q_new = 127.5 / q;

            if(max_color * q_new > 255)
            {
                try //Деление на 0
                {
                    R = 255 / point.R;
                }
                catch(Exception)
                {
                    R = 0;
                }
                try
                {
                    G = 255 / point.G;
                }
                catch (Exception)
                {
                    G = 0;
                }
                try
                {
                    B = 255 / point.B;
                }
                catch (Exception)
                {
                    B = 0;
                }
            }
            else
            {
                R = point.R * q_new;
                G = point.G * q_new;
                B = point.B * q_new;
            }
            /*MessageBox.Show("R:" + point.R + " G:" + point.G + " B:" + point.B + " Max:" + max_color+"\n" +
                "Новые R:" + R + " G:" + G + " B:" + B + " с коэффициентом: " + q_new, "Информация");*/
            Color newColor = Color.FromArgb(255, (int)R, (int)G, (int)B);
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