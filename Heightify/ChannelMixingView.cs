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
    public partial class ChannelMixingView : Form
    {
        public ChannelMixingView()
        {
            InitializeComponent();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            // rgb values checkup
            if (!double.TryParse(guna2TextBox1.Text, out double red) || !double.TryParse(guna2TextBox2.Text, out double green) || !double.TryParse(guna2TextBox3.Text, out double blue))
            {
                MessageBox.Show("Incorrect values ​​entered for RGB channels.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog1.Title = "Select an Image File";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string imagePath = openFileDialog1.FileName;

                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        Mat processedImage;

                        Mat image = Cv2.ImRead(imagePath, ImreadModes.Grayscale);

                        image = Cv2.ImRead(imagePath, ImreadModes.AnyColor);
                        processedImage = RGBchannel(image, red, green, blue);

                        Cv2.ImShow("Processed Image", processedImage);
                        Cv2.WaitKey(0);
                        Cv2.DestroyAllWindows();

                        SaveAndDisplayImage(processedImage);
                    }
                }
            }
        }


        private Mat RGBchannel(Mat image, double redChannel, double greenChannel, double blueChannel)
        {
            // has an image rgb channels?
            if (image.Channels() != 3)
            {
                MessageBox.Show("Error: the image must have three channels (RGB)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            // splitting the image channels
            Mat[] channels = Cv2.Split(image);

            // weight coefficients for each channel
            double[] weights = { redChannel, greenChannel, blueChannel };

            if (channels.Length != 3 || channels[0].Size() != channels[1].Size() || channels[1].Size() != channels[2].Size())
            {
                MessageBox.Show("Error: Image channel sizes do not match", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            // matrix for storing the resulting image
            Mat result = new Mat(image.Size(), MatType.CV_32F);

            //for each channel
            for (int i = 0; i < channels.Length; i++)
            {
                channels[i].ConvertTo(channels[i], MatType.CV_32F);
                channels[i] *= weights[i];
            }

            // combining into one channel
            Cv2.Add(channels[0], channels[1], result);
            Cv2.Add(result, channels[2], result);

            // normalizing height map values
            Cv2.Normalize(result, result, 0, 255, NormTypes.MinMax);

            // converting the resulting matrix to the CV_8U type(for a pixel to have 0-255 values)
            result.ConvertTo(result, MatType.CV_8U);

            return result;
        }


        private void SaveAndDisplayImage(Mat image)
        {
            // have an image?
            if (image == null)
            {
                MessageBox.Show("Error: wrong image path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            saveFileDialog.Title = "Where to save the file";
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
