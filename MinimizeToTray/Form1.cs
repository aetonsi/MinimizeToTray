using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinimizeToTray
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.DoubleClick += (s, e) =>
            {
                Visible = !Visible;
            };
            notifyIcon1.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon1.Visible = true;
            notifyIcon1.Text = Application.ProductName;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Exit", null, (s, e) => { Application.Exit(); });
            notifyIcon1.ContextMenuStrip = contextMenu;

            Console.WriteLine("Running!");

            // Standard message loop to catch click-events on notify icon
            // Code after this method will be running only after Application.Exit()
            string[] args = Environment.GetCommandLineArgs();
            args = args.Skip(1).ToArray();
            listBox1.Items.Add(string.Join(",", args));


            notifyIcon1.Visible = false;
        }
    }
}
