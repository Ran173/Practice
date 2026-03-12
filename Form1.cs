using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
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
        private Bitmap[] _frames;
        private bool _isAnimating = false;
        private Timer _animTimer;
        private int _currentFrame = 0;
        private PictureBox _fishPicture;
        private const int MAX_FRAMES = 9; // ★ 限制最多20帧

        public Form1()
        {
            InitializeComponent();
            InitWoodenFish();
        }

        private void InitWoodenFish()
        {
            //主窗体
            this.Text = "电子木鱼";
            this.Size = new System.Drawing.Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;

            //木鱼图片控件
            _fishPicture = new PictureBox();
            _fishPicture.Name = "fishPicture";
            _fishPicture.Size = new System.Drawing.Size(640, 360);
            _fishPicture.Location = new System.Drawing.Point(90, 50);
            _fishPicture.SizeMode = PictureBoxSizeMode.Zoom;
            _fishPicture.Cursor = System.Windows.Forms.Cursors.Hand;

            try
            {
                string gifPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wooden_fish.gif");
                if (!File.Exists(gifPath))
                {
                    throw new FileNotFoundException($"找不到文件: {gifPath}");
                }

                // 加载GIF并拆分成帧
                LoadGifFrames(gifPath);

                // 显示第一帧
                if (_frames != null && _frames.Length > 0)
                {
                    _fishPicture.Image = _frames[0];
                }
            }
            catch (Exception ex)
            {
                _fishPicture.BackColor = System.Drawing.Color.LightGray;
                _fishPicture.Text = "木鱼图片\n加载失败";
                _fishPicture.Font = new System.Drawing.Font("微软雅黑", 14);
                MessageBox.Show($"图片加载失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.Controls.Add(_fishPicture);

            //点击事件
            _fishPicture.Click += (s, e) => PlayFishAnimationOnce();

            //计数标签初始化
            Label countLabel = new Label();
            countLabel.Name = "countLabel";
            countLabel.Text = $"功德+{tapCount}";
            countLabel.Font = new System.Drawing.Font("微软雅黑", 16, System.Drawing.FontStyle.Bold);
            countLabel.Location = new System.Drawing.Point(350, 430);
            countLabel.AutoSize = true;
            this.Controls.Add(countLabel);

            //音效初始化
            InitializeSound();

            //初始化定时器 - 用于逐帧播放动画
            _animTimer = new Timer { Interval = 50 }; // 50ms切换一帧
            _animTimer.Tick += AnimTimer_Tick;
        }

        /// <summary>
        /// 将GIF动画拆分为独立帧（最多MAX_FRAMES帧）
        /// </summary>
        private void LoadGifFrames(string gifPath)
        {
            using (Image gifImage = Image.FromFile(gifPath))
            {
                FrameDimension frameDimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
                int frameCount = gifImage.GetFrameCount(frameDimension);

                // ★ 限制帧数不超过MAX_FRAMES
                int framesToLoad = Math.Min(frameCount, MAX_FRAMES);
                _frames = new Bitmap[framesToLoad];

                for (int i = 0; i < framesToLoad; i++)
                {
                    gifImage.SelectActiveFrame(frameDimension, i);
                    _frames[i] = new Bitmap(gifImage);
                }
            }
        }

        private void InitializeSound()
        {
            try
            {
                string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wooden_fish.wav");
                if (!File.Exists(soundPath))
                {
                    throw new FileNotFoundException($"找不到声音文件: {soundPath}");
                }
                fishSoundPlayer = new SoundPlayer(soundPath);
                fishSoundPlayer.Load();
                useSystemSound = false;
            }
            catch (Exception ex)
            {
                useSystemSound = true;
                MessageBox.Show($"音效加载失败，将使用系统提示音替代！\n原因: {ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void PlayFishAnimationOnce()
        {
            // 如果正在动画中，忽略新的点击（防止重复触发）
            if (_isAnimating) return;

            if (_frames == null || _frames.Length == 0) return;

            // 播放音效
            PlaySound();

            // 更新计数
            tapCount++;
            Label countLabel = this.Controls["countLabel"] as Label;
            if (countLabel != null)
            {
                countLabel.Text = $"功德+{tapCount}";
            }

            // 开始动画 - 只播放一遍
            _isAnimating = true;
            _currentFrame = 0;
            _animTimer.Start();
        }

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            if (_frames == null || _frames.Length == 0) return;

            if (!_isAnimating) return;

            try
            {
                // 显示当前帧
                _fishPicture.Image = _frames[_currentFrame];

                _currentFrame++;

                // 检查是否播放完成
                if (_currentFrame >= _frames.Length)
                {
                    // 停止定时器
                    _animTimer.Stop();
                    _isAnimating = false;

                    // 重置到第一帧（静止状态）
                    _currentFrame = 0;
                    _fishPicture.Image = _frames[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"动画帧切换失败: {ex.Message}");
                _animTimer.Stop();
                _isAnimating = false;
            }
        }

        private void PlaySound()
        {
            try
            {
                if (useSystemSound)
                {
                    SystemSounds.Asterisk.Play();
                }
                else
                {
                    if (fishSoundPlayer != null && fishSoundPlayer.IsLoadCompleted)
                    {
                        fishSoundPlayer.Play();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"音效播放失败: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // 停止定时器
            if (_animTimer != null)
            {
                _animTimer.Stop();
                _animTimer.Dispose();
            }

            // 清理所有帧
            if (_frames != null)
            {
                foreach (var frame in _frames)
                {
                    frame?.Dispose();
                }
            }

            // 清理资源
            fishSoundPlayer?.Dispose();
            _fishPicture?.Dispose();

            // 清理控件
            foreach (Control control in this.Controls)
            {
                control?.Dispose();
            }
        }
    }
}
