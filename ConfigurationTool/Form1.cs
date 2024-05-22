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

namespace ConfigurationTool
{
    public partial class Form1 : Form
    {
        private string folderPath = Properties.Settings.Default.FolderPath;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string command = "/C cd C:\\Users\\Admin\\Documents\\PRS\\System\\PRS_System & php artisan serve --host 192.168.8.62 --port 8080";
            System.Diagnostics.Process.Start("CMD.exe", command);
        }

        private void Choose_Folder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                DialogResult result = dlg.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
                {
                    this.PRSSystemFolderPath.Text = dlg.SelectedPath;
                    this.folderPath = dlg.SelectedPath;
                    Properties.Settings.Default.FolderPath = dlg.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void IconButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Png Icon|*.png;";
                DialogResult result = ofd.ShowDialog();

                if (result == DialogResult.OK)
                {
                    this.Icon.Image.Dispose();
                    File.Copy(ofd.FileName, Path.Combine(this.folderPath + "/public/Icon.png"), true);
                    this.Icon.Image = Image.FromFile(this.folderPath + "/public/Icon.png");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.PRSSystemFolderPath.Text = Properties.Settings.Default.FolderPath;

            if (!string.IsNullOrEmpty(this.folderPath))
            {
                if (File.Exists(this.folderPath + "/.env"))
                {
                    this.CompanyName.Enabled = true;
                    this.IconButton.Enabled = true;
                    this.Host.Enabled = true;
                    this.Port.Enabled = true;
                    this.DBName.Enabled = true;
                    this.Username.Enabled = true;
                    this.Password.Enabled = true;

                    if (File.Exists(this.folderPath + "/public/Icon.png"))
                    {
                        this.Icon.Image = Image.FromFile(this.folderPath + "/public/Icon.png");
                        this.Icon.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }
            }
        }
    }
}
