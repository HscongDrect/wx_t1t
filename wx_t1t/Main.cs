﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace wx_t1t
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }
        private Action OnsettlementSuccess;
        private Action<string> OnPostFail;

        private Action getuserinfoSuccess;
        private Action getfriendsscoreSuccess;
        System.Threading.Timer Thread_Time;
        long times { get; set; }
        static string session_id { get; set; }
        static int version = 9;
        long startTime { get; set; }
        long endTime { get; set; }
        int currentScore { get; set; }
        int bestscore { get; set; }
        int score { get; set; }

        string base_site = "https://mp.weixin.qq.com/wxagame/";

        string referer = "https://servicewechat.com/wx7c8d593b2c3a7703/6/page-frame.html";

        string USER_AGENT = "Mozilla/5.0 (Linux; Android 8.0; ALP-AL00 Build/HUAWEIALP-AL00; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/57.0.2987.132 MQQBrowser/6.2 TBS/043811 Mobile Safari/537.36 MicroMessenger/6.6.1.1220(0x26060133) NetType/WIFI Language/zh_CN";
        //string USER_AGENT = "MicroMessenger/6.6.1.1220(0x26060133) NetType/WIFI Language/zh_CN"
        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SessionId.Text))
            {
                session_id = SessionId.Text;
                score = (int)ScoreNum.Value;
                button1.Enabled = false;
                button1.Text = "提交中.";
                OnsettlementSuccess = () =>
                {
                    RunInMainthread(() =>
                    {
                        Thread_Time.Dispose();
                        button1.Text = "提交";
                        button1.Enabled = true;
                        MessageBox.Show("修改成功");

                        wxagame_bottlereport();
                    });
                };
                OnPostFail = (err) =>
                {
                    RunInMainthread(() =>
                    {
                        Thread_Time.Dispose();
                        MessageBox.Show("修改失败!"+err);
                        button1.Enabled = true;
                        button1.Text = "提交";
                    });
                };

                getuserinfoSuccess = () =>
                {
                    RunInMainthread(() =>
                    {
                        RunAsync(() =>
                        {
                            wxagame_getfriendsscore();
                        });
                    });
                };
                getfriendsscoreSuccess = () =>
                {
                    RunInMainthread(() =>
                    {
                        RunAsync(() =>
                        {
                            Thread_Time = new System.Threading.Timer(wxagame_init, null, 0, 60000);
                            wxagame_settlement();
                        });
                    });
                };
                RunAsync(() =>
                {
                    wxagame_getuserinfo();
                });

            }
            else
            {
                MessageBox.Show("\"session_id\" 不能为空!");
            }
        }
        private void wxagame_getuserinfo()
        {
            PostDate pd = new PostDate();
            pd.base_req.session_id = session_id;

            var jSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            string content = JsonConvert.SerializeObject(pd, jSetting);

            var client = new RestClient(base_site + "wxagame_getuserinfo");
            client.UserAgent = USER_AGENT;
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("referer", referer);
            request.AddParameter("application/json", content, ParameterType.RequestBody);
            try
            {
                IRestResponse response = client.Execute(request);
                Debug.WriteLine(response.Content);
                var resultJS = JsonConvert.DeserializeObject<Result>(response.Content);
                if (resultJS.base_resp.errcode == 0)
                {
                    getuserinfoSuccess?.Invoke();
                }
                else
                {
                    OnPostFail?.Invoke("\r\n错误："+response.Content);
                }
            }
            catch (Exception)
            {
                OnPostFail?.Invoke("");
                throw;
            }
        }


        private void wxagame_getfriendsscore()
        {
            PostDate pd = new PostDate();
            pd.base_req.session_id = session_id;

            var jSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            string content = JsonConvert.SerializeObject(pd, jSetting);
            var client = new RestClient(base_site + "wxagame_getfriendsscore");
            client.UserAgent = USER_AGENT;
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("referer", referer);
            request.AddParameter("application/json", content, ParameterType.RequestBody);
            try
            {
                IRestResponse response = client.Execute(request);
                Debug.WriteLine(response.Content);
                var resultJS = JsonConvert.DeserializeObject<Result>(response.Content);
                if (resultJS.base_resp.errcode == 0)
                {
                    if (resultJS.my_user_info!=null)
                    {
                        times = resultJS.my_user_info.times + 1;
                        bestscore = resultJS.my_user_info.history_best_score;
                        getfriendsscoreSuccess?.Invoke();
                    }
                    else
                    {
                        OnPostFail?.Invoke("\r\n错误：" + "账号正在小黑屋中。。");
                    }
                }
                else
                {
                    OnPostFail?.Invoke(response.Content);
                }

            }
            catch (Exception)
            {
                OnPostFail?.Invoke("");
                throw;
            }
        }
        private void wxagame_init(object o)
        {
            PostDate pd = new PostDate();
            pd.base_req.session_id = session_id;
            pd.version=version;

            var jSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            string content = JsonConvert.SerializeObject(pd, jSetting);


            var client = new RestClient(base_site + "wxagame_init");
            client.UserAgent = USER_AGENT;
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("referer", referer);
            request.AddParameter("application/json", content, ParameterType.RequestBody);
            try
            {
                IRestResponse response = client.Execute(request);
                Debug.WriteLine(response.Content);
                //var resultJS = JsonConvert.DeserializeObject<Result>(response.Content);
                //if (resultJS.base_resp.errcode == 0)
                //{
                    //wxagame_settlement();
                //}

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void wxagame_settlement()
        {
            PostDate pd = new PostDate();
            pd.base_req.session_id = session_id;
            //var action_data = Datestr();
            pd.action_data = Datestr();

            var jSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            string content = JsonConvert.SerializeObject(pd, jSetting);

            var client = new RestClient(base_site + "wxagame_settlement");
            client.UserAgent = USER_AGENT;
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("referer", referer);
            request.AddParameter("application/json", content, ParameterType.RequestBody);

            IRestResponse response1 = client.Execute(request);
            IRestResponse response = client.Execute(request);
            Debug.WriteLine(response.Content);

            var resultJS = ReadToObject(response.Content);
            if (resultJS.base_resp.errcode == 0)
            {
                OnsettlementSuccess?.Invoke();
            }
            else
            {
                OnPostFail?.Invoke("\r\n错误：" + response.Content);
            }
        }
        private void wxagame_bottlereport()
        {
            PostDate pd = new PostDate();

            pd.base_req.session_id = session_id;
            pd.base_req.client_info = new ClientInfo();
            pd.report_list = new List<ReportList>();
            var rp1 = new ReportList();
            var rp2 = new ReportList();
            rp1.ts = (long)Math.Round((decimal)startTime/1000);
            rp1.type = 10;
            rp2.ts= (long)Math.Round((decimal)endTime / 1000);
            rp2.type = 2;
            rp2.duration = rp2.ts - rp1.ts;
            rp2.best_score = bestscore;
            rp2.times = times;
            rp2.score = score;
            rp2.break_record = score > bestscore ? 1 : 0;
            pd.report_list.Add(rp1);
            pd.report_list.Add(rp2);
            var jSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            string content = JsonConvert.SerializeObject(pd, jSetting);
            Debug.WriteLine(content);

            var client = new RestClient(base_site + "wxagame_init");
            client.UserAgent = USER_AGENT;
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("referer", referer);
            request.AddParameter("application/json", content, ParameterType.RequestBody);
            try
            {
                IRestResponse response = client.Execute(request);
                Debug.WriteLine(response.Content);
            }
            catch (Exception)
            {
                throw;
            }

        }

        private string Datestr()
        {
            currentScore = 0;
            int perScore = 1;
            int addScore = 0;
            long succeedTime =0;
            long mouseDownTime;
            int order = 15;
            int StayTime;
            bool musicScore=false;
            IList<int> OrderList = new List<int>();
            IList<bool> IsDouble = new List<bool>();
            int Count = 0;

            for (int i = 0; i < 100; i++)
            {
                if (i<60)
                {
                    OrderList.Add(15);//1
                    IsDouble.Add(false);
                }
                else
                {
                    if (i < 78)
                    {
                        IsDouble.Add(false);
                        OrderList.Add(26);//5
                    }
                    else if (i < 86)
                    {
                        IsDouble.Add(false);
                        OrderList.Add(17);//10
                    }
                    else if (i < 95)
                    {
                        IsDouble.Add(true);
                        OrderList.Add(24);//15
                    }
                    else
                    {
                        IsDouble.Add(true);
                        OrderList.Add(19);//30
                    }
                } 

            }
            ActionDate ad = new ActionDate();

            GameData gd = new GameData();

            startTime = GetTimeStamp(DateTime.Now);

            gd.action = new List<object>();
            gd.musicList = new List<bool>();
            gd.touchList = new List<object>();
            gd.steps = new List<IList<double>>();
            gd.timestamp = new List<long>();

            do
            {
                Random ran = new Random();
                int stoptime = ran.Next(200, 500);

                int t = ran.Next(300, 1000);
                double d = ran.NextDouble()*4/1000+1.88;
                double duration = t / 1000;
                int o = ran.Next(0, 99);
                order=OrderList[o];
                if (order!=15)
                {
                    if (Count<4)
                    {
                        order = 15;
                        StayTime = 0;
                    }
                    else
                    {
                        musicScore = true;

                        StayTime = ran.Next(2000, 3000);
                    }
                }
                else
                {
                    StayTime = 0;
                }
                if (IsDouble[o])
                {
                    if (Count < 1)
                    {
                        perScore = 1;
                    }
                    else
                    {
                        perScore = perScore * 2 > 32 ? 32 : perScore * 2;
                    }
                }
                else
                {
                    perScore = 1;
                }

                var calY = Math.Round(2.75 - d * duration , 2);
                gd.action.Add(new object[3] { duration, calY, false });
                gd.musicList.Add(musicScore);
                var x = ran.Next(230, 245);
                var y = ran.Next(500, 530);

                var touch_x = x+(x % 4)*0.25;
                var touch_y = y + (y % 4) * 0.25;
                gd.touchList.Add(new object[2] { touch_x, touch_y });
                IList<double> touchMoveList = new List<double>();

                if (t<410)
                {
                    for (int l = 0; l < 3; l++)
                    {
                        touchMoveList.Add(touch_x);
                        touchMoveList.Add(touch_y);
                    }
                }
                else if (t<450)
                {
                    for (int l = 0; l < 4; l++)
                    {
                        touchMoveList.Add(touch_x);
                        touchMoveList.Add(touch_y);
                    }

                }
                else
                {
                    for (int l = 0; l < 5; l++)
                    {
                        touchMoveList.Add(touch_x);
                        touchMoveList.Add(touch_y);
                    }

                }

                gd.steps.Add(touchMoveList);
                if (succeedTime==0)
                {
                    succeedTime = startTime;
                }
                 
                int WaitTime = ran.Next(1000, 3000);
                mouseDownTime = succeedTime + StayTime+ WaitTime;

                gd.timestamp.Add(mouseDownTime);
                succeedTime = mouseDownTime+(long)Math.Round((135 + 15 * duration) * 2000 / 720)+ t;
                switch (order)
                {
                    case 26:
                        addScore=5;
                        break;
                    case 17:
                        addScore = 10;
                        break;
                    case 24:
                        addScore = 15;
                        break;
                    case 19:
                        addScore = 30;
                        break;
                    default:
                        addScore=0;
                        break;
                }
                currentScore = currentScore + perScore+addScore;

                Count++;

            } while (currentScore<= score);

            var s=gd.timestamp[Count-1] - startTime+200;
            for (int i = 0; i < Count; i++)
            {
                gd.timestamp[i] = gd.timestamp[i] - s;
            }

            startTime = startTime - s;
            endTime = succeedTime-s;
            ad.score = score;
            ad.times = times;
            gd.seed = startTime;

            gd.version = 2;
            var s2 = WriteFromObject<GameData>(gd);
            ad.game_data = s2;
            var text = WriteFromObject<ActionDate>((Object)ad);
            var ActionData = AESEncrypt(text, session_id);

            return ActionData;
        }

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="text"></param>
        /// <param name="originKey"></param>
        /// <returns></returns>
        private string AESEncrypt(string text, string originKey)
           
        {
            var password = originKey.Substring(0, 16);
            var iv = originKey.Substring(0, 16);
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            rijndaelCipher.Mode = CipherMode.CBC;

            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = 128;

            rijndaelCipher.BlockSize = 128;

            byte[] pwdBytes = Encoding.UTF8.GetBytes(password);

            byte[] keyBytes = new byte[16];

            int len = pwdBytes.Length;

            if (len > keyBytes.Length) len = keyBytes.Length;

            Array.Copy(pwdBytes, keyBytes, len);

            rijndaelCipher.Key = keyBytes;

            byte[] ivBytes = keyBytes;

            rijndaelCipher.IV = ivBytes;
           ICryptoTransform transform = rijndaelCipher.CreateEncryptor();

            byte[] plainText = Encoding.UTF8.GetBytes(text);
            byte[] cipherBytes = transform.TransformFinalBlock(plainText, 0, plainText.Length);

            return Convert.ToBase64String(cipherBytes);

        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="text"></param>
        /// <param name="originKey"></param>
        /// <returns></returns>
        public static string AESDecrypt(string text, string originKey)
        {
            var password = originKey.Substring(0, 16);
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            rijndaelCipher.Mode = CipherMode.CBC;

            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = 128;

            rijndaelCipher.BlockSize = 128;

            byte[] encryptedData = Convert.FromBase64String(text);

            byte[] pwdBytes = Encoding.UTF8.GetBytes(password);

            byte[] keyBytes = new byte[16];

            int len = pwdBytes.Length;

            if (len > keyBytes.Length) len = keyBytes.Length;

            Array.Copy(pwdBytes, keyBytes, len);

            rijndaelCipher.Key = keyBytes;

            byte[] ivBytes = keyBytes;
            rijndaelCipher.IV = ivBytes;

            ICryptoTransform transform = rijndaelCipher.CreateDecryptor();

            byte[] plainText = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

            return Encoding.UTF8.GetString(plainText);

        }
        private string WriteFromObject<T>(Object ad)
        {
            MemoryStream ms = new MemoryStream();

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(ms, ad);
            byte[] json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);

        }
        private Result ReadToObject(string json)
        {
            Result deserialized = new Result();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(deserialized.GetType());
            deserialized = ser.ReadObject(ms) as Result;
            ms.Close();
            return deserialized;
        }
        private long GetTimeStamp(DateTime dateTime)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (dateTime.ToUniversalTime().Ticks - dt1970.Ticks) / 10000;
        }
        void RunAsync(Action action)
        {
            ((Action)(delegate () {
                action?.Invoke();
            })).BeginInvoke(null, null);
        }
        void RunInMainthread(Action action)
        {
            this.BeginInvoke((Action)(delegate () {
                action?.Invoke();
            }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = AESDecrypt(textBox1.Text, SessionId.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text = AESEncrypt(textBox1.Text, SessionId.Text);

        }
    }

    public class BaseResp
    {
        public int errcode { get; set; }
        public long ts { get; set; }
    }
    public class Result
    {
        public BaseResp base_resp { get; set; }
        public string version { get; set; }
        public MyUserInfo my_user_info { get; set; }
        public string nickname { get; set; }
        public string headimg { get; set; }
    }
    public class MyUserInfo
    {
        public string nickname { get; set; }
        public string headimg { get; set; }
        public IList<object> score_info { get; set; }
        public int history_best_score { get; set; }
        public int week_best_score { get; set; }
        public int grade { get; set; }
        public int times { get; set; }
        public IList<object> hongbao_list { get; set; }
    }

    [DataContract]
    public class GameData
    {

        [DataMember(Order = 0, IsRequired = true)]
        public long seed { get; set; }
        [DataMember(Order = 1, IsRequired = true)]
        public IList<object> action { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public IList<bool> musicList { get; set; }
        [DataMember(Order = 3, IsRequired = true)]
       public IList<object> touchList { get; set; }
        [DataMember(Order = 4, IsRequired = true)]
        public IList<IList<double>> steps { get; set; }
        [DataMember(Order = 5, IsRequired = true)]
        public IList<long> timestamp { get; set; }
            
        [DataMember(Order = 6, IsRequired = true)]
        public int version { get; set; }
    }
    [DataContract]
    public class ActionDate
    {
        [DataMember(Order = 0, IsRequired = true)]
        public int score { get; set; }
        [DataMember(Order = 1, IsRequired = true)]
        public long times { get; set; }
        [DataMember(Order = 2, IsRequired = true)]
        public string game_data { get; set; }

    }

    [DataContract]
    public class ClientInfo
    {
        public ClientInfo()
        {
            platform = "android";
            brand = "HUAWEI";
            model = "ALP-AL00";
            system = "Android 8.0";
        }
        [DataMember(Order = 0)]
        public string platform { get; set; }
        [DataMember(Order = 1)]
        public string brand { get; set; }
        [DataMember(Order = 2)]
        public string model { get; set; }
        [DataMember(Order = 3)]
        public string system { get; set; }
    }
    [DataContract]
    public class BaseReq
    {
        public BaseReq()
        {
            fast = 1;
            //client_info = new ClientInfo();
        }
        [DataMember(Order = 0)]
        public string session_id { get; set; }
        [DataMember(Order = 1)]
        //[DefaultValue(1)]
        public int fast { get; set; }
        [DataMember(Order = 2)]
        public ClientInfo client_info { get; set; }
    }
    public class ReportList
    {
        public long ts { get; set; }
        public int type { get; set; }
        public int? score { get; set; }
        public int? best_score { get; set; }
        public int? break_record { get; set; }
        public long? duration { get; set; }
        public long? times { get; set; }
    }
    [DataContract]
    public class PostDate
    {
        public PostDate()
        {
            //report_list = new List<ReportList>();
            base_req = new BaseReq();
        }
        [DataMember(Order = 0)]
        public BaseReq base_req { get; set; }
        [DataMember(Order = 1)]
        public int? version { get; set; }
        [DataMember(Order = 2)]
        public IList<ReportList> report_list { get; set; }
        [DataMember(Order = 3)]
        public string action_data { get; set; }
    }
}
