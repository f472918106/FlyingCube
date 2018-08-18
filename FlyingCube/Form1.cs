using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlyingCube.Service;

namespace FlyingCube
{
    public partial class MainForm : Form
    { 
        private AsyncHttpService HttpService = null;

        public MainForm()
        {
            InitializeComponent();
            buttonPause.Enabled = false;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (HttpService == null)
            {
                HttpService = new AsyncHttpService(@"http://localhost:30000/", 20, 1000, this);
            }
            HttpService.Start();
            timer1.Enabled = true;
            buttonStart.Enabled = false;
            buttonPause.Enabled = true;
            buttonPause.Focus();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (HttpService != null)
            {
                if (richTextBox_MsgQueue.Text.Length > 10000)
                {
                    richTextBox_MsgQueue.Text = "";
                }
                richTextBox_Console.Text = "[" + DateTime.Now + "]\n当前消息队列共" + AsyncHttpService.MsgQueue.Count + "条消息.\n" + "当前作业队列共" + AsyncTaskService.TaskQueue.Count + "项作业";
            }
        }

        private void richTextBox_MsgQueue_TextChanged(object sender, EventArgs e)
        {
            richTextBox_MsgQueue.SelectionStart = richTextBox_MsgQueue.Text.Length;
            richTextBox_MsgQueue.ScrollToCaret();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            HttpService.Stop();
            timer1.Enabled = false;
            buttonPause.Enabled = false;
            buttonStart.Enabled = true;
            richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] " + "暂停中...\n";
            buttonStart.Focus();
        }

        private void buttonRestart_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void richTextBox_Console_TextChanged(object sender, EventArgs e)
        {
            richTextBox_Console.SelectionStart = richTextBox_Console.Text.Length;
            richTextBox_Console.ScrollToCaret();
        }
    }
}
