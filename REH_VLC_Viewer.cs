using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using LibVLCSharp.Shared;

namespace Richard_VLC
{
    public partial class REH_VLC_Viewer : Form
    {
        public enum button_image
        {
            to_begin = 0, play = 1, pause = 2, stop = 3, to_end = 4, single_forw = 5, single_back = 6
        }
        public enum run_state
        {
            simulate, // playhead moves with each tick
            vlc,      // VLC control playhead
            user,     // user controls playhead
            paused    // lock playhead
        }

        public class cVideoDim
        {
            public int height { get; set; } = 0;
            public int width { get; set; } = 0;
            public double hgt_wdt { get; set; } = 0.0;
            public double wdt_hgt { get; set; } = 0.0;

        }
        public cVideoDim video_dimensions = new cVideoDim();
        public double frame_rate = 30.0;  // frames per second
        public long video_length_frames = 0; //  (int)(frame_rate * 60.0 * 5.0);  // frames
        public double video_length_ms = 0;
        public double current_pos = 0.0; // current frame
        public TimeSpan current_time = new TimeSpan(0); // current offset
        public double current_speed = 1.0;

        public bool auto_play_on_load = true;

        public run_state CURRENT_RUN_STATE = run_state.paused;
        public run_state start_run_state = run_state.paused;
        public bool good_aspect = false;  // this.good_aspect = this._mp.AspectRatio?.Contains(":");
        public bool good_framerate = false;  // this.good_framerate = this._mp.Fps > 0.5;
        public bool good_length = false;  // this.good_length = this._mp.Length > 1;

        public double hold_speed = -9999;
        public bool hold_reverse = false;
        public bool hold_playing = false;
        public bool hold_paused = false;
        private DateTime lasttrackBarSpeedMouseDown = DateTime.MinValue;
        private Point initialMouseVideoLocation = new System.Drawing.Point(0, 0);
        private Point initialVideoBoxLocation = new System.Drawing.Point(0, 0);
        private Point mouseVideoLocation = new System.Drawing.Point(0, 0);
        private int videoZoomWidth = 0; // = this.pnlVideoZoom.Width;
        private double videoZoomAspect = 1.0;
        private DateTime lastZoomPan = DateTime.MinValue;
        private DateTime lastTrackSpeed = DateTime.MinValue;
        private DateTime lastInfo = DateTime.MinValue;
        private DateTime lastButtons = DateTime.MinValue;
        private DateTime lastJogShuttle = DateTime.MinValue;
        private DateTime lastPlayHead = DateTime.MinValue;

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
        public bool show_track_speed = false;
        public bool show_buttons = false;
        public bool show_jog_shuttle = false;
        public bool show_play_head = false;

        public bool lock_zoom_viewer = false;
        public bool lock_info = false;
        public bool lock_track_speed = false;
        public bool lock_buttons = false;
        public bool lock_jog_shuttle = false;
        public bool lock_play_head = true;

        public bool dragging_box = false;

        public string video_file = "";

        public bool isFullscreen = false;
        public bool isPlaying = false;
        public Size oldVideoSize;
        public Size oldFormSize;
        public Point oldVideoLocation;
        private bool _updatingPlayHead = false;

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

                    if (this.CURRENT_RUN_STATE == run_state.paused) continue;

                    double pos = this.current_pos;
                    TimeSpan tpos = this.current_time;

                    if (this.CURRENT_RUN_STATE == run_state.simulate) {

                        // ------------------------------------------------------ we move the playhead on each tick

                        if (this.single_framing_forward) {
                            pos += 0.5;  // 1/2 frame per 500 ms
                            tpos = tpos.Add(TimeSpan.FromMilliseconds(500));
                        } else if (this.single_framing_backward) {
                            pos -= 0.5;
                            tpos = tpos.Add(TimeSpan.FromMilliseconds(-500));
                        } else if (this.playing && !this.paused) {
                            double fr = 0.5 * (this.frame_rate * this.current_speed);
                            if (this.reverse_motion) {
                                pos -= fr;
                                tpos = TimeSpan.FromSeconds(pos / this.frame_rate);
                            } else {
                                pos += fr;
                                tpos = TimeSpan.FromSeconds(pos / this.frame_rate);
                            }
                        }
                    } else if (this.CURRENT_RUN_STATE == run_state.vlc) {

                        // ------------------------------------------------------ we fetch the state from VLC and update the interface

                        string AspectRatio = (this.good_aspect ? "" : this._mp?.AspectRatio ?? "");
                        float Fps = (this.good_framerate ? (float)this.frame_rate : this._mp?.Fps ?? 0);
                        long Length = (this.good_length ? (long)this.video_length_ms : this._mp?.Length ?? 0);

                        fetch_video_properties(AspectRatio,
                                               Fps,
                                               Length);

                        // this.current_pos = this.video_length * this._mp.Position;
                        // var pos_secs = this.current_pos / this.frame_rate;
                        // this.current_time = TimeSpan.FromSeconds((double)this._mp.Time / 1000.0);

                        if (this.good_length) {
                            pos = (double)this.video_length_frames * (this._mp?.Position ?? 0.0);
                        }
                        long time_ms = this._mp?.Time ?? -1;
                        if (time_ms < 0) {
                            if (this.good_framerate) {
                                tpos = TimeSpan.FromSeconds(pos / this.frame_rate);
                            } else {
                                tpos = new TimeSpan(0); // only know relative-position
                            }
                        } else {
                            tpos = TimeSpan.FromMilliseconds(time_ms);
                        }

                    } else if (this.CURRENT_RUN_STATE == run_state.paused) {

                        // ------------------------------------------------------ user is controlling playhead
                    }

