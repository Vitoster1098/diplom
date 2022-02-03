using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;

namespace Diplom
{
    public partial class Form1 : Form
    {        
        public Form1()
        {
            InitializeComponent();
        }

        string progpath = AppDomain.CurrentDomain.BaseDirectory;
        string connectionString = "", path = "";
        private OleDbConnection connection;
        string[] exsamplesPath;
        double avgBrightness = 127.5;
        Bitmap selBitmap = null;
        OleDbCommand dbCommand = new OleDbCommand();
        pixelAnalyse analyse;

        private void connect_Click(object sender, EventArgs e) //Соединение с БД
        {
            if(connectionString == "")
            {
                базуДанныхToolStripMenuItem_Click(sender, e);
            }
            else
            {
                makeConnection(connectionString, connect_status);
            }            
        }
        public void makeConnection(string newconnectionString, PictureBox status)
        {
            try
            {
                connection = new OleDbConnection(newconnectionString);
                connection.Open();
                connectionString = newconnectionString;
                status.BackColor = Color.Green;
                conStatusLbl.Text = "Соединено с:" + connectionString;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                connection = null;
                status.BackColor = Color.Red;
                return;
            }            
        }

        public void clearForms()
        {
            chart1.Series[0].Points.Clear();
            chart2.Series[0].Points.Clear();
            chart3.Series[0].Points.Clear();

            pictureBox2.Image.Dispose();
            pictureBox2.Image = null;
            progressBar1.Value = 0;
            avgLabel.Text = "Средняя яркость:";
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

            string query = "INSERT INTO Exsamples (Path, Diagnose) VALUES ('" + path + "', '" + diagnose + "')";
            dbCommand = new OleDbCommand(query, connection);
            dbCommand.ExecuteNonQuery();

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
            if (connectionString == "") { return; }
            try
            {
                connection.Close();
                connect_status.BackColor = Color.Red;
                listBox1.Items.Clear();
                conStatusLbl.Text = "Статус: нет соединения с БД";
                connectionString = "";
                clearForms();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                return;
            }           
        }

        private void fillListbox() //Формирует список имен файлов с бд
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
                        ret[listBox1.Items.Count-1] = '\\' + reader["Path"].ToString();
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

        private void очиститьВсёToolStripMenuItem_Click(object sender, EventArgs e) //Полное удаление данных в бд
        {
            if (connectionString == "") { return; }
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
                string newpath = listBox1.Items[listBox1.SelectedIndex].ToString().Substring(1);               

                string query = @"SELECT * FROM Spot_info WHERE ID_photo='" + newpath + "'";
                dbCommand = new OleDbCommand(query, connection);
                OleDbDataReader reader = dbCommand.ExecuteReader();

                analyse = new pixelAnalyse(progressBar1); //очистка

                while (reader.Read())
                {
                    try
                    {
                        analyse.setInfo(new Point((int)reader["X_point"], (int)reader["Y_point"]), Color.FromArgb(255, (int)reader["R"], (int)reader["G"], (int)reader["B"]));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\r\n" + "X:" + (int)reader["X_point"] + "\r\n" + "Y:" + (int)reader["Y_point"], "Ошибка присвоения цвета");
                    }
                }

                Bitmap fromBase = new Bitmap(analyse.getMax(true) + 1, analyse.getMax(false) + 1);
                /*Graphics graphics = Graphics.FromImage(fromBase);
                graphics.Clear(Color.White);*/

                selBitmap = analyse.getBitmapByInfo(fromBase);
                pictureBox2.Image = selBitmap;            

                avgBrightness = analyse.getAverageBrightness();
                avgLabel.Text = "Средняя яркость: " + Math.Round(avgBrightness, 2);
                avR.Text = "Среднее R: " + Math.Round(analyse.getAverageGistogramm("R"), 3);
                avG.Text = "Среднее G: " + Math.Round(analyse.getAverageGistogramm("G"), 3);
                avB.Text = "Среднее B: " + Math.Round(analyse.getAverageGistogramm("B"), 3);

                chart1.Series[0].Points.Clear();
                chart2.Series[0].Points.Clear();
                chart3.Series[0].Points.Clear();
                chart4.Series[0].Points.Clear();
                chart4.Series[1].Points.Clear();
                chart4.Series[2].Points.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }            
        }

