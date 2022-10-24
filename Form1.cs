using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace обработка_изображений_1
{
    public partial class Form1 : Form
    {
        Bitmap image;
        public Form1()
        {
            InitializeComponent();
        }

        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
        private Color GlobalThresholdcalculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor;
            if (sourceColor.R > 127)
                resultColor = Color.FromArgb(255, 255, 255);
            else
                resultColor = Color.FromArgb(0, 0, 0);
            return resultColor;
        }

        public static int[] HistogrammcalculateNewPixelColor(Bitmap sourceImage)
        {
            int[] result = new int[256];

            for (int i = 0; i < sourceImage.Height; i++)
                for (int j = 0; j < sourceImage.Width; j++)
                {
                    Color color = sourceImage.GetPixel(j, i);
                    result[color.R]++;
                }

            return result;
        }


        private void globalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap sourceImage = image;
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            //Вычислить гистограмму.
            int[] hist= HistogrammcalculateNewPixelColor(sourceImage);

            // Отсечь 5% от мин и макс пикселей.

            int sumHist = hist.Sum();
            int cut = (int)(sumHist * 0.05); // 5% 

            for (int i = 0; i < 255; i++)
            {
                if (hist[i] < cut)
                {
                    cut -= hist[i];
                    hist[i] = 0;
                }
                else
                {
                    hist[i] -= cut;
                }
                if (cut == 0) break;

            }

            cut = (int)(sumHist * 0.05);

            for (int i = 255; i < 0; i--)
            {
                if (hist[i] < cut)
                {
                    cut -= hist[i];
                    hist[i] = 0;
                }
                else
                {
                    hist[i] -= cut;
                }
                if (cut == 0) break;

            }

            // Найти взвешенное среднее
            int t = 0;

            int weight = 0;
            for (int i = 0; i < 255; i++)
            {
                if (hist[i] == 0) continue;

                weight += hist[i] * i;
            }

            // Вычисление порога
            t = (int)(weight / hist.Sum());

            for (int y = 0; y < sourceImage.Height; y++)
                for (int x = 0; x < sourceImage.Width; x++)
                {
                    Color color = sourceImage.GetPixel(x, y);
                    if (color.R >= t) resultImage.SetPixel(x, y, Color.White);
                    else resultImage.SetPixel(x, y, Color.Black);

                }
            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();
        }

        private Color NiblackcalculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            double avgSum = 0;
            double standardDev = 0;
            double avgStandardDev = 0;
            double k = -0.2;
            int sum = 0;
            int threshold = 0;
            int w = 5;
            int sqCount = (int)Math.Pow(w * 2 + 1, 2);

            for (int l = -w; l <= w; l++)
            {
                for (int m = -w; m <= w; m++)
                {

                    int idX = Clamp(x + m, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    sum += neighborColor.R;
                }
            }
            avgSum = sum / sqCount;
            for (int l = -w; l <= w; l++)
            {
                for (int m = -w; m <= w; m++)
                {
                    int idX = Clamp(x + m, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    standardDev += Math.Pow(neighborColor.R - avgSum, 2);
                }
            }
            avgStandardDev = Math.Sqrt(standardDev / sqCount);
            threshold = (int)(avgStandardDev * k + avgSum);
            if (sourceImage.GetPixel(x, y).R > threshold)
                return Color.FromArgb(255, 255, 255);
            else
                return Color.FromArgb(0, 0, 0);
        }

        private void globalThresholdToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Bitmap sourceImage = image;
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, GlobalThresholdcalculateNewPixelColor(sourceImage, i, j));
                }
            }

            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();
        }

        private void niblackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap sourceImage = image;
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, NiblackcalculateNewPixelColor(sourceImage, i, j));
                }
            }

            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();
        }


        private void загрузитьФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp | All Files (*.*) | *.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
        }
        private void сохранитьФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null) //если в pictureBox есть изображение
            {
                //создание диалогового окна "Сохранить как..", для сохранения изображения
                SaveFileDialog savedialog = new SaveFileDialog();
                savedialog.Title = "Сохранить как...";
                //отображать ли предупреждение, если пользователь указывает имя уже существующего файла
                savedialog.OverwritePrompt = true;
                //отображать ли предупреждение, если пользователь указывает несуществующий путь
                savedialog.CheckPathExists = true;
                //список форматов файла, отображаемый в поле "Тип файла"
                savedialog.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";
                //отображается ли кнопка "Справка" в диалоговом окне
                savedialog.ShowHelp = true;
                if (savedialog.ShowDialog() == DialogResult.OK) //если в диалоговом окне нажата кнопка "ОК"
                {
                    try
                    {
                        image.Save(savedialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
    }
}
