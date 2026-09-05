using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra;

namespace Heightify
{
    public partial class NormalToHeightView : Form
    {
        public NormalToHeightView()
        {
            InitializeComponent();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            openFileDialog1.Title = "Select an Image File";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string imagePath = openFileDialog1.FileName;
                Bitmap image = new Bitmap(imagePath);

                ProcessImage(image);
            }
        }

        private void ProcessImage(Bitmap image)
        {
            Bitmap normalMap = ConvertToNormalMap(image);
            Bitmap curvatureMap = ConvertToCurvatureMap(normalMap);
            Bitmap heightMap = ConvertToHeightMap(curvatureMap);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            saveFileDialog.Title = "Where to save the file";
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string outputPath = saveFileDialog.FileName;
                heightMap.Save(outputPath);

                // insert the picture into pictureBox
                pictureBox1.Image = heightMap;
            }
        }


        private Bitmap ConvertToNormalMap(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            Bitmap normalMap = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    float nx = (pixelColor.R / 255.0f) * 2 - 1;
                    float ny = (pixelColor.G / 255.0f) * 2 - 1;
                    float nz = (pixelColor.B / 255.0f) * 2 - 1;
                    float length = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
                    nx /= length;
                    ny /= length;
                    nz /= length;
                    Color normalColor = Color.FromArgb((int)((nx + 1) * 127), (int)((ny + 1) * 127), (int)((nz + 1) * 127));
                    normalMap.SetPixel(x, y, normalColor);
                }
            }
            return normalMap;
        }

        private Bitmap ConvertToCurvatureMap(Bitmap normalMap)
        {
            int width = normalMap.Width;
            int height = normalMap.Height;
            Bitmap curvatureMap = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = normalMap.GetPixel(x, y);
                    float curvature = (pixelColor.R / 255.0f) * 2 - 1;
                    int curvatureValue = (int)((curvature + 1) * 127.5f);
                    curvatureValue = Math.Max(0, Math.Min(255, curvatureValue));
                    Color curvatureColor = Color.FromArgb(curvatureValue, curvatureValue, curvatureValue);
                    curvatureMap.SetPixel(x, y, curvatureColor);
                }
            }
            return curvatureMap;
        }

        private Bitmap ConvertToHeightMap(Bitmap curvatureMap)
        {
            int width = curvatureMap.Width;
            int height = curvatureMap.Height;
            Bitmap heightMap = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color curvatureColor = curvatureMap.GetPixel(x, y);
                    float curvature = (curvatureColor.R / 255.0f) * 2 - 1;
                    float heightValue = curvature;
                    int heightIntValue = (int)((heightValue + 1) * 127.5f);
                    heightIntValue = Math.Max(0, Math.Min(255, heightIntValue));
                    Color heightColor = Color.FromArgb(heightIntValue, heightIntValue, heightIntValue);
                    heightMap.SetPixel(x, y, heightColor);
                }
            }
            return heightMap;
        }
    }
}
