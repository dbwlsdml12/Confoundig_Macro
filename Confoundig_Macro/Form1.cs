using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Discord;
using Discord.WebSocket;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;

using Mat = OpenCvSharp.Mat;
//MTA4NjAzMTEyMzE5ODUyNTUxMA.GEmypC.jkWxde8Co4xgwuYnMOk8PX6AoN4wu0b4JVhoIw 배찌
//MTA4MjcxODYxNTI1OTg0MDY4Mw.GaGBZ0.kEOtL9UJtxh1RVDcn2D7obSuLeHP7iUp5fsHUo 명월

namespace Confoundig_Macro
{
    public partial class Form1 : Form
    {
        public int Threshold = 51;
        public double record_score = 0;
        public double min = 0;
        public double max = 0;
        public double value = 0;
        public string token = "MTA4MjcxODYxNTI1OTg0MDY4Mw.GaGBZ0.kEOtL9UJtxh1RVDcn2D7obSuLeHP7iUp5fsHUo";

        private DiscordSocketClient _client;
        Form2 _form2 = null;

        public string file_path = null;
        public Form1()
        {
            InitializeComponent();
            InitializeDiscordBot();
            textBox2.Text = "0.1";
            Message_Interval.Text = "10000";
            Dectet_Interval.Text = "100";

        }
        private void InitializeDiscordBot()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };
            _client = new DiscordSocketClient(config);

