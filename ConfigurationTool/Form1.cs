using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace ConfigurationTool
{
    public partial class Form1 : Form
    {
        private string folderPath = Properties.Settings.Default.FolderPath;
        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
        private Process process;
        private ProcessStartInfo startInfo;
        private bool isOpened = false;

        public Form1()
        {
            InitializeComponent();
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
                    this.Load_Form();
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
                    try
                    {
                        this.Icon.Image.Dispose();
                        this.Icon.Image = null;
                        File.Copy(ofd.FileName, Path.Combine(this.folderPath + "/public/Icon.png"), true);
                        this.Icon.Image = Image.FromFile(this.folderPath + "/public/Icon.png");
                    } 
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void Load_Form()
        {
            this.PRSSystemFolderPath.Text = Properties.Settings.Default.FolderPath;
            this.Server_Host.Text = Properties.Settings.Default.Host;
            this.Server_Port.Text = Properties.Settings.Default.Port;

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
                    this.Start_Server.Enabled = true;
                    this.Connection_Button.Enabled = true;
                    this.Save.Enabled = true;
                    this.Reset.Enabled = true;

                    this.Read_Env(this.folderPath + "/.env");

                    this.CompanyName.Text = keyValuePairs["APP_NAME"].Trim('"');
                    this.Host.Text = keyValuePairs["DB_HOST"];
                    this.Port.Text = keyValuePairs["DB_PORT"];
                    this.DBName.Text = keyValuePairs["DB_DATABASE"];
                    this.Username.Text = keyValuePairs["DB_USERNAME"];
                    this.Password.Text = keyValuePairs["DB_PASSWORD"];

                    if (File.Exists(this.folderPath + "/public/Icon.png"))
                    {
                        this.Icon.Image = Image.FromFile(this.folderPath + "/public/Icon.png");
                        this.Icon.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                } else
                {
                    this.CompanyName.Enabled = false;
                    this.IconButton.Enabled = false;
                    this.Host.Enabled = false;
                    this.Port.Enabled = false;
                    this.DBName.Enabled = false;
                    this.Username.Enabled = false;
                    this.Password.Enabled = false;
                    this.Start_Server.Enabled = false;
                    this.Connection_Button.Enabled = false;
                    this.Save.Enabled = false;
                    this.Reset.Enabled = false;

                    if (File.Exists(this.folderPath + "/public/Icon.png"))
                    {
                        this.Icon.Image = Image.FromFile(this.folderPath + "/public/Icon.png");
                        this.Icon.SizeMode = PictureBoxSizeMode.Zoom;
                    } else
                    {
                        if (this.Icon.Image != null)
                        {
                            this.Icon.Image.Dispose();
                            this.Icon.Image = null;
                        }
                    }
                }
            }
        }

        private void Read_Env(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            keyValuePairs.Clear();

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue; 
                }

                string[] parts = line.Split('=');
                keyValuePairs.Add(parts[0], parts[1]);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.process = new Process();
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(this.Exited);
            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            this.Load_Form();
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            this.Output_Box.Invoke((MethodInvoker)delegate
            {
                this.Output_Box.Text += outLine.Data;
                this.Output_Box.Text += "\n";
            });
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (!File.Exists(this.folderPath + "/.env"))
            {
                MessageBox.Show("File not found!");
                return;
            }

            string[] lines = File.ReadAllLines(this.folderPath + "/.env");

            keyValuePairs["APP_NAME"] = '"' + this.CompanyName.Text + '"';
            keyValuePairs["DB_HOST"] = this.Host.Text;
            keyValuePairs["DB_PORT"] = this.Port.Text;
            keyValuePairs["DB_DATABASE"] = this.DBName.Text;
            keyValuePairs["DB_USERNAME"] = this.Username.Text;
            keyValuePairs["DB_PASSWORD"] = this.Password.Text;

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i])) { 
                    continue;
                }

                lines[i] = lines[i].Split('=')[0] + '=' + keyValuePairs[lines[i].Split('=')[0]];
            }

            try
            {
                File.WriteAllLines(this.folderPath + "/.env", lines);
                MessageBox.Show("Saved Successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Connection_Button_Click(object sender, EventArgs e)
        {
            string stringConnection = @"Server=" + this.Host.Text + "; Database=" + this.DBName.Text + "; User Id=" + this.Username.Text + "; Password=" + this.Password.Text + ";";

            if (!string.IsNullOrEmpty(this.Port.Text))
            {
                stringConnection = @"Server=" + this.Host.Text + "," + this.Port.Text + "; Database=" + this.DBName.Text + "; User Id=" + this.Username.Text + "; Password=" + this.Password.Text + ";";
            }

                using (SqlConnection conn = new SqlConnection(stringConnection))
            {
                try
                {
                    conn.Open();
                    MessageBox.Show("Connection Successful");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void Start_Click(object sender, EventArgs e)
        {
            if(!this.isOpened)
            {
                this.Start_Server.Text = "Close Server";

                Properties.Settings.Default.Host = this.Server_Host.Text;
                Properties.Settings.Default.Port = this.Server_Port.Text;
                Properties.Settings.Default.Save();

                this.startInfo = new ProcessStartInfo("cmd.exe", "/C cd " + folderPath + " & php artisan serve --host " + Properties.Settings.Default.Host +" --port " + Properties.Settings.Default.Port)
                {
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                this.process.StartInfo = startInfo;
                this.isOpened = true;

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            } else
            {
                process.CloseMainWindow();
            }
        }

        private void Exited(object sender, EventArgs e)
        {
            this.isOpened = false;

            process.CancelOutputRead();
            process.CancelErrorRead();

            this.Output_Box.Invoke((MethodInvoker)delegate
            {
                this.Output_Box.Text += "Server Closed!";
                this.Output_Box.Text += "\n";
                this.Start_Server.Text = "Start Server";
            });
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            this.Output_Box.Text = string.Empty;
        }
    }
}
