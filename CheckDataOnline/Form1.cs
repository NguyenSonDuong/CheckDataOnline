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
using xNet;

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
        public void AddLog(RichTextBox rtb, String message,Color color)
        {
            rtb.Invoke(new MethodInvoker(() => {
                rtb.AppendText(message + "\n");
            }));
        }
        private void Controller_SuccessEvent(object sender, EventArgs e)
        {
            AddLog(rtbDataSuccess, sender.ToString(),Color.Green);
        }

        private void Controller_ErrorEvent(object sender, EventArgs e)
        {
            AddLog(rtbLog, sender.ToString(), Color.Red);
        }

        private void Controller_ProcessEvent(object sender, EventArgs e)
        {
            AddLog(rtbLog, sender.ToString(), Color.Red);
            if (sender.ToString().Contains("[PROXY DEQUEUE]"))
                AddLog(rtbDataProxyRun, sender.ToString(), Color.Red);
            if (sender.ToString().Contains("[RUN]"))
                AddLog(rtbDataRun, sender.ToString(), Color.Red);
            if (sender.ToString().Contains("[DATA DEQUEUE]"))
                AddLog(rtbDataProxyRun, sender.ToString(), Color.Red);
        }

        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void bunifuButton2_Click(object sender, EventArgs e)
        {
            String proxy = rtbProxyImport.Text;
            String data = rtbDataImport.Text;
            int qutityThread = 0;
            if(!Int32.TryParse(txtQuatityThread.Text,out qutityThread))
            {
                MessageBox.Show("Vui lòng nhập số");
                return;
            }
            controller.QuatityThread = qutityThread;
            controller.SetupData(data.Split('\n'), proxy.Split('\n'));
            controller.Run();
            //String MSD = "DTC175524801030034";
            //String pass = "NguyenDuong";
            //HttpRequest http = new HttpRequest();

            //http.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0";
            //http.AddHeader("Accept", "application/json, text/plain, */*");
            //http.AddHeader("hostname", "svonline.vn");
            //http.Referer = "Referer";
            //http.AddHeader("sso_token", "undefined");
            //http.AddHeader("refresh_token", "undefined");
            //http.AddHeader("agent", "{\"brower\":\"svo-web\",\"version\":\"3.6.0\"}");
            //http.AddHeader("hostname", "svonline.vn");
            //String check_smart_name = http.Post("https://api.dhdt.vn/account/login/check-smartname", "{\"smartname\":\"" + MSD + "\",\"acc_type\":\"all\"}", "application/json;charset=utf-8").ToString();
            //JObject jsonCheckSmartName = JObject.Parse(check_smart_name);
            //String _id = jsonCheckSmartName["list_acc"][0]["_id"].ToString();
            //http = new HttpRequest();
            //http.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0";
            //http.AddHeader("Accept", "application/json, text/plain, */*");
            //http.AddHeader("hostname", "svonline.vn");
            //http.Referer = "Referer";
            //http.AddHeader("sso_token", "undefined");
            //http.AddHeader("refresh_token", "undefined");
            //http.AddHeader("agent", "{\"brower\":\"svo-web\",\"version\":\"3.6.0\"}");
            //http.AddHeader("hostname", "svonline.vn");
            //String strGetToken = http.Post("https://api.dhdt.vn/account/login/passwd", "{\"type\":\"user\",\"_id\":\""+_id+"\",\"passwd\":\""+pass+"\"}", "application/json;charset=utf-8").ToString();
        }

        private void bunifuButton3_Click(object sender, EventArgs e)
        {
            controller.Stop();
        }
    }
}