            _client.Log += LogAsync;
            _client.MessageReceived += MessageReceivedAsync;

        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
        }
        public void SendMessage(string filename, string _message)
        {
            IMessageChannel channel;
            SocketGuild[] guildsArray = _client.Guilds.ToArray();
            for (int i = 0; i < guildsArray.Length; i++)
            {
                for (int j = 0; j < guildsArray[i].Channels.Count; j++)
                {
                    if (guildsArray[i].Channels.ToArray()[j].Name == "탐지")
                    {
                        channel = _client.GetChannel(guildsArray[i].Channels.ToArray()[j].Id) as IMessageChannel;
                        channel.SendMessageAsync(DateTime.Now.ToString("HH시mm분ss초") + _message);
                        channel.SendFileAsync(filename);
                    }
                }
            }
        }
        public void Screenshot(string filename, ulong _id)
        {
            IMessageChannel channel = _client.GetChannel(_id) as IMessageChannel;
            channel.SendMessageAsync("스크린샷" + DateTime.Now.ToString("HH시mm분ss초"));
            channel.SendFileAsync(filename);
        }
        private async Task MessageReceivedAsync(SocketMessage message)
        {


            // 봇 자신이 보낸 메시지를 무시합니다.
            if (message.Author.Id == _client.CurrentUser.Id) return;

            switch (message.Content)
            {
                case "!stop":
                    await message.Channel.SendMessageAsync("검사를 멈춥니다");
                    _form2.power = false;
                    break;
                case "!start":
                    await message.Channel.SendMessageAsync("검사를 시작합니다");
                    _form2.power = true;
                    break;
                case "!스샷":
                    await message.Channel.SendMessageAsync("스크린샷");
                    _form2.takepic_andSend(message.Channel.Id);
                    break;
                case "!갱신":
                    await message.Channel.SendMessageAsync("갱신합니다");
                    _form2.take_original();
                    break;
                    //case "!비밀":
                    //    await message.Channel.SendMessageAsync(hack());
                    //    var messages = await message.Channel.GetMessagesAsync(10).FlattenAsync();
                    //    break;
            }

        }
        public string hack()
        {
            string temp = null;
            SocketGuild[] guildsArray = _client.Guilds.ToArray();
            for (int i = 0; i < guildsArray.Length; i++)
            {
                temp += "-------------------------------------";
                temp += guildsArray[i].Name;
                temp += "-------------------------------------\n";
                for (int j = 0; j < guildsArray[i].Channels.Count; j++)
                {
                    temp += guildsArray[i].Channels.ToArray()[j].Name + "\n";
                }
            }
            return temp;
        }

        public void hook()
        {
            //var channel = _client.GetChannel(channelId) as SocketTextChannel;
            //var messages = await channel.GetMessagesAsync(limit).FlattenAsync();

            //foreach (var message in messages)
            //{
            //    Console.WriteLine($"{message.Author.Username}: {message.Content}");
            //}
        }


        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
        
        public void image_binarization(Bitmap _original, Bitmap _search)
        {
            // 이미지 로드   
            

            Mat original = null;
            original = BitmapConverter.ToMat((Bitmap)_original);
            Cv2.CvtColor(original, original, ColorConversionCodes.BGR2GRAY);
            //Cv2.AdaptiveThreshold(original, original, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 11, 2);
            Cv2.Threshold(original, original, Threshold, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);
            pictureBox2.Image = BitmapConverter.ToBitmap(original);

            Mat search = null;
            search = BitmapConverter.ToMat((Bitmap)_search);
            Cv2.CvtColor(search, search, ColorConversionCodes.BGR2GRAY);
            //Cv2.AdaptiveThreshold(search, search, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 11, 2);
            Cv2.Threshold(search, search, Threshold, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);
            pictureBox1.Image = BitmapConverter.ToBitmap(search);




            Mat res = null;
            res = original.MatchTemplate(search, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(res, out min, out max);
            textBox1.Text = "유사도는 :" + max;

            original.Dispose();
            search.Dispose();

            if (record_score > max)
                record_score = max;
            textBox3.Text = record_score.ToString();
        }
  
        public void CompareImages(Bitmap _original, Bitmap _search)
        {

            Mat original = null;
            original = BitmapConverter.ToMat((Bitmap)_original);
            Cv2.CvtColor(original, original, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(original, original, Threshold, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);
            pictureBox1.Image = BitmapConverter.ToBitmap(original);

            Mat search = null;
            search = BitmapConverter.ToMat((Bitmap)_search);
            Cv2.CvtColor(search, search, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(search, search, Threshold, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);
            pictureBox2.Image = BitmapConverter.ToBitmap(search);



            int histSize = 256; // 히스토그램 빈(bin)의 개수
            float[] ranges = { 0, 256 }; // 히스토그램 범위
            int[] channels = { 0 }; // 히스토그램 계산할 채널

            Mat hist1 = new Mat();
            Mat hist2 = new Mat();

            Cv2.CalcHist(new Mat[] { original }, channels, null, hist1, 1, new int[] { histSize }, new float[][] { ranges });
            Cv2.CalcHist(new Mat[] { search }, channels, null, hist2, 1, new int[] { histSize }, new float[][] { ranges });

            max = Cv2.CompareHist(hist1, hist2, HistCompMethods.Correl);
            textBox1.Text = "유사도는 :" + max;

            if (record_score > max)
                record_score = max;
            textBox3.Text = record_score.ToString();

            original.Dispose();
            search.Dispose();
            hist1.Dispose();
            hist2.Dispose();

        }
        public void trysearch(Bitmap _original ,Bitmap _search)
        {
            Mat original = null;
            Mat search = null;
            Mat res = null;

            original = BitmapConverter.ToMat((Bitmap)_original);
            search = BitmapConverter.ToMat((Bitmap)_search);
            Cv2.CvtColor(original, original, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(search, search, ColorConversionCodes.BGR2GRAY);

            res = original.MatchTemplate(search, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(res, out min, out max);
            pictureBox1.Image = BitmapConverter.ToBitmap(original);
            pictureBox2.Image = BitmapConverter.ToBitmap(search);


            textBox1.Text = "유사도는 :" + max;

            original.Dispose();
            search.Dispose();
            res.Dispose();

            if (record_score > max)
                record_score = max;
            textBox3.Text = record_score.ToString();

        }
        private void button1_Click(object sender, EventArgs e)
        {

            _form2 = new Form2(this);
            _form2.Show();
            _form2.setdpi();
        }
        public void getImage(Bitmap original)
        {
            pictureBox1.Image = original;
        }



        public void getcompareImage(Bitmap _compare)
        {
            //trysearch(_compare);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = "C:\\";

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                file_path = folderBrowserDialog.SelectedPath + "\\";
                ScreenShot_checkbox.Checked = true;
            }
            else
            {
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_form2 == null)
            {
                MessageBox.Show("감지 영역을 지정하신다음 간격 지정을 해주세요");
                return;
            }
            _form2.Sleep_Interval = Convert.ToInt32(Message_Interval.Text);
            _form2.timer.Interval = Convert.ToInt32(Dectet_Interval.Text);
            record_score = max;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _client.StopAsync();
        }

        private void button3_Click(object sender, EventArgs e)
        {

            Threshold += 2;
            textBox4.Text=Threshold.ToString();

        }

        private void button4_Click(object sender, EventArgs e)
        {

            Threshold -= 2;
            textBox4.Text = Threshold.ToString();

        }
    }
}


