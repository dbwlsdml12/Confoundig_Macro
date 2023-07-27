using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Cloud.Vision.V1;
using Image = Google.Cloud.Vision.V1.Image;
using Timer = System.Windows.Forms.Timer;
using Point = System.Drawing.Point;
namespace Confoundig_Macro
{
    public partial class Form2 : Form
    {
        public Bitmap original = new Bitmap(100, 100);
        public Bitmap search = new Bitmap(100, 100);
        public Timer timer = new Timer();

        public string filepath = null;
        public bool power = true;
        public int Sleep_Interval = 10000;

        private Form1 form1 = null;
        private string _filename = null;
        private int count = 0;
        private Rectangle Rect;
        private double dpi = 1;
        private int prevtime = 0;
        private int currenttime = 0;
        public Form2(Form1 getform)
        {
            InitializeComponent();
            form1 = getform;
            filepath = form1.file_path;
            // 구글 비전 API키 (토큰) 받아오기
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"C:\Users\park\Desktop\key\instant-pivot-359019-d0af92a429e5.json");
        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.O:
                    take_original();
                    break;
                case Keys.P:
                    timer.Tick += loop;
                    timer.Start();

                    break;
                case Keys.S:
                    power = false;
                    break;
                case Keys.R:
                    power = true;
                    break;
            }
        }
        public void take_current()
        {
            setdpi();

            using (Bitmap psearch = new Bitmap(Rect.Width, Rect.Height))
            {
                using (Graphics gr = Graphics.FromImage(psearch))
                {
                    gr.CopyFromScreen(Rect.X, Rect.Y, 0, 0, psearch.Size);
                }
                search.Dispose();
                search = new Bitmap(psearch);
            }


        }
        public void take_original()
        {
            setdpi();
            //original.Dispose();
            //original = new Bitmap(Rect.Width, Rect.Height);

            //Graphics gr = Graphics.FromImage(original);

            //gr.CopyFromScreen(Rect.X, Rect.Y, 0, 0, original.Size);
            //form1.Image_checkbox.Checked = true;
            //form1.pictureBox1.Image = original;

            using (Bitmap temp = new Bitmap(Rect.Width, Rect.Height))
            {
                using (Graphics gr = Graphics.FromImage(temp))
                {
                    gr.CopyFromScreen(Rect.X, Rect.Y, 0, 0, temp.Size);
                }

                // 이전 original 이미지 객체를 해제하고 새로운 객체를 할당
                original.Dispose();
                original = new Bitmap(temp);
            }

            form1.Image_checkbox.Checked = true;

        }
        void loop(object sender, EventArgs e)
        {
            int Minute = DateTime.Now.Hour * 60 + DateTime.Now.Minute;

            currenttime = Environment.TickCount & Int32.MaxValue;
            take_current();

            if (power == false || currenttime < prevtime + Sleep_Interval)
                return;
            
            for (int i = 0; i < 6; i++)
            {
                if (Minute > 110 + 240 * i && Minute < 180 + 240 * i)
                {
                   return;
                }
            }

            CompareImage();

            //if (form1.max > 0.97)
            //    take_original();
        }




        private void CompareImage()
        {
            //form1.trysearch(original,search);
            form1.image_binarization(original,search);
            //form1.CompareImages(original, search);
            if (form1.max < Convert.ToDouble(form1.textBox2.Text))
            {
                prevtime = Environment.TickCount & Int32.MaxValue; ;

                saveImage();
                //Check_label();
                form1.SendMessage(_filename, "\nsomething is detected\n");
                Console.WriteLine("감지됐음");
                _filename = null;
            }
        }

        //jepg 인코딩
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == mimeType)
                {
                    return codec;
                }
            }

            return null;
        }
        //스크린샷 저장
        private void saveImage()
        {
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 80L);
            ImageCodecInfo jpegEncoder = GetEncoderInfo("image/jpeg");

            _filename = filepath + "교역_" + DateTime.Now.ToString("HH시mm분ss초fff") + ".jpeg";

            search.Save(_filename, jpegEncoder, encoderParams);

        }
        //!스샷

        public void takepic_andSend(ulong _id)
        {
            setdpi();
            Bitmap ScreenShot = new Bitmap(Rect.Width, Rect.Height);

            Graphics gr = Graphics.FromImage(ScreenShot);

            gr.CopyFromScreen(Rect.X, Rect.Y, 0, 0, ScreenShot.Size);




            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 80L);
            ImageCodecInfo jpegEncoder = GetEncoderInfo("image/jpeg");

            _filename = filepath + "교역_" + DateTime.Now.ToString("HH시mm분ss초fff") + ".jpeg";

            ScreenShot.Save(_filename, jpegEncoder, encoderParams);

            form1.Screenshot(_filename, _id);
            ScreenShot.Dispose();


        }

        private void Check_label()
        {
            var client = ImageAnnotatorClient.Create();
            var image = Image.FromBytes(File.ReadAllBytes(_filename));

            var Object = client.DetectLabels(image);

            for (int i = 0; i < Object.Count; i++)
            {
                if (Object[i].Description == "Boat" || Object[i].Description == "Ship" || Object[i].Description.Contains("Vehicle"))
                {
                    string message = $"\n{Object[i].Description}과 유사율: {Object[i].Score}\n";
                    form1.SendMessage(_filename, message);
                    return;
                }
            }
            count++;

        }

        private void check_text()
        {
            var client = ImageAnnotatorClient.Create();
            var image = Image.FromBytes(File.ReadAllBytes(_filename));

            var _text = client.DetectText(image);

            string buffer = _text.FirstOrDefault()?.Description;

            string[] Name = buffer.Split('\n');

            string message = null;
            for (int i = 0; i < Name.Length; i++)
            {
                if (Name[i].Contains("하리하라"))
                {
                    message = Name[i - 1] + "\n하리하라 연합\n";
                }
            }
            if (message == null)
            {
                return;
            }
        }
        public void setdpi()
        {
            Rect.X = Convert.ToInt32(this.Location.X+10 * dpi);
            Rect.Y = Convert.ToInt32(this.Location.Y+32 * dpi);
            Rect.Width = Convert.ToInt32(this.Size.Width-20 * dpi);
            Rect.Height = Convert.ToInt32(this.Size.Height -40* dpi);

        }

    }
}
