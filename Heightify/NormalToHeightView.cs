using System;
using System.Drawing;
using System.Windows.Forms;

namespace Heightify
{
    public partial class NormalToHeightView : Form
    {
        private Bitmap _generatedHeightmap;

        public NormalToHeightView()
        {
            InitializeComponent();
        }

        private void LoadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select Normal Map Image";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (Bitmap sourceImage = new Bitmap(openFileDialog.FileName))
                    {
                        var processor = new HeightmapProcessor(x => x);

                        // Dispose previously generated bitmap to prevent unmanaged GDI+ leaks
                        _generatedHeightmap?.Dispose();
                        _generatedHeightmap = processor.ConvertNormalToHeightMap(sourceImage);
                    }

                    pictureBox1.Image = _generatedHeightmap;
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (_generatedHeightmap == null)
            {
                MessageBox.Show("No generated heightmap available to save.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                saveFileDialog.Title = "Save Generated Heightmap";
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _generatedHeightmap.Save(saveFileDialog.FileName);
                    MessageBox.Show("Heightmap successfully exported.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

    }
}