                    Set_Current_Pos(pos,tpos, update_playhead: true, seek_in_video: false);

                    if (!this.show_zoom_viewer && this.pnlVideoFull.Visible) {
                        DateTime now = DateTime.Now;
                        if ((now - this.lastZoomPan).TotalSeconds > 0.5) { // hide after N secs
                            this.pnlVideoFull.Visible = false;
                        }
                    }

                    if (!lock_track_speed && !this.show_track_speed && this.trackBarSpeed.Visible) {
                        DateTime now = DateTime.Now;
                        if ((now - this.lastTrackSpeed).TotalSeconds > 0.5) { // hide after N secs
                            this.trackBarSpeed.Visible = false;
                        }
                    }
                    if (!lock_info && !this.show_info && this.dataGridView1.Visible) {
                        DateTime now = DateTime.Now;
                        if ((now - this.lastInfo).TotalSeconds > 0.5) { // hide after N secs
                            this.dataGridView1.Visible = false;
                        }
                    }
                    if (!lock_jog_shuttle && !this.show_jog_shuttle && this.trackBarJogShuttle.Visible) {
                        DateTime now = DateTime.Now;
                        if ((now - this.lastJogShuttle).TotalSeconds > 0.5) { // hide after N secs
                            this.trackBarJogShuttle.Visible = false;
                        }
                    }
                    if (!lock_play_head && !this.show_play_head && this.trackBarPlayHead.Visible) {
                        DateTime now = DateTime.Now;
                        if ((now - this.lastPlayHead).TotalSeconds > 0.5) { // hide after N secs
                            this.trackBarPlayHead.Visible = false;
                        }
                    }
                }
            } catch (OperationCanceledException) {
                // Normal shutdown.
            }
        }

        private void fetch_video_properties(string? AspectRatio, float fps, long length_ms)
        {
            if (!this.good_aspect) {

                Debug.WriteLine($"fetch_video_properties: AspectRatio [{AspectRatio}]");

                this.video_dimensions = GetHeightWidthRatio(AspectRatio);

                Update_Value("aspect_ratio");

                var aspect_ratio = this.video_dimensions.hgt_wdt;

                if (this.video_dimensions.hgt_wdt > 0) {
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
            if (!this.good_framerate) {

                Debug.WriteLine($"fetch_video_properties: fps [{fps}]");

                this.good_framerate = fps > 0.5;
                if (good_framerate) {
                    this.frame_rate = fps;
                    Update_Value("frame_rate");
                    if (this.good_length) {
                        // we are receiving the length in milliseconds way before frame-rate
                        // (re)-calculate length in frames when we know both
                        this.video_length_frames = (int)(this.frame_rate * ((double)length_ms / 1000.0));
                        trackBarPlayHead.Maximum = (int)this.video_length_frames;
                        Update_Value("length");
                    }
                }
            }
            if (!this.good_length) {

                Debug.WriteLine($"fetch_video_properties: length [{length_ms}]");

                this.good_length = length_ms > 1;
                if (this.good_length) {
                    this.video_length_ms = length_ms;
                    if (this.good_framerate) {
                        this.video_length_frames = (int)(this.frame_rate * ((double)length_ms / 1000.0));
                        trackBarPlayHead.Maximum = (int)this.video_length_frames;
                        Update_Value("length");
                    }
                }
            }
        }

        private void butBegin_Click(object sender, EventArgs e)
        {
            if (this.CURRENT_RUN_STATE == run_state.vlc) {
                this.videoView1?.MediaPlayer?.SeekTo(new TimeSpan(0));
            }
            Set_Current_Pos(0.0,new TimeSpan(0), update_playhead: true, seek_in_video: true);
        }

        private void butPlay_Click(object sender, EventArgs e)
        {

            if (this.playing && !this.paused) {
                // playing   Play => PAUSE
                if (this.CURRENT_RUN_STATE == run_state.vlc) {
                    this.videoView1?.MediaPlayer?.Pause();
                }

                this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
                this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
                this.paused = true;
            } else {
                // not playing   Play => PLAY
                if (this.CURRENT_RUN_STATE == run_state.vlc) {
                    this.videoView1?.MediaPlayer?.Play();
                }

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
            if (this.CURRENT_RUN_STATE == run_state.vlc) {
                this.videoView1?.MediaPlayer?.Stop();
            }

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
                if (this.CURRENT_RUN_STATE == run_state.vlc) {
                    this.videoView1?.MediaPlayer?.Pause();
                }
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
            this.start_run_state = this.CURRENT_RUN_STATE;
            this.CURRENT_RUN_STATE = run_state.simulate;
            this.single_framing_backward = true;
            Update_Value("mode");

            if (Math.Abs(this.current_pos) < 0.5) {
                return;
            }
            if (this.current_pos <= 0) {
                Set_Current_Pos(0.0,new TimeSpan(0), update_playhead: true, seek_in_video: true);
                return;
            }
            double pos = this.current_pos - 1.0;
            TimeSpan tpos = new TimeSpan(0);
            if (this.frame_rate > 0)
                tpos = TimeSpan.FromSeconds(pos / this.frame_rate);

            Set_Current_Pos(pos,tpos, update_playhead: true, seek_in_video: true);
        }

        private void butSingleBack_MouseUp(object sender, MouseEventArgs e)
        {
            this.CURRENT_RUN_STATE = this.start_run_state;
            this.single_framing_backward = false;
            Update_Value("mode");
        }

        private void butSingleFwd_Click(object sender, EventArgs e)
        {
        }
        private void butSingleFwd_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.playing && !this.paused) {
                // playing   Play => PAUSE
                if (this.CURRENT_RUN_STATE == run_state.vlc) {
                    this.videoView1?.MediaPlayer?.Pause();
                }
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
            this.start_run_state = this.CURRENT_RUN_STATE;
            this.CURRENT_RUN_STATE = run_state.simulate;
            this.single_framing_forward = true;
            Update_Value("mode");

            if ((int)this.current_pos >= this.video_length_frames) {
                return;
            }
            // *** for moving forward one frame, there is a command
            // *** that we could use INSTEAD of calculating pos & tpos
            double pos = this.current_pos + 1.0;
            TimeSpan tpos = new TimeSpan(0);
            if (this.frame_rate > 0)
                tpos = TimeSpan.FromSeconds(pos / this.frame_rate);

            Set_Current_Pos(pos, tpos, update_playhead: true, seek_in_video: true);
        }

        private void butSingleFwd_MouseUp(object sender, MouseEventArgs e)
        {
            this.CURRENT_RUN_STATE = this.start_run_state;
            this.single_framing_forward = false;
            Update_Value("mode");
        }

        private void trackBarPlayHead_Scroll(object sender, EventArgs e)
        {
            if (_updatingPlayHead) return;

            this.start_run_state = this.CURRENT_RUN_STATE;
            this.CURRENT_RUN_STATE = run_state.paused; // disable background while scrolling... we are controlling playhead + vlc

            System.Windows.Forms.TrackBar myTB = (System.Windows.Forms.TrackBar)sender;

            Debug.WriteLine($"trackBarPlayHead_Scroll: FRAME {myTB.Value.ToString()} - SECONDS {current_time.ToString()}");

            double pos = (double)myTB.Value;

            TimeSpan tpos = TimeSpan.FromSeconds(pos / this.frame_rate);

            Set_Current_Pos(pos, tpos, update_playhead: false, seek_in_video: true);

            this.CURRENT_RUN_STATE = this.start_run_state;

            Update_Value("current_pos");
            Update_Value("current_time");

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

        //private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        //{
        //    Debug.WriteLine("dataGridView1_MouseClick");
        //    this.show_info = !this.show_info;
        //    this.lock_info = true;
        //}

        private void Set_Current_Pos(double pos, TimeSpan tpos, bool update_playhead = true, bool seek_in_video = true)
        {
            if (this.video_length_frames <= 0 || this.frame_rate <= 0) return;

            this.current_pos = Math.Min(Math.Max(0.0, pos), (double)this.video_length_frames);
            if (tpos.TotalMilliseconds <= 0 && pos > 0.0) {
                this.current_time = TimeSpan.FromSeconds((double)this.current_pos / this.frame_rate);
            } else {
                this.current_time = tpos;
            }

            if (seek_in_video) {
                if (this.start_run_state == run_state.vlc) {
                    this.videoView1?.MediaPlayer?.SeekTo(this.current_time);
                }
            }
            if (update_playhead) {
                this._updatingPlayHead = true;
                this.trackBarPlayHead.Value = (int)Math.Min(Math.Max((double)this.trackBarPlayHead.Minimum, pos), (double)this.trackBarPlayHead.Maximum);
                this._updatingPlayHead = false;
            }

            Update_Value("current_pos");
            Update_Value("current_time");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Load_Buttons();

            Init_Data();

            this.startWidth = this.pnlVideoFull.Width;
            this.startHeight = this.pnlVideoFull.Height;

            var aspect_ratio = string.Format("{0}:{1}", this.pnlVIDEO.Width, this.pnlVIDEO.Height);

            fetch_video_properties(aspect_ratio, 30, (int)((5.0 * 60.0) * 1000.0)); // assume 5.0 minute long video @ 30 fps

            this.good_aspect = false;
            this.good_framerate = false;
            this.good_length = false;

            double targetY = 1.0;
            this.speed_1 = Math.Log(targetY / 0.004) / Math.Log(1000);  // a = 0.004  b = 1000
            this.speed_ff = Math.Log((1.0 / this.frame_rate) / 0.004) / Math.Log(1000);  // a = 0.004  b = 1000

            trackBarSpeed.Value = (int)(speed_ff * (double)trackBarSpeed.Maximum);  // set marker at 1 second per frame
            trackBarSpeed.Value = (int)(speed_1 * (double)trackBarSpeed.Maximum);   // set marker at 100% speed
            trackBarPlayHead.Maximum = (int)this.video_length_frames;

            trackBarJogShuttle.Minimum = -50;
            trackBarJogShuttle.Maximum = 50;
            trackBarJogShuttle.Value = 0;

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
            this.toolTip1.SetToolTip(this.pnlVideoFull, "Thumbnail of source video to set Zoom / Pan");
            this.toolTip1.SetToolTip(this.dataGridView1, "Technical details");
            this.toolTip1.SetToolTip(this.trackBarJogShuttle, "Hold and drag to Scroll left or right, let go to resume.");
            this.toolTip1.SetToolTip(this.butBegin, "Goto Start of Video");
            this.toolTip1.SetToolTip(this.butSingleBack, "Hold-down to move back frame by frame.");
            this.toolTip1.SetToolTip(this.butSingleFwd, "Hold-down to move forward frame by frame.");
            this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
            this.toolTip1.SetToolTip(this.butStop, "Click to STOP");

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

            this.show_track_speed = true;
            this.trackBarSpeed.Visible = this.show_track_speed;  // speed control on left
            this.lastTrackSpeed = DateTime.MinValue;

            this.show_zoom_viewer = false;
            this.pnlVideoFull.Visible = this.show_zoom_viewer;  // zoom/pan viewer
            this.lastZoomPan = DateTime.MinValue;

            this.show_info = false;
            this.dataGridView1.Visible = this.show_info;  // information
            this.lastInfo = DateTime.MinValue;

            this.show_buttons = true;
            this.butBegin.Visible = this.show_buttons;
            this.butSingleBack.Visible = this.show_buttons;
            this.butPlay.Visible = this.show_buttons;
            this.butSingleFwd.Visible = this.show_buttons;
            this.butStop.Visible = this.show_buttons & false;
            this.lastButtons = DateTime.MinValue;

            this.show_jog_shuttle = true;
            this.trackBarJogShuttle.Visible = this.show_jog_shuttle;  // jog-shuttle
            this.lastJogShuttle = DateTime.MinValue;

            this.show_play_head = true;
            this.trackBarPlayHead.Visible = this.show_play_head;    // play-head
            this.lastPlayHead = DateTime.MinValue;

            _ = RunBackgroundTaskAsync();

            this.start_run_state = run_state.simulate;
            this.CURRENT_RUN_STATE = run_state.simulate;
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
            string[] keys = { "current_pos", "current_time", "current_speed", "frame_rate", "width", "height", "length", "reverse_motion", "mode" };
            string[] key_names = { "current_pos", "current_time", "current_speed", "frame_rate", "width", "height", "length", "reverse_motion", "mode" };
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
                    case "width":
                    case "height":
                        drNew["Units"] = "pixels";
                        break;
                    case "length":
                        drNew["Units"] = "total number of frames (est)";
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
                        case "aspect_ratio":
                            Update_Value("width");
                            Update_Value("height");
                            break;
                        case "width":
                            row["Value"] = this.video_dimensions.width;
                            break;
                        case "height":
                            row["Value"] = this.video_dimensions.height;
                            break;
                        case "length":
                            row["Value"] = this.video_length_frames;
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

            this.pnlVideoFull.Visible = this.show_zoom_viewer;

            int numberOfClicks = e.Delta * SystemInformation.MouseWheelScrollLines;

            Debug.WriteLine($"pnlVIDEO_MouseWheel: {numberOfClicks}");

            int newwdt = this.videoZoomWidth;

            if (numberOfClicks < 0) {
                newwdt += (int)((double)this.pnlVideoFull.Width * 0.03);
            } else {
                newwdt -= (int)((double)this.pnlVideoFull.Width * 0.03);
            }
            newwdt = Math.Max(20, Math.Min(this.pnlVideoFull.Width, newwdt));

            this.videoZoomWidth = newwdt;

            Zoom_Pan();
        }

        public void Zoom_Full()
        {
            this.videoZoomWidth = this.pnlVideoFull.Width;

            if (this._mp != null && this.video_dimensions.width > 0) {
                int cropX = 0;
                int cropY = 0;
                int cropWidth = this.video_dimensions.width;
                int cropHeight = (int)((double)cropWidth * this.video_dimensions.hgt_wdt);

                string geometryString = $"{cropWidth + cropX}x{cropHeight + cropY}+{cropX}+{cropY}";

                Debug.WriteLine($"ZOOM-PAN: {geometryString}");

                this._mp.CropGeometry = geometryString;
            }
            this.pnlVideoZoom.Left = 0;
            this.pnlVideoZoom.Top = 0;
            this.pnlVideoZoom.Width = this.pnlVideoFull.Width;
            this.pnlVideoZoom.Height = this.pnlVideoFull.Height;
        }

        public void Zoom_Pan()
        {
            int newwdt = Math.Max(20, Math.Min(this.pnlVideoFull.Width, this.videoZoomWidth));

            int newhgt = (int)((double)newwdt / this.videoZoomAspect);

            //  newhgt = Math.Max(20, Math.Min(this.pnlVideoFull.Height, newhgt));

            int x0 = 0; // center x
            int y0 = 0; // center y

            int x1 = 0; // left
            int x2 = 0; // right
            int y1 = 0; // left
            int y2 = 0; // right

            if (this.dragging_box) {
                // this.initialMouseVideoLocation = this.mouseVideoLocation;
                // this.initialVideoBoxLocation = new System.Drawing.Point(this.pnlVideoZoom.Left, this.pnlVideoZoom.Top);

                int deltax = (int)(((double)(this.mouseVideoLocation.X - this.initialMouseVideoLocation.X) / (double)this.pnlVIDEO.Width) * (double)this.pnlVideoFull.Width);
                int deltay = (int)(((double)(this.mouseVideoLocation.Y - this.initialMouseVideoLocation.Y) / (double)this.pnlVIDEO.Height) * (double)this.pnlVideoFull.Height);
                x0 = (int)((double)(this.initialVideoBoxLocation.X + deltax) + 0.5 * (double)newwdt);
                y0 = (int)((double)(this.initialVideoBoxLocation.Y + deltay) + 0.5 * (double)newhgt);

            } else {
                x0 = (int)(((double)this.mouseVideoLocation.X / (double)this.pnlVIDEO.Width) * (double)this.pnlVideoZoom.Width);
                y0 = (int)(((double)this.mouseVideoLocation.Y / (double)this.pnlVIDEO.Height) * (double)this.pnlVideoZoom.Height);
                // when zooming in/out, if user points to center of thier screen, x0 is not in the middle of pnlVideoFull
                // they are pointing to center of pnlVideoZoom
                x0 += this.pnlVideoZoom.Left;
                y0 += this.pnlVideoZoom.Top;
                // however, after zoomin in, they expect the object they were point to to end up where their cursor is pointing
                // the zoom is almost guaranteed to shift this object
            }
            x1 = (int)((double)x0 - 0.5 * (double)newwdt);
            x2 = (int)((double)x0 + 0.5 * (double)newwdt);
            y1 = (int)((double)y0 - 0.5 * (double)newhgt);
            y2 = (int)((double)y0 + 0.5 * (double)newhgt);

            for (int i = 1; i <= 2; i++) {
                if (x1 < 0) {
                    x1 = 0;
                    x2 = newwdt;
                } else if (x2 > this.pnlVideoFull.Width) {
                    x1 = this.pnlVideoFull.Width - newwdt;
                    x2 = this.pnlVideoFull.Width;
                }
                if (y1 < 0) {
                    y1 = 0;
                    y2 = newhgt;
                } else if (y2 > this.pnlVideoFull.Height) {
                    y1 = this.pnlVideoFull.Height - newhgt;
                    y2 = this.pnlVideoFull.Height;
                }

                if (i == 1) {
                    if (this.dragging_box) 
                        break;
                    int x0new = (int)(((double)this.mouseVideoLocation.X / (double)this.pnlVIDEO.Width) * (double)newwdt);
                    int y0new = (int)(((double)this.mouseVideoLocation.Y / (double)this.pnlVIDEO.Height) * (double)newhgt);
                    // when zooming in/out, if user points to center of thier screen, x0 is not in the middle of pnlVideoFull
                    // they are pointing to center of pnlVideoZoom
                    x0new += x1;
                    y0new += y1;
                    int xdelta = x0 - x0new;
                    int ydelta = y0 - y0new;
                    x1 += xdelta;  x2 += xdelta;
                    y1 += ydelta;  y2 += ydelta;
                }
            }

            //x1 = Math.Max(0, Math.Min(this.pnlVideoFull.Width - 10, x1));
            //x2 = Math.Max(x1 + 20, Math.Min(this.pnlVideoFull.Width, x2));
            //y1 = Math.Max(0, Math.Min(this.pnlVideoFull.Height - 10, y1));
            //y2 = Math.Max(y1 + 20, Math.Min(this.pnlVideoFull.Height, y2));

            //if ((x2 - x1) < 20) {
            //    if (x1 < (int)(0.5 * (double)this.pnlVideoFull.Width)) {
            //        x2 = x1 + 20;
            //    } else {
            //        x1 = x2 - 20;
            //    }
            //}
            //if ((y2 - y1) < 20) {
            //    if (y1 < (int)(0.5 * (double)this.pnlVideoFull.Height)) {
            //        y2 = y1 + 20;
            //    } else {
            //        y1 = y2 - 20;
            //    }
            //}

            if (this._mp != null && this.video_dimensions.width > 0) {
                //int x = (int)((double)x0 - 0.5 * (double)newwdt);
                //int y = (int)((double)y0 - 0.5 * (double)newhgt);
                int x = x1;
                int y = y1;
                int cropX = (int)(((double)x / (double)this.pnlVideoFull.Width) * (double)this.video_dimensions.width);
                int cropY = (int)(((double)y / (double)this.pnlVideoFull.Height) * (double)this.video_dimensions.height);
                int cropWidth = (int)(((double)newwdt / (double)this.pnlVideoFull.Width) * (double)this.video_dimensions.width);
                int cropHeight = (int)((double)cropWidth * this.video_dimensions.hgt_wdt);

                cropWidth = Math.Min(this.video_dimensions.width, Math.Max(20, cropWidth));
                cropHeight = Math.Min(this.video_dimensions.height, Math.Max(20, cropHeight));
                //cropX = Math.Min(this.video_dimensions.width - cropWidth, Math.Max(0, cropX));
                //cropY = Math.Min(this.video_dimensions.height - cropHeight, Math.Max(0, cropY));
                //cropX = Math.Min(this.video_dimensions.width - 1, Math.Max(0, cropX));
                //cropY = Math.Min(this.video_dimensions.height - 1, Math.Max(0, cropY));

                // Construct the standard geometry string
                // Formats to: "800x600+100+50"   right x bottom + left + top
                string geometryString = $"{cropWidth + cropX}x{cropHeight + cropY}+{cropX}+{cropY}";

                Debug.WriteLine($"ZOOM-PAN: {geometryString}");

                this._mp.CropGeometry = geometryString;
            }

            this.pnlVideoZoom.Left = x1;
            this.pnlVideoZoom.Top = y1;
            this.pnlVideoZoom.Width = Math.Max(20, x2 - x1);
            this.pnlVideoZoom.Height = Math.Max(20, y2 - y1);
        }
        private void pnlVideoFull_MouseClick(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("pnlVideoFull_MouseClick");

            //  if (this.pnlVideoFull.Visible) {
            DateTime now = DateTime.Now;
            if ((now - this.lastZoomPan).TotalSeconds < 0.5) { // don't toggle viewer if it was just used  (avoid false clicks)
                return;
            }
            //  }

            if (this.pnlVideoFull.Visible) {
                // hide viewer
                Debug.WriteLine("pnlVideoFull_MouseClick: hide viewer");
                this.pnlVideoFull.Visible = false;
                this.lastZoomPan = DateTime.MinValue;
                this.show_zoom_viewer = false;
            } else {
                // show viewer and lock it
                Debug.WriteLine("pnlVideoFull_MouseClick: SHOW viewer");
                this.lastZoomPan = DateTime.MinValue;
                this.pnlVideoFull.Visible = true;
                this.show_zoom_viewer = true;
            }
        }

        private void pnlVIDEO_DoubleClick(object sender, EventArgs e)
        {
            Debug.WriteLine("pnlVIDEO_DoubleClick");

            Zoom_Full();

            // double-click always comes after MouseClick
            // so if that MouseClick hid the viewer, then it was Visble before the click, and probably locked
            //    if it showed the viewer, then it was hidden before the click

            // if it's hidden now, assume it was locked and visible
            // if it's visible now, assume it was hidden



            //if (this.pnlVideoFull.Visible) {
            //    // assume it was hidden and MouseClick (wrongly) showed it and locked it

            //    this.show_zoom_viewer = false;  // ask bkgnd to hide viewer

            //    this.lastZoomPan = DateTime.Now;

            //    this.pnlVideoFull.Visible = true;

            //} else {
            //    // assume it was visble and locked and MouseClick (wrongly) hid it and unlocked it

            //    this.pnlVideoFull.Visible = true;  // MouseClick just wrongfully hid it, we show it, this causes a nasty flash ;-(

            //    this.show_zoom_viewer = true;  // lock it
            //    this.lastZoomPan = DateTime.MinValue;
            //}

            //this.videoZoomWidth = this.pnlVideoFull.Width;

            //int x0 = (int)(0.5 * (double)this.pnlVIDEO.Width);
            //int y0 = (int)(0.5 * (double)this.pnlVIDEO.Height);

            //this.mouseVideoLocation = new System.Drawing.Point(x0, y0);

            //this.dragging_box = false;

        }

        private void pnlVIDEO_MouseDown(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("pnlVIDEO_MouseDown");
            this.mouseVideoLocation = new System.Drawing.Point(e.X, e.Y);
            this.initialMouseVideoLocation = new System.Drawing.Point(e.X, e.Y);
            //int x0 = (int)(((double)this.pnlVideoZoom.Left / (double)this.pnlVideoFull.Width) * (double)this.pnlVIDEO.Width);
            //int y0 = (int)(((double)this.pnlVideoZoom.Top / (double)this.pnlVideoFull.Height) * (double)this.pnlVIDEO.Height);
            //this.initialVideoBoxLocation = new System.Drawing.Point(x0, y0);
            this.initialVideoBoxLocation = new System.Drawing.Point(this.pnlVideoZoom.Left, this.pnlVideoZoom.Top);
            this.dragging_box = true;
        }

        private void pnlVIDEO_MouseMove(object sender, MouseEventArgs e)
        {
            if (!this.dragging_box) {
                if ((e.X < (this.trackBarSpeed.Width + 10)) && (e.Y <= this.trackBarSpeed.Height)) {
                    if ((e.Y > 200) && (e.Y < this.trackBarSpeed.Height - 100)) {
                        if (!this.trackBarSpeed.Visible)
                            this.trackBarSpeed.Visible = true;
                        this.show_track_speed = true;
                    }
                    this.lastTrackSpeed = DateTime.Now;

                    this.show_info = false;
                    this.show_jog_shuttle = false;
                    this.show_play_head = false;
                } else if ((e.X > (pnlVIDEO.Width - this.dataGridView1.Width - 15)) && (e.Y <= this.dataGridView1.Height)) {
                    if ((e.X > (pnlVIDEO.Width - 100)) && (e.Y < 100)) {
                        if (!this.dataGridView1.Visible)
                            this.dataGridView1.Visible = true;
                        this.show_info = true;
                    }
                    this.lastInfo = DateTime.Now;

                    this.show_track_speed = false;
                    this.show_jog_shuttle = false;
                    this.show_play_head = false;
                } else if ((e.X > this.trackBarPlayHead.Left) && (e.Y > (trackBarJogShuttle.Top - 0))) {
                    if ((e.X > (this.trackBarPlayHead.Left + 100)) && (e.X < (pnlVIDEO.Width - 100))) {
                        if (!this.trackBarJogShuttle.Visible)
                            this.trackBarJogShuttle.Visible = true;
                        this.show_jog_shuttle = true;
                        this.lastJogShuttle = DateTime.Now;
                    }
                    if (e.Y > (trackBarPlayHead.Top - 100)) {
                        if (!this.trackBarPlayHead.Visible)
                            this.trackBarPlayHead.Visible = true;
                        this.show_play_head = true;
                        this.lastPlayHead = DateTime.Now;
                    }

                    this.show_track_speed = false;
                    this.show_info = false;
                } else {
                    this.show_track_speed = false;
                    this.show_info = false;
                    this.show_jog_shuttle = false;
                    this.show_play_head = false;
                }
            }

            //if (always_display_controls) {
            //    displ_hide_controls = false;
            //}

            //if (!this.dragging_box && displ_hide_controls) {
            //    if (e.Y > (this.trackBarPlayHead.Top - 30)) {
            //        if (this.trackBarPlayHead.Visible == false) {
            //            this.trackBarPlayHead.Visible = true;
            //            this.butBegin.Visible = true;
            //            this.butSingleBack.Visible = true;
            //            this.butPlay.Visible = true;
            //            this.butSingleFwd.Visible = true;
            //            this.butStop.Visible = true;  // playing ***
            //        }
            //    } else {
            //        if (this.trackBarPlayHead.Visible == true) {
            //            this.trackBarPlayHead.Visible = false;
            //            this.butBegin.Visible = false;
            //            this.butSingleBack.Visible = false;
            //            this.butPlay.Visible = false;
            //            this.butSingleFwd.Visible = false;
            //            this.butStop.Visible = false;  // playing ***
            //        }
            //    }
            //    if (e.Y > (this.trackBarJogShuttle.Top - 30)) {
            //        if (this.trackBarJogShuttle.Visible == false) {
            //            this.trackBarJogShuttle.Visible = true;
            //        }
            //    } else {
            //        if (this.trackBarJogShuttle.Visible == true) {
            //            this.trackBarJogShuttle.Visible = false;
            //        }
            //    }
            //    if (e.X < (this.trackBarSpeed.Width + this.trackBarSpeed.Left + 30)) {
            //        if (this.trackBarSpeed.Visible == false) {
            //            this.trackBarSpeed.Visible = true;
            //        }
            //    } else {
            //        if (this.trackBarSpeed.Visible == true) {
            //            this.trackBarSpeed.Visible = false;
            //        }
            //    }
            //    //  if ((e.X > (this.dataGridView1.Left - 30)) && (e.Y < (this.dataGridView1.Top + this.dataGridView1.Height + 30))) {
            //    if ((e.X > (this.pnlVIDEO.Width - 30)) && (e.Y < 30)) {
            //        if (this.dataGridView1.Visible == false) {
            //            this.dataGridView1.Visible = true;
            //        }
            //    } else {
            //        if (this.dataGridView1.Visible == true && !this.show_info) {
            //            this.dataGridView1.Visible = false;
            //        }
            //    }
            //}

            Point new_location = new System.Drawing.Point(e.X, e.Y);

            if (this.dragging_box) {

                //if ((Math.Abs(new_location.X - this.mouseVideoLocation.X) < 3) &&
                //    (Math.Abs(new_location.Y - this.mouseVideoLocation.Y) < 3)) {
                //    this.mouseVideoLocation = new_location;
                //    return; // avoid false drag
                //}
                this.mouseVideoLocation = new_location;

                Debug.WriteLine("pnlVIDEO_MouseMove");

                this.lastZoomPan = DateTime.Now;

                this.pnlVideoFull.Visible = this.show_zoom_viewer;

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

        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Title = "Select VIDEO to play";
            this.openFileDialog1.InitialDirectory = "G:\\";

            if (this.openFileDialog1.ShowDialog() == DialogResult.OK) {

                this.video_file = this.openFileDialog1.FileName;

                this.Text = "Richard's VLC Viewer - " + this.video_file;

                if (this.playing && !this.paused) {
                    this.videoView1?.MediaPlayer?.Stop();
                }

                this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
                this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
                this.playing = false;
                this.paused = false;
                this.single_framing_forward = false;
                this.single_framing_backward = false;

                Update_Value("mode");
                Update_Value("reverse_motion");

                this.butStop.Visible = false;

                //    FileInfo fi = new FileInfo(this.video_file);
                //    this.videoView1.SetMedia(fi);


                using var media = new Media(this._libVLC, this.video_file);


                this.start_run_state = run_state.paused;
                this.CURRENT_RUN_STATE = run_state.paused;  // VLC owns playhead

                // VIDEO properties are usually empty until video actually starts playing

                this.good_aspect = false;
                this.good_framerate = false;
                this.good_length = false;

                string AspectRatio = ""; // this._mp?.AspectRatio ?? "";
                long length = 0; //         this._mp?.Length ?? 0;
                float fps = 0; //           this._mp?.Fps ?? 0;

                await media.Parse(MediaParseOptions.ParseLocal, -1, CancellationToken.None); // MediaPlayer usually doe not return any info.  fetch it by hand

                if (string.IsNullOrWhiteSpace(AspectRatio)) {
                    var tracks = media.Tracks;
                    foreach (var track in tracks) {
                        if (track.TrackType == TrackType.Video) {
                            AspectRatio = string.Format("{0}:{1}", track.Data.Video.Width, track.Data.Video.Height);
                            break;
                        }
                    }
                }
                if (length <= 0) {
                    length = media.Duration;
                }

                fetch_video_properties(AspectRatio, fps, length);

                if (!this.good_aspect) {
                    // AspectRatio unknown -- assume 16:9

                    var aspect_ratio = (9.0 / 16.0);

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
                }
                this.pnlVideoZoom.Width = this.pnlVideoFull.Width;
                this.pnlVideoZoom.Height = this.pnlVideoFull.Height;

                this.videoZoomWidth = this.pnlVideoZoom.Width;
                this.videoZoomAspect = (double)this.pnlVideoZoom.Width / (double)this.pnlVideoZoom.Height;

                // -------------------------------------

                if (!this.good_framerate) {
                    // invent a frame-rate, but don't set good_framerate = true
                    this.frame_rate = 30.0;  // hopefully this is correct !!
                    Update_Value("frame_rate");
                }

                if (!this.good_length) {
                    // invent a length, but don't set good_length = true
                    this.video_length_frames = (int)(this.frame_rate * 60.0 * 5.0);   // assume 5.0 minute long video
                    trackBarPlayHead.Maximum = (int)this.video_length_frames;
                    Update_Value("length");
                }

                //this.current_pos = this.video_length * this._mp.Position;
                //var pos_secs = this.current_pos / this.frame_rate;
                //this.current_time = TimeSpan.FromSeconds((double)this._mp.Time / 1000.0);

                double pos = 0;  // assume playhead is @ beginning

                Set_Current_Pos(pos,new TimeSpan(0), update_playhead: true, seek_in_video: false);

                //for (int i = 1; i <= 2; i++) {
                //    bool vis = i == 1 ? false : true;
                //    this.trackBarPlayHead.Visible = vis;
                //    this.butBegin.Visible = vis;
                //    this.butSingleBack.Visible = vis;
                //    this.butPlay.Visible = vis;
                //    this.butSingleFwd.Visible = vis;
                //    this.butStop.Visible = vis;  // playing ***
                //    this.trackBarJogShuttle.Visible = vis;
                //    this.trackBarSpeed.Visible = vis;
                //    this.dataGridView1.Visible = vis;
                //}

                Zoom_Full();

                this.start_run_state = run_state.vlc;
                this.CURRENT_RUN_STATE = run_state.vlc;  // VLC owns playhead (*** should wait until video starts playing)

                this._mp?.Media = media;  // now assign Media object to Player

                if (auto_play_on_load) {
                    
                    this.videoView1?.MediaPlayer?.Play();

                    this.butPlay.Image = this.imageList1.Images[(int)button_image.pause];
                    this.toolTip1.SetToolTip(this.butPlay, "Click to PAUSE");
                    this.butStop.Visible = true;
                    this.playing = true;
                    this.paused = false;
                    this.reverse_motion = false;
                    Update_Value("mode");
                    Update_Value("reverse_motion");
                }
            }
        }


        public cVideoDim GetHeightWidthRatio(string? AspectRatio)
        {
            cVideoDim video_dim = new cVideoDim();

            if (string.IsNullOrWhiteSpace(AspectRatio)) {
                return video_dim;
            }
            try {
                Match match = Regex.Match(
                    AspectRatio,
                    @"^\s*(\d+)\s*:\s*(\d+)\s*$");

                if (!match.Success)
                    return video_dim;

                double width = double.Parse(match.Groups[1].Value);
                double height = double.Parse(match.Groups[2].Value);

                if ((width <= 1.0) || (height <= 1.0))
                    return video_dim;

                video_dim.height = (int)height;
                video_dim.width = (int)width;

                video_dim.hgt_wdt = height / width;
                video_dim.wdt_hgt = width / height;
            } catch {
                video_dim.height = video_dim.width = 0;
                video_dim.hgt_wdt = video_dim.wdt_hgt = 0.0;

                return video_dim;
            }
            return video_dim;
        }

        // ============================================================================ MediaPlayer Events

        private void trackBarSpeed_Click(object sender, EventArgs e)
        {
            this.lock_track_speed = !this.lock_track_speed;
        }
        private void dataGridView1_Click(object sender, EventArgs e)
        {
            this.lock_info = !this.lock_info;
        }
        private void trackBarJogShuttle_Click(object sender, EventArgs e)
        {
            this.lock_jog_shuttle = !this.lock_jog_shuttle;
        }
        private void trackBarPlayHead_Click(object sender, EventArgs e)
        {
            this.lock_play_head = !this.lock_play_head;
        }


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