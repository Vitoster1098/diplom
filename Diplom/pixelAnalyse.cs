using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Diplom
{
    class pixelAnalyse
    {
        private Bitmap bitmap = new Bitmap(100, 100);
        private double avgBrightness = 0;
        public int[] avR = new int[256];
        public int[] avG = new int[256];
        public int[] avB = new int[256];

        public Info[] data = new Info[1];

        public struct Info
        {
            private Point pos;
            private Color color;

            public Info(Point pos, Color color) 
            {
                this.pos = pos;
                this.color = color;
            }
            public void setPos(Point pos)
            {
                this.pos = pos;
            }
            public void setColor(Color color)
            {
                this.color = color;
            }
            public Point getPoint()
            {
                return pos;
            }
            public Color getColor()
            {
                return color;
            }
        }

        public pixelAnalyse()
        {

        }

        public Bitmap getBitmap()
        {
            return bitmap;
        }

        public pixelAnalyse(Bitmap bitmap)
        {
            /*Bitmap temp = new Bitmap(bitmap.Width, bitmap.Height);
            Graphics graphics = Graphics.FromImage(temp);
            graphics.Clear(Color.White);
            this.bitmap = new Bitmap(temp);
            this.bitmap = bitmap;
            avgBrightness = getAverageBrightness(bitmap);*/
            this.bitmap = bitmap;
            initInfo();
        }

        private void initInfo()
        {
            int counter = 0;
            for(int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    Color clr = bitmap.GetPixel(i, j);                                            
                    data[counter] = new Info(new Point(i, j), clr);
                    counter++;
                    Array.Resize(ref data, data.Length + 1);
                }                   
            }
        }

        public void setInfo(Point pos, Color color)
        {
            data[data.Length-1] = new Info(new Point(pos.X, pos.Y), color);
            Array.Resize(ref data, data.Length + 1);
        }
        public int getMax(bool XorY) //Если передаём True - ищет по X, иначе по Y
        {
            int ret = -1;
            if (XorY)
                ret= data[0].getPoint().X;
            else
                ret = data[0].getPoint().Y;

            for (int i = 0; i < data.Length - 1; i++)
            {
                if (ret < data[i].getPoint().X && XorY)
                {
                    ret = data[i].getPoint().X;
                }
                if(ret < data[i].getPoint().Y && !XorY)
                {
                    ret = data[i].getPoint().Y;
                }
            }
            
            return ret;
        }

        public Bitmap getBitmapByInfo(Bitmap temp)
        {
            for(int i = 0; i < data.Length - 1; i++)
            {
                temp.SetPixel(data[i].getPoint().X, data[i].getPoint().Y, data[i].getColor());
            }
            this.bitmap = temp;
            return temp;
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

        /*public double getAverageBrightness(Bitmap bitmap)
        {           
            BitmapData bpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr intPtr = bpdata.Scan0;

            byte[] da = new byte[(bitmap.Width * bitmap.Height) * 3];
            Marshal.Copy(intPtr, da, 0, da.Length);

            double lab = 0;
            int whiteCounter = 0;
            for (int i = 0; i < da.Length-1; i += 3)
            {
                if (da[i] != 255 && da[i + 1] != 255 && da[i + 2] != 255)
                    lab += (int)(da[i] * 0.2126) + (da[i + 1] * 0.7152) + (da[i + 2] * 0.0722);
                else
                    whiteCounter++;
            }
            bitmap.UnlockBits(bpdata);

            return lab / (da.Length - 1 - whiteCounter);
        }*/

        public double getAverageBrightness()
        {
            double sum = 0;
            for (int i = 0; i < data.Length - 1; i++)
            {
                sum += (data[i].getColor().R * 0.2126) + (data[i].getColor().G * 0.7152) + (data[i].getColor().B * 0.0722);
            }
            avgBrightness = sum / data.Length;
            return sum / data.Length;
        }

        public void changeBrightness()
        {
            avgBrightness = getAverageBrightness();
            /*BitmapData bpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr intPtr = bpdata.Scan0;

            byte[] da = new byte[(bitmap.Width * bitmap.Height) * 3];
            Marshal.Copy(intPtr, da, 0, da.Length);*/

            clearAvg();

            /*for (int i = 0; i < da.Length - 1; i += 3)
            {
                if(da[i] != 255 && da[i + 1] != 255 && da[i + 2] != 255)
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
            bitmap.UnlockBits(bpdata);*/

            for (int i = 0; i < data.Length - 1; i++)
            {
                Color newclr = newBrightness(data[i].getColor(), avgBrightness);
                setAvgColor(newclr);
                data[i].setColor(newclr);
            }
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
            byte R = 0, G = 0, B = 0;
            int[] mx = { point.R, point.G, point.B };
            int max = mx.Max();            
            double q_new = 127.5 / q;
            double q1 = q_new;
            double max1 = max * q1;

            if(max1 > 255)
            {
                q1 = 255 / max;
            }

            R = (byte)(point.R * q1);
            G = (byte)(point.G * q1);
            B = (byte)(point.B * q1);
            
            /*MessageBox.Show("R:" + point.R + " G:" + point.G + " B:" + point.B + " Max:" + max_color+"\n" +
                "Новые R:" + R + " G:" + G + " B:" + B + " с коэффициентом: " + q_new, "Информация");*/
            Color newColor = Color.FromArgb(255, R, G, B);
            return newColor;
        }
    }
}