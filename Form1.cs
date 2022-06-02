using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AForge;//v2.2.5.0
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Math.Geometry;
using System.Drawing.Imaging;

//Remove ambiguousness between AForge.Image and System.Drawing.Image
//Remove ambiguousness between AForge.Point and System.Drawing.Point
using Point = System.Drawing.Point; 

namespace CameraTracking
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection VideoCapTureDevices;
        private VideoCaptureDevice videoSource;

        public Form1()
        {
            InitializeComponent();
        }

        int R; 
        int G;
        int B;

        private void Form1_Load(object sender, EventArgs e)
        {
            VideoCapTureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo videoDevice in VideoCapTureDevices)
            {
                comboBox1.Items.Add(videoDevice.Name);
            }
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
            else {
                MessageBox.Show("No video sources found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button1.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                videoSource = new VideoCaptureDevice(VideoCapTureDevices[comboBox1.SelectedIndex].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(Finalvideo_NewFrame);
                videoSource.VideoResolution = selectResolution(videoSource);// ** set resolution 2.2.5.0 Version **
                videoSource.Start();
                button2.Enabled = true;
                button5.Enabled = true;
            } else {
                MessageBox.Show("Choose a video source", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 设置视频分辨率
        /// </summary>
        /// <param name="device"></param>
        /// <returns>兼容列表</returns>
        private static VideoCapabilities selectResolution(VideoCaptureDevice device)
        {
            foreach (var cap in device.VideoCapabilities)
            {
                // 120/240 兼容性设置，暂不用
                //if (cap.FrameSize.Height == 120)
                //    return cap;
                //if (cap.FrameSize.Height == 240)
                //    return cap;
                if (cap.FrameSize.Height == 480)
                    return cap;
                //if (cap.FrameSize.Height == 640)
                //    return cap;
                //if (cap.FrameSize.Height == 1080)
                //    return cap;
                //if (cap.FrameSize.Width == 1920)
                //    return cap;
            }
            return device.VideoCapabilities.Last();
        }


        private Bitmap image, image1, image2, image3;
        void Finalvideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            image = (Bitmap)eventArgs.Frame.Clone();
            image1 = (Bitmap)eventArgs.Frame.Clone();
            if (templateMatchingMark)
            {
                image2 = (Bitmap)eventArgs.Frame.Clone();
            }                

            pictureBox1.Image = image;

            if (rdiobtnR.Checked)
            {
                // 创建过滤器
                EuclideanColorFiltering filter = new EuclideanColorFiltering();
                // 设置中心颜色和半径
                filter.CenterColor = new RGB(Color.FromArgb(215, 0, 0));
                filter.Radius = 100;
                // 应用过滤器
                filter.ApplyInPlace(image1);

                nesnebul(image1);
            }

            if (rdiobtnB.Checked)
            {
                // 创建过滤器
                EuclideanColorFiltering filter = new EuclideanColorFiltering();
                // 设置中心颜色和半径
                filter.CenterColor = new RGB(Color.FromArgb(30, 144, 255));
                filter.Radius = 100;
                // 应用过滤器
                filter.ApplyInPlace(image1);

                nesnebul(image1);
            }

            if (rdiobtnG.Checked)
            {
                // 创建过滤器
                EuclideanColorFiltering filter = new EuclideanColorFiltering();
                // 设置中心颜色和半径
                filter.CenterColor = new RGB(Color.FromArgb(0, 215, 0));
                filter.Radius = 100;
                // 应用过滤器
                filter.ApplyInPlace(image1);

                nesnebul(image1);
            }

            if (rdbtnUserColor.Checked)
            {
                // 创建过滤器
                EuclideanColorFiltering filter = new EuclideanColorFiltering();
                // set center colol and radius
                filter.CenterColor = new RGB(Color.FromArgb(R, G, B));
                filter.Radius = 100;
                // 应用过滤器
                filter.ApplyInPlace(image1);

                nesnebul(image1);
            }
        }
        public void nesnebul(Bitmap image)
        {
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.MinWidth = 5;
            blobCounter.MinHeight = 5;
            blobCounter.FilterBlobs = true;
            blobCounter.ObjectsOrder = ObjectsOrder.Size;
            //Grayscale griFiltre = new Grayscale(0.2125, 0.7154, 0.0721);
            //Grayscale griFiltre = new Grayscale(0.2, 0.2, 0.2);
            //Bitmap griImage = griFiltre.Apply(image);

            BitmapData objectsData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            // 灰度
            Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            UnmanagedImage grayImage = grayscaleFilter.Apply(new UnmanagedImage(objectsData));
            // 解锁图像
            image.UnlockBits(objectsData);


            blobCounter.ProcessImage(image);
            Rectangle[] rects = blobCounter.GetObjectsRectangles();
            Blob[] blobs = blobCounter.GetObjectsInformation();
            pictureBox2.Image = image;


            if (rdiobtnTekCisimTakibi.Checked)
            {
                //单个对象跟踪--------

                foreach (Rectangle recs in rects)
                {
                    if (rects.Length > 0)
                    {
                        Rectangle objectRect = rects[0];
                        //Graphics g = Graphics.FromImage(image);
                        Graphics g = pictureBox1.CreateGraphics();
                        using (Pen pen = new Pen(Color.FromArgb(252, 3, 26), 2))
                        {
                            g.DrawRectangle(pen, objectRect);
                        }
                        // 获取轮廓矩形的坐标。
                        int objectX = objectRect.X + (objectRect.Width / 2);
                        int objectY = objectRect.Y + (objectRect.Height / 2);
                        // g.DrawString(objectX.ToString() + "X" + objectY.ToString(), new Font("Arial", 12), Brushes.Red, new System.Drawing.Point(250, 1));
                        g.Dispose();

                        if (chkKoordinatiGoster.Checked)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                richTextBox1.Text = objectRect.Location.ToString() + "\n" + richTextBox1.Text + "\n"; ;
                            });
                        }
                    }
                }
            }



            if (rdiobtnCokCisimTakibi.Checked)
            {
                //多目标跟踪-------

                for (int i = 0; rects.Length > i; i++)
                {
                    Rectangle objectRect = rects[i];
                    //Graphics g = Graphics.FromImage(image);
                    Graphics g = pictureBox1.CreateGraphics();
                    using (Pen pen = new Pen(Color.FromArgb(252, 3, 26), 2))
                    {
                        g.DrawRectangle(pen, objectRect);
                        g.DrawString((i + 1).ToString(), new Font("Arial", 12), Brushes.Red, objectRect);
                    }
                    //获取轮廓矩形的坐标。
                    int objectX = objectRect.X + (objectRect.Width / 2);
                    int objectY = objectRect.Y + (objectRect.Height / 2);
                    //  g.DrawString(objectX.ToString() + "X" + objectY.ToString(), new Font("Arial", 12), Brushes.Red, new System.Drawing.Point(250, 1));


                    if (chkboxMesafeOlcer.Checked)
                    {
                        if (rects.Length > 1)
                        {
                            for (int j = 0; j < rects.Length - 1; j++)
                            {
                                int ilkx = (rects[j].Right + rects[j].Left) / 2;
                                int ilky = (rects[j].Top + rects[j].Bottom) / 2;

                                int ikix = (rects[j + 1].Right + rects[j + 1].Left) / 2;
                                int ikiy = (rects[j + 1].Top + rects[j + 1].Bottom) / 2;

                                g = pictureBox1.CreateGraphics();
                                //g.DrawLine(Pens.Red, rects[j].Location, rects[j + 1].Location);
                                //g.DrawLine(Pens.Blue, rects[0].Location, rects[rects.Length - 1].Location);
                                g.DrawLine(Pens.Red, ilkx, ilky, ikix, ikiy);
                            }
                        }

                        if (rects.Length == 2)
                        {

                            Rectangle ilk = rects[0];
                            Rectangle iki = rects[1];

                            int ilkX = ilk.X + (ilk.Width / 2);
                            int ilkY = ilk.Y + (ilk.Height / 2);

                            int ikiX = iki.X + (iki.Width / 2);
                            int ikiY = iki.Y + (iki.Height / 2);

                            //1 pixel (X) = 0.0264583333333334 centimeter [cm]

                            double formul = Math.Floor((Math.Sqrt((Math.Pow((ilkX - ikiX), 2)) + Math.Pow((ilkY - ikiY), 2))) * 0.0264);

                            string uzaklikY = "Y-" + Convert.ToString(ilkX - ikiX);
                            string uzaklikX = "X-" + Convert.ToString(ilkY - ikiY);

                            string distance = uzaklikX + " " + uzaklikY;

                            AForge.Imaging.Drawing.Line(objectsData, new IntPoint((int)ilkX, (int)ilkY), new IntPoint((int)ikiX, (int)ikiY), Color.Blue);


                            this.Invoke((MethodInvoker)delegate
                            {
                                richTextBox2.Text = formul.ToString() + " cm\n" + richTextBox2.Text + " cm\n"; ;
                            });


                            if (chkboxMesafeKordinati.Checked)
                            {

                                this.Invoke((MethodInvoker)delegate
                                {
                                    richTextBox3.Text = distance.ToString() + "\n" + richTextBox3.Text + "\n"; ;
                                });
                            }
                        }
                    }
                    g.Dispose();
                }
            }



            if (rdiobtnGeoSekil.Checked)
            {

                SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

                Graphics g = pictureBox1.CreateGraphics();
                Pen yellowPen = new Pen(Color.Yellow, 2); // 边界
                Pen redPen = new Pen(Color.Red, 2);       // 四边形
                Pen brownPen = new Pen(Color.Brown, 2);   // 具有已知子类型的四边形
                Pen greenPen = new Pen(Color.Green, 2);   // 已知三角形
                Pen bluePen = new Pen(Color.Blue, 2);     // 三角形

                for (int i = 0, n = blobs.Length; i < n; i++)
                {
                    List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);

                    AForge.Point center;
                    float radius;

                    // 圈？
                    if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                    {
                        g.DrawEllipse(yellowPen,
                            (float)(center.X - radius), (float)(center.Y - radius),
                            (float)(radius * 2), (float)(radius * 2));
                    }
                    else
                    {
                        List<IntPoint> corners;

                        // 是三角形 或 四边形
                        if (shapeChecker.IsConvexPolygon(edgePoints, out corners))
                        {
                            // 获取子类型
                            PolygonSubType subType = shapeChecker.CheckPolygonSubType(corners);

                            Pen pen;

                            if (subType == PolygonSubType.Unknown)
                            {
                                pen = (corners.Count == 4) ? redPen : bluePen;
                            }
                            else
                            {
                                pen = (corners.Count == 4) ? brownPen : greenPen;
                            }

                            g.DrawPolygon(pen, ToPointsArray(corners));
                        }
                    }
                }

                yellowPen.Dispose();
                redPen.Dispose();
                greenPen.Dispose();
                bluePen.Dispose();
                brownPen.Dispose();
                g.Dispose();
            }
        }

        // Conver list of AForge.NET's points to array of .NET points
        private Point[] ToPointsArray(List<IntPoint> points)
        {
            Point[] array = new Point[points.Count];

            for (int i = 0, n = points.Count; i < n; i++)
            {
                array[i] = new Point(points[i].X, points[i].Y);
            }

            return array;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            templateMatchingMark = false;
            videoSource.SignalToStop();//videoSource.Stop();
            if (videoSource != null && videoSource.IsRunning && pictureBox1.Image != null)
            {                
                pictureBox1.Image.Dispose();
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            R = trackBar1.Value;
            textColorValueRGB.BackColor = Color.FromArgb(R, G, B);
            lbR.Text = R.ToString();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            G = trackBar2.Value;
            textColorValueRGB.BackColor = Color.FromArgb(R, G, B);
            lbG.Text = G.ToString();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            B = trackBar3.Value;
            textColorValueRGB.BackColor = Color.FromArgb(R, G, B);
            lbB.Text = B.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // signal to stop
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
            }

            pictureBox1.Image.Dispose();

            Application.Exit();
        }

        bool PickingColor = true; 
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // 取色器
            if (e.Button == MouseButtons.Left)
            {
                if (PickingColor) 
                {
                    int x = e.X;
                    int y = e.Y;

                    using (Bitmap bmp = pictureBox1.Image as Bitmap)
                    {
                        Color pixelColor = bmp.GetPixel(x, y);
                        byte alpha = pixelColor.A;
                        byte red = pixelColor.R;
                        byte green = pixelColor.G;
                        byte blue = pixelColor.B;

                        trackBar1.Value = red;
                        trackBar2.Value = green;
                        trackBar3.Value = blue;

                        B = blue;
                        G = green;
                        R = red;

                        textColorValueRGB.BackColor = Color.FromArgb(R, G, B);
                    }
                    PickingColor = false;
                    Cursor = Cursors.Arrow;
                    rdbtnUserColor.Checked = true;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.Cross;
            PickingColor = true;
        }

        private bool templateMatchingMark;
        private string templateMatchingFileName;
        private void pictureBox3_DoubleClick(object sender, EventArgs e)
        {
            // 设置匹配图像模板
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "PNG图片|*.png*|JPG图像|*.jpg|所有文件|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                templateMatchingFileName = openFileDialog.FileName;
                pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;

                image3 = ConvertToFormat(ReadImageFile(templateMatchingFileName), System.Drawing.Imaging.PixelFormat.Format24bppRgb);             
                // 灰度化
                Grayscale grayscale = new Grayscale(0.2125, 0.7154, 0.0721);
                // 二值化
                Threshold binarization = new Threshold(85);
                // 降噪点
                BlobsFiltering denoise3 = new BlobsFiltering(5, 5, image3.Width, image3.Height);
                // 应用图像处理
                image3 = grayscale.Apply(image3);
                image3 = binarization.Apply(image3);
                image3 = denoise3.Apply(image3);

                pictureBox3.Image = ReadImageFile(templateMatchingFileName);
                templateMatchingMark = true;
                checkBox1.Enabled = true;
                if (videoSource != null && videoSource.IsRunning) {
                    checkBox1.Checked = true;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                timer1.Enabled = true;
            }
            else { 
                timer1.Enabled = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (templateMatchingMark)
            {
                button5_Click(sender, e);
            }                
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // signal to stop
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // 模板匹配
            if (templateMatchingFileName == null)
            {
                pictureBox3_DoubleClick(sender, e);

                return;
            }
            if (templateMatchingMark) {
                templateMatchingMark = false;
                // 匹配精度设置大于70%会给出列表
                ExhaustiveTemplateMatching templateMatching = new ExhaustiveTemplateMatching(0.7f);

                // 灰度化
                Grayscale grayscale = new Grayscale(0.2125, 0.7154, 0.0721);
                // 二值化
                Threshold binarization = new Threshold(85);
                // 降噪点
                BlobsFiltering denoise2 = new BlobsFiltering(5, 5, image2.Width, image2.Height);
                // 应用图像处理
                try
                {
                    image2 = grayscale.Apply(image2);
                    image2 = binarization.Apply(image2);
                    image2 = denoise2.Apply(image2);
                }
                catch (Exception)
                {
                    //throw;
                }

                // 处理图像以查找与指定模板的匹配项
                var compare = templateMatching.ProcessImage(image2, image3);

                if (compare.Length > 0)
                {
                    label2.Text = "相似度：" + Math.Round((compare[0].Similarity * 100),2).ToString() + "%";
                }
                else {
                    label2.Text = "相似度：低于70%";
                }

                if (!rdbtnUserColor.Checked && !rdbtnUserColor.Checked && !rdbtnUserColor.Checked && !rdbtnUserColor.Checked) {
                    pictureBox2.Image = image2;
                }
                pictureBox3.Image = image3;
                templateMatchingMark = true;
            }
            checkBox1.Checked = true;
            button5.Enabled = false;
        }

        /// <summary>
        /// 通过 FileStream 来打开文件，实现不锁定 Image 文件，不影响其他用户访问此文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Bitmap ReadImageFile(string path)
        {
            if (System.IO.File.Exists(path))
            {            
                System.IO.FileStream fs = System.IO.File.OpenRead(path);
                int filelength = 0;
                filelength = (int)fs.Length;
                Byte[] image = new Byte[filelength];
                fs.Read(image, 0, filelength); 
                System.Drawing.Image result = System.Drawing.Image.FromStream(fs);
                fs.Close();
                Bitmap bitmap = new Bitmap(result);
                return bitmap;
            }
            return null;
        }

        /// <summary>
        /// 图像格式转换
        /// </summary>
        /// <param name="image"></param>
        /// <param name="format">System.Drawing.Imaging.PixelFormat.Format24bppRgb</param>
        /// <returns></returns>
        public static Bitmap ConvertToFormat(System.Drawing.Image image, PixelFormat format)
        {
            Bitmap copy = new Bitmap(image.Width, image.Height, format);
            using (Graphics gr = Graphics.FromImage(copy))
            {
                gr.DrawImage(image, new Rectangle(0, 0, copy.Width, copy.Height));
            }
            return copy;
        }

    }
}
