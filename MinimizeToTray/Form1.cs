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

        private void separator(string text = "")
        {
            textBox1.AppendText(System.Environment.NewLine + " =====================" + text + "===================== ");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ///////////////////////////////// SETUP VARIABLES /////////////////////////////////
            args = Environment.GetCommandLineArgs();
            icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            balloonIcon = System.Windows.Forms.ToolTipIcon.Info;


            ///////////////////////////////// SETUP FORM, CHECK ARGUMENTS /////////////////////////////////
            if (!args.Skip(1).Any())
            {
                quit(1);
            }
            TextBox.CheckForIllegalCrossThreadCalls = false;
            //BeginInvoke(new MethodInvoker(delegate{Hide();}));


            ///////////////////////////////// SETUP VARIABLES /////////////////////////////////
            string program = "cmd.exe";
            string arguments = "/c " + string.Join(" ", args.Skip(1).ToArray()) + " 2>&1";
            textBox1.AppendText(program + " " + arguments + System.Environment.NewLine);
            separator();


            ///////////////////////////////// SETUP NOTIFY ICON /////////////////////////////////
            notifyIcon1.Icon = icon;
            notifyIcon1.Text = productInfo() + " - " + Path.GetFileName(args.Skip(1).ToArray()[0]);
            notifyIcon1.BalloonTipIcon = balloonIcon;
            notifyIcon1.BalloonTipTitle = productInfo();
            notifyIcon1.BalloonTipText = "Still running in the tray: " + string.Join(" ", args.Skip(1).ToArray());
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
            process.Exited += (sender, args) =>
            {
                separator();
                Visible = true;
            };


            ///////////////////////////////// START PROCESS /////////////////////////////////
            process.Start();
            // process.BeginErrorReadLine();
            process.BeginOutputReadLine();
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
            ///////////////////////////////// CANCEL CLOSING AND SHOW NOTIFICATION /////////////////////////////////
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Visible = false;
                e.Cancel = true;
                notifyIcon1.ShowBalloonTip(5000);
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
    }
}
