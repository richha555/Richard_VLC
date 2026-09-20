using System.Data;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
using LibVLCSharp.Shared;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace Richard_VLC
{
    public partial class REH_VLC_Viewer : Form
    {
        public enum button_image
        {
            to_begin = 0, play = 1, pause = 2, stop = 3, to_end = 4, single_forw = 5, single_back = 6
        }
        public enum play_state
        {
            stopped, paused, playing
        }
        public enum run_state
        {
            simulate,    // playhead moves with each tick
            vlc,         // VLC control playhead
            user,        // user controls playhead
            paused       // lock playhead
        }

        public class cVideoDim
        {
            public int height { get; set; } = 0;
            public int width { get; set; } = 0;
            public double hgt_wdt { get; set; } = 0.0;
            public double wdt_hgt { get; set; } = 0.0;

        }
        public class cVideo
        {
            public string full_path { get; set; } = "";
            public string file_name { get; set; } = "";
            public string title { get; set; } = "";
        }

        public class DarkColorTable : ProfessionalColorTable
        {
            private static readonly Color Dark = Color.FromArgb(45, 45, 48);
            private static readonly Color Darker = Color.FromArgb(30, 30, 30);
            private static readonly Color Selected = Color.FromArgb(70, 70, 75);

            // MenuStrip
            public override Color MenuStripGradientBegin => Dark;
            public override Color MenuStripGradientEnd => Dark;

            // Dropdown itself
            public override Color ToolStripDropDownBackground => Dark;

            // Dropdown/image margin
            public override Color ImageMarginGradientBegin => Dark;
            public override Color ImageMarginGradientMiddle => Dark;
            public override Color ImageMarginGradientEnd => Dark;

            // Selected menu item
            public override Color MenuItemSelected => Selected;
            public override Color MenuItemSelectedGradientBegin => Selected;
            public override Color MenuItemSelectedGradientEnd => Selected;

            // Top-level menu item when pressed
            public override Color MenuItemPressedGradientBegin => Selected;
            public override Color MenuItemPressedGradientMiddle => Selected;
            public override Color MenuItemPressedGradientEnd => Selected;

            // Borders
            public override Color MenuBorder => Darker;
            public override Color MenuItemBorder => Darker;
        }
        public class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuRenderer()
                : base(new DarkColorTable())
            {
            }
        }

        public cVideo current_video = new cVideo();
        public cVideoDim video_dimensions = new cVideoDim();

        public bool video_loaded = false;
        public double frame_rate = 30.0;  // frames per second
        public long video_length_frames = 0; //  (int)(frame_rate * 60.0 * 5.0);  // frames
        public double video_length_ms = 0;
        public double current_pos = 0.0; // current frame
        public TimeSpan current_time = new TimeSpan(0); // current offset
        public double current_speed = 1.0;

        public bool super_slow = false;

        public bool auto_play_on_load = true;

        public run_state CURRENT_RUN_STATE = run_state.paused;
        public run_state start_run_state = run_state.paused;
        public bool good_aspect = false;  // this.good_aspect = this._mp?.AspectRatio?.Contains(":");
        public bool good_framerate = false;  // this.good_framerate = this._mp?.Fps > 0.5;
        public bool good_length = false;  // this.good_length = this._mp?.Length > 1;

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

        private DateTime lastAction = DateTime.MinValue;
        private DateTime resumeRead = DateTime.MinValue;

        private int startWidth;
        private int startHeight;

        private int topSpeedMarker = 200;
        private int botSpeedMarker = 100;


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

        public bool speed_scrolling = false;
        public bool jog_shutt_scrolling = false;
        public bool playhead_scrolling = false;
        public bool back_moving = false;
        public bool forw_moving = false;

        public bool single_framing_forward = false;
        public bool single_framing_backward = false;

        public bool always_display_controls = true;
        public bool show_zoom_viewer = false;
        public bool show_info = false;
        public bool show_track_speed = false;
        public bool show_buttons = false;
        public bool in_buttons = false;
        public bool show_jog_shuttle = false;
        public bool show_play_head = false;

        public bool lock_zoom_viewer = false;
        public bool lock_info = false;
        public bool lock_track_speed = false;
        public bool lock_buttons = false;
        public bool lock_jog_shuttle = false;
        public bool lock_play_head = true;

        public bool dragging_box = false;

        public bool isFullscreen = false;
        public bool isPlaying = false;
        public Size oldVideoSize;
        public Size oldFormSize;
        public Point oldVideoLocation;
        private int _updatePlayHeadCnt = 0;

        Dictionary<double, Label> speedLabels = new();
        Dictionary<double, Label> trackMarkers = new();

        public LibVLC _libVLC;
        public MediaPlayer _mp;
        public Media media;

        private readonly CancellationTokenSource _cts = new();

        public REH_VLC_Viewer()
        {
            InitializeComponent();
        }

        // ================================================================================================================== BACKGROUND LOOP

        private async Task RunBackgroundTaskAsync()
        {
            long last_time = -1;
            bool time_changed = false;
            DateTime last_change = DateTime.MinValue;

            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));

            try {
                while (await timer.WaitForNextTickAsync(_cts.Token)) {

                    if (this.speed_scrolling && !this.show_track_speed) {  // when mouse moves out of speed-control
                        // assume mouse up has not triggered !!
                        this.speed_scrolling = false;
                        trackBarSpeed_End();
                    }
                    if (this.jog_shutt_scrolling && !this.show_jog_shuttle) {  // when mouse moves out of jog-shuttle
                        // assume mouse up has not triggered !!
                        this.jog_shutt_scrolling = false;
                        trackBarJogShuttle_End();
                    }
                    if (this.playhead_scrolling && !this.show_play_head) {  // when mouse moves out of playhead
                        // assume mouse up has not triggered !!
                        this.playhead_scrolling = false;
                        trackBarPlayHead_End();
                    }
                    //if (this.back_moving && !this.in_buttons) {  // when mouse moves out of button area
                    //    // assume mouse up has not triggered !!
                    //    this.back_moving = false;
                    //    Button_Back_End();
                    //}
                    //if (this.forw_moving && !this.in_buttons) {  // when mouse moves out of button area
                    //    // assume mouse up has not triggered !!
                    //    this.forw_moving = false;
                    //    Button_Forward_End();
                    //}


                    if (!this.lock_track_speed && !this.show_track_speed && this.trackBarSpeed.Visible) {
                        if ((DateTime.Now - this.lastTrackSpeed).TotalSeconds > 0.5) { // hide after N secs
                            show_hide_speed(false);
                        }
                    }
                    if (!this.lock_info && !this.show_info && this.dataGridView1.Visible) {
                        if ((DateTime.Now - this.lastInfo).TotalSeconds > 0.5) { // hide after N secs
                            this.dataGridView1.Visible = false;
                        }
                    }
                    if (!this.lock_jog_shuttle && !this.show_jog_shuttle && this.trackBarJogShuttle.Visible) {
                        if ((DateTime.Now - this.lastJogShuttle).TotalSeconds > 0.5) { // hide after N secs
                            this.trackBarJogShuttle.Visible = false;
                        }
                    }
                    if (!this.lock_play_head && !this.show_play_head && this.trackBarPlayHead.Visible) {
                        if ((DateTime.Now - this.lastPlayHead).TotalSeconds > 0.5) { // hide after N secs
                            this.trackBarPlayHead.Visible = false;
                        }
                    }
                    if (this.CURRENT_RUN_STATE == run_state.paused) continue;

                    double pos = this.current_pos;
                    TimeSpan tpos = this.current_time;

                    time_changed = true;

                    if (this.CURRENT_RUN_STATE == run_state.simulate || this.CURRENT_RUN_STATE == run_state.user || this.super_slow) {

                        // ------------------------------------------------------ we move the playhead on each tick

                        tpos = new TimeSpan(0);

                        if (this.single_framing_forward) {
                            pos += 0.5;  // 1/2 frame per 500 ms
                            //tpos = tpos.Add(TimeSpan.FromMilliseconds(500));
                        } else if (this.single_framing_backward) {
                            pos -= 0.5;
                            //tpos = tpos.Add(TimeSpan.FromMilliseconds(-500));
                        } else if ((this.playing && !this.paused) || this.super_slow) {
                            double fr = 0.5 * (this.frame_rate * this.current_speed);
                            if (this.reverse_motion) {
                                pos -= fr;
                                //tpos = TimeSpan.FromSeconds(pos / this.frame_rate);
                            } else {
                                pos += fr;
                                //tpos = TimeSpan.FromSeconds(pos / this.frame_rate);
                            }
                        } else {
                            time_changed = false;
                        }
                    } else if (this.CURRENT_RUN_STATE == run_state.vlc) {

                        // ------------------------------------------------------ we fetch the state from VLC and update the interface

                        string AspectRatio = (this.good_aspect ? "" : this._mp?.AspectRatio ?? "");
                        float Fps = (this.good_framerate ? (float)this.frame_rate : this._mp?.Fps ?? 0);
                        long Length = (this.good_length ? (long)this.video_length_ms : this._mp?.Length ?? 0);

                        fetch_video_properties(AspectRatio,
                                               Fps,
                                               Length);

                        pos = -1;
                        tpos = new TimeSpan(0);

                        // this.current_pos = this.video_length * this._mp?.Position;
                        // var pos_secs = this.current_pos / this.frame_rate;
                        // this.current_time = TimeSpan.FromSeconds((double)this._mp?.Time / 1000.0);

                        if (this.good_length) {
                            // VLC pos is not the best, it's from 0.0 to 1.0
                            // where 1.0 is the length of the video
                            pos = (double)this.video_length_frames * (this._mp?.Position ?? 0.0);
                        }
                        // VLC tpos is much better because it's millisec's from start
                        // however, it's empty more than it's set

                        long time_ms = this._mp?.Time ?? -1;

                        if (time_ms <= 0) {
                            //if (this.good_framerate) {
                            //    tpos = TimeSpan.FromSeconds(pos / this.frame_rate);
                            //} else {
                            //    tpos = new TimeSpan(0); // only know relative-position
                            //}
                            time_changed = false;
                        } else {

                            time_changed = (time_ms != last_time);

                            if (time_changed) {
                                last_change = DateTime.Now;

                                tpos = TimeSpan.FromMilliseconds(time_ms);
                                pos = -1;  // make sure Set_Current_Pos uses time_ms

                                if (DateTime.Now < this.resumeRead) {
                                    time_changed = false;  // user user just did something with the video, give VLC time to process
                                } else {                   // before going back to reading data
                                    last_time = time_ms;
                                }
                            }
                        }

                    } else if (this.CURRENT_RUN_STATE == run_state.paused) {

                        time_changed = false;  // -------------------------------- user is controlling playhead
                    }

                    if (time_changed) {
                        if (this.CURRENT_RUN_STATE == run_state.user || this.super_slow) {
                            Set_Current_Pos(pos, tpos, update_playhead: true, seek_in_video: true, from_timer: true);
                        } else {
                            Set_Current_Pos(pos, tpos, update_playhead: true, seek_in_video: false, from_timer: true);
                        }
                    }

                    if (!this.show_zoom_viewer && this.pnlVideoFull.Visible) {
                        if ((DateTime.Now - this.lastZoomPan).TotalSeconds > 0.5) { // hide after N secs
                            this.pnlVideoFull.Visible = false;
                        }
                    }
                }
            } catch (OperationCanceledException) {
                // Normal shutdown.
            }
        }

        private void Reset_MouseDowns()
        {
            if (this.speed_scrolling) {  // assume mouse up has not triggered !!
                this.speed_scrolling = false;
                trackBarSpeed_End();
            }
            if (this.jog_shutt_scrolling) {  // assume mouse up has not triggered !!
                this.jog_shutt_scrolling = false;
                trackBarJogShuttle_End();
            }
            if (this.playhead_scrolling) {  // assume mouse up has not triggered !!
                this.playhead_scrolling = false;
                trackBarPlayHead_End();
            }
            if (this.back_moving) {  // assume mouse up has not triggered !!
                this.back_moving = false;
                Button_Back_End();
            }
            if (this.forw_moving) {  // assume mouse up has not triggered !!
                this.forw_moving = false;
                Button_Forward_End();
            }

        }

        // ================================================================================================================== UTILS

        private void Set_Current_Pos(double pos, TimeSpan tpos, bool update_playhead = true, bool seek_in_video = true, bool from_timer = false)
        {
            if (this.video_length_frames <= 0 || this.frame_rate <= 0) return;

            bool have_pos = pos > 0;
            bool have_tpos = tpos.TotalMilliseconds > 0;

            // if we are given a tpos, we should use it   tpos => pos
            // else calculate tpos from pos               pos => tpos

            if (have_tpos) {
                // have tpos ... use it
                this.current_time = tpos; // tpos was passed in, use it
                // tpos = pos / fr
                // pos = tpos * fr
                pos = this.frame_rate * (tpos.TotalMilliseconds / 1000.0);
            } else {
                // no tpos ... use pos
                this.current_time = TimeSpan.FromSeconds(Math.Max(0.0, pos) / this.frame_rate);
            }
            this.current_pos = Math.Min(Math.Max(0.0, pos), (double)this.video_length_frames);
            //  Debug.WriteLine($"Set_Current_Pos: curr_pos = {this.current_pos}");  <<<< *** until we find a way to print .... instead of spamming Output window

            if (seek_in_video) {
                if (this.video_loaded) {
                    Start_Action("Set_Current_Pos", $"SEEK-TO: {this.current_time}  (pos = {this.current_pos})");
                    this._mp?.SeekTo(this.current_time);
                    // Application.DoEvents();
                }
            }
            if (update_playhead) {
                // ChatGPT says that the ScrollEvent will not be generated if the Value is changed while another event handler continues 
                // running on the UI thread.   ChatGPT says the ScrollEvent will be generated by the Timer if just after changing Value
                // the Timer awaits its next Tick.   This is the exact opposite of what is stated below.
                if (from_timer) {
                    //  when playhead is moved from timer, it appears to trigger a SCROLL event that gets processed async
                    //  
                    // this._updatePlayHeadCnt += 1; ---Scroll event does not seem to get triggered anymore
                } else {
                    this._updatePlayHeadCnt += 1;  // --- when invoked from an event handler, it seems we still need this
                }
                this.trackBarPlayHead.Value = (int)Math.Min(Math.Max((double)this.trackBarPlayHead.Minimum, pos), (double)this.trackBarPlayHead.Maximum);
                // Application.DoEvents();
                //  this._updatingPlayHead = false;  --- let trackBarPlayHead_Scroll clear the flag so it will still be true even it the event is processed at a later time
            }

            Update_Value("current_pos");
            Update_Value("current_time");
        }

        private void Start_Action(string routine, string action)
        {
            Debug.WriteLine($"{routine}: MediaPlayer {action}");
            this.lastAction = DateTime.Now;
            this.resumeRead = this.lastAction.AddMilliseconds(1500);
        }

        private void Set_Play_State(play_state PlayState)
        {
            switch (PlayState) {
                case play_state.stopped:  // [play]
                    // playing   Play => STOP
                    Debug.WriteLine($"Set_Play_State:  STOP");
                    this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
                    this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
                    this.butStop.Visible = false;
                    this.playing = false;
                    this.paused = false;
                    this.reverse_motion = false;
                    this.single_framing_forward = false;
                    this.single_framing_backward = false;
                    break;
                case play_state.playing:  // [pause]   [stop]
                    Debug.WriteLine($"Set_Play_State:  PLAY");
                    this.butPlay.Image = this.imageList1.Images[(int)button_image.pause];
                    this.toolTip1.SetToolTip(this.butPlay, "Click to PAUSE");
                    this.butStop.Visible = true;
                    this.playing = true;
                    this.paused = false;
                    break;
                case play_state.paused:   // [resume]  [stop]
                    // playing   Play => PAUSE
                    Debug.WriteLine($"Set_Play_State:  PAUSE");
                    this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
                    this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
                    this.butStop.Visible = true;
                    this.playing = true;
                    this.paused = true;
                    break;
            }
            Update_Value("mode");
            Update_Value("reverse_motion");
        }

        // Source - https://stackoverflow.com/a/21885479
        // Posted by Plater, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-09-11, License - CC BY-SA 4.0

        public static bool SetStyle(Control c, ControlStyles Style, bool value)
        {
            bool retval = false;

            if (c != null) {
                Type typeTB = typeof(Control);
                System.Reflection.MethodInfo misSetStyle = typeTB.GetMethod("SetStyle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (misSetStyle != null) {
                    misSetStyle.Invoke(c, new object[] { Style, value });
                    retval = true;
                }
            }
            return retval;
        }

        private void show_hide_speed(bool is_visible)
        {
            this.trackBarSpeed.Visible = is_visible;
            foreach (var lab in this.speedLabels.Values) {
                lab.Visible = is_visible;
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

                    Update_Value("width");
                    Update_Value("zoom");
                }
            }
            if (!this.good_framerate) {

                Debug.WriteLine($"fetch_video_properties: fps [{fps}]");

                this.good_framerate = fps > 0.5;
                if (good_framerate) {
                    this.frame_rate = fps;
                    Update_Value("frame_rate");

                    drawSpeedMarkers();

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

        // ================================================================================================================== OPEN

        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Title = "Select VIDEO to play";
            this.openFileDialog1.InitialDirectory = "G:\\";

            if (this.openFileDialog1.ShowDialog() == DialogResult.OK) {

                this.current_video.full_path = this.openFileDialog1.FileName;

                this.Text = "Richard's VLC Viewer - " + this.current_video.full_path;

                if (this.playing || this.paused) {
                    if (video_loaded) {
                        Start_Action("openToolStripMenuItem_Click", "STOP");
                        this._mp?.Stop();
                        // Application.DoEvents();
                    }
                }

                Set_Play_State(play_state.stopped);

                //    FileInfo fi = new FileInfo(this.video_file);
                //    this.videoView1.SetMedia(fi);


                Debug.WriteLine("-------------------------------------------------------------------------------------------------");
                Debug.WriteLine($"| openToolStripMenuItem_Click:  LOADING: {this.current_video.full_path}");

                using var media = new Media(this._libVLC, this.current_video.full_path);

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

                    drawSpeedMarkers();
                }

                if (!this.good_length) {
                    // invent a length, but don't set good_length = true
                    this.video_length_frames = (int)(this.frame_rate * 60.0 * 5.0);   // assume 5.0 minute long video
                    trackBarPlayHead.Maximum = (int)this.video_length_frames;
                    Update_Value("length");
                }

                //this.current_pos = this.video_length * this._mp?.Position;
                //var pos_secs = this.current_pos / this.frame_rate;
                //this.current_time = TimeSpan.FromSeconds((double)this._mp?.Time / 1000.0);

                double pos = 0;  // assume playhead is @ beginning

                Set_Current_Pos(pos, new TimeSpan(0), update_playhead: true, seek_in_video: false);

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

                this.video_loaded = true;

                if (auto_play_on_load) {

                    Start_Action("openToolStripMenuItem_Click", "PLAY");
                    this._mp?.Play();
                    // Application.DoEvents();

                    Set_Play_State(play_state.playing);
                }

                Update_Value("mode");

                //this.pnlOverlay.BringToFront();
                //this.labSpeedIndicator.BringToFront();
                //this.labSpeedIndicator.Invalidate();
            }
        }

        // ================================================================================================================== BUTTONS

        private void butBegin_Click(object sender, EventArgs e)
        {
            Button_Begin();
        }
        private void Button_Begin()
        {
            Debug.WriteLine("---");
            Debug.WriteLine($"| butBegin_Click");

            if (this.video_loaded) {
                Start_Action("butBegin_Click", "SEEK TO 0");
                this._mp?.SeekTo(new TimeSpan(0));
            }
            Set_Current_Pos(0.0, new TimeSpan(0), update_playhead: true, seek_in_video: true);

            Update_Value("mode");
        }

        private void butPlay_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("---");
            Debug.WriteLine($"| butPlay_Click");

            Button_Play_Pause();
        }
        private void Button_Play_Pause()
        {
            if (this.playing && !this.paused) {
                // playing   Play => PAUSE
                if (this.video_loaded) {
                    Start_Action("butPlay_Click", "PAUSE");
                    this._mp?.Pause();
                    // Application.DoEvents();
                }
                Set_Play_State(play_state.paused);
            } else {
                // not playing   Play => PLAY
                if (this.video_loaded) {
                    Start_Action("butPlay_Click", "PLAY");
                    this._mp?.Play();
                    // Application.DoEvents();
                }
                Set_Play_State(play_state.playing);
            }
        }

        private void butStop_Click(object sender, EventArgs e)
        {
            Button_Stop();
        }
        private void Button_Stop()
        {
            Debug.WriteLine("---");
            Debug.WriteLine($"| butStop_Click");

            if (this.video_loaded) {
                Start_Action("butStop_Click", "STOP");
                this._mp?.Stop();
                // Application.DoEvents();
            }
            Set_Play_State(play_state.stopped);

            //  Set_Current_Pos(0);   ... don't do this it's annoying
        }

        private void butSingleBack_Click(object sender, EventArgs e)
        {
        }
        private void butSingleBack_MouseDown(object sender, MouseEventArgs e)
        {
            Button_Back_Start();
        }

        private void Button_Back_Start()
        {
            Debug.WriteLine("---");
            Debug.WriteLine($"| butSingleBack_MouseDown");

            Reset_MouseDowns();

            this.back_moving = true;

            this.hold_playing = this.playing;
            this.hold_paused = this.paused;
            this.start_run_state = this.CURRENT_RUN_STATE;

            this.CURRENT_RUN_STATE = run_state.paused;  // disable timer until we finished doing our stuff

            //if (this.playing && !this.paused) {
            //    // playing   Play => PAUSE
            //    if (this.CURRENT_RUN_STATE == run_state.vlc) {
            //        Start_Action("butSingleBack_MouseDown","PAUSE");
            //        this._mp?.Pause();
            //    }
            //    this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
            //    this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
            //    this.paused = true;
            //    Update_Value("mode");
            //} else if (!playing) {
            //    this.playing = true;
            //    this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
            //    this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
            //    this.butStop.Visible = true;
            //    this.paused = true;
            //    Update_Value("mode");
            //}
            //this.start_run_state = this.CURRENT_RUN_STATE;
            //this.CURRENT_RUN_STATE = run_state.simulate;
            //this.single_framing_backward = true;
            //Update_Value("mode");

            if (Math.Abs(this.current_pos) < 0.5) {
                this.back_moving = false;  // mouse-up is no longer needed
                return;
            }

            if (this.current_pos <= 0) {
                Set_Current_Pos(0.0, new TimeSpan(0), update_playhead: true, seek_in_video: true);
                this.back_moving = false;  // mouse-up is no longer needed
                return;
            }

            // we are trying to move the play head.
            // we press play to force it to (re) load the video
            // and then press pause to go into paused mode

            if (this.video_loaded) {
                // --------------- almost always ends up playing one frame with audio and then stopping
                //// Play + PAUSE
                //Start_Action("butSingleBack_MouseDown","PLAY");
                //this._mp?.Play();
                //// we hope that .Seek will display the frame we scrolled to in pause-mode
                //Start_Action("butSingleBack_MouseDown","PAUSE");
                //this._mp?.Pause();
            }

            if (this.playing && !this.paused) {
                // playing   Play => PAUSE
                Set_Play_State(play_state.paused);

                if (this.video_loaded) {
                    Start_Action("butSingleBack_MouseDown", "PAUSE");
                    this._mp?.Pause();
                    // Application.DoEvents();
                }
            }

            // we make the first move

            double pos = this.current_pos - 1.0;
            TimeSpan tpos = new TimeSpan(0);
            //if (this.frame_rate > 0)
            //    tpos = TimeSpan.FromSeconds(pos / this.frame_rate);
            Debug.WriteLine($"butSingleBack_MouseDown: => pos = {pos}");

            Set_Current_Pos(pos, tpos, update_playhead: true, seek_in_video: true);
            Debug.WriteLine($"butSingleBack_MouseDown: After Set_Current_Pos - curr_pos = {this.current_pos}");

            // now let timer take over, until we release mouse button

            this.playing = true;  // tell timer to process ticks
            this.paused = false;

            this.single_framing_backward = true;

            this.CURRENT_RUN_STATE = run_state.user;    // timer controls playhead

            Update_Value("mode");
        }

        private void butSingleBack_MouseUp(object sender, MouseEventArgs e)
        {
            Button_Back_End();
        }
        private void Button_Back_End()
        {
            Debug.WriteLine($"| butSingleBack_MouseUp");

            this.back_moving = false;

            //this.playing = this.hold_playing;   single forward pauses and moves one frame forward.  
            //this.paused = this.hold_paused;     afterwards stay paused !!

            if (this.hold_playing) {
                // we were playing, wea re now paused and will stay paused
                this.playing = true;
                this.paused = true;
            } else {
                this.playing = false;
                this.paused = false;
            }

            this.CURRENT_RUN_STATE = this.start_run_state;
            this.single_framing_backward = false;

            // after back, leave in paused mode

            //if (this.hold_playing && !this.hold_paused) {
            //    // playing   Play => PAUSE
            //    if (this.video_loaded) {
            //        Start_Action("butSingleBack_MouseUp","PLAY");
            //        this._mp?.Play();  // resume play
            //    }
            //    this.playing = true;
            //    this.paused = false;
            //}

            Update_Value("mode");
        }

        private void butSingleFwd_Click(object sender, EventArgs e)
        {
        }
        private void butSingleFwd_MouseDown(object sender, MouseEventArgs e)
        {
            Button_Forward_Start();
        }
        private void Button_Forward_Start()
        {
            Debug.WriteLine("---");
            Debug.WriteLine($"| butSingleFwd_MouseDown");

            Reset_MouseDowns();

            this.forw_moving = true;

            this.hold_playing = this.playing;
            this.hold_paused = this.paused;
            this.start_run_state = this.CURRENT_RUN_STATE;

            this.CURRENT_RUN_STATE = run_state.paused;  // disable timer until we finished doing our stuff

            //if (this.playing && !this.paused) {
            //    // playing   Play => PAUSE
            //    if (this.CURRENT_RUN_STATE == run_state.vlc) {
            //        Start_Action("butSingleFwd_MouseDown","PAUSE");
            //        this._mp?.Pause();
            //    }
            //    // playing   Play => PAUSE
            //    this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
            //    this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
            //    this.paused = true;
            //    Update_Value("mode");
            //} else if (!playing) {
            //    this.playing = true;
            //    this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
            //    this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
            //    this.butStop.Visible = true;
            //    this.paused = true;
            //    Update_Value("mode");
            //}
            //this.start_run_state = this.CURRENT_RUN_STATE;
            //this.CURRENT_RUN_STATE = run_state.simulate;
            //this.single_framing_forward = true;
            //Update_Value("mode");

            if ((int)this.current_pos >= this.video_length_frames) {
                this.forw_moving = false;  // mouse-up is no longer needed
                return;
            }

            // we are trying to move the play head.
            // we press play to force it to (re) load the video
            // and then press pause to go into paused mode

            if (this.video_loaded) {
                // --------------- almost always ends up playing one frame with audio and then stopping
                //// Play + PAUSE
                //Start_Action("butSingleFwd_MouseDown","PLAY");
                //this._mp?.Play();
                //// Application.DoEvents();
                //// we hope that .Seek will display the frame we scrolled to in pause-mode
                //Start_Action("butSingleFwd_MouseDown","PAUSE");
                //this._mp?.Pause();
                //// Application.DoEvents();
            }

            if (this.playing && !this.paused) {
                // playing   Play => PAUSE
                Set_Play_State(play_state.paused);

                if (this.video_loaded) {
                    Start_Action("butSingleFwd_MouseDown", "PAUSE");
                    this._mp?.Pause();
                    // Application.DoEvents();
                }
            }

            // we make the first move

            bool MOVE_BY_POS = false;  // when moving forward SEEK only works if frame has been buffered

            if (MOVE_BY_POS) {
                // *** for moving forward one frame, there is a command
                // *** that we could use INSTEAD of calculating pos & tpos
                double pos = this.current_pos + 1.0;
                TimeSpan tpos = new TimeSpan(0);
                //if (this.frame_rate > 0)
                //    tpos = TimeSpan.FromSeconds(pos / this.frame_rate);
                Set_Current_Pos(pos, tpos, update_playhead: true, seek_in_video: true);
                // now let timer take over, until we release mouse button
            } else {
                Start_Action("butSingleFwd_MouseDown", "NEXT FRAME");
                this._mp?.NextFrame();
                double pos = this.current_pos + 1.0;
                TimeSpan tpos = new TimeSpan(0);
                Set_Current_Pos(pos, tpos, update_playhead: true, seek_in_video: false);
            }

            this.playing = true;  // tell timer to process ticks
            this.paused = false;

            this.single_framing_forward = true;

            this.CURRENT_RUN_STATE = run_state.user;    // timer controls playhead

            Update_Value("mode");
        }

        private void butSingleFwd_MouseUp(object sender, MouseEventArgs e)
        {
            Button_Forward_End();
        }
        private void Button_Forward_End()
        {
            Debug.WriteLine($"| butSingleFwd_MouseUp");

            this.forw_moving = false;

            //this.playing = this.hold_playing;   single forward pauses and moves one frame forward.  
            //this.paused = this.hold_paused;     afterwards stay paused !!

            if (this.hold_playing) {
                // we were playing, wea re now paused and will stay paused
                this.playing = true;
                this.paused = true;
            } else {
                this.playing = false;
                this.paused = false;
            }

            this.CURRENT_RUN_STATE = this.start_run_state;
            this.single_framing_forward = false;

            // after forward, leave in paused mode

            Update_Value("mode");
        }

        // ================================================================================================================== TRACK-BARS

        private void trackBarPlayHead_MouseDown(object sender, MouseEventArgs e)
        {
            trackBarPlayHead_Start();
        }

        private void trackBarPlayHead_Start()
        {
            Debug.WriteLine("---");
            Debug.WriteLine($"| trackBarPlayHead_MouseDown");

            Reset_MouseDowns();

            this.playhead_scrolling = true;

            this.hold_speed = this.current_speed;
            this.hold_reverse = this.reverse_motion;
            this.hold_playing = this.playing;
            this.hold_paused = this.paused;

            this.start_run_state = this.CURRENT_RUN_STATE;

            this.CURRENT_RUN_STATE = run_state.paused;  // disable timer until we finished doing our stuff

            // we are trying to move the play head.
            // we press play to force it to (re) load the video
            // and then press pause to go into paused mode

            //if (this.video_loaded) {
            //    // Play + PAUSE
            //    Start_Action("trackBarPlayHead_MouseDown","PLAY");
            //    this._mp?.Play();
            //    // Application.DoEvents();
            //    // we hope that .Seek will display the frame we scrolled to in pause-mode
            //    Start_Action("trackBarPlayHead_MouseDown","PAUSE");
            //    this._mp?.Pause();
            //    // Application.DoEvents();
            //}

            if (this.playing && !this.paused) {
                // playing   Play => PAUSE
                Set_Play_State(play_state.paused);

                if (this.video_loaded) {
                    Start_Action("butSingleFwd_MouseDown", "PAUSE");
                    this._mp?.Pause();
                    // Application.DoEvents();
                }
            }

            this.CURRENT_RUN_STATE = run_state.paused; // disable background while scrolling... we are controlling playhead + vlc

            Update_Value("mode");
        }
        private void trackBarPlayHead_Scroll(object sender, EventArgs e)
        {
            if (_updatePlayHeadCnt > 0) {
                _updatePlayHeadCnt -= 1;  // let the system know we handled the event
                Debug.WriteLine($"trackBarPlayHead_Scroll: Ignoring non-user Scroll   _updatePlayHeadCnt => {_updatePlayHeadCnt}");
                return;
            }

            if (!this.playhead_scrolling) {
                trackBarPlayHead_Start();   // mousedown did not trigger !!!
            }

            System.Windows.Forms.TrackBar myTB = (System.Windows.Forms.TrackBar)sender;

            Debug.WriteLine($"trackBarPlayHead_Scroll: TO FRAME {myTB.Value.ToString()} - FROM SECONDS {current_time.ToString()}");

            double pos = (double)myTB.Value;

            //  TimeSpan tpos = TimeSpan.FromSeconds(pos / this.frame_rate);
            TimeSpan tpos = new TimeSpan(0);

            Set_Current_Pos(pos, tpos, update_playhead: false, seek_in_video: true);

            Update_Value("current_pos");
            Update_Value("current_time");
            Update_Value("mode");
        }
        private void trackBarPlayHead_MouseUp(object sender, MouseEventArgs e)
        {
            trackBarPlayHead_End();
        }
        private void trackBarPlayHead_End()
        {
            Debug.WriteLine($"| trackBarPlayHead_MouseUp");

            this.playhead_scrolling = false;

            this.CURRENT_RUN_STATE = this.start_run_state;

            if (this.hold_playing && !this.hold_paused) {
                // playing   PAUSE => Play
                Set_Play_State(play_state.playing);

                if (this.video_loaded) {
                    Start_Action("trackBarPlayHead_MouseUp", "PLAY");
                    this._mp?.Play();  // resume play
                    // Application.DoEvents();
                }
                this.playing = true;
                this.paused = false;
            }
            Update_Value("mode");
        }

        private void trackBarJogShuttle_MouseDown(object sender, MouseEventArgs e)
        {
            trackBarJogShuttle_Begin();
        }
        private void trackBarJogShuttle_Begin()
        {
            Debug.WriteLine("---");
            Debug.WriteLine($"| trackBarJogShuttle_MouseDown");

            Reset_MouseDowns();

            this.jog_shutt_scrolling = true;

            this.hold_speed = this.current_speed;
            this.hold_reverse = this.reverse_motion;
            this.hold_playing = this.playing;
            this.hold_paused = this.paused;

            this.start_run_state = this.CURRENT_RUN_STATE;

            this.CURRENT_RUN_STATE = run_state.paused;  // disable timer until we finished doing our stuff

            //if (!this.playing) {
            //    // go to PLAY
            //    this.butPlay.Image = this.imageList1.Images[(int)button_image.pause];
            //    this.toolTip1.SetToolTip(this.butPlay, "Click to PAUSE");
            //    this.butStop.Visible = true;
            //    this.playing = true;
            //    this.paused = false;
            //    Update_Value("mode");
            //} else if (this.paused) {
            //    // paused => PLAY
            //    this.butPlay.Image = this.imageList1.Images[(int)button_image.pause];
            //    this.toolTip1.SetToolTip(this.butPlay, "Click to PAUSE");
            //    this.paused = false;
            //    Update_Value("mode");
            //}

            // we are trying to move the play head.
            // we press play to force it to (re) load the video
            // and then press pause to go into paused mode

            //if (this.video_loaded) {
            //    // Play + PAUSE
            //    Start_Action("trackBarJogShuttle_MouseDown","PLAY");
            //    this._mp?.Play();
            //    // Application.DoEvents();
            //    // we hope that .Seek will display the frame we scrolled to in pause-mode
            //    Start_Action("trackBarJogShuttle_MouseDown","PAUSE");
            //    this._mp?.Pause();
            //    // Application.DoEvents();
            //}

            if (this.playing && !this.paused) {

                // don't touch buttons, we'll return to play when finished 

                // playing   Play => PAUSE
                Set_Play_State(play_state.paused);

                if (this.video_loaded) {
                    Start_Action("trackBarJogShuttle_MouseDown", "PAUSE");
                    this._mp?.Pause();
                }
                this.paused = true;
            }

            this.playing = true;  // tell timer to process ticks
            this.paused = false;

            this.CURRENT_RUN_STATE = run_state.user;    // timer controls playhead

            Update_Value("mode");
        }

        private void trackBarJogShuttle_Scroll(object sender, EventArgs e)
        {
            Debug.WriteLine($"| trackBarJogShuttle_Scroll");

            if (!this.jog_shutt_scrolling) {
                trackBarJogShuttle_Begin();   // mousedown did not trigger !!!
            }

            System.Windows.Forms.TrackBar myTB = (System.Windows.Forms.TrackBar)sender;

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
            Update_Value("mode");
        }

        private void trackBarJogShuttle_MouseUp(object sender, MouseEventArgs e)
        {
            trackBarJogShuttle_End();
        }
        private void trackBarJogShuttle_End()
        {
            Debug.WriteLine($"| trackBarJogShuttle_MouseUp");

            this.jog_shutt_scrolling = false;

            trackBarJogShuttle.Value = 0;  // spring back to center

            //if ((this.hold_playing != this.playing) || (this.hold_paused != this.paused)) {
            //    if (this.hold_paused) {
            //        // go back to PAUSE mode
            //        this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
            //        this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
            //        this.paused = true;
            //        Update_Value("mode");
            //    } else if (!this.hold_playing) {
            //        // go back to STOP mode
            //        this.butPlay.Image = this.imageList1.Images[(int)button_image.play];
            //        this.toolTip1.SetToolTip(this.butPlay, "Click to PLAY");
            //        this.playing = false;
            //        this.paused = false;
            //        Update_Value("mode");
            //    }
            //}

            if (this.hold_speed == -9999) {
                this.current_speed = 1;
                this.reverse_motion = false;
            } else {
                this.current_speed = this.hold_speed;
                this.reverse_motion = this.hold_reverse;
                this.hold_speed = -9999;
            }
            if (Math.Abs(this.current_speed - 1.0) < 0.05) {
                this.current_speed = 1.0;
            }

            double fr_rate = this.frame_rate * this.current_speed;
            if (fr_rate >= 1.0) {
                Start_Action("trackBarJogShuttle_MouseUp", $"SETRATE({this.current_speed})");
                this._mp?.SetRate((float)this.current_speed);
                // Application.DoEvents();
                this.super_slow = false;
            } else {
                this.super_slow = true;
            }
            this.CURRENT_RUN_STATE = this.start_run_state;

            if (this.hold_playing && !this.hold_paused) {
                // playing   PAUSE => Play
                Set_Play_State(play_state.playing);

                if (this.video_loaded) {
                    Start_Action("trackBarJogShuttle_MouseUp", "PLAY");
                    this._mp?.Play();  // resume play
                    // Application.DoEvents();
                }
                this.playing = true;
                this.paused = false;
            }
            Update_Value("current_speed");
            Update_Value("reverse_motion");
            Update_Value("mode");
        }

        private void drawSpeedMarkers()
        {
            this.pnlVIDEO.SuspendLayout();
            //SuspendLayout();

            Label lab;

            lab = drawSpeedMarker(2.0, Color.Blue); this.topSpeedMarker = lab.Location.Y;
            lab = drawSpeedMarker(1.0, Color.White);
            lab = drawSpeedMarker(0.5, Color.Red);
            lab = drawSpeedMarker(0.25, Color.Red);

            // 1 sec_fr = 1.0 / (frame_rate * s)
            // N = 1 / (fr * s)
            // fr * s = 1 / N
            // s = 1 / (fr * N)

            for (int N = 1; N <= 5; N++) {
                double s = 1.0 / (this.frame_rate * (double)N);
                lab = drawSpeedMarker(s, Color.Magenta);
                if (N == 5) {
                    this.botSpeedMarker = lab.Location.Y;
                }
            }

            this.pnlVIDEO.ResumeLayout(false);
            this.pnlVIDEO.PerformLayout();
            //ResumeLayout(false);
            //PerformLayout();
        }

        private Label drawSpeedMarker(double speed, Color clr)
        {
            Label lab = new();

            if (speed > (double)this.trackBarSpeed.Maximum || speed < 0.0) return lab;

            int y0 = -1;
            double last_speed = 0.0;
            for (int y = 0; y <= (double)this.trackBarSpeed.Maximum; y++) {
                double x = (double)y / (double)this.trackBarSpeed.Maximum;
                double s = 0.004 * Math.Pow(1000, x);  // a = 0.004  b = 1000
                if (s >= speed && last_speed <= speed) {
                    y0 = y; break;
                } else {
                    last_speed = s;
                }
            }
            //this.pnlVIDEO.SuspendLayout();

            // Map the trackBar value (0..Maximum) to panel pixel coordinates so the
            // marker lines up with the TrackBar visual position.
            int pixelY = (int)Math.Round(((double)y0 / (double)this.trackBarSpeed.Maximum) * (double)this.trackBarSpeed.Height);
            pixelY = this.trackBarSpeed.Location.Y + (this.trackBarSpeed.Height - pixelY) + 5;

            //  bool have_lab = this.speedLabels.Any(l => l.Tag?.Equals(speed) ?? false);

            if (this.speedLabels.ContainsKey(speed)) {
                lab = this.speedLabels[speed];
                lab.Location = new System.Drawing.Point(this.labSpeed.Location.X, pixelY);
            } else {
                lab.AutoSize = true;
                lab.BackColor = this.labSpeed.BackColor;
                lab.FlatStyle = labSpeed.FlatStyle;
                lab.Font = labSpeed.Font;
                lab.ForeColor = clr;
                lab.Location = new System.Drawing.Point(this.labSpeed.Location.X, pixelY);
                lab.Name = String.Format("labSpeed{0}", y0);
                lab.Size = labSpeed.Size;
                lab.Text = labSpeed.Text;
                lab.Tag = speed;
                lab.Visible = this.trackBarSpeed.Visible;

                this.pnlVIDEO.Controls.Add(lab);

                // Ensure the dynamically added label is on top of other controls
                // (some video rendering controls can appear above child controls).
                lab.BringToFront();

                this.speedLabels.Add(speed, lab);
            }

            //this.pnlVIDEO.ResumeLayout(false);
            //this.pnlVIDEO.PerformLayout();
            return lab;
        }

        private void drawTrackMarkers()
        {
            double margin = 12.0;

            foreach (KeyValuePair<double, Label> kv in this.trackMarkers) {
                double pos = kv.Key;
                Label lab = kv.Value;

                int x0 = (int)((double)(this.trackBarPlayHead.Width - 2.0 * margin) * (pos / (double)this.trackBarPlayHead.Maximum));
                x0 += (this.trackBarPlayHead.Left + (int)margin);
                x0 -= (int)((double)this.labMarker.Width * 0.5);

                lab.Location = new System.Drawing.Point(x0, this.labMarker.Location.Y);
            }
        }

        private Label drawTrackMarker(double pos, Color clr)
        {
            Label lab = new();

            double margin = 12.0;

            if (pos > (double)this.trackBarPlayHead.Maximum || pos < 0.0) return lab;

            int x0 = (int)((double)(this.trackBarPlayHead.Width - 2.0 * margin) * (pos / (double)this.trackBarPlayHead.Maximum));
            x0 += (this.trackBarPlayHead.Left + (int)margin);
            x0 -= (int)((double)this.labMarker.Width * 0.5);

            //this.pnlVIDEO.SuspendLayout();

            //  bool have_lab = this.posLabels.Any(l => l.Tag?.Equals(pos) ?? false);

            if (this.trackMarkers.ContainsKey(pos)) {
                lab = this.trackMarkers[pos];
                lab.Location = new System.Drawing.Point(x0, this.labSpeed.Location.Y);
            } else {
                lab.AutoSize = true;
                lab.BackColor = this.labMarker.BackColor;
                lab.FlatStyle = labMarker.FlatStyle;
                lab.Font = labMarker.Font;
                lab.ForeColor = clr;
                lab.Location = new System.Drawing.Point(x0, this.labMarker.Location.Y);
                lab.Name = String.Format("labMarker{0}", x0);
                lab.Size = labMarker.Size;
                lab.Anchor = labMarker.Anchor;
                lab.Text = labMarker.Text;
                lab.Tag = pos;
                lab.Visible = true;

                lab.Click += Label_Click;

                this.pnlVIDEO.Controls.Add(lab);

                // Ensure the dynamically added label is on top of other controls
                // (some video rendering controls can appear above child controls).
                lab.BringToFront();

                this.trackMarkers.Add(pos, lab);
            }

            //this.pnlVIDEO.ResumeLayout(false);
            //this.pnlVIDEO.PerformLayout();
            return lab;
        }

        private void Toggle_Theme(bool darkMode)
        {
            ApplyTheme(darkMode, this);
        }
        private void ApplyTheme(bool darkMode, Control parent)
        {
            Color bkgnd = darkMode ? Color.FromArgb(45, 45, 48) : SystemColors.Control;
            Color foregnd = darkMode ? Color.White : SystemColors.ControlText;

            Debug.WriteLine($"ApplyTheme( {parent.Name}");

            if (parent.Name == "pnlVIDEO") {
                parent.ForeColor = foregnd; parent.BackColor = bkgnd;
            } else if (parent.Name == "trackBarSpeed" || parent.Name == "trackBarJogShuttle" ||
                       parent.Name.Contains("labSpeed")) {
                parent.BackColor = darkMode ? Color.FromArgb(94, 94, 100) : Color.DimGray;
            } else if (parent.Name == "trackBarPlayHead" ||
                       parent.Name.Contains("labMarker")) {
                parent.BackColor = bkgnd;
            } else if (parent.Name.Contains("ToolStripMenuItem")) {
                parent.ForeColor = foregnd; parent.BackColor = bkgnd;
            } else if (parent.Name.StartsWith("but")) {
                bool useStyle = darkMode ? true : false;
                Color butColor = darkMode ? bkgnd : Color.Transparent;
                Color butHoverColor = darkMode ? Color.FromArgb(70, 70, 75) : Color.AntiqueWhite;
                if (parent is System.Windows.Forms.Button but) {
                //  parent.UseVisualStyleBackColor = useStyle;
                //  parent.BackColor = butColor;
                    but.FlatAppearance.MouseOverBackColor = butHoverColor;
                }
            }

            foreach (Control control in parent.Controls) {
                ApplyTheme(darkMode, control);
            }

            // Handle ToolStrip / MenuStrip items
            if (parent is MenuStrip menuStrip) {  // returns true if parent can be typecast to a MenuStrip

                Debug.WriteLine($"theming {menuStrip.Name}  (MenuStrip)");

                menuStrip.BackColor = bkgnd;
                menuStrip.ForeColor = foregnd;

                ApplyMenuTheme(darkMode, menuStrip);

                ApplyToolStripItems(darkMode, menuStrip.Items);

            } else if (parent is ToolStrip toolStrip) {  // returns true if parent can be typecast to a ToolStrip

                Debug.WriteLine($"theming {toolStrip.Name}  (ToolStrip)");

                toolStrip.BackColor = bkgnd;
                toolStrip.ForeColor = foregnd;

                ApplyToolstripTheme(darkMode, toolStrip);

                ApplyToolStripItems(darkMode, toolStrip.Items);
            }
        }

        private void ApplyToolStripItems(bool darkMode, ToolStripItemCollection items)
        {
            Color bkgnd = darkMode ? Color.FromArgb(45, 45, 48) : SystemColors.Control;
            Color foregnd = darkMode ? Color.White : SystemColors.ControlText;

            foreach (ToolStripItem item in items) {

                if (item is ToolStripMenuItem menuItem) {
                    Debug.WriteLine($"theming {item.Name}  (ToolStripMenuItem)");
                    item.BackColor = bkgnd;
                    item.ForeColor = foregnd;
                } else {
                    Debug.WriteLine($"theming {item.Name}  (ToolStripItem)");
                    item.BackColor = bkgnd;
                    item.ForeColor = foregnd;
                }

                if (item is ToolStripDropDownItem dropDownItem) {

                    Debug.WriteLine($"theming {dropDownItem.Name}  (ToolStripDropDownItem)");

                    dropDownItem.DropDown.BackColor = bkgnd;
                    dropDownItem.DropDown.ForeColor = foregnd;

                    ApplyToolStripItems(darkMode, dropDownItem.DropDownItems);
                }
            }
        }

        private void ApplyMenuTheme(bool darkMode, MenuStrip menuStrip)
        {
            Debug.WriteLine($"ApplyMenuTheme {menuStrip.Name}  (MenuStrip)");

            if (darkMode) {
                menuStrip.RenderMode = ToolStripRenderMode.Professional;
                menuStrip.Renderer =
                    new DarkMenuRenderer();
                //  new ToolStripProfessionalRenderer(new DarkColorTable());

            //  menuStrip.ForeColor = Color.White;

            } else {
                menuStrip.RenderMode = ToolStripRenderMode.ManagerRenderMode;
                menuStrip.Renderer =
                    new ToolStripProfessionalRenderer(new ProfessionalColorTable());

            //  menuStrip.ForeColor = SystemColors.ControlText;
            }
        }
        private void ApplyToolstripTheme(bool darkMode, ToolStrip toolStrip)
        {
            Debug.WriteLine($"ApplyToolstripTheme {toolStrip.Name}  (ToolStrip)");

            if (darkMode) {
                toolStrip.RenderMode = ToolStripRenderMode.Professional;
                toolStrip.Renderer =
                    new DarkMenuRenderer();
                //  new ToolStripProfessionalRenderer(new DarkColorTable());

                //  menuStrip.ForeColor = Color.White;

            } else {
                toolStrip.RenderMode = ToolStripRenderMode.ManagerRenderMode;
                toolStrip.Renderer =
                    new ToolStripProfessionalRenderer(new ProfessionalColorTable());

                //  menuStrip.ForeColor = SystemColors.ControlText;
            }
        }


        private void Label_Click(object? sender, EventArgs e)
        {
            Label lbl = (Label)sender;

            Console.WriteLine($"Clicked label: {lbl.Name}");
            Console.WriteLine($"Text: {lbl.Text}");

            double pos = (double)lbl.Tag;
            TimeSpan tpos = new TimeSpan(0);

            Set_Current_Pos(pos, tpos, update_playhead: true, seek_in_video: true);

        }

        private void trackBarSpeed_MouseDown(object sender, MouseEventArgs e)
        {
            trackBarSpeed_Begin();
        }
        private void trackBarSpeed_Begin()
        {
            Debug.WriteLine("---");
            Debug.WriteLine($"| trackBarSpeed_MouseDown");

            Reset_MouseDowns();

            this.speed_scrolling = true;

            System.Windows.Forms.TrackBar myTB = trackBarSpeed; //  (System.Windows.Forms.TrackBar)sender;

            DateTime now = DateTime.Now;

            if ((now - this.lasttrackBarSpeedMouseDown).TotalMilliseconds <= SystemInformation.DoubleClickTime) {

                // Double-click detected

                this.speed_scrolling = false;
                this.lasttrackBarSpeedMouseDown = DateTime.MinValue;

                myTB.Value = (int)(speed_1 * (double)trackBarSpeed.Maximum);   // set marker at 100% speed

                this.current_speed = 1.0;
                this.reverse_motion = false; // go back to forward motion
                this.super_slow = false;

                Start_Action("trackBarSpeed_MouseDown", $"SETRATE({this.current_speed})");
                this._mp?.SetRate((float)this.current_speed);
                // Application.DoEvents();

                Update_Value("reverse_motion");
                Update_Value("current_speed");
                Update_Value("mode");

                return;
            }

            this.start_run_state = this.CURRENT_RUN_STATE;

            this.hold_speed = this.current_speed;
            this.hold_reverse = this.reverse_motion;
            this.hold_playing = this.playing;
            this.hold_paused = this.paused;

            // this.reverse_motion = false; // go back to forward motion   <<<  let's see if we can change the reverse speed !!!

            // Update_Value("reverse_motion");

            // we are trying to move the play head.
            // we press play to force it to (re) load the video
            // and then press pause to go into paused mode

            if (this.video_loaded) {
                // Play + PAUSE
                Start_Action("trackBarSpeed_MouseDown", "PLAY");
                this._mp?.Play();  // play to force video to (re)load
                // Application.DoEvents();

                if (this.hold_playing && !this.hold_paused) {
                    //  Start_Action("trackBarSpeed_MouseDown","PAUSE");
                    //  this._mp?.Pause();  // <<< video was playing, leave it playing
                } else {
                    //  Start_Action("trackBarSpeed_MouseDown","PAUSE");
                    //  this._mp?.Pause();  // <<< video was paused, pause it
                }
                // Application.DoEvents();
            }
            this.lasttrackBarSpeedMouseDown = now;
        }


        private void trackBarSpeed_Scroll(object sender, EventArgs e)
        {
            Debug.WriteLine($"| trackBarSpeed_Scroll");

            if (!this.speed_scrolling) {
                trackBarSpeed_Begin();   // mousedown did not trigger !!!
            }

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

            if (this.video_loaded) {
                if (fr_rate >= 1.0) {
                    Start_Action("trackBarSpeed_Scroll", $"SETRATE({this.current_speed})");
                    this._mp?.SetRate((float)this.current_speed);
                    // Application.DoEvents();
                    this.super_slow = false;
                } else {
                    this.super_slow = true;
                }
            }
            Update_Value("current_speed");
            Update_Value("mode");
        }

        private void trackBarSpeed_MouseUp(object sender, MouseEventArgs e)
        {
            trackBarSpeed_End();
        }
        private void trackBarSpeed_End()
        {
            Debug.WriteLine($"| trackBarSpeed_MouseUp");

            this.speed_scrolling = false;

            // if video was paused before speed change, it is left paused now

            this.CURRENT_RUN_STATE = this.start_run_state;
            // this.super_slow = false;  << determined by playrate
            this.reverse_motion = this.hold_reverse;

            Update_Value("reverse_motion");
            Update_Value("current_speed");
            Update_Value("mode");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)  // override the OOTB handler
        {
            if (keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.J || keyData == Keys.K) {

                Control focused = GetFocusedControl(this);

                if ((focused?.Name == this.trackBarPlayHead.Name) && this.show_play_head) {  // if the trackbar has the focus AND mouse is still in trackbar
                    // Let the playhead TrackBar process Left/Right normally.
                    _updatePlayHeadCnt = 0;  // don't let trackBarPlayHead ignore the event
                    return base.ProcessCmdKey(ref msg, keyData);
                }
                if ((focused?.Name == this.trackBarSpeed.Name) && this.show_track_speed) {  // if the speed-control has the focus AND mouse is still in trackbar
                    // Let the playhead speed-trackbar process Left/Right normally.
                    return base.ProcessCmdKey(ref msg, keyData);
                }

                // Nobody else should process the arrow key.
                ShortcutEvent(this, new KeyEventArgs(keyData));

                return true;
            }

            bool handled = ShortcutEvent(this, new KeyEventArgs(keyData));

            if (handled) { 
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);  // pass it to the overridden handler
        }

        private Control GetFocusedControl(Control parent)
        {
            if (parent is ContainerControl container &&
                container.ActiveControl != null) {
                return GetFocusedControl(container.ActiveControl);  // keep calling ourselves until we find something in the hierarchy that is not a container
            }

            return parent;
        }

        public bool ShortcutEvent(object? sender, KeyEventArgs e)
        {
            bool handled = true;

            switch (e.KeyCode) {
                case Keys.Escape:
                    if (isFullscreen) {  // from fullscreen to window
                        //this.FormBorderStyle = FormBorderStyle.Sizable; // change form style
                        //this.WindowState = FormWindowState.Normal; // back to normal size
                        //this.Size = oldFormSize;
                        //menuStrip1.Visible = true; // the return of the menu strip 
                        //videoView1.Size = oldVideoSize; // make video the same size as the form
                        //videoView1.Location = oldVideoLocation; // remove the offset
                        //isFullscreen = false;
                    }
                    break;
                case Keys.Space: // Pause and Play
                    Button_Play_Pause();
                    break;
                case Keys.J: // skip 1% backwa
                case Keys.Left:
                    Button_Back_Start();
                    Button_Back_End();
                    break;
                case Keys.K: // skip 1% forwards
                case Keys.Right:
                    Button_Forward_Start();
                    Button_Forward_End();
                    break;
                case Keys.OemPeriod:
                    drawTrackMarker(this.current_pos, Color.Cyan);
                    break;
                default:
                    handled = false;
                    break;
            }
            return handled;
        }

        //private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        //{
        //    Debug.WriteLine("dataGridView1_MouseClick");
        //    this.show_info = !this.show_info;
        //    this.lock_info = true;
        //}

        // ================================================================================================================== INIT

        private void Form1_Load(object sender, EventArgs e)
        {
            Load_Buttons();

            Init_Data();

            this.darkModeToolStripMenuItem.Checked = true;

        //  Toggle_Theme(true);

        //  ToolStripManager.Renderer = new DarkMenuRenderer();


            this.pnlOverlay.Visible = false;
            this.labFPS.Visible = false;
            this.labSpeedIndicator.Visible = false;

            //  this.labFPS.Visible = false;

            //bool itWorked = SetStyle(this.labFPS, ControlStyles.SupportsTransparentBackColor, true);
            //this.labFPS.BackColor = Color.FromArgb(16, Color.Black);

            this.startWidth = this.pnlVideoFull.Width;
            this.startHeight = this.pnlVideoFull.Height;

            this.topSpeedMarker = 200;
            this.botSpeedMarker = this.trackBarSpeed.Height - 100;

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
                //  this.trackBarSpeed.Visible = false;
                show_hide_speed(false);
                this.trackBarJogShuttle.Visible = false;
                this.dataGridView1.Visible = false;
            }

            int x0 = (int)(0.5 * (double)this.pnlVIDEO.Width);
            int y0 = (int)(0.5 * (double)this.pnlVIDEO.Height);

            this.mouseVideoLocation = new System.Drawing.Point(x0, y0);

            Set_Play_State(play_state.stopped);

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
            //  this.KeyDown += new KeyEventHandler(ShortcutEvent);  KeyDown is now being processed by ProcessCmdKey
            //
            //                                                       if a key needs to be processed by ShortcutEvent
            //                                                       ProcessCmdKey calls ShortcutEvent and tells WinForms to ignore the event
            //                                                       otherwise key-event is passed to OOTB handler

            this.oldVideoSize = videoView1.Size;
            this.oldFormSize = this.Size;
            this.oldVideoLocation = this.videoView1.Location;

            var options = new[] { "--no-mouse-events", "--no-keyboard-events" };  // https://wiki.videolan.org/VLC_command-line_help/
            this._libVLC = new LibVLC(options);
            this._mp = new MediaPlayer(_libVLC);

            this._mp?.EnableMouseInput = false;
            this._mp?.EnableKeyInput = false;

            this._mp?.EndReached += MediaPlayer_EndReached;

            // this._mp?.EnableMouseInput = false;

            // this._mp?.Hwnd = this.pnlVIDEO.Handle;

            this.videoView1.MediaPlayer = _mp;

            this.show_track_speed = true;

            show_hide_speed(this.show_track_speed);  // speed control on left, no lock, so it should fade
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

            Update_Value("mode");

            this.toolStripMode.Text = "READY";
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
            string[] keys = { "current_pos", "current_time", "current_speed", "frame_rate", "width", "height", "zoom", "length", "reverse_motion", "mode" };
            string[] key_names = { "Current Position", "Current Time", "Current Speed", "Frame-Rate", "Width", "Height", "Zoom Factor", "Video Length", "Reverse-Motion", "Mode" };
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
                    case "zoom":
                        drNew["Units"] = " x Normal";
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
                if (row["Property"]?.ToString().Contains(key) ?? false) {
                    //string val = row["Value"]?.ToString() ?? "";
                    //string units = row["Units"]?.ToString() ?? "";
                    switch (key) {
                        case "current_pos":
                            row["Value"] = this.current_pos;
                            string pval = String.Format("{0:00000000} / {1:00000000}", this.current_pos, this.video_length_frames);
                            this.toolStripFrameNumber.Text = pval;
                            break;
                        case "current_time":
                            int tot_hrs = (int)Math.Floor(this.current_time.TotalHours);
                            int mins = this.current_time.Minutes;
                            int secs = this.current_time.Seconds;
                            int msecs = this.current_time.Milliseconds;
                            string valt = $"{tot_hrs:00}:{mins:00}:{secs:00}.{msecs:000}";
                            this.toolStripTimeOffset.Text = valt;
                            row["Value"] = valt;
                            //this.labSpeedIndicator.Text = s;
                            //this.labSpeedIndicator.BringToFront();
                            //this.labSpeedIndicator.Invalidate();
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
                        case "zoom":
                            double fact = (double)this.pnlVideoFull.Width / (double)this.videoZoomWidth;
                            if (fact < 1.01) {
                                this.toolStripZoom.Text = "";
                            } else {
                                this.toolStripZoom.Text = String.Format("  -  Zoom {0:0.00}   (double click to reset zoom)", fact);
                            }
                            row["Value"] = fact;
                            break;
                        case "length":
                            row["Value"] = this.video_length_frames;
                            string pval2 = String.Format("{0:0000000} / {1:0000000}", this.current_pos, this.video_length_frames);
                            this.toolStripFrameNumber.Text = pval2;
                            break;
                        case "current_speed":
                            row["Value"] = this.current_speed;
                            string tval = this.current_speed.ToString("0.000");
                            if (tval == "1.000" || tval == "1,000") {
                                tval = "normal speed";
                            } else {
                                tval += " x speed  (double click to reset)";
                            }
                            this.toolStripSpeed.Text = tval;
                            Update_Value("frame_rate");
                            break;
                        case "frame_rate":
                            double fr_rate = this.frame_rate * this.current_speed;
                            string frval = "";
                            string frunits = "";
                            if (fr_rate < 1.0) {
                                double secs_per_fr = 1.0 / fr_rate;
                                frval = secs_per_fr.ToString("0.0");
                                frunits = "seconds per frame";
                            } else {
                                frval = fr_rate.ToString("0.0");
                                frunits = "frames per second";
                            }
                            row["Value"] = frval;
                            row["Units"] = frunits;
                            this.toolStripTrackFPS.Text = $"{frval} {frunits}";
                            break;
                        case "reverse_motion":
                            row["Value"] = this.reverse_motion ? "True" : "False";
                            break;
                        case "mode":
                            string smode = "";
                            if (this.CURRENT_RUN_STATE == run_state.user || this.super_slow) {
                                if (this.single_framing_forward) {
                                    smode = ">>";
                                } else if (this.single_framing_backward) {
                                    smode = "<<";
                                } else {
                                    if (this.reverse_motion) {
                                        smode = "<< =";
                                    } else {
                                        smode = "<< = >>";
                                    }
                                }
                            } else {
                                smode = this.paused ? "PAUSED" : this.playing ? "PLAYING" : "STOPPED";
                                if (!this.paused && this.playing) {
                                    Debug.WriteLine("STOP HERE");
                                }
                            }
                            row["Value"] = smode;
                            this.toolStripMode.Text = smode;
                            break;
                    }
                    break;
                }
            }
        }

        // ================================================================================================================== PAN / ZOOM

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

            Update_Value("zoom");

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

                this._mp?.CropGeometry = geometryString;
                // Application.DoEvents();
            }
            this.pnlVideoZoom.Left = 0;
            this.pnlVideoZoom.Top = 0;
            this.pnlVideoZoom.Width = this.pnlVideoFull.Width;
            this.pnlVideoZoom.Height = this.pnlVideoFull.Height;

            Update_Value("zoom");
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
                    x1 += xdelta; x2 += xdelta;
                    y1 += ydelta; y2 += ydelta;
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

            if (this.video_loaded && this.video_dimensions.width > 0) {
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

                this._mp?.CropGeometry = geometryString;
                // Application.DoEvents();
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
            Update_Value("zoom");
        }

        private void pnlVIDEO_DoubleClick(object sender, EventArgs e)
        {
            Debug.WriteLine("| pnlVIDEO_DoubleClick");

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
                    // in TrackBarSpeed
                    if ((e.Y > this.topSpeedMarker) && (e.Y < this.botSpeedMarker)) {
                        if (!this.trackBarSpeed.Visible) {
                            show_hide_speed(true);
                        }
                        this.show_track_speed = true;
                    }
                    this.lastTrackSpeed = DateTime.Now;

                    this.show_info = false;
                    this.show_jog_shuttle = false;
                    this.show_play_head = false;
                    this.in_buttons = false;
                } else if ((e.X > (pnlVIDEO.Width - this.dataGridView1.Width - 15)) && (e.Y <= this.dataGridView1.Height)) {
                    // in INFO Box
                    if ((e.X > (pnlVIDEO.Width - 100)) && (e.Y < 100)) {
                        if (!this.dataGridView1.Visible)
                            this.dataGridView1.Visible = true;
                        this.show_info = true;
                    }
                    this.lastInfo = DateTime.Now;

                    this.show_track_speed = false;
                    this.show_jog_shuttle = false;
                    this.show_play_head = false;
                    this.in_buttons = false;
                } else if ((e.X > this.trackBarPlayHead.Left) && (e.Y > (trackBarJogShuttle.Top - 0))) {
                    // Shuttle & Playhead Area
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
                    this.in_buttons = false;
                } else if ((e.X < (this.trackBarPlayHead.Left)) && (e.Y > (this.butStop.Top - 10))) {
                    this.in_buttons = true;
                } else {
                    this.show_track_speed = false;
                    this.show_info = false;
                    this.show_jog_shuttle = false;
                    this.show_play_head = false;
                    this.in_buttons = false;
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
            Debug.WriteLine("| pnlVIDEO_MouseUp");
            this.dragging_box = false;
        }


        // ============================================================================ MediaPlayer Events

        private void MediaPlayer_EndReached(object? sender, EventArgs e)
        {
            Debug.WriteLine("| MediaPlayer_EndReached");

            if (this.playing || this.paused) {
                if (InvokeRequired) {
                    BeginInvoke(new Action(() =>
                    {
                        Set_Play_State(play_state.stopped);
                    }));
                } else {
                    Set_Play_State(play_state.stopped);
                }
            }
        }

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
            //  this.lock_play_head = !this.lock_play_head;
        }

        private void REH_VLC_Viewer_SizeChanged(object sender, EventArgs e)
        {
            drawSpeedMarkers();
            drawTrackMarkers();
        }

        private void videoView1_Click(object sender, EventArgs e)
        {

        }

        private void darkModeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            bool dark_mode = this.darkModeToolStripMenuItem.Checked;

            Toggle_Theme(dark_mode);
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