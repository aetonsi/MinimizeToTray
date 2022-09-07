using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace MinimizeToTray
{
    public partial class Form1 : Form
    {
        public string[] args;
        public Icon icon;
        public ToolTipIcon balloonIcon;
        public Process process;

        public Form1()
        {
            InitializeComponent();
        }

        private string productInfo()
        {
            return Application.ProductName + " v" + Application.ProductVersion;
        }

        private void quit(int code)
        {
            Application.Exit();
            Environment.Exit(code);
        }

        private void killProcess()
        {
            if (!process.HasExited)
            {
                process.CloseMainWindow();
                process.Close();
                process.Kill(true);
            }
        }

        private void separator(string text = "")
        {
            textBox1.AppendText(System.Environment.NewLine + " =====================" + (text == "" ? "" : " " + text + " " ) + "===================== ");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ///////////////////////////////// SETUP VARIABLES /////////////////////////////////
            args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            balloonIcon = System.Windows.Forms.ToolTipIcon.Info;


            ///////////////////////////////// SETUP FORM, CHECK ARGUMENTS /////////////////////////////////
            if (!args.Any())
            {
                quit(1);
            }
            TextBox.CheckForIllegalCrossThreadCalls = false;
            //BeginInvoke(new MethodInvoker(delegate{Hide();}));


            ///////////////////////////////// SETUP VARIABLES /////////////////////////////////
            string program = "cmd.exe";
            string arguments = "/c " + string.Join(" ", args) + " 2>&1";
            textBox1.AppendText(program + " " + arguments + System.Environment.NewLine);
            separator();


            ///////////////////////////////// SETUP NOTIFY ICON /////////////////////////////////
            notifyIcon1.Icon = icon;
            notifyIcon1.Text = productInfo() + " - " + Path.GetFileName(args[0]);
            notifyIcon1.BalloonTipIcon = balloonIcon;
            notifyIcon1.BalloonTipTitle = productInfo();
            notifyIcon1.BalloonTipText = "Still running in the tray: " + string.Join(" ", args);
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(program + " " + arguments, null, (s, e) => Clipboard.SetText(program + " " + arguments) );
            contextMenu.Items.Add("---", null, (s, e) => { });
            contextMenu.Items.Add("Show", null, (s, e) => this.Show());
            contextMenu.Items.Add("Hide", null, (s, e) => this.Hide());
            contextMenu.Items.Add("Stop", null, (s, e) => killProcess());
            contextMenu.Items.Add("Exit", null, (s, e) => {
                killProcess();
                quit(0);
            });
            notifyIcon1.ContextMenuStrip = contextMenu;


            ///////////////////////////////// SETUP PROCESS /////////////////////////////////
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
            process.ErrorDataReceived += (sender, args) => textBox1.AppendText(System.Environment.NewLine + (args.Data));
            process.Exited += (sender, args) =>
            {
                separator();
                Visible = true;
            };


            ///////////////////////////////// START PROCESS /////////////////////////////////
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            timer1.Start();
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            /*StreamWriter myStreamWriter = process.StandardInput;
            myStreamWriter.Write(e.KeyChar);
            e.Handled = true;
            string text = e.KeyChar.ToString().Replace("\r", "\r\n");
            textBox1.AppendText(text);*/
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ///////////////////////////////// CANCEL CLOSING AND SHOW NOTIFICATION, IF PROCESS HAS NOT EXITED YET /////////////////////////////////
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (process != null && !process.HasExited)
                {
                    this.Visible = false;
                e.Cancel = true;
                notifyIcon1.ShowBalloonTip(5000);
                } // else exit
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            ///////////////////////////////// TOGGLE VISIBILITY /////////////////////////////////
            Visible = !Visible;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            ///////////////////////////////// HIDE ON STARTUP /////////////////////////////////
            Visible = false;
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            ///////////////////////////////// SHOW FORM ON BALLOONTIP CLICK /////////////////////////////////
            Visible = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(process != null)
            {
                process.Refresh();
                bool xxx = process.HasExited; // BUGFIX, .Refresh() doesn't trigger the Exited event, but reading .HasExited does...........
            }
        }
    }
}
