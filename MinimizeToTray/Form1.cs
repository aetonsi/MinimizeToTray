using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace MinimizeToTray
{
    public partial class Form1 : Form
    {
        public Process process;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BeginInvoke(new MethodInvoker(delegate
            {
                Hide();
            }));
            TextBox.CheckForIllegalCrossThreadCalls = false;


            string[] args = Environment.GetCommandLineArgs();
            string program = "cmd.exe";
            string arguments = "/c " + string.Join(" ", args.Skip(1).ToArray()) + " 2>&1";
            textBox1.AppendText(program + " " + arguments + System.Environment.NewLine);
            textBox1.Text += System.Environment.NewLine + ("=================");


            notifyIcon1.DoubleClick += (s, e) => Visible = !Visible;
            notifyIcon1.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon1.Visible = true;
            notifyIcon1.Text = Application.ProductName + 
                " v" + Application.ProductVersion + 
                " - " + Path.GetFileName(args.Skip(1).ToArray()[0]);


            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(program + " " + arguments, null, (s, e) => Clipboard.SetText(program + " " + arguments) );
            contextMenu.Items.Add("---", null, (s, e) => { });
            contextMenu.Items.Add("Show", null, (s, e) => this.Show());
            contextMenu.Items.Add("Hide", null, (s, e) => this.Hide());
            contextMenu.Items.Add("Exit", null, (s, e) => {
                if(!process.HasExited)
                {
                    process.Kill(true);
                }
                this.Close();
                Application.Exit();
            });
            notifyIcon1.ContextMenuStrip = contextMenu;
              

            process = new Process();
            // Configure the process using the StartInfo properties.
            process.StartInfo.FileName = program;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            // process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.OutputDataReceived += (sender, args) => textBox1.AppendText(System.Environment.NewLine + (args.Data));
            process.Exited += (sender, args) =>
            {
                textBox1.AppendText(System.Environment.NewLine + " ========================================== ");
                this.Show();
                notifyIcon1.Visible = false;
            };


            process.Start();
            // process.BeginErrorReadLine();
            process.BeginOutputReadLine();
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            StreamWriter myStreamWriter = process.StandardInput;
            myStreamWriter.Write(e.KeyChar);
            e.Handled = true;
            string text = e.KeyChar.ToString().Replace("\r", "\r\n");
            textBox1.AppendText(text);
        }

    }
}
