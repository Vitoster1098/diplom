using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Diplom
{
    public partial class Form1 : Form
    {        
        public Form1()
        {
            InitializeComponent();
        }

        string progpath = AppDomain.CurrentDomain.BaseDirectory;
        string connectionStringDefault = "";
        string connectionString = @"", path = "";
        private OleDbConnection connection;
        string[] exsamplesPath;
        double q = 0, avgBrightness = 127.5;
        OleDbCommand dbCommand = new OleDbCommand();
        pixelAnalyse analyse = new pixelAnalyse();

        private void connect_Click(object sender, EventArgs e) //Соединение с БД
        {
            makeConnection(textBox1.Text, connect_status);
        }
        public void makeConnection(string newconnectionString, PictureBox status)
        {
            try
            {
                connection = new OleDbConnection(newconnectionString);
                connection.Open();
                connectionString = newconnectionString;
                status.BackColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                connection = null;
                status.BackColor = Color.Red;
                return;
            }            
        }

        public void ScanFile(string path, string diagnose="Меланома")
        {
            Bitmap image;
            try
            {
                image = new Bitmap(path);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            /*BitmapData bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * image.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(ptr, rgbValues, 0, bytes);*/

            string query = "INSERT INTO Exsamples (Path, Diagnose) VALUES ('" + path + "', '" + diagnose + "')";
            dbCommand = new OleDbCommand(query, connection);
            dbCommand.ExecuteNonQuery();

            //Пока не вспомнил как с маршал работать
            /*for (int counter = 2; counter < rgbValues.Length; counter += 3)
            {                          
                if(rgbValues[counter-2] == 255 && rgbValues[counter-1] == 255 && rgbValues[counter] == 255)
                {
                    continue;
                }
                else
                {
                    addData(rgbValues[counter - 2], rgbValues[counter - 1], rgbValues[counter], path, diagnose, , );
                }
            }
            image.UnlockBits(bmpData);*/

                    for (int i = 0; i < image.Width; ++i)
                    {
                        for (int j = 0; j < image.Height; ++j)
                        {
                            if (image.GetPixel(i, j).R == 255 && image.GetPixel(i, j).G == 255 && image.GetPixel(i, j).B == 255)
                                continue;
                            else
                                addData(image.GetPixel(i, j).R, image.GetPixel(i, j).G, image.GetPixel(i, j).B, path, diagnose, i, j);
                        }
                    }
        }

        void addData(byte R, byte G, byte B, string path, string diagnose, int x, int y) //Заполнение информации о пикселях изображений
        {
            string query = "INSERT INTO Spot_info (ID_photo, X_point, Y_point, R, G, B) "
                + "VALUES ('" + path + "', " + x + ", " + y + ", " + R + ", " + G + ", " + B + ")";
            dbCommand = new OleDbCommand(query, connection);
            dbCommand.ExecuteNonQuery();
        }

        bool checkPath(string path) //True - есть запись, False - нет
        {
            string query = "SELECT * FROM Exsamples WHERE Path='" + path + "'";
            dbCommand = new OleDbCommand(query, connection);
            OleDbDataReader reader = dbCommand.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Close();
                return true;
            }
            else
            {
                reader.Close();
                return false;
            }
        }

        private void disconnect_Click(object sender, EventArgs e) //Сброс соединения с БД
        {
            connectionString = "";
            try
            {
                connection.Close();
                connect_status.BackColor = Color.Red;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }            
        }

        private void default_connection_Click(object sender, EventArgs e) //Восстановление стандартного соединения
        {
            connection.Close();
            makeConnection(connectionStringDefault, connect_status);
        }

        private void fillListbox()
        {
            exsamplesPath = GetPath();
        }

        public string[] GetPath() //Заполняет листбокс именами экземпляров из бд и возвращает список имён файлов
        {
            string query = "SELECT * FROM Exsamples";
            string querycount = "SELECT count(*) as RowCount FROM Exsamples";
            OleDbCommand command = new OleDbCommand(querycount, connection);
            OleDbDataReader reader = command.ExecuteReader();
            reader.Read();
            var rowCount = (int)reader["RowCount"];

            command = new OleDbCommand(query, connection);
            reader = command.ExecuteReader();
            int count = 0;
            string[] ret = new string[rowCount];
            listBox1.Items.Clear();

            try
            {
                if (rowCount == 0)
                {
                    listBox1.Items.Add("База данных пуста");
                }
                else
                {
                    while (reader.Read())
                    {
                        listBox1.Items.Add('\\' + reader["Path"].ToString());
                        ret[count] = '\\' + reader["Path"].ToString();
                        count++;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
            reader.Close();
            return ret;
        }

        private void Form1_Load(object sender, EventArgs e) //Старт программы
        {
            connectionStringDefault = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + progpath.Replace("Debug\\", "Release") + @"base.mdb'";
            connectionString = connectionStringDefault;
            connection = new OleDbConnection(connectionStringDefault);
            textBox1.Text = connectionStringDefault;
            makeConnection(textBox1.Text, connect_status);

            fillListbox();
        }

        private void очиститьВсёToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dbCommand.Connection = connection;
            try
            {
                dbCommand.CommandText = "DELETE FROM Spot_info";
                dbCommand.ExecuteNonQuery();
                dbCommand.CommandText = "DELETE FROM Exsamples";
                dbCommand.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
            fillListbox();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) //клик по листбоксу
        {
            //MessageBox.Show(progpath.ToString() + listBox1.Items[listBox1.SelectedIndex].ToString(), "Уведомление");
            try
            {
                pictureBox1.Image = new Bitmap(progpath + listBox1.Items[listBox1.SelectedIndex]);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                return;
            }
            string newpath = listBox1.Items[listBox1.SelectedIndex].ToString().Substring(1);
            //MessageBox.Show(newpath, "newpath");
            string xyquery = @"SELECT MAX(X_point) AS max_x, MAX(Y_point) AS max_y FROM Spot_info WHERE ID_photo='" + newpath + "'";
            OleDbCommand command = new OleDbCommand(xyquery, connection);
            OleDbDataReader reader = command.ExecuteReader();

            reader.Read();
            var x = (int)reader["max_x"];
            var y = (int)reader["max_y"];
            //MessageBox.Show("X:" + x + " Y:" + y);
            Bitmap fromBase = new Bitmap(x+1, y+1);

            string query = @"SELECT * FROM Spot_info WHERE ID_photo='" + newpath + "'";
            command = new OleDbCommand(query, connection);
            reader = command.ExecuteReader();
            

            while (reader.Read())
            {
                try
                {
                    fromBase.SetPixel((int)reader["X_point"], (int)reader["Y_point"], Color.FromArgb(255, (int)reader["R"], (int)reader["G"], (int)reader["B"]));
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message + "\r\n" + "X:" + (int)reader["X_point"] + "\r\n" + "Y:" + (int)reader["Y_point"], "Ошибка присвоения цвета");
                }
            }

            pictureBox2.Image = fromBase;
            avgBrightness = analyse.getAverageBrightness(new Bitmap(pictureBox1.Image));

            chart1.Series[0].Points.Clear();
            chart2.Series[0].Points.Clear();
            chart3.Series[0].Points.Clear();
        }

        private void button1_Click(object sender, EventArgs e) //Изменить яркость
        {
            /*try
            {
                q = Convert.ToDouble(qField.Text);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex, "Ошибка");
                return;
            }*/
            
            avgLabel.Text ="Средняя яркость: " + avgBrightness;
            Bitmap bitmap = new Bitmap(pictureBox2.Image);

            BitmapData bpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr intPtr = bpdata.Scan0;

            byte[] da = new byte[(bitmap.Width * bitmap.Height) * 3];
            Marshal.Copy(intPtr, da, 0, da.Length);

            for(int i = 0; i < da.Length - 1; i += 3)
            {
                Color clr = Color.FromArgb(255, da[i], da[i + 1], da[i + 2]);
                Color newclr = analyse.newBrightness(clr, avgBrightness);
                da[i] = newclr.R;
                da[i + 1] = newclr.G;
                da[i + 2] = newclr.B;
            }

            Marshal.Copy(da, 0, intPtr, da.Length);
            bitmap.UnlockBits(bpdata);
            pictureBox2.Image = bitmap;

            analyse.setRGB(chart1, chart2, chart3);
        }

        private void фотоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = progpath.Replace("Debug\\", "Release");
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                else
                {
                    path = dialog.SelectedPath;
                }
            }

            string[] allfiles = Directory.GetFiles(path);
            progressBar1.Maximum = allfiles.Length;
            progressBar1.Value = 0;

            foreach (string filename in allfiles)
            {
                var dn = Path.GetDirectoryName(filename);
                dn = dn.Substring(dn.LastIndexOf('\\') + 1);
                //MessageBox.Show(filename);
                if (!checkPath(Path.Combine(dn, Path.GetFileName(filename)))) //Если нет записи о изображении - добавляем, иначе скип
                {
                    ScanFile(Path.Combine(dn, Path.GetFileName(filename))); //Вызов функции обработки изображений в папке                    
                    progressBar1.Value++;
                }
            }
            fillListbox();
        }
    }
}