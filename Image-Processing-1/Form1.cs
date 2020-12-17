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
                    int intensity = (int)(0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B);
                    Color color = Color.FromArgb(intensity, intensity, intensity);
                    image1.SetPixel(i, j, color);
                }
            }
        }

        public void MedianFilter()
        {
            for (int i = 0; i < image1.Width; i++)
            {
                for (int j = 0; j < image1.Height; j++)
                {
                    int[] medianR = new int[9];
                    int[] medianG = new int[9];
                    int[] medianB = new int[9];

                    int p = 0;
                    for (int l = -1; l <= 1; l++)
                        for (int k = -1; k <= 1; k++)
                        {
                            int idX = Clamp(i + k, 0, image1.Width - 1);
                            int idY = Clamp(j + l, 0, image1.Height - 1);

                            medianR[p] = image1.GetPixel(idX, idY).R;
                            medianG[p] = image1.GetPixel(idX, idY).G;
                            medianB[p++] = image1.GetPixel(idX, idY).B;
                        }
                    Array.Sort(medianR);
                    Array.Sort(medianG);
                    Array.Sort(medianB);

                    Color color = Color.FromArgb(
                        Clamp(medianR[4], 0, 255),
                        Clamp(medianG[4], 0, 255),
                        Clamp(medianB[4], 0, 255));
                    image1.SetPixel(i, j, color);
                }
            }
        }
  
        public void SobelFilter()
        {
            const int size = 3;
            float[,] kernel = new float[size, size] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            float[,] kernelY = new float[size, size] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            for (int i = 0; i < image1.Width; i++)
            {
                for (int j = 0; j < image1.Height; j++)
                {
                    int radiusX = kernel.GetLength(0) / 2;
                    int radiusY = kernel.GetLength(1) / 2;
                    float resultR = 0;
                    float resultG = 0;
                    float resultB = 0;
                    float gradientX = 0;
                    float gradientY = 0;
                    for (int l = -radiusY; l <= radiusY; l++)
                        for (int k = -radiusX; k <= radiusX; k++)
                        {
                            int idX = Clamp(i + k, 0, image1.Width - 1);
                            int idY = Clamp(j + l, 0, image1.Height - 1);
                            Color neighborColor = image1.GetPixel(idX, idY);
                            resultR += neighborColor.R * kernel[k + radiusX, l + radiusY] + neighborColor.R * kernelY[k + radiusX, l + radiusY];
                            resultG += neighborColor.G * kernel[k + radiusX, l + radiusY] + neighborColor.G * kernelY[k + radiusX, l + radiusY];
                            resultB += neighborColor.B * kernel[k + radiusX, l + radiusY] + neighborColor.B * kernelY[k + radiusX, l + radiusY];
                            gradientX += kernel[k + radiusX, l + radiusY] * (neighborColor.R *  + neighborColor.G * neighborColor.B);
                            gradientY += kernelY[k + radiusX, l + radiusY] * (neighborColor.R * + neighborColor.G * neighborColor.B);
                        }
                    Color color = Color.FromArgb(
                        Clamp((int)resultR, 0, 255),
                        Clamp((int)resultG, 0, 255),
                        Clamp((int)resultB, 0, 255));
                    image2.SetPixel(i, j, color);

                    if (gradientX == 0)
                        edge[i, j] = 90;
                    else
                        edge[i, j] = (int)(Math.Atan(gradientY / gradientX) * 180 / Math.PI);
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
                        int intens1 = image2.GetPixel(i, j).R + image2.GetPixel(i, j).G + image2.GetPixel(i, j).B;
                        int intens2 = image2.GetPixel(i, j + 1).R + image2.GetPixel(i, j + 1).G + image2.GetPixel(i, j + 1).B;
                        int intens3 = image2.GetPixel(i, j - 1).R + image2.GetPixel(i, j - 1).G + image2.GetPixel(i, j - 1).B;
                        if ((intens1 < intens2) || (intens1 < intens3))
                            image1.SetPixel(i - 1, j - 1, Color.FromArgb(0, 0, 0));
                        else
                            image1.SetPixel(i, j, image2.GetPixel(i, j));
                    }
                    
                    if ((-112.5 < gradient && gradient <= -67.5) || (67.5 < gradient && gradient <= 112.5))
                    {
                        int intens1 = image2.GetPixel(i, j).R + image2.GetPixel(i, j).G + image2.GetPixel(i, j).B;
                        int intens2 = image2.GetPixel(i + 1, j).R + image2.GetPixel(i + 1, j).G + image2.GetPixel(i + 1, j).B;
                        int intens3 = image2.GetPixel(i - 1, j).R + image2.GetPixel(i - 1, j).G + image2.GetPixel(i - 1, j).B;
                        if ((intens1 < intens2) || (intens1 < intens3))
                            image1.SetPixel(i - 1, j - 1, Color.FromArgb(0, 0, 0));
                        else
                            image1.SetPixel(i, j, image2.GetPixel(i, j));
                    }

                    if ((-67.5 < gradient && gradient <= -22.5) || (112.5 < gradient && gradient <= 157.5))
                    {
                        int intens1 = image2.GetPixel(i, j).R + image2.GetPixel(i, j).G + image2.GetPixel(i, j).B;
                        int intens2 = image2.GetPixel(i - 1, j + 1).R + image2.GetPixel(i - 1, j + 1).G + image2.GetPixel(i - 1, j + 1).B;
                        int intens3 = image2.GetPixel(i + 1, j - 1).R + image2.GetPixel(i + 1, j - 1).G + image2.GetPixel(i + 1, j - 1).B;
                        if ((intens1 < intens2) || (intens1 < intens3))
                            image1.SetPixel(i - 1, j - 1, Color.FromArgb(0, 0, 0));
                        else
                            image1.SetPixel(i, j, image2.GetPixel(i, j));
                    }

                    if ((-157.5 < gradient && gradient <= -112.5) || (22.5 < gradient && gradient <= 67.5))
                    {
                        int intens1 = image2.GetPixel(i, j).R + image2.GetPixel(i, j).G + image2.GetPixel(i, j).B;
                        int intens2 = image2.GetPixel(i + 1, j + 1).R + image2.GetPixel(i + 1, j + 1).G + image2.GetPixel(i + 1, j + 1).B;
                        int intens3 = image2.GetPixel(i - 1, j - 1).R + image2.GetPixel(i - 1, j - 1).G + image2.GetPixel(i - 1, j - 1).B;
                        if ((intens1 < intens2) || (intens1 < intens3))
                            image1.SetPixel(i - 1, j - 1, Color.FromArgb(0, 0, 0));
                        else
                            image1.SetPixel(i, j, image2.GetPixel(i, j));
                    }
                }
            }
        }

        public void DualThresholdFiltering()
        {
            int down = 90;
            int up = 120;
            for (int i = 1; i < image1.Width - 1; i++)
            {
                for (int j = 1; j < image1.Height - 1; j++)
                {
                    if (image1.GetPixel(i, j).R > up && image1.GetPixel(i, j).G > up && image1.GetPixel(i, j).B > up)
                        image2.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                    else if (image1.GetPixel(i, j).R < down && image1.GetPixel(i, j).G < down && image1.GetPixel(i, j).B < down)
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

            GrayScaleFilter();
            MedianFilter();
            SobelFilter();
            noneMax();
            DualThresholdFiltering();
            pictureBox1.Image = image1;
            pictureBox1.Refresh();
            pictureBox2.Image = image2;
            pictureBox2.Refresh();
        }

        private void removeNoiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.Visible = false;
            label2.Visible = false;
            pictureBox1.Visible = false;
            pictureBox2.Visible = false;

            OpenFileDialog dialog1 = new OpenFileDialog();
            dialog1.Filter = "Image files|*.png;*.jpg;*.bmp|All filec(*.*)|*.*";
            if (dialog1.ShowDialog() == DialogResult.OK)
            {
                label1.Visible = true;
                label1.Text = "Processing...";
                image1 = new Bitmap(dialog1.FileName);
                pictureBox1.Image = image1;
                pictureBox1.Width = 380;
                pictureBox1.Visible = true;
                pictureBox1.Refresh();

            }
            else { return; }

            if (dialog1.ShowDialog() == DialogResult.OK)
            {
                label2.Visible = true;
                label2.Text = "Processing...";
                image2 = new Bitmap(dialog1.FileName);
                pictureBox2.Image = image2;
                pictureBox2.Width = 380;
                pictureBox2.Visible = true;
                pictureBox2.Refresh();

            }
            else { return; }





            int Clamp(int value, int min, int max)
            {
                if (value < min)
                    return min;
                if (value > max)
                    return max;
                return value;
            }
            //MyMedianFilter
            int size = 3;
            Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                int[] medianR = new int[size * size];
                int[] medianG = new int[size * size];
                int[] medianB = new int[size * size];

                int radius = size / 2;
                int i = 0;
                for (int l = -radius; l <= radius; l++)
                    for (int k = -radius; k <= radius; k++)
                    {
                        int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                        int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                        medianR[i] = sourceImage.GetPixel(idX, idY).R;
                        medianG[i] = sourceImage.GetPixel(idX, idY).G;
                        medianB[i++] = sourceImage.GetPixel(idX, idY).B;
                    }
                Array.Sort(medianR);
                Array.Sort(medianG);
                Array.Sort(medianB);

                return Color.FromArgb(
                    Clamp(medianR[size * size / 2], 0, 255),
                    Clamp(medianG[size * size / 2], 0, 255),
                    Clamp(medianB[size * size / 2], 0, 255));
            }


            //Time start

            var watch1 = System.Diagnostics.Stopwatch.StartNew();
            //OpenCV Median filter
            Mat srcImage = Cv2.ImRead(dialog1.FileName);
            Mat filteredImage = new Mat();
            Cv2.MedianBlur(srcImage, filteredImage, 3);
            //Time end
            watch1.Stop();
            label2.Text = "OpenCV MedianFilter elapsed time: " + watch1.ElapsedMilliseconds.ToString() + "ms";

            image2 = filteredImage.ToBitmap();
            pictureBox2.Image = image2;
            pictureBox2.Show();
            pictureBox2.Refresh();

            Bitmap resultImage = new Bitmap(image1.Width, image1.Height);
            


            //Time start
            watch1 = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < image1.Width; i++)
            {
                for (int j = 0; j < image1.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(image1, i, j));
                }
            }
            watch1.Stop();
            //Time end
            label1.Text = "MyMedianFilter elapsed time: " + watch1.ElapsedMilliseconds.ToString() + "ms";


            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();

        }

    }
    }
