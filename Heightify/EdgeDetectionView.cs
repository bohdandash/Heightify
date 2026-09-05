using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Heightify
{
    public partial class EdgeDetectionView : Form
    {
        public EdgeDetectionView()
        {
            InitializeComponent();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog1.Title = "Select an Image File";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string imagePath = openFileDialog1.FileName; // the path to the selected image

                    // check if not null or empty
                    if (!string.IsNullOrEmpty(imagePath))
                    {

                        Mat processedImage;
                        Mat invertedImage;


                        Mat image = Cv2.ImRead(imagePath, ImreadModes.Grayscale);

                        
                        image = Cv2.ImRead(imagePath, ImreadModes.Grayscale);
                        processedImage = ProcessWithSobelAndGaussian(image);

                       
                        Cv2.ImShow("Processed Image", processedImage);
                        invertedImage = InvertColors(processedImage);
                        Cv2.ImShow("Reversed Image", invertedImage);
                        Cv2.WaitKey(0);
                        Cv2.DestroyAllWindows();

                        SaveAndDisplayImage(invertedImage);


                    }
                }
            }
        }

        private Mat ProcessWithSobelAndGaussian(Mat image)
        {
            // Gaussian filter to smooth the image.
            Mat blurredImage = new Mat();
            Cv2.GaussianBlur(image, blurredImage, new OpenCvSharp.Size(3, 3), sigmaX: 0);

            // Sobel operator to highlight the boundaries
            Mat Gx = blurredImage.Sobel(MatType.CV_64F, 1, 0, ksize: 3);
            Mat Gy = blurredImage.Sobel(MatType.CV_64F, 0, 1, ksize: 3);

            // calculate the gradient value
            Mat gradientMagnitude = new Mat();
            Cv2.Sqrt(Gx.Mul(Gx) + Gy.Mul(Gy), gradientMagnitude);

            // normalize the gradient value and convert it to an 8-bit image
            Cv2.Normalize(gradientMagnitude, gradientMagnitude, 0, 255, NormTypes.MinMax);
            gradientMagnitude.ConvertTo(gradientMagnitude, MatType.CV_8U);

            return gradientMagnitude;
        }

        private static Mat InvertColors(Mat image)
        {
            Mat invertedImage = new Mat(image.Rows, image.Cols, MatType.CV_8U);

            for (int y = 0; y < image.Rows; y++)
            {
                for (int x = 0; x < image.Cols; x++)
                {
                    // get the current pixel intensity value
                    byte intensity = image.At<byte>(y, x);

                    // invert intensity
                    byte invertedIntensity = (byte)(255 - intensity);

                    // set the inverted intensity for a pixel in an image
                    invertedImage.Set<byte>(y, x, invertedIntensity);
                }
            }

            return invertedImage;
        }

        private void SaveAndDisplayImage(Mat image)
        {
            // have an image?
            if (image == null)
            {
                MessageBox.Show("Error: wrong image path", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // save
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            saveFileDialog.Title = "Select an Image File";
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string outputPath = saveFileDialog.FileName;
                Cv2.ImWrite(outputPath, image);

                using (var fs = new FileStream(outputPath, FileMode.Open, FileAccess.Read))
                {
                    pictureBox1.Image = System.Drawing.Image.FromStream(fs);
                }
            }
        }

    }
}
