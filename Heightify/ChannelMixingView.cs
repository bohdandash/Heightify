using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Heightify
{
    public partial class ChannelMixingView : Form
    {
        private Bitmap _processedBitmap;
        private readonly HeightmapProcessor _processor = new HeightmapProcessor(x => x);

        public ChannelMixingView()
        {
            InitializeComponent();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            // floating-point parsing
            var culture = CultureInfo.InvariantCulture;
            if (!double.TryParse(guna2TextBox1.Text, NumberStyles.Float, culture, out double red) ||
                !double.TryParse(guna2TextBox2.Text, NumberStyles.Float, culture, out double green) ||
                !double.TryParse(guna2TextBox3.Text, NumberStyles.Float, culture, out double blue))
            {
                MessageBox.Show("Please enter valid numeric channel weights (e.g. 0.3, 0.59, 0.11).", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select Image for Channel Mixing";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _processedBitmap?.Dispose();
                        _processedBitmap = _processor.MixChannelsToHeightmap(openFileDialog.FileName, red, green, blue);

                        // display directly 
                        pictureBox1.Image = _processedBitmap;

                        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                        {
                            saveFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                            saveFileDialog.Title = "Save Processed Heightmap";
                            saveFileDialog.RestoreDirectory = true;

                            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                _processedBitmap.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                                MessageBox.Show("Heightmap successfully saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Processing failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }

    }
}
