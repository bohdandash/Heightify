using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Heightify
{
    public partial class TransferFunctionView : Form
    {
        private Func<double, double> selectedFunction;

        public TransferFunctionView()
        {
            InitializeComponent();
            InitializeRadioButtons();
        }

        private void InitializeRadioButtons()
        {
            radioButtonLogarithmic.CheckedChanged += RadioButton_CheckedChanged;
            radioButtonLinear.CheckedChanged += RadioButton_CheckedChanged;
            radioButtonPolynomial.CheckedChanged += RadioButton_CheckedChanged;
            radioButtonExponential.CheckedChanged += RadioButton_CheckedChanged;
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null && radioButton.Checked)
            {
                selectedFunction = GetFunctionForRadioButton(radioButton);
            }
        }

        private Func<double, double> GetFunctionForRadioButton(RadioButton radioButton)
        {
            switch (radioButton.Name)
            {
                case "radioButtonLogarithmic":
                    return LogarithmicFunction;
                case "radioButtonLinear":
                    return LinearFunction;
                case "radioButtonPolynomial":
                    return PolynomialFunction;
                case "radioButtonExponential":
                    return ExponentialFunction;
                default:
                    return null;
            }
        }

        // conversion functions
        private double LogarithmicFunction(double intensity)
        {
            double logbase = Convert.ToDouble(guna2TextBox4.Text);
            return (double)Math.Min(Math.Max(Math.Log(intensity + 1, logbase), 0), 1);
        }

        private double LinearFunction(double intensity)
        {
            return intensity;
        }

        private double PolynomialFunction(double intensity)
        {
            // example of a polynomial function
            double a = Convert.ToDouble(guna2TextBox1.Text);
            double b = Convert.ToDouble(guna2TextBox2.Text);
            double c = Convert.ToDouble(guna2TextBox3.Text);
            return Math.Min(Math.Max(a * intensity * intensity + b * intensity + c, 0), 1);
        }

        private double ExponentialFunction(double intensity)
        {
            // example of an exponential function
            double baseValue = 2.71828182846;
            double exponent = Convert.ToDouble(guna2TextBox6.Text);
            return Math.Max(0, Math.Min(1, (double)Math.Pow(baseValue, intensity * exponent) - 1));
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string inputImagePath = openFileDialog.FileName; // get image path

                // converting an image into a height map
                HeightMapConverter converter = new HeightMapConverter(selectedFunction);
                Bitmap inputImage = new Bitmap(inputImagePath);
                Bitmap heightMap = converter.ConvertToHeightMap(inputImage);

                pictureBox1.Image = heightMap;

                converter.SaveHeightMap(heightMap);
            }
        }

        // converting an image to a heightmap
        private class HeightMapConverter
        {
            private Func<double, double> function;

            public HeightMapConverter(Func<double, double> function)
            {
                this.function = function;
            }

            public Bitmap ConvertToHeightMap(Bitmap inputImage)
            {
                int width = inputImage.Width;
                int height = inputImage.Height;
                Bitmap heightMap = new Bitmap(width, height);

                // go through each pixel of the image
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // get the color
                        Color pixelColor = inputImage.GetPixel(x, y);

                        // converting to a shade of gray
                        int grayValue = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);

                        // the value is in the range from 0 to 255?
                        grayValue = Math.Max(0, Math.Min(255, grayValue));

                        // get the height value
                        double heightValue = function(grayValue / 255f);

                        // set the height value for a pixel on the height map
                        heightMap.SetPixel(x, y, Color.FromArgb((int)(heightValue * 255), (int)(heightValue * 255), (int)(heightValue * 255)));
                    }
                }

                return heightMap;
            }

            public void SaveHeightMap(Bitmap heightMap)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                saveFileDialog.Title = "Select an Image File";
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string outputPath = saveFileDialog.FileName;
                    heightMap.Save(outputPath);
                    MessageBox.Show("Карта висот збережена на носії.");
                }
            }

        }

    }
}
