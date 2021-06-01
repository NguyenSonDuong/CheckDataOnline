using CheckDataOnline.controller;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckDataOnline
{
    public partial class Form1 : Form
    {
        public ControllerRequest controller;
        public Form1()
        {
            InitializeComponent();
            controller = new ControllerRequest();
            controller.ProcessEvent += Controller_ProcessEvent;
            controller.ErrorEvent += Controller_ErrorEvent;
            controller.SuccessEvent += Controller_SuccessEvent;
        }
        public void AddLog(RichTextBox rtb, String message, Color color)
        {
            rtb.Invoke(new MethodInvoker(() =>
            {
                rtb.SelectionColor = color;
                rtb.AppendText(message + "\n");
            }));
        }
        private void Controller_SuccessEvent(object sender, EventArgs e)
        {
            AddLog(rtbDataSuccess, sender.ToString(), Color.Green);
        }

        private void Controller_ErrorEvent(object sender, EventArgs e)
        {
            AddLog(rtbLog, sender.ToString(), Color.Red);
        }

        private void Controller_ProcessEvent(object sender, EventArgs e)
        {
            AddLog(rtbLog, sender.ToString(), Color.Red);
            if (sender.ToString().Contains("[PROXY DEQUEUE]"))
            {
                AddLog(rtbDataProxyRun, sender.ToString(), Color.Teal);
                //rtbProxyImport.Invoke(new MethodInvoker(() =>
                //{
                //    rtbProxyImport.Text = String.Join("\n", rtbProxyImport.Lines.ToList().Find((item) => !item.Equals(sender.ToString().Replace("[PROXY DEQUEUE]", "").Trim())));
                //}));
                
            }
            if (sender.ToString().Contains("[RUN]"))
                AddLog(rtbDataRun, sender.ToString(), Color.Teal);
            if (sender.ToString().Contains("[DATA DEQUEUE]"))
            {
                AddLog(rtbDataProxyRun, sender.ToString(), Color.Teal);
                //rtbDataImport.Invoke(new MethodInvoker(() =>
                //{
                //    rtbDataImport.Text = String.Join("\n", rtbDataImport.Lines.ToList().Find((item) => !item.Equals(sender.ToString().Replace("[DATA DEQUEUE]", "").Trim())));
                //}));
            }
        }

        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void bunifuButton2_Click(object sender, EventArgs e)
        {
            String proxy = rtbProxyImport.Text;
            String data = rtbDataImport.Text;
            int qutityThread = 1;
            if (!Int32.TryParse(txtQuatityThread.Text, out qutityThread))
            {
                MessageBox.Show("Vui lòng nhập số");
                return;
            }
            controller.QuatityThread = qutityThread;
            controller.SetupData(data.Split('\n'), proxy.Split('\n'));
            controller.Run();

        }

        private void bunifuButton3_Click(object sender, EventArgs e)
        {
            controller.Stop();
        }
    }
}
