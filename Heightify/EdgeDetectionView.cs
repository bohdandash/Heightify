using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Heightify
{
    public partial class EdgeDetectionView : Form
    {

        private Bitmap _processedBitmap;
        private readonly HeightmapProcessor _processor = new HeightmapProcessor(x => x);

        public EdgeDetectionView()
        {
            InitializeComponent();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select Image for Edge Detection";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _processedBitmap?.Dispose();
                        _processedBitmap = _processor.GenerateEdgeHeightmap(openFileDialog.FileName);

                        pictureBox1.Image = _processedBitmap;

                        // immediate export
                        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                        {
                            saveFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                            saveFileDialog.Title = "Save Processed Heightmap";
                            saveFileDialog.RestoreDirectory = true;

                            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                _processedBitmap.Save(saveFileDialog.FileName, ImageFormat.Png);
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
