using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Heightify
{
    public partial class MainContainerForm : Form
    {
        //to create a round angle for app design
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]

        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        public MainContainerForm()
        {
            InitializeComponent();
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
        }

        public void Loadform(object Form)
        {
            if(this.formshow.Controls.Count > 0)
            {
                this.formshow.Controls.RemoveAt(0);
            }
            Form f = Form as Form;
            f.TopLevel = false;
            f.Dock = DockStyle.Fill;
            this.formshow.Controls.Add(f);
            this.formshow.Tag = f;
            f.Show();
        }


        private void CloseButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void QuestionButton_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/bohdandash/ProjectHeightify";

            // open
            System.Diagnostics.Process.Start(url);
        }

        // when button TensorButton is clicked
        private void TensorButton_Click(object sender, EventArgs e)
        {
            TensorButton.FillColor = Color.FromArgb(30, 170, 231);

            RGBButton.FillColor = Color.FromArgb(15, 15, 23);
            SobelButton.FillColor = Color.FromArgb(15, 15, 23);
            INH.FillColor = Color.FromArgb(15, 15, 23);

            Loadform(new TransferFunctionView());
        }

        private void RGBButton_Click(object sender, EventArgs e)
        {
            RGBButton.FillColor = Color.FromArgb(30, 170, 231);

            TensorButton.FillColor = Color.FromArgb(15, 15, 23);
            SobelButton.FillColor = Color.FromArgb(15, 15, 23);
            INH.FillColor = Color.FromArgb(15, 15, 23);

            Loadform(new ChannelMixingView());
        }

        private void SobelButton_Click(object sender, EventArgs e)
        {
            SobelButton.FillColor = Color.FromArgb(30, 170, 231);

            TensorButton.FillColor = Color.FromArgb(15, 15, 23);
            RGBButton.FillColor = Color.FromArgb(15, 15, 23);
            INH.FillColor = Color.FromArgb(15, 15, 23);

            Loadform(new EdgeDetectionView());
        }

        private void INH_Click(object sender, EventArgs e)
        {
            INH.FillColor = Color.FromArgb(30, 170, 231);
            TensorButton.FillColor = Color.FromArgb(15, 15, 23);
            RGBButton.FillColor = Color.FromArgb(15, 15, 23);
            SobelButton.FillColor = Color.FromArgb(15, 15, 23);

            Loadform(new NormalToHeightView());
        }
    }
}
