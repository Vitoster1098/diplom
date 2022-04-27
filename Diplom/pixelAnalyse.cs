using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Diplom
{
    class pixelAnalyse
    {
        private Bitmap bitmap = new Bitmap(100, 100);
        private double avgBrightness = 0;
        public double[] avR = new double[256];
        public double[] avG = new double[256];
        public double[] avB = new double[256];

        public Info[] data = new Info[1];

        ProgressBar bar = null;

        public struct Info //Хранение данных о месте на битмап и его цвета
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

        public pixelAnalyse(ProgressBar bar)
        {
            this.bar = bar;
        }

        public Bitmap getBitmap()
        {
            return bitmap;
        }

        public pixelAnalyse(Bitmap bitmap) //инициализаця старым способом, пока посидит
        {
            this.bitmap = bitmap;
            initInfo();
        }

        private void initInfo() //установка значений в структуру на основе битмапа, поддержка старого способа
        {
            for(int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color clr = bitmap.GetPixel(i, j);
                    setInfo(new Point(i, j), clr);
                }                   
            }
        }

        public void setInfo(Point pos, Color color) //установка значений в структуру
        {
            data[data.Length-1] = new Info(new Point(pos.X, pos.Y), color);
            setAvgColor(color);
            Array.Resize(ref data, data.Length + 1);
        }
        public int getMax(bool XorY) //Если передаём True - ищет по X, иначе по Y - возвращает максимальное значения
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

        public Bitmap getBitmapByInfo(Bitmap temp) //на основе данных структуры формирует и возвращает битмап (на входе данные о размере битмапа)
        {
            bar.Maximum = data.Count();
            bar.Value = 1;

            for(int i = 0; i < data.Length - 1; i++)
            {
                temp.SetPixel(data[i].getPoint().X, data[i].getPoint().Y, data[i].getColor());
                bar.Value++;
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

        public void setAvgColor(Color avg) //заполнение данных гистограмм
        {
            avR[avg.R]++;
            avG[avg.G]++;
            avB[avg.B]++;
        }

        public void clearAvg() //очистка данных гистограмм
        {
            for (int i = 0; i < avR.Length - 1; avR[i++] = 0) { }
            for (int i = 0; i < avG.Length - 1; avG[i++] = 0) { }
            for (int i = 0; i < avB.Length - 1; avB[i++] = 0) { }
        }

        public double getAverageBrightness() //расчет нового значения средней яркости на основе данных структуры
        {
            double sum = 0;
            for (int i = 0; i < data.Length - 1; i++)
            {
                sum += (data[i].getColor().R * 0.2126) + (data[i].getColor().G * 0.7152) + (data[i].getColor().B * 0.0722);
            }
            avgBrightness = sum / data.Length;
            return sum / data.Length;
        }

        public void changeBrightness(string typeProc) //изменение яркости с сохранением в структуру
        {
            avgBrightness = getAvgBrightness();            

            bar.Maximum = data.Count();
            bar.Value = 0;

            if (typeProc == "avgBright")
            {
                clearAvg();
                for (int i = 0; i < data.Length - 1; i++)
                {
                    Color newclr = newBrightness(data[i].getColor(), avgBrightness, true);
                    setAvgColor(newclr);
                    data[i].setColor(newclr);
                    bar.Value++;
                }
            }
            if (typeProc == "avgDefl")
            {
                double y = 0, sum = 0, D = 0, Sg = 0;
                double[] w = new double[256];

                for(int i = 0; i < avR.Length-1; ++i)
                {
                    sum += avR[i];
                }
                for (int i = 0; i < w.Length - 1; ++i)
                {
                    w[i] = avR[i] / sum; //Частота
                    y += w[i] * i;
                }
                MessageBox.Show("Сумма:" + sum + "\r\nСреднее:"+y);
                for (int i = 0; i < w.Length - 1; ++i)
                {
                    D += (Math.Pow(i, 2) * w[i]) - Math.Pow(y, 2);
                }
                Sg = Math.Sqrt(Math.Abs(D));
                MessageBox.Show("Дисперсия:" + D + "\r\nОтклонение:"+Sg);
                clearAvg();

                for (int i = 0; i < data.Length - 1; i++)
                {
                    Color newclr = newBrightness(data[i].getColor(), Sg, false);
                    setAvgColor(newclr);
                    data[i].setColor(newclr);
                    bar.Value++;
                }
            }
            if (typeProc == "median")
            {

            }
        }

        public void setRGB(Chart r, Chart g, Chart b, Chart rgb) //отрисовка гистограмм
        {
            r.Series[0].Points.Clear();
            g.Series[0].Points.Clear();
            b.Series[0].Points.Clear();
            rgb.Series[0].Points.Clear();
            rgb.Series[1].Points.Clear();
            rgb.Series[2].Points.Clear();

            for (int i = 0; i < avR.Length - 1; ++i)
            {
                r.Series[0].Points.AddXY(i, avR[i] / data.Length);
                g.Series[0].Points.AddXY(i, avG[i] / data.Length);
                b.Series[0].Points.AddXY(i, avB[i] / data.Length);
                rgb.Series[0].Points.AddXY(i, avR[i] / data.Length);
                rgb.Series[1].Points.AddXY(i, avG[i] / data.Length);
                rgb.Series[2].Points.AddXY(i, avB[i] / data.Length);
            }
        }

        public double getAverageGistogramm(string type) //Получение среднего значения гистограммы по R G B
        {
            double av = 0;

            switch (type)
            {
                case "R":
                    {
                        for(int i = 0; i < avR.Length - 1; ++i)
                        {
                            av += (avR[i] / 255) * i;
                        }
                        break;
                    }
                case "G":
                    {
                        for (int i = 0; i < avG.Length - 1; ++i)
                        {
                            av += (avG[i] / 255) * i;
                        }
                        break;
                    }
                case "B":
                    {
                        for (int i = 0; i < avB.Length - 1; ++i)
                        {
                            av += (avB[i] / 255) * i;
                        }
                        break;
                    }
            }
            return av / 255;
        }

        public Color newBrightness(Color point, double q, bool isAvgBright) //расчет новой яркости
        {
            byte R = 0, G = 0, B = 0;
            int[] mx = { point.R, point.G, point.B };
            int max = mx.Max();
            double q_new = q;

            if (isAvgBright)
            {
                q_new = 127.5 / q;
            }
            
            double q1 = q_new;
            double max1 = max * q1;

            if(max1 > 255)
            {
                q1 = 255 / max;
            }

            R = (byte)(point.R * q1);
            G = (byte)(point.G * q1);
            B = (byte)(point.B * q1);
            
            Color newColor = Color.FromArgb(255, R, G, B);
            return newColor;
        }
    }
}