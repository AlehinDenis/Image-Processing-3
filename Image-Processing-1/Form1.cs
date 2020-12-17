using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Image_Processing_1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Bitmap image1, image2;
        int[,] edge;
        int[,] grayImage;

        int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public void GrayScaleFilter()
        {
            for (int i = 0; i < image1.Width; i++)
            {
                for (int j = 0; j < image1.Height; j++)
                {
                    
                    Color sourceColor = image1.GetPixel(i, j);
                    grayImage[i, j] = (int)(0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B);
                }
            }
        }

        public void MedianFilter()
        {
            for (int i = 0; i < image1.Width; i++)
            {
                for (int j = 0; j < image1.Height; j++)
                {
                    int[] median = new int[9];

                    int p = 0;
                    for (int l = -1; l <= 1; l++)
                        for (int k = -1; k <= 1; k++)
                        {
                            int idX = Clamp(i + k, 0, image1.Width - 1);
                            int idY = Clamp(j + l, 0, image1.Height - 1);

                            median[p++] = grayImage[idX, idY];
                        }
                    Array.Sort(median);

                    median[4] = Clamp(median[4], 0, 255);
                    Color color = Color.FromArgb(median[4], median[4], median[4]);
                }
            }
        }
  
        public void edges()
        {
            const int size = 3;
            float[,] kernel = new float[size, size] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            float[,] kernelY = new float[size, size] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            for (int i = 1; i < image1.Width - 1; i++)
            {
                for (int j = 1; j < image1.Height - 1; j++)
                {
                    int radiusX = kernel.GetLength(0) / 2;
                    int radiusY = kernel.GetLength(1) / 2;
                    float gradientX = 0;
                    float gradientY = 0;
                    for (int l = -radiusY; l <= radiusY; l++)
                    {
                        for (int k = -radiusX; k <= radiusX; k++)
                        {
                            int idX = Clamp(i + k, 0, image1.Width - 1);
                            int idY = Clamp(j + l, 0, image1.Height - 1);
                            gradientX += kernel[k + radiusX, l + radiusY] * grayImage[idX, idY];
                            gradientY += kernelY[k + radiusX, l + radiusY] * grayImage[idX, idY];
                        }
                    }
                    if (gradientX == 0)
                        edge[i - 1, j - 1] = 90;
                    else
                        edge[i - 1, j - 1] = (int)(Math.Atan(gradientY / gradientX) * 180 / Math.PI);
                }
            }
        }

        public void SobelFilter()
        {
            const int size = 3;
            float[,] kernel = new float[size, size] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            float[,] kernelY = new float[size, size] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            for (int i = 1; i < image1.Width - 1; i++)
            {
                for (int j = 1; j < image1.Height - 1; j++)
                {
                    int radiusX = kernel.GetLength(0) / 2;
                    int radiusY = kernel.GetLength(1) / 2;
                    float gradientX = 0;
                    float gradientY = 0;
                    for (int l = -radiusY; l <= radiusY; l++)
                        for (int k = -radiusX; k <= radiusX; k++)
                        {
                            int idX = Clamp(i + k, 0, image1.Width - 1);
                            int idY = Clamp(j + l, 0, image1.Height - 1);
                            gradientX += kernel[k + radiusX, l + radiusY] * grayImage[idX, idY];
                            gradientY += kernelY[k + radiusX, l + radiusY] * grayImage[idX, idY];
                        }

                    grayImage[i - 1, j - 1] = Clamp((int)Math.Sqrt(gradientX * gradientX + gradientY + gradientY), 0, 255);
                }
            }
        }

        public void noneMax()
        {
            for (int i = 1; i < image2.Width - 1; i++)
            {
                for (int j = 1; j < image2.Height - 1; j++)
                {
                    float gradient = edge[i, j];
                    if ((-22.5 < gradient && gradient <= 22.5) || (157.5 < gradient && gradient <= -157.5))
                    {
                        if ((grayImage[i, j] < grayImage[i, j + 1]) || (grayImage[i, j] < grayImage[i, j - 1]))
                            grayImage[i, j] = 0;
                    }
                    
                    if ((-112.5 < gradient && gradient <= -67.5) || (67.5 < gradient && gradient <= 112.5))
                    {
                        if ((grayImage[i, j] < grayImage[i + 1, j]) || (grayImage[i, j] < grayImage[i -1, j]))
                            grayImage[i, j] = 0;
                    }

                    if ((-67.5 < gradient && gradient <= -22.5) || (112.5 < gradient && gradient <= 157.5))
                    {
                        if ((grayImage[i, j] < grayImage[i - 1, j + 1]) || (grayImage[i, j] < grayImage[i + 1, j - 1]))
                            grayImage[i, j] = 0;
                    }

                    if ((-157.5 < gradient && gradient <= -112.5) || (22.5 < gradient && gradient <= 67.5))
                    {
                        if ((grayImage[i, j] < grayImage[i + 1, j + 1]) || (grayImage[i, j] < grayImage[i - 1, j - 1]))
                            grayImage[i, j] = 0;
                    }
                }
            }
        }

        public void DualThresholdFiltering()
        {
            int down = 100;
            int up = 150;
            for (int i = 1; i < image1.Width - 1; i++)
            {
                for (int j = 1; j < image1.Height - 1; j++)
                {
                    if (grayImage[i, j] > up)
                        image2.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                    else if (grayImage[i, j] < down)
                        image2.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    //else image2.SetPixel(i, j, Color.FromArgb(127, 127, 127));
                } 
            } 
        }

        private void addNoiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog1 = new OpenFileDialog();
            dialog1.Filter = "Image files|*.png;*.jpg;*.bmp|All filec(*.*)|*.*";
            if (dialog1.ShowDialog() == DialogResult.OK)
            {
                image1 = new Bitmap(dialog1.FileName);
                pictureBox1.Image = image1;
                pictureBox1.Refresh();
            }
            else { return; }

            image2 = new Bitmap(image1.Width,image1.Height);
            edge = new int[image1.Width, image1.Height];
            grayImage = new int[image1.Width, image1.Height];

            GrayScaleFilter();
            MedianFilter();
            edges();
            SobelFilter();
            noneMax();
            DualThresholdFiltering();
            pictureBox2.Image = image2;
            pictureBox2.Refresh();
        }


        public static Bitmap HoughLine(Bitmap inputBitmap, int cross_num = 100)
        {
            int width = inputBitmap.Width;
            int height = inputBitmap.Height;
            int rho_max = (int)Math.Floor(Math.Sqrt(width * width + height * height)) + 1;                                                                             
            var PreLines = new Dictionary<(int, int), int>();


            double AngleToRadians(int angle) => angle * Math.PI / 180.0;
            double GetRho(int x, int y, int k) => (y * Math.Sin(AngleToRadians(k))) + (x * Math.Cos(AngleToRadians(k)));
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pixel = inputBitmap.GetPixel(x, y);
                    if (pixel.ToArgb() != Color.White.ToArgb())
                    {
                        for (int k = 0; k < 180; k++)
                        {

                            var rho = GetRho(x, y, k);

                            var rhoIndex = (int)Math.Round(rho / 2 + rho_max / 2);
                            var key = (rhoIndex, k);
                            if (!PreLines.TryGetValue(key, out var count))
                            {
                                PreLines.Add(key, 1);
                            }
                            else
                            {
                                PreLines[key] = count + 1;
                            }
                        }
                    }
                }
            }

            var lines = PreLines.Where(z => z.Value >= cross_num).Select(z => (z.Key.Item1, z.Key.Item2)).ToList();

            var outputBitmap = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {

                    outputBitmap.SetPixel(x, y, inputBitmap.GetPixel(x, y));

                    var pixel = inputBitmap.GetPixel(x, y);
                    if (pixel.ToArgb() != Color.White.ToArgb())
                    {
                        for (int k = 0; k < 180; k++)
                        {
                            var rho = GetRho(x, y, k);
                            var rho_int = (int)Math.Round(rho / 2 + rho_max / 2);

                            if (lines.Any(l => l.Item1 == rho_int && l.Item2 == k))
                            {
                                outputBitmap.SetPixel(x, y, Color.Red);
                            }
                        }
                    }
                }
            }
            return outputBitmap;
        }

    private void removeNoiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog1 = new OpenFileDialog();
            dialog1.Filter = "Image files|*.png;*.jpg;*.bmp|All filec(*.*)|*.*";
            if (dialog1.ShowDialog() == DialogResult.OK)
            {
                image1 = new Bitmap(dialog1.FileName);
                pictureBox1.Image = image1;
                pictureBox1.Refresh();
            }
            else { return; }

            pictureBox2.Image = HoughLine(image1, 100);
            pictureBox2.Refresh();
        }

    }
}