        private void базуДанныхToolStripMenuItem_Click(object sender, EventArgs e) //Открытие базы данных
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Database files (*.mdb)|*.mdb|All files (*.*)|*.*";
            if(fileDialog.ShowDialog() == DialogResult.OK)
            {
                connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + fileDialog.FileName + "'";                
                makeConnection(connectionString, connect_status);
                fillListbox();
            }
        }

        private void button1_Click(object sender, EventArgs e) //Изменить яркость
        {               
            if(analyse.data.Length == 1) { return; } //если не заполняли данными - остановка

            try
            {
                analyse.changeBrightness();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка задания новой яркости");
                return;
            }

            selBitmap = new Bitmap(analyse.getBitmapByInfo(new Bitmap(selBitmap.Width, selBitmap.Height))); //получение битмапа на основе данных из бд
            pictureBox2.Image = selBitmap;

            analyse.setRGB(chart1, chart2, chart3, chart4);
            avgBrightness = analyse.getAverageBrightness();
            avgLabel.Text = "Средняя яркость: " + Math.Round(avgBrightness, 3);
            avR.Text = "Среднее R: " + Math.Round(analyse.getAverageGistogramm("R"), 3);
            avG.Text = "Среднее G: " + Math.Round(analyse.getAverageGistogramm("G"), 3);
            avB.Text = "Среднее B: " + Math.Round(analyse.getAverageGistogramm("B"), 3);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            analyse = new pixelAnalyse(progressBar1);

            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;

            chart2.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart2.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart2.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart2.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;

            chart3.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart3.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart3.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart3.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart4.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
        }

        private void очиститьПоляToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clearForms();
        }

        private void сохранитьГистограммToolStripMenuItem_Click(object sender, EventArgs e) //Сохранить данные гистограммы в БД
        {
            if(analyse.data.Length == 1) { return; }           
            string ID_photo = listBox1.SelectedItem.ToString().Substring(1);

            string query = "SELECT COUNT(*) FROM Gist_info WHERE ID_photo='" + ID_photo + "'";
            dbCommand = new OleDbCommand(query, connection);
            OleDbDataReader reader = dbCommand.ExecuteReader();

            if (reader.HasRows)
            {
                MessageBox.Show("В БД уже есть информация о гистограммах к этому изображению", "Ошибка");
                return;
            }

            string R = (analyse.avR[0]).ToString(), 
                G = (analyse.avG[0]).ToString(), 
                B = (analyse.avB[0]).ToString();

            for(int i = 1; i < analyse.avR.Length - 1; ++i)
            {
                R += ":" + analyse.avR[i];
                G += ":" + analyse.avG[i];
                B += ":" + analyse.avB[i];
            }
            string avBright = "";
            avBright = avR.Text.Substring(10) + ":" + avG.Text.Substring(10) + ":" + avB.Text.Substring(10);

            query = "INSERT INTO Gist_info (ID_photo, gistR, gistG, gistB, avBright) "
               + "VALUES ('" + ID_photo + "', '" + R + "', '" + G + "', '" + B + "', '" + avBright + "')";
            dbCommand = new OleDbCommand(query, connection);
            dbCommand.ExecuteNonQuery();
        }

        private void загрузитьToolStripMenuItem_Click(object sender, EventArgs e) //Загрузить данные гистограммы из БД
        {
            string query = @"SELECT * FROM Gist_info WHERE ID_photo ='" + listBox1.SelectedItem.ToString().Substring(1) + "'";
            dbCommand = new OleDbCommand(query, connection);
            OleDbDataReader reader = dbCommand.ExecuteReader();

            if (!reader.HasRows)
            {
                MessageBox.Show("В БД нет информации о гистограммах к этому изображению", "Ошибка");
                return;
            }            

            while (reader.Read())
            {
                analyse.avR = Array.ConvertAll((reader["gistR"]).ToString().Split(':'), double.Parse);
                analyse.avG = Array.ConvertAll((reader["gistG"]).ToString().Split(':'), double.Parse);
                analyse.avB = Array.ConvertAll((reader["gistB"]).ToString().Split(':'), double.Parse);

                string[] avRGB = (reader["avBright"]).ToString().Split(':');
                avR.Text = "Среднее R: " + avRGB[0];
                avG.Text = "Среднее G: " + avRGB[1];
                avB.Text = "Среднее B: " + avRGB[2];
            }
            reader.Close();

            analyse.setRGB(chart1, chart2, chart3, chart4);
        }

        private void фотоToolStripMenuItem_Click(object sender, EventArgs e) //Загрузить новые экземпляры в бд
        {
            if(connectionString == "")
            {
                MessageBox.Show("Создайте подключение к БД для дальнейшей работы", "Ошибка");
                return;
            }
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