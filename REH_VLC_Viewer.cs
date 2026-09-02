using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;
using LibVLCSharp.Shared;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.Devices;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Richard_VLC
{
    public partial class REH_VLC_Viewer : Form
    {
        enum button_image
        {
            to_begin = 0, play = 1, pause = 2, stop = 3, to_end = 4, single_forw = 5, single_back = 6
        }
        public double frame_rate = 30.0;  // frames per second
        public int video_length = 0; //  (int)(frame_rate * 60.0 * 5.0);  // frames
        public double current_pos = 0.0; // current frame
        public TimeSpan current_time = new TimeSpan(0); // current offset
        public double current_speed = 1.0;

        public bool SIMULATE_PLAY = true;
        public bool good_aspect = false;  // this.good_aspect = this._mp.AspectRatio?.Contains(":");
        public bool good_framerate = false;  // this.good_framerate = this._mp.Fps > 0.5;
        public bool good_length = false;  // this.good_length = this._mp.Length > 1;

        public double hold_speed = -9999;
        public bool hold_reverse = false;
        public bool hold_playing = false;
        public bool hold_paused = false;
        private DateTime lasttrackBarSpeedMouseDown = DateTime.MinValue;
        private Point mouseVideoLocation = new System.Drawing.Point(0, 0);
        private int videoZoomWidth = 0; // = this.pnlVideoZoom.Width;
        private double videoZoomAspect = 1.0;
        private DateTime lastZoomPan = DateTime.MinValue;

        private int startWidth;
        private int startHeight;


        public double speed_1 = 0.7;
        public double speed_ff = 0.1;

        public double min_speed = 0.004;  // 8 sec's per frame = (1 / 8) frames per sec
                                          // norm speed is 30 frames per sec
                                          // 30 * speed = (1 / 8)
                                          // speed = (1 / (30 * 8))
        public double max_speed = 4.0;

        public bool reverse_motion = false;
        public bool playing = false;
        public bool paused = false;

        public bool single_framing_forward = false;
        public bool single_framing_backward = false;

        public bool always_display_controls = true;
        public bool show_zoom_viewer = false;
        public bool show_info = false;

        public bool dragging_box = false;

        public string video_file = "";

        public bool isFullscreen = false;
        public bool isPlaying = false;
        public Size oldVideoSize;
        public Size oldFormSize;
        public Point oldVideoLocation;

        public LibVLC _libVLC;
        public MediaPlayer _mp;
        public Media media;

        private readonly CancellationTokenSource _cts = new();

        public REH_VLC_Viewer()
        {
            InitializeComponent();
        }

        private async Task RunBackgroundTaskAsync()
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));

            try {
                while (await timer.WaitForNextTickAsync(_cts.Token)) {

                    double pos = this.current_pos;

                    if (this.SIMULATE_PLAY) {
                        if (this.single_framing_forward) {
                            pos += 0.5;  // 1/2 frame per 500 ms
                        } else if (this.single_framing_backward) {
                            pos -= 0.5;
                        } else if (this.playing && !this.paused) {
                            double fr = 0.5 * (this.frame_rate * this.current_speed);
                            if (this.reverse_motion) {
                                pos -= fr;
                            } else {
                                pos += fr;
                            }
                        }
                    } else {
                        if (!this.good_aspect) {
                            var aspect_ratio = GetHeightWidthRatio(this._mp.AspectRatio);
                            if (aspect_ratio > 0) {
                                this.good_aspect = true;
                            }
                            if (this.good_aspect) {
                                if (aspect_ratio < 0.6) {
                                    this.pnlVideoFull.Width = this.startWidth;
                                    this.pnlVideoFull.Height = (int)(aspect_ratio * (double)this.startWidth);
                                } else {
                                    this.pnlVideoFull.Height = this.startHeight;
                                    this.pnlVideoFull.Width = (int)((double)this.startHeight / aspect_ratio);
                                }
                                this.pnlVideoZoom.Width = this.pnlVideoFull.Width;
                                this.pnlVideoZoom.Height = this.pnlVideoFull.Height;

                                this.videoZoomWidth = this.pnlVideoZoom.Width;
                                this.videoZoomAspect = (double)this.pnlVideoZoom.Width / (double)this.pnlVideoZoom.Height;
                            }
                        }
                        if (!good_framerate) {
                            this.good_framerate = this._mp.Fps > 0.5;
                            if (good_framerate) {
                                this.frame_rate = this._mp.Fps;
                                Update_Value("frame_rate");
                            }
                        }
                        if (!good_length) {
                            this.good_length = this._mp.Length > 1;
                            if (good_length) {
                                this.video_length = (int)(this.frame_rate * ((double)this._mp.Length / 1000.0));
                                trackBarPlayHead.Maximum = this.video_length;
                            }
                        }
                        // this.current_pos = this.video_length * this._mp.Position;
                        // var pos_secs = this.current_pos / this.frame_rate;
                        // this.current_time = TimeSpan.FromSeconds((double)this._mp.Time / 1000.0);

                        pos = (double)this.video_length * this._mp.Position;
                    }
                    Set_Current_Pos(pos);

                    if (this.pnlVideoFull.Visible && !this.show_zoom_viewer) {
                        DateTime now = DateTime.Now;
                        if ((now - this.lastZoomPan).TotalSeconds > 0.5) { // hide after N secs
                            this.lastZoomPan = DateTime.MinValue;
                            this.pnlVideoFull.Visible = false;
                        }
                    }
                }
            } catch (OperationCanceledException) {
                // Normal shutdown.
            }
        }

        private void butBegin_Click(object sender, EventArgs e)
        {
            this.videoView1?.MediaPlayer?.SeekTo(new TimeSpan(0));

            Set_Current_Pos(0);
        }

        private void butPlay_Click(object sender, EventArgs e)
        {

            if (this.playing && !this.paused) {
                // playing   Play => PAUSE
                this.videoView1?.MediaPlayer?.Pause();

                this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
                this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
                this.paused = true;
            } else {
                // not playing   Play => PLAY
                this.videoView1?.MediaPlayer?.Play();

                this.butPlay.Image = this.imageList1.Images[(int)button_image.pause];
                this.toolTip1.SetToolTip(this.butPlay, "Click to PAUSE");
                this.butStop.Visible = true;
                this.playing = true;
                this.paused = false;
                this.reverse_motion = false;
            }
            Update_Value("mode");
            Update_Value("reverse_motion");
        }

        private void butStop_Click(object sender, EventArgs e)
        {
            this.videoView1?.MediaPlayer?.Stop();

            if (this.playing && !this.paused) {
                this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
                this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
                this.playing = false;
                this.paused = false;
                this.single_framing_forward = false;
                this.single_framing_backward = false;
            } else if (this.paused) {
                this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
                this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
                this.playing = false;
                this.paused = false;
                this.single_framing_forward = false;
                this.single_framing_backward = false;
            }
            Update_Value("mode");
            Update_Value("reverse_motion");

            this.butStop.Visible = false; // *** should hide it with a timer

            //  Set_Current_Pos(0);   ... don't do this it's annoying
        }

        private void butSingleBack_Click(object sender, EventArgs e)
        {
        }
        private void butSingleBack_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.playing && !this.paused) {
                // playing   Play => PAUSE
                this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
                this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
                this.paused = true;
                Update_Value("mode");
            } else if (!playing) {
                this.playing = true;
                this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
                this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
                this.butStop.Visible = true;
                this.paused = true;
                Update_Value("mode");
            }
            this.single_framing_backward = true;
            Update_Value("mode");

            if (Math.Abs(this.current_pos) < 0.5) {
                return;
            }
            if (this.current_pos <= 0) {
                Set_Current_Pos(0.0);
                return;
            }
            Set_Current_Pos(this.current_pos - 1.0);
        }

        private void butSingleBack_MouseUp(object sender, MouseEventArgs e)
        {
            this.single_framing_backward = false;
            Update_Value("mode");
        }

        private void butSingleFwd_Click(object sender, EventArgs e)
        {
        }
        private void butSingleFwd_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.playing) {
                // playing   Play => PAUSE
                this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
                this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
                this.paused = true;
                Update_Value("mode");
            } else if (!playing) {
                this.playing = true;
                this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
                this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
                this.butStop.Visible = true;
                this.paused = true;
                Update_Value("mode");
            }
            this.single_framing_forward = true;
            Update_Value("mode");

            if ((int)this.current_pos >= this.video_length) {
                return;
            }
            Set_Current_Pos(this.current_pos + 1.0);
        }

        private void butSingleFwd_MouseUp(object sender, MouseEventArgs e)
        {
            this.single_framing_forward = false;
            Update_Value("mode");
        }

        private void trackBarPlayHead_Scroll(object sender, EventArgs e)
        {
            System.Windows.Forms.TrackBar myTB = (System.Windows.Forms.TrackBar)sender;

            this.current_pos = (double)myTB.Value;

            this.current_time = TimeSpan.FromSeconds((double)this.current_pos / this.frame_rate);

            Update_Value("current_pos");
            Update_Value("current_time");

            Debug.WriteLine($"trackBarPlayHead_Scroll: FRAME {myTB.Value.ToString()} - SECONDS {current_time.ToString()}");
        }

        private void trackBarSpeed_Scroll(object sender, EventArgs e)
        {
            System.Windows.Forms.TrackBar myTB = (System.Windows.Forms.TrackBar)sender;

            double x = (double)myTB.Value / (double)myTB.Maximum;

            // (0.0, 0.004)
            // (0.7, 1.0)
            // (1.0, 4.0)

            // current_speed = 8.577142857 * x * x - 4.581142857 * x + 0.004;
            //          y = 8,5771x ^ 2 -         4,5811x +         0,004

            //   current_speed = 0.0201 * Math.Pow(200, x) - 0.0161;
            //   current_speed = 0.0815 * Math.Pow(50, x) - 0.0775;
            //   current_speed = 0.444 * Math.Pow(10, x) - 0.440;
            //   current_speed = 7.992 * x + 0.448 - 0.444 * Math.Pow(10, x);  // inverted curve
            //   current_speed = 3.996 * x + 0.004; // linear
            this.current_speed = 0.004 * Math.Pow(1000, x) - 0;  // a = 0.004  b = 1000

            //double targetY = 1.0;
            //double x0 = Math.Log(targetY / 0.004) / Math.Log(1000);  // a = 0.004  b = 1000

            double fr_rate = this.frame_rate * this.current_speed;

            if (fr_rate < 1.0) {
                double secs_per_fr = 1.0 / fr_rate;
                Debug.WriteLine($"trackBarSpeed_Scroll: {x}  => speed {this.current_speed}  => {(int)secs_per_fr} seconds per frame");
            } else {
                Debug.WriteLine($"trackBarSpeed_Scroll: {x}  => speed {this.current_speed}  => {(int)fr_rate} frames per second");
            }
            this.reverse_motion = false; // go back to forward motion

            Update_Value("reverse_motion");
            Update_Value("current_speed");

        }

        private void trackBarSpeed_MouseDown(object sender, MouseEventArgs e)
        {
            System.Windows.Forms.TrackBar myTB = (System.Windows.Forms.TrackBar)sender;

            DateTime now = DateTime.Now;

            if ((now - this.lasttrackBarSpeedMouseDown).TotalMilliseconds <= SystemInformation.DoubleClickTime) {
                // Double-click detected
                this.lasttrackBarSpeedMouseDown = DateTime.MinValue;

                myTB.Value = (int)(speed_1 * (double)trackBarSpeed.Maximum);   // set marker at 100% speed

                this.current_speed = 1.0;
                this.reverse_motion = false; // go back to forward motion

                Update_Value("reverse_motion");
                Update_Value("current_speed");

                return;
            }

            this.lasttrackBarSpeedMouseDown = now;
        }
        private void trackBarJogShuttle_Scroll(object sender, EventArgs e)
        {
            System.Windows.Forms.TrackBar myTB = (System.Windows.Forms.TrackBar)sender;

            if (this.hold_speed == -9999) {
                this.hold_speed = this.current_speed;
                this.hold_reverse = this.reverse_motion;
                this.hold_playing = this.playing;
                this.hold_paused = this.paused;
            }

            if (!this.playing) {
                // go to PLAY
                this.butPlay.Image = this.imageList1.Images[(int)button_image.pause];
                this.toolTip1.SetToolTip(this.butPlay, "Click to PAUSE");
                this.butStop.Visible = true;
                this.playing = true;
                this.paused = false;
                Update_Value("mode");
            } else if (this.paused) {
                // paused => PLAY
                this.butPlay.Image = this.imageList1.Images[(int)button_image.pause];
                this.toolTip1.SetToolTip(this.butPlay, "Click to PAUSE");
                this.paused = false;
                Update_Value("mode");
            }

            double x = (double)myTB.Value;

            double speed_offset = 10.0 * x / (double)trackBarJogShuttle.Maximum;

            double fr_rate = this.frame_rate;

            if (speed_offset < 0.0) {
                fr_rate = this.frame_rate + (-1.0) * speed_offset * this.frame_rate;
                this.reverse_motion = true;
                Debug.WriteLine($"trackBarJogShuttle_Scroll: scroll back at {speed_offset} x normal speed => {fr_rate} frames per second");
                this.current_speed = fr_rate / this.frame_rate;
            } else {
                fr_rate = this.frame_rate + speed_offset * this.frame_rate;
                this.reverse_motion = false;
                Debug.WriteLine($"trackBarJogShuttle_Scroll: scroll forward at {speed_offset} x normal speed => {fr_rate} frames per second");
                this.current_speed = fr_rate / this.frame_rate;
            }
            Update_Value("current_speed");
            Update_Value("reverse_motion");
        }
        private void trackBarJogShuttle_MouseUp(object sender, MouseEventArgs e)
        {
            trackBarJogShuttle.Value = 0;

            if ((this.hold_playing != this.playing) || (this.hold_paused != this.paused)) {
                if (this.hold_paused) {
                    // go back to PAUSE mode
                    this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
                    this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
                    this.paused = true;
                    Update_Value("mode");
                } else if (!this.hold_playing) {
                    // go back to STOP mode
                    this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
                    this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
                    this.playing = false;
                    this.paused = false;
                    Update_Value("mode");
                }
            }


            if (this.hold_speed == -9999) {
                this.current_speed = 1;
                this.reverse_motion = false;
            } else {
                this.current_speed = this.hold_speed;
                this.reverse_motion = this.hold_reverse;
                this.hold_speed = -9999;
            }

            Update_Value("current_speed");
            Update_Value("reverse_motion");
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("dataGridView1_MouseClick");
            this.show_info = !this.show_info;
        }

        private void Set_Current_Pos(double pos)
        {
            this.current_pos = Math.Min(Math.Max(0.0, pos), (double)this.video_length);
            this.current_time = TimeSpan.FromSeconds((double)this.current_pos / this.frame_rate);
            this.trackBarPlayHead.Value = (int)Math.Min(Math.Max((double)this.trackBarPlayHead.Minimum, pos), (double)this.trackBarPlayHead.Maximum);

            Update_Value("current_pos");
            Update_Value("current_time");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Load_Buttons();

            Init_Data();

            this.video_length = (int)(this.frame_rate * 60.0 * 5.0);   // assume 5.0 minute long video

            double targetY = 1.0;
            this.speed_1 = Math.Log(targetY / 0.004) / Math.Log(1000);  // a = 0.004  b = 1000
            this.speed_ff = Math.Log((1.0 / this.frame_rate) / 0.004) / Math.Log(1000);  // a = 0.004  b = 1000

            trackBarSpeed.Value = (int)(speed_ff * (double)trackBarSpeed.Maximum);  // set marker at 1 second per frame
            trackBarSpeed.Value = (int)(speed_1 * (double)trackBarSpeed.Maximum);   // set marker at 100% speed
            trackBarPlayHead.Maximum = this.video_length;

            trackBarJogShuttle.Minimum = -50;
            trackBarJogShuttle.Maximum = 50;
            trackBarJogShuttle.Value = 0;

            this.startWidth = this.pnlVideoFull.Width;
            this.startHeight = this.pnlVideoFull.Height;

            this.pnlVideoZoom.Left = 0;
            this.pnlVideoZoom.Top = 0;
            this.pnlVideoZoom.Width = this.pnlVideoFull.Width;
            this.pnlVideoZoom.Height = this.pnlVideoFull.Height;

            this.videoZoomWidth = this.pnlVideoZoom.Width;
            this.videoZoomAspect = (double)this.pnlVideoZoom.Width / (double)this.pnlVideoZoom.Height;

            if (!this.always_display_controls) {
                this.pnlVideoFull.Visible = false;
                this.trackBarSpeed.Visible = false;
                this.trackBarJogShuttle.Visible = false;
                this.dataGridView1.Visible = false;
            }

            int x0 = (int)(0.5 * (double)this.pnlVIDEO.Width);
            int y0 = (int)(0.5 * (double)this.pnlVIDEO.Height);

            this.mouseVideoLocation = new System.Drawing.Point(x0, y0);

            this.butPlay.Image = this.imageList1.Images[(int)button_image.play];

            Update_Value("current_pos");
            Update_Value("current_time");
            Update_Value("current_speed");
            //Update_Value("frame_rate");
            Update_Value("reverse_motion");
            Update_Value("mode");

            this.toolTip1.SetToolTip(this.trackBarSpeed, "Set playback-Speed - SLOW @ bottom - FAST @ top - Double-Click = normal speed.");
            this.toolTip1.SetToolTip(this.trackBarJogShuttle, "Hold and drag to Scroll left or right, let go to resume.");
            this.toolTip1.SetToolTip(this.pnlVideoFull, "Thumbnail of source video to set Zoom / Pan");
            this.toolTip1.SetToolTip(this.dataGridView1, "Technical details");
            this.toolTip1.SetToolTip(this.butBegin, "Goto Start of Video");
            this.toolTip1.SetToolTip(this.butSingleBack, "Hold-down to move back frame by frame.");
            this.toolTip1.SetToolTip(this.butSingleFwd, "Hold-down to move forward frame by frame.");
            this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
            this.toolTip1.SetToolTip(this.butStop, "Click to STOP");

            this.butStop.Visible = false;

            Core.Initialize();

            this.KeyPreview = true;
        //  this.KeyDown += new KeyEventHandler(ShortcutEvent);
            this.oldVideoSize = videoView1.Size;
            this.oldFormSize = this.Size;
            this.oldVideoLocation = this.videoView1.Location;

            var options = new[] { "--no-mouse-events", "--no-keyboard-events" };  // https://wiki.videolan.org/VLC_command-line_help/
            this._libVLC = new LibVLC(options);
            this._mp = new MediaPlayer(_libVLC);

            this._mp.EnableMouseInput = false;
            this._mp.EnableKeyInput = false;

            // this._mp.EnableMouseInput = false;

            // this._mp.Hwnd = this.pnlVIDEO.Handle;

            this.videoView1.MediaPlayer = _mp;

            _ = RunBackgroundTaskAsync();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts.Cancel();
        }

        private void Load_Buttons()
        {
            int bord_wdt = 5;
            int inp_size = 100;
            int but_marg = 0;
            int but_size = (int)(this.butBegin.Width) - but_marg * 2;  // target size
            int out_size = but_size - bord_wdt * 2;  // area to fill

            Debug.WriteLine("LOADING BITMAPS...");

            this.imageList1.ImageSize = new Size(out_size + bord_wdt * 2, out_size + bord_wdt * 2);

            using Bitmap bmp = new Bitmap("C:\\Users\\Richard\\source\\repos\\Richard_VLC\\buttons_plus.png");

            int[] xoffs = { 0, 118, 236, 355, 473, 593, 711 };

            int hgt = Math.Min(inp_size, bmp.Height);

            for (int n = 0; n < 7; n++) {
                int x = xoffs[n];
                if (x >= bmp.Width) break;
                int wdt = Math.Min(inp_size, bmp.Width - x);
                Rectangle r = new Rectangle(xoffs[n], 0, wdt, hgt);

                using Bitmap original = bmp.Clone(r, bmp.PixelFormat);

                // Create the resized image
                Bitmap resized = new Bitmap(out_size + bord_wdt * 2, out_size + bord_wdt * 2); // output is same size as input, only with a boarder around it

                using (Graphics g = Graphics.FromImage(resized)) {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                    int nwdt;
                    int nhgt;
                    if ((wdt == inp_size) && (hgt == inp_size)) {
                        nhgt = out_size;
                        nwdt = out_size;
                    } else if (wdt < hgt) {
                        // vertical
                        nhgt = out_size;
                        nwdt = (int)((double)out_size * (double)(wdt / hgt));
                    } else {
                        // horiz
                        nhgt = (int)((double)out_size * (double)(hgt / wdt));
                        nwdt = out_size;
                    }

                    Debug.WriteLine($"{n}] {wdt} x {hgt} =>  {nwdt} x {nhgt}");

                    g.DrawImage(original,
                        new Rectangle(bord_wdt, bord_wdt, nwdt, nhgt),
                        new Rectangle(0, 0, wdt, hgt),
                        GraphicsUnit.Pixel);
                }

                this.imageList1.Images.Add(resized);
            }
            // imageList1[0] = to begin
            // imageList1[1] = play
            // imageList1[2] = pause
            // imageList1[3] = stop
            // imageList1[4] = to end

            this.butBegin.Image = this.imageList1.Images[(int)button_image.to_begin];
            this.butSingleBack.Image = imageList1.Images[(int)button_image.single_back];
            this.butPlay.Image = imageList1.Images[(int)button_image.play];
            this.butSingleFwd.Image = imageList1.Images[(int)button_image.single_forw];
            this.butStop.Image = imageList1.Images[(int)button_image.stop];

            Init_Button(this.butBegin);
            Init_Button(this.butSingleBack);
            Init_Button(this.butPlay);
            Init_Button(this.butSingleFwd);
            Init_Button(this.butStop);
        }

        private void Init_Button(System.Windows.Forms.Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseDownBackColor = Color.AliceBlue;
            button.FlatAppearance.MouseOverBackColor = Color.AntiqueWhite;
            button.BackColor = Color.Transparent;
            button.UseVisualStyleBackColor = false;
            //  button.Image = imageList1.Images[0];
            button.Text = "";
        }

        private void Init_Data()
        {
            string[] cols = { "Property", "Value", "Units" };
            string[] keys = { "current_pos", "current_time", "current_speed", "frame_rate", "reverse_motion", "mode" };
            string[] key_names = { "current_pos", "current_time", "current_speed", "frame_rate", "reverse_motion", "mode" };
            DataTable dt2 = new DataTable();

            foreach (string col in cols) {
                DataColumn dc2 = new DataColumn(col); //, System.Type.GetType(ttyp))
                dc2.Caption = col;
                dt2.Columns.Add(dc2);
            }

            for (int r = 0; r < keys.Count(); r++) {
                string key = keys[r];
                string caption = key_names[r];
                DataRow drNew = dt2.NewRow();

                drNew["Property"] = key;

                switch (key) {
                    case "current_pos":
                        //drNew["Value"] = (int)this.current_pos;
                        drNew["Units"] = "frames";
                        break;
                    case "current_time":
                        //int tot_hrs = (int)Math.Floor(this.current_time.TotalHours);
                        //int mins = this.current_time.Minutes;
                        //int secs = this.current_time.Seconds;
                        //drNew["Value"] = $"{tot_hrs:00}:{mins:00}:{secs:00}";
                        drNew["Units"] = "time-offset";
                        break;
                    case "current_speed":
                        //drNew["Value"] = this.current_speed.ToString("0.000");
                        drNew["Units"] = "relative speed (to frame-rate)";
                        break;
                    case "frame_rate":
                        //double fr_rate = this.frame_rate * this.current_speed;
                        //if (fr_rate < 1.0) {
                        //    double secs_per_fr = 1.0 / fr_rate;
                        //    drNew["Value"] = secs_per_fr.ToString("0.0");
                        //    drNew["Units"] = "seconds per frame";
                        //} else {
                        //    drNew["Value"] = (int)fr_rate;
                        //    drNew["Units"] = "frames per second";
                        //}
                        break;
                    case "reverse_motion":
                        //drNew["Value"] = this.reverse_motion;
                        drNew["Units"] = "travelling in reverse";
                        break;
                    case "mode":
                        //drNew["Value"] = paused ? "PAUSED" : playing ? "PLAYING" : "STOPPED";
                        drNew["Units"] = "";
                        break;
                }
                dt2.Rows.Add(drNew);
            }

            this.dataGridView1.DataSource = dt2;
            this.dataGridView1.AutoResizeColumns();
        }


        private void Update_Value(string key)
        {
            DataTable? dt2 = (DataTable)this.dataGridView1.DataSource;
            if (dt2 == null) return;

            foreach (DataRow row in dt2.Rows) {
                if (row["Property"].ToString().Contains(key)) {
                    switch (key) {
                        case "current_pos":
                            row["Value"] = (int)this.current_pos;
                            break;
                        case "current_time":
                            int tot_hrs = (int)Math.Floor(this.current_time.TotalHours);
                            int mins = this.current_time.Minutes;
                            int secs = this.current_time.Seconds;
                            row["Value"] = $"{tot_hrs:00}:{mins:00}:{secs:00}";
                            break;
                        case "current_speed":
                            row["Value"] = this.current_speed.ToString("0.000");
                            Update_Value("frame_rate");
                            break;
                        case "frame_rate":
                            double fr_rate = this.frame_rate * this.current_speed;
                            if (fr_rate < 1.0) {
                                double secs_per_fr = 1.0 / fr_rate;
                                row["Value"] = secs_per_fr.ToString("0.0");
                                row["Units"] = "seconds per frame";
                            } else {
                                row["Value"] = (int)fr_rate;
                                row["Units"] = "frames per second";
                            }
                            break;
                        case "reverse_motion":
                            row["Value"] = this.reverse_motion;
                            break;
                        case "mode":
                            if (this.single_framing_forward) {
                                row["Value"] = ">>";
                            } else if (this.single_framing_backward) {
                                row["Value"] = "<<";
                            } else {
                                row["Value"] = paused ? "PAUSED" : playing ? "PLAYING" : "STOPPED";
                            }
                            break;
                    }
                    break;
                }
            }
        }

        private void pnlVIDEO_MouseWheel(object sender, MouseEventArgs e)
        {
            this.lastZoomPan = DateTime.Now;

            this.pnlVideoFull.Visible = true;

            int numberOfClicks = e.Delta * SystemInformation.MouseWheelScrollLines;

            Debug.WriteLine($"pnlVIDEO_MouseWheel: {numberOfClicks}");

            int newwdt = this.videoZoomWidth;

            if (numberOfClicks > 0) {
                newwdt += (int)((double)this.pnlVideoFull.Width * 0.05);
            } else {
                newwdt -= (int)((double)this.pnlVideoFull.Width * 0.05);
            }
            newwdt = Math.Max(20, Math.Min(this.pnlVideoFull.Width, newwdt));

            this.videoZoomWidth = newwdt;

            Zoom_Pan();
        }

        public void Zoom_Pan()
        {
            int newwdt = this.videoZoomWidth;

            int newhgt = (int)((double)newwdt / this.videoZoomAspect);

            newhgt = Math.Max(20, Math.Min(this.pnlVideoFull.Height, newhgt));

            int x0 = (int)(((double)this.mouseVideoLocation.X / (double)this.pnlVIDEO.Width) * (double)this.pnlVideoFull.Width);
            int y0 = (int)(((double)this.mouseVideoLocation.Y / (double)this.pnlVIDEO.Height) * (double)this.pnlVideoFull.Height);

            int x1 = (int)((double)x0 - 0.5 * (double)newwdt);
            int x2 = (int)((double)x0 + 0.5 * (double)newwdt);
            int y1 = (int)((double)y0 - 0.5 * (double)newhgt);
            int y2 = (int)((double)y0 + 0.5 * (double)newhgt);

            x1 = Math.Max(0, Math.Min(this.pnlVideoFull.Width - 10, x1));
            x2 = Math.Max(x1 + 20, Math.Min(this.pnlVideoFull.Width, x2));
            y1 = Math.Max(0, Math.Min(this.pnlVideoFull.Height - 10, y1));
            y2 = Math.Max(y1 + 20, Math.Min(this.pnlVideoFull.Height, y2));

            if ((x2 - x1) < 20) {
                if (x1 < (int)(0.5 * (double)this.pnlVideoFull.Width)) {
                    x2 = x1 + 20;
                } else {
                    x1 = x2 - 20;
                }
            }
            if ((y2 - y1) < 20) {
                if (y1 < (int)(0.5 * (double)this.pnlVideoFull.Height)) {
                    y2 = y1 + 20;
                } else {
                    y1 = y2 - 20;
                }
            }

            this.pnlVideoZoom.Left = x1;
            this.pnlVideoZoom.Top = y1;
            this.pnlVideoZoom.Width = Math.Max(20, x2 - x1);
            this.pnlVideoZoom.Height = Math.Max(20, y2 - y1);
        }
        private void pnlVIDEO_MouseClick(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("pnlVIDEO_MouseClick");

            //  if (this.pnlVideoFull.Visible) {
            DateTime now = DateTime.Now;
            if ((now - this.lastZoomPan).TotalSeconds < 0.5) { // don't toggle viewer if it was just used  (avoid false clicks)
                return;
            }
            //  }

            if (this.pnlVideoFull.Visible) {
                // hide viewer
                Debug.WriteLine("pnlVIDEO_MouseClick: hide viewer");
                this.pnlVideoFull.Visible = false;
                this.lastZoomPan = DateTime.MinValue;
                this.show_zoom_viewer = false;
            } else {
                // show viewer and lock it
                Debug.WriteLine("pnlVIDEO_MouseClick: SHOW viewer");
                this.lastZoomPan = DateTime.MinValue;
                this.pnlVideoFull.Visible = true;
                this.show_zoom_viewer = true;
            }
        }

        private void pnlVIDEO_DoubleClick(object sender, EventArgs e)
        {
            Debug.WriteLine("pnlVIDEO_DoubleClick");

            // double-click always comes after MouseClick
            // so if that MouseClick hid the viewer, then it was Visble before the click, and probably locked
            //    if it showed the viewer, then it was hidden before the click

            // if it's hidden now, assume it was locked and visible
            // if it's visible now, assume it was hidden



            if (this.pnlVideoFull.Visible) {
                // assume it was hidden and MouseClick (wrongly) showed it and locked it

                this.show_zoom_viewer = false;  // ask bkgnd to hide viewer

                this.lastZoomPan = DateTime.Now;

                this.pnlVideoFull.Visible = true;

            } else {
                // assume it was visble and locked and MouseClick (wrongly) hid it and unlocked it

                this.pnlVideoFull.Visible = true;  // MouseClick just wrongfully hid it, we show it, this causes a nasty flash ;-(

                this.show_zoom_viewer = true;  // lock it
                this.lastZoomPan = DateTime.MinValue;
            }

            this.videoZoomWidth = this.pnlVideoFull.Width;

            int x0 = (int)(0.5 * (double)this.pnlVIDEO.Width);
            int y0 = (int)(0.5 * (double)this.pnlVIDEO.Height);

            this.mouseVideoLocation = new System.Drawing.Point(x0, y0);

            Zoom_Pan();

        }

        private void pnlVIDEO_MouseDown(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("pnlVIDEO_MouseDown");
            this.mouseVideoLocation = new System.Drawing.Point(e.X, e.Y);
            this.dragging_box = true;
        }

        private void pnlVIDEO_MouseMove(object sender, MouseEventArgs e)
        {
            bool displ_hide_controls = true;
            if (always_display_controls) {
                displ_hide_controls = false;
            }

            if (!this.dragging_box && displ_hide_controls) {
                if (e.Y > (this.trackBarPlayHead.Top - 30)) {
                    if (this.trackBarPlayHead.Visible == false) {
                        this.trackBarPlayHead.Visible = true;
                        this.butBegin.Visible = true;
                        this.butSingleBack.Visible = true;
                        this.butPlay.Visible = true;
                        this.butSingleFwd.Visible = true;
                        this.butStop.Visible = true;  // playing ***
                    }
                } else {
                    if (this.trackBarPlayHead.Visible == true) {
                        this.trackBarPlayHead.Visible = false;
                        this.butBegin.Visible = false;
                        this.butSingleBack.Visible = false;
                        this.butPlay.Visible = false;
                        this.butSingleFwd.Visible = false;
                        this.butStop.Visible = false;  // playing ***
                    }
                }
                if (e.Y > (this.trackBarJogShuttle.Top - 30)) {
                    if (this.trackBarJogShuttle.Visible == false) {
                        this.trackBarJogShuttle.Visible = true;
                    }
                } else {
                    if (this.trackBarJogShuttle.Visible == true) {
                        this.trackBarJogShuttle.Visible = false;
                    }
                }
                if (e.X < (this.trackBarSpeed.Width + this.trackBarSpeed.Left + 30)) {
                    if (this.trackBarSpeed.Visible == false) {
                        this.trackBarSpeed.Visible = true;
                    }
                } else {
                    if (this.trackBarSpeed.Visible == true) {
                        this.trackBarSpeed.Visible = false;
                    }
                }
                //  if ((e.X > (this.dataGridView1.Left - 30)) && (e.Y < (this.dataGridView1.Top + this.dataGridView1.Height + 30))) {
                if ((e.X > (this.pnlVIDEO.Width - 30)) && (e.Y < 30)) {
                    if (this.dataGridView1.Visible == false) {
                        this.dataGridView1.Visible = true;
                    }
                } else {
                    if (this.dataGridView1.Visible == true && !this.show_info) {
                        this.dataGridView1.Visible = false;
                    }
                }
            }

            Point new_location = new System.Drawing.Point(e.X, e.Y);

            if (this.dragging_box) {

                if ((Math.Abs(new_location.X - this.mouseVideoLocation.X) < 3) &&
                    (Math.Abs(new_location.Y - this.mouseVideoLocation.Y) < 3)) {
                    this.mouseVideoLocation = new_location;
                    return; // avoid false drag
                }
                this.mouseVideoLocation = new_location;

                Debug.WriteLine("pnlVIDEO_MouseMove");

                this.lastZoomPan = DateTime.Now;

                this.pnlVideoFull.Visible = true;

                Zoom_Pan();

            } else {
                this.mouseVideoLocation = new_location;
            }
        }

        private void pnlVIDEO_MouseUp(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("pnlVIDEO_MouseUp");
            this.dragging_box = false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Title = "Select VIDEO to play";
            this.openFileDialog1.InitialDirectory = "G:\\";

            if (this.openFileDialog1.ShowDialog() == DialogResult.OK) {
                this.video_file = this.openFileDialog1.FileName;

                this.Text = "Richard's VLC Viewer - " + this.video_file;

          //    FileInfo fi = new FileInfo(this.video_file);
          //    this.videoView1.SetMedia(fi);

                this._mp.Media = new Media(this._libVLC, this.video_file);

                this.SIMULATE_PLAY = false;

                var aspect_ratio = GetHeightWidthRatio(this._mp.AspectRatio);
                if (aspect_ratio > 0) {
                    this.good_aspect = true;
                } else {
                    this.good_aspect = false;
                    aspect_ratio = (9.0 / 16.0);
                }

                // 16:9: 0,5625  The standard widescreen format for modern TVs, computer monitors, and YouTube videos.
                // 4:3:  0,75    The older "box-like" standard used for vintage televisions and early computer screens.
                // 1:1:  1,00    A completely square format often used for profile pictures and social media posts.
                // 9:16: 1,7777  A vertical format used for mobile stories and phone

                // less than 0,5 conserve width & adjust height
                // greater than 1.0 conserve height & adjust width

                if (aspect_ratio < 0.6) {
                    this.pnlVideoFull.Width = this.startWidth;
                    this.pnlVideoFull.Height = (int)(aspect_ratio * (double)this.startWidth);
                } else {
                    this.pnlVideoFull.Height = this.startHeight;
                    this.pnlVideoFull.Width = (int)((double)this.startHeight / aspect_ratio);
                }
                this.pnlVideoZoom.Width = this.pnlVideoFull.Width;
                this.pnlVideoZoom.Height = this.pnlVideoFull.Height;

                this.videoZoomWidth = this.pnlVideoZoom.Width;
                this.videoZoomAspect = (double)this.pnlVideoZoom.Width / (double)this.pnlVideoZoom.Height;

                // -------------------------------------

                this.good_framerate = this._mp.Fps > 0.5;

                this.frame_rate = this._mp.Fps <= 0.5 ? 30.0 : this._mp.Fps;  // hopefully this is correct !!
                Update_Value("frame_rate");

                this.good_length = this._mp.Length > 1;

                if (this._mp.Length < 1000) {
                    this.video_length = (int)(this.frame_rate * 60.0 * 5.0);   // assume 5.0 minute long video
                } else {
                    this.video_length = (int)(this.frame_rate * ((double)this._mp.Length / 1000.0));
                }

                //this.current_pos = this.video_length * this._mp.Position;
                //var pos_secs = this.current_pos / this.frame_rate;
                //this.current_time = TimeSpan.FromSeconds((double)this._mp.Time / 1000.0);
                double pos = (double)this.video_length * this._mp.Position;

                trackBarPlayHead.Maximum = this.video_length;

                Set_Current_Pos(pos);

                for (int i = 1; i <= 2; i++) {
                    bool vis = i == 1 ? false : true;
                    this.trackBarPlayHead.Visible = vis;
                    this.butBegin.Visible = vis;
                    this.butSingleBack.Visible = vis;
                    this.butPlay.Visible = vis;
                    this.butSingleFwd.Visible = vis;
                    this.butStop.Visible = vis;  // playing ***
                    this.trackBarJogShuttle.Visible = vis;
                    this.trackBarSpeed.Visible = vis;
                    this.dataGridView1.Visible = vis;
                }
            }
        }


        public double GetHeightWidthRatio(string? AspectRatio)
        {
            const double default_hgt_wdt = 0.0;

            if (string.IsNullOrWhiteSpace(AspectRatio)) {
                return default_hgt_wdt;
            }
            try {
                Match match = Regex.Match(
                    AspectRatio,
                    @"^\s*(\d+)\s*:\s*(\d+)\s*$");

                if (!match.Success)
                    return default_hgt_wdt;

                double width = double.Parse(match.Groups[1].Value);
                double height = double.Parse(match.Groups[2].Value);

                if (width <= 0)
                    return default_hgt_wdt;

                return height / width;
            } catch {
                return default_hgt_wdt;
            }
        }

        // ============================================================================ MediaPlayer Events

        //Public event EncounteredError
        //Public event EndReached
        //Public event Forward
        //Public event LengthChanged
        //Public event MediaChanged
        //Public event Opening
        //Public event PausableChanged
        //Public event Paused
        //Public event Playing
        //Public event PositionChanged
        //Public event ScrambledChanged
        //Public event SeekableChanged
        //Public event SnapshotTaken
        //Public event Stopped
        //Public event TimeChanged
        //Public event TitleChanged
        //Public event VideoOutChanged

    }

}