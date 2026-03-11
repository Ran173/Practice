using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;




namespace EWF1
{
    public partial class Form1 : Form
    {
        //敲击计数
        private int tapCount = 0;
        //音效播放
        private SoundPlayer fishSoundPlayer;
        private bool useSystemSound = true;

        //gif动画控制
        private Image _fishGif;
        private bool _isAnimating = false;

        public Form1()
        {
            InitializeComponent();
            InitWoodenFish();
        }


        private void InitWoodenFish() {
            //主窗体
            this.Text = "电子木鱼";
            this.Size = new System.Drawing.Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            //木鱼图片控件
            PictureBox fishPicture = new PictureBox();
            fishPicture.Name = "fishPicture";
            fishPicture.Size = new System.Drawing.Size(640, 360);
            fishPicture.Location = new System.Drawing.Point(90, 50);
            fishPicture.SizeMode = PictureBoxSizeMode.Zoom;//等比放大

            try {
                //fishPicture.Load("https://img95.699pic.com/element/40098/0204.png_860.png");
                //fishPicture.Image = Image.FromFile("wooden_fish.gif");
                string gifPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wooden_fish.gif");
                _fishGif = Image.FromFile(gifPath);
                fishPicture.Image = _fishGif;

                ImageAnimator.UpdateFrames(_fishGif);
            }
            catch {
                fishPicture.BackColor = System.Drawing.Color.LightGray;
                fishPicture.Text = "木鱼图片";
                fishPicture.Font = new System.Drawing.Font("微软雅黑", 14);
                //fishPicture.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            }
            //fishPicture.Click += FishPicture_Click;
            //fishPicture.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Controls.Add( fishPicture );

            //点击事件
            fishPicture.Click += (s, e) => PlayFishAnimationOnce();


            //计数初始化
            Label countLabel = new Label();
            countLabel.Name = "countLabel";
            countLabel.Text = $"功德+{tapCount}";
            countLabel.Font = new System.Drawing.Font("微软雅黑", 16, System.Drawing.FontStyle.Bold);
            countLabel.Location = new System.Drawing.Point(400, 400);
            this.Controls.Add(countLabel);

            //音效初始化
            try
            {
                // 尝试加载本地音效文件
                fishSoundPlayer = new SoundPlayer("wooden_fish.wav");
                fishSoundPlayer.Load(); // 主动加载，失败则触发catch
                useSystemSound = false; // 标记使用本地音效
            }
            catch
            {
                // 本地音效加载失败，标记使用系统提示音
                useSystemSound = true;
                MessageBox.Show("木鱼音效文件未找到，将使用系统提示音替代！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        //
        private void PlayFishAnimationOnce()
        {
            PictureBox fishPic = this.Controls["fishPicture"] as PictureBox;
            if (fishPic.Image == null || _isAnimating) return;

            if(useSystemSound)
                SystemSounds.Asterisk.Play();
            else
                fishSoundPlayer.Play();

            tapCount++;
            (this.Controls["countLabel"] as Label).Text = $"功德+{tapCount}";

            _isAnimating = true;
            ImageAnimator.Animate(_fishGif, (s, e) =>
            {
                ImageAnimator.UpdateFrames(_fishGif);
                fishPic.Invalidate();
            });

            Timer animTimer = new Timer { Interval = 500 };
            animTimer.Tick += (s, e) =>
            {
                ImageAnimator.StopAnimate(_fishGif, null);
                _isAnimating = false;
                animTimer.Stop();
                animTimer.Dispose();
            };
            animTimer.Start();

        }

        ////图片点击事件
        //private void FishPicture_Click(object sender, EventArgs e)
        //{
        //    TapWoodenFish();
        //    //重新加载GIF
        //    PictureBox fishPic = sender as PictureBox;
        //    if (fishPic.Image != null)
        //    {
        //        fishPic.Image = Image.FromFile("wooden_fish.gif");
        //    }
        //}

        ////敲击逻辑
        //private void TapWoodenFish() {
        //    // 播放音效（根据标记选择播放方式）
        //    if (useSystemSound)
        //    {
        //        // 直接调用系统提示音的Play()方法（正确方式）
        //        System.Media.SystemSounds.Beep.Play();
        //    }
        //    else
        //    {
        //        // 播放本地音效文件（原有逻辑）
        //        fishSoundPlayer.Play();
        //    }
        //    tapCount++;
        //    (this.Controls["countLable"] as Label).Text = $"功德+{tapCount}";
        //    //抖动效果
        //    PictureBox fishPic = this.Controls["fishPicture"] as PictureBox;
        //    fishPic.Location = new System.Drawing.Point(fishPic.Location.X + 5, fishPic.Location.Y);
        //    System.Threading.Thread.Sleep(50);
        //    fishPic.Location = new System.Drawing.Point(fishPic.Location.X - 5, fishPic.Location.Y);
        //}


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            fishSoundPlayer?.Dispose();
            //PictureBox fishPic = this.Controls["fishPicture"] as PictureBox;
            _fishGif?.Dispose();
            //autoTapTimer?.Dispose();
        }


    }
}
