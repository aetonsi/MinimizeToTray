// *1 REMOVED BECAUSE IT IS IMPOSSIBLE TO MAINTAIN CONSOLE OUTPUT ORDER
// *2 REMOVED BECAUSE IT MAKES IT IMPOSSIBLE TO TYPE INTO CONSOLE WINDOWS LAUNCHED BY THE REQUESTED PROGRAM

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace MinimizeToTray
{
    public partial class Form1 : Form
    {
        public string[] arguments;
        public string requestedProgramPath;
        public string[] requestedProgramArguments;
        public string internalProgramPath;
        public string[] internalProgramArguments;
        public Icon icon;
        public ToolTipIcon balloonIcon;
        public Process process;

        public Form1()
        {
            InitializeComponent();
        }

        private string requestedProgramCommandLine(bool alsorequestedProgramArguments = true)
        {
            return requestedProgramPath + (alsorequestedProgramArguments ? " " + string.Join(" ", requestedProgramArguments) : "");
        }

        private string internalProgramCommandLine(bool alsoInternalProgramArgs = true)
        {
            return internalProgramPath + (alsoInternalProgramArgs ? " " + string.Join(" ", internalProgramArguments) : "");
        }

        private string productInfo(bool alsoRequestedProgramCommandLine = false)
        {
            return Application.ProductName + " v" + Application.ProductVersion
                + (alsoRequestedProgramCommandLine ? " - " + requestedProgramCommandLine() : "");
        }

        private void quit(int code)
        {
            Application.Exit();
            Environment.Exit(code);
        }

        private void killProcess()
        {
            timer1.Stop();
            if (process != null && !process.HasExited)
            {
                process.CloseMainWindow();
                if(!process.HasExited) process.Kill(true);
            }
        }

        private void separator(string text = "")
        {
            textBox1.AppendText(System.Environment.NewLine + " =====================" + (text == "" ? "" : " " + text + " " ) + "===================== ");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ///////////////////////////////// SETUP VARIABLES AND PRELIMINARY CHECKS /////////////////////////////////
            arguments = Environment.GetCommandLineArgs().Skip(1).ToArray();
            if (!arguments.Any()) quit(1);
            requestedProgramPath = arguments[0];
            requestedProgramArguments = arguments.Skip(1).ToArray();
            icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            balloonIcon = System.Windows.Forms.ToolTipIcon.Info;
            internalProgramPath = "cmd.exe";
            internalProgramArguments = (new string[] { "/c"})
                .Concat(requestedProgramCommandLine().Split(" "))
                .Concat(new string[] { "2>&1" }) // REDIRECTING ERR TO OUT BECAUSE OF *1
                .ToArray();


            ///////////////////////////////// SETUP FORM /////////////////////////////////
            TextBox.CheckForIllegalCrossThreadCalls = false;
            //BeginInvoke(new MethodInvoker(delegate{Hide();}));
            this.Text = productInfo(true);


            ///////////////////////////////// SETUP NOTIFY ICON /////////////////////////////////
            notifyIcon1.Icon = icon;
            notifyIcon1.Text = productInfo(true);
            notifyIcon1.BalloonTipIcon = balloonIcon;
            notifyIcon1.BalloonTipTitle = productInfo();
            notifyIcon1.BalloonTipText = "Still running in the tray: " + requestedProgramCommandLine();
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(internalProgramCommandLine(), null, (s, e) => Clipboard.SetText(internalProgramCommandLine()));
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
            process.StartInfo.FileName = internalProgramPath;
            process.StartInfo.Arguments = string.Join(" ", internalProgramArguments);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            // process.StartInfo.RedirectStandardError = true; // SEE *1
            // process.StartInfo.RedirectStandardInput = true; // SEE *2
            process.StartInfo.UseShellExecute = false;
            process.OutputDataReceived += (sender, args) => textBox1.AppendText(System.Environment.NewLine + (args.Data));
            // process.ErrorDataReceived += (sender, args) => textBox1.AppendText(System.Environment.NewLine + (args.Data)); // SEE *1
            process.Exited += (sender, args) =>
            {
                separator();
                this.Visible = true;
            };


            ///////////////////////////////// START PROGRAM /////////////////////////////////
            textBox1.AppendText(internalProgramCommandLine() + System.Environment.NewLine);
            separator();
            process.Start();
            process.BeginOutputReadLine();
            // process.BeginErrorReadLine(); // SEE *1
            // process.WaitForExit(); // DO NOT USE: HANGS EVERYTHING
            timer1.Start();
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // SEE *2
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
                } else {
                    killProcess();
                    quit(0);
                }
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            ///////////////////////////////// TOGGLE VISIBILITY /////////////////////////////////
            this.Visible = !this.Visible;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            ///////////////////////////////// HIDE ON STARTUP /////////////////////////////////
            this.Visible = false;
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            ///////////////////////////////// SHOW FORM ON BALLOONTIP CLICK /////////////////////////////////
            this.Visible = true;
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
