using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;

namespace Heightify
{
    public partial class TransferFunctionView : Form
    {
        private Func<double, double> _selectedFunction;
        private Bitmap _currentBitmap;

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

            // Set default function to prevent null reference issues
            radioButtonLinear.Checked = true;
            _selectedFunction = LinearFunction;
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                _selectedFunction = ResolveSelectedTransferFunction(radioButton.Name);
            }
        }

        /// <summary>
        /// Builds the transfer function delegate, caching UI parameters beforehand
        /// to avoid expensive UI thread queries inside the inner pixel loop.
        /// </summary>
        private Func<double, double> ResolveSelectedTransferFunction(string radioButtonName)
        {
            var culture = CultureInfo.InvariantCulture;

            switch (radioButtonName)
            {
                case "radioButtonLogarithmic":
                    double logBase = double.TryParse(guna2TextBox4.Text, NumberStyles.Float, culture, out double parsedBase) && parsedBase > 1.0
                        ? parsedBase
                        : 2.0;
                    return intensity => Math.Min(Math.Max(Math.Log(intensity + 1.0, logBase), 0.0), 1.0);

                case "radioButtonLinear":
                    return LinearFunction;

                case "radioButtonPolynomial":
                    // Cache polynomial coefficients once
                    double.TryParse(guna2TextBox1.Text, NumberStyles.Float, culture, out double a);
                    double.TryParse(guna2TextBox2.Text, NumberStyles.Float, culture, out double b);
                    double.TryParse(guna2TextBox3.Text, NumberStyles.Float, culture, out double c);
                    return intensity => Math.Min(Math.Max(a * intensity * intensity + b * intensity + c, 0.0), 1.0);

                case "radioButtonExponential":
                    const double baseValue = Math.E;
                    double.TryParse(guna2TextBox6.Text, NumberStyles.Float, culture, out double exponent);
                    return intensity => Math.Max(0.0, Math.Min(1.0, Math.Pow(baseValue, intensity * exponent) - 1.0));

                default:
                    return LinearFunction;
            }
        }

        private static double LinearFunction(double intensity) => intensity;

        private void LoadButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select Source Heightmap Image";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Refresh delegate with the latest parameters from textboxes
                    _selectedFunction = GetCurrentActiveFunction();

                    var processor = new HeightmapProcessor(_selectedFunction);

                    using (Bitmap originalImage = new Bitmap(openFileDialog.FileName))
                    {
                        _currentBitmap?.Dispose();
                        _currentBitmap = processor.ConvertToHeightMap(originalImage);
                    }

                    pictureBox1.Image = _currentBitmap;
                }
            }
        }

        private Func<double, double> GetCurrentActiveFunction()
        {
            if (radioButtonLogarithmic.Checked) return ResolveSelectedTransferFunction("radioButtonLogarithmic");
            if (radioButtonPolynomial.Checked) return ResolveSelectedTransferFunction("radioButtonPolynomial");
            if (radioButtonExponential.Checked) return ResolveSelectedTransferFunction("radioButtonExponential");
            return LinearFunction;
        }

        // Dedicated UI action for saving - decoupled from calculation engine
        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (_currentBitmap == null)
            {
                MessageBox.Show("No generated heightmap to save.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                saveFileDialog.Title = "Save Processed Heightmap";
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _currentBitmap.Save(saveFileDialog.FileName);
                    MessageBox.Show("Heightmap successfully saved to storage.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
