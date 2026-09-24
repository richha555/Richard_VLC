namespace Richard_VLC
{
    partial class REH_VLC_Viewer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(REH_VLC_Viewer));
            butBegin = new Button();
            butSingleBack = new Button();
            butPlay = new Button();
            butSingleFwd = new Button();
            butStop = new Button();
            trackBarPlayHead = new TrackBar();
            trackBarSpeed = new TrackBar();
            pnlVIDEO = new Panel();
            statusStrip1 = new StatusStrip();
            toolStripTrackFPS = new ToolStripStatusLabel();
            toolStripStatusDiv1 = new ToolStripStatusLabel();
            toolStripSpeed = new ToolStripStatusLabel();
            toolStripSpring1 = new ToolStripStatusLabel();
            toolStripMode = new ToolStripStatusLabel();
            toolStripZoom = new ToolStripStatusLabel();
            toolStripSpring2 = new ToolStripStatusLabel();
            toolStripTimeOffset = new ToolStripStatusLabel();
            toolStripStatusDiv2 = new ToolStripStatusLabel();
            toolStripFrameNumber = new ToolStripStatusLabel();
            pnlOverlay = new SemiTransparentPanel();
            labSpeedIndicator = new TransparentLabel();
            labFPS = new Label();
            labMarker = new Label();
            labSpeed = new Label();
            pnlVideoFull = new Panel();
            pnlVideoZoom = new Panel();
            dataGridView1 = new DataGridView();
            trackBarJogShuttle = new TrackBar();
            videoView1 = new LibVLCSharp.WinForms.VideoView();
            imageList1 = new ImageList(components);
            toolTip1 = new ToolTip(components);
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            editMarkersToolStripMenuItem = new ToolStripMenuItem();
            darkModeToolStripMenuItem = new ToolStripMenuItem();
            videoNavMenuItem1 = new ToolStripMenuItem();
            infoBoxMenuItem2 = new ToolStripMenuItem();
            contentsToolStripMenuItem = new ToolStripMenuItem();
            indexToolStripMenuItem = new ToolStripMenuItem();
            searchToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            customizeToolStripMenuItem = new ToolStripMenuItem();
            optionsToolStripMenuItem = new ToolStripMenuItem();
            undoToolStripMenuItem = new ToolStripMenuItem();
            redoToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            cutToolStripMenuItem = new ToolStripMenuItem();
            copyToolStripMenuItem = new ToolStripMenuItem();
            pasteToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            selectAllToolStripMenuItem = new ToolStripMenuItem();
            openFileDialog1 = new OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)trackBarPlayHead).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarSpeed).BeginInit();
            pnlVIDEO.SuspendLayout();
            statusStrip1.SuspendLayout();
            pnlVideoFull.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarJogShuttle).BeginInit();
            ((System.ComponentModel.ISupportInitialize)videoView1).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // butBegin
            // 
            butBegin.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            butBegin.BackColor = Color.Transparent;
            butBegin.Location = new Point(3, 564);
            butBegin.Name = "butBegin";
            butBegin.Size = new Size(34, 34);
            butBegin.TabIndex = 0;
            butBegin.Text = "|<";
            butBegin.UseVisualStyleBackColor = false;
            butBegin.Click += butBegin_Click;
            // 
            // butSingleBack
            // 
            butSingleBack.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            butSingleBack.BackColor = Color.Transparent;
            butSingleBack.Location = new Point(39, 564);
            butSingleBack.Name = "butSingleBack";
            butSingleBack.Size = new Size(34, 34);
            butSingleBack.TabIndex = 0;
            butSingleBack.Text = "<<";
            butSingleBack.UseVisualStyleBackColor = false;
            butSingleBack.Click += butSingleBack_Click;
            butSingleBack.MouseDown += butSingleBack_MouseDown;
            butSingleBack.MouseUp += butSingleBack_MouseUp;
            // 
            // butPlay
            // 
            butPlay.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            butPlay.BackColor = Color.Transparent;
            butPlay.Location = new Point(75, 564);
            butPlay.Name = "butPlay";
            butPlay.Size = new Size(34, 34);
            butPlay.TabIndex = 0;
            butPlay.Text = ">";
            butPlay.UseVisualStyleBackColor = false;
            butPlay.Click += butPlay_Click;
            // 
            // butSingleFwd
            // 
            butSingleFwd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            butSingleFwd.BackColor = Color.Transparent;
            butSingleFwd.Location = new Point(111, 564);
            butSingleFwd.Name = "butSingleFwd";
            butSingleFwd.Size = new Size(34, 34);
            butSingleFwd.TabIndex = 0;
            butSingleFwd.Text = ">>";
            butSingleFwd.UseVisualStyleBackColor = false;
            butSingleFwd.Click += butSingleFwd_Click;
            butSingleFwd.MouseDown += butSingleFwd_MouseDown;
            butSingleFwd.MouseUp += butSingleFwd_MouseUp;
            // 
            // butStop
            // 
            butStop.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            butStop.BackColor = Color.Transparent;
            butStop.Location = new Point(147, 564);
            butStop.Name = "butStop";
            butStop.Size = new Size(34, 34);
            butStop.TabIndex = 0;
            butStop.Text = "X";
            butStop.UseVisualStyleBackColor = false;
            butStop.Click += butStop_Click;
            // 
            // trackBarPlayHead
            // 
            trackBarPlayHead.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trackBarPlayHead.BackColor = SystemColors.Control;
            trackBarPlayHead.Location = new Point(186, 561);
            trackBarPlayHead.Name = "trackBarPlayHead";
            trackBarPlayHead.Size = new Size(956, 45);
            trackBarPlayHead.TabIndex = 1;
            trackBarPlayHead.Click += trackBarPlayHead_Click;
            trackBarPlayHead.Scroll += trackBarPlayHead_Scroll;
            // 
            // trackBarSpeed
            // 
            trackBarSpeed.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            trackBarSpeed.BackColor = Color.DimGray;
            trackBarSpeed.LargeChange = 10;
            trackBarSpeed.Location = new Point(3, 0);
            trackBarSpeed.Maximum = 100;
            trackBarSpeed.Name = "trackBarSpeed";
            trackBarSpeed.Orientation = Orientation.Vertical;
            trackBarSpeed.Size = new Size(45, 555);
            trackBarSpeed.TabIndex = 1;
            trackBarSpeed.Click += trackBarSpeed_Click;
            trackBarSpeed.Scroll += trackBarSpeed_Scroll;
            trackBarSpeed.MouseDown += trackBarSpeed_MouseDown;
            // 
            // pnlVIDEO
            // 
            pnlVIDEO.BackColor = SystemColors.Control;
            pnlVIDEO.Controls.Add(statusStrip1);
            pnlVIDEO.Controls.Add(pnlOverlay);
            pnlVIDEO.Controls.Add(labSpeedIndicator);
            pnlVIDEO.Controls.Add(labFPS);
            pnlVIDEO.Controls.Add(labMarker);
            pnlVIDEO.Controls.Add(labSpeed);
            pnlVIDEO.Controls.Add(pnlVideoFull);
            pnlVIDEO.Controls.Add(dataGridView1);
            pnlVIDEO.Controls.Add(trackBarSpeed);
            pnlVIDEO.Controls.Add(trackBarPlayHead);
            pnlVIDEO.Controls.Add(butBegin);
            pnlVIDEO.Controls.Add(butSingleBack);
            pnlVIDEO.Controls.Add(butPlay);
            pnlVIDEO.Controls.Add(butSingleFwd);
            pnlVIDEO.Controls.Add(butStop);
            pnlVIDEO.Controls.Add(trackBarJogShuttle);
            pnlVIDEO.Controls.Add(videoView1);
            pnlVIDEO.Dock = DockStyle.Fill;
            pnlVIDEO.Location = new Point(0, 24);
            pnlVIDEO.Name = "pnlVIDEO";
            pnlVIDEO.Size = new Size(1146, 627);
            pnlVIDEO.TabIndex = 3;
            pnlVIDEO.DoubleClick += pnlVIDEO_DoubleClick;
            pnlVIDEO.MouseDown += pnlVIDEO_MouseDown;
            pnlVIDEO.MouseMove += pnlVIDEO_MouseMove;
            pnlVIDEO.MouseUp += pnlVIDEO_MouseUp;
            pnlVIDEO.MouseWheel += pnlVIDEO_MouseWheel;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripTrackFPS, toolStripStatusDiv1, toolStripSpeed, toolStripSpring1, toolStripMode, toolStripZoom, toolStripSpring2, toolStripTimeOffset, toolStripStatusDiv2, toolStripFrameNumber });
            statusStrip1.Location = new Point(0, 605);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1146, 22);
            statusStrip1.TabIndex = 11;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripTrackFPS
            // 
            toolStripTrackFPS.Name = "toolStripTrackFPS";
            toolStripTrackFPS.Size = new Size(0, 17);
            // 
            // toolStripStatusDiv1
            // 
            toolStripStatusDiv1.Name = "toolStripStatusDiv1";
            toolStripStatusDiv1.Size = new Size(24, 17);
            toolStripStatusDiv1.Text = "  -  ";
            // 
            // toolStripSpeed
            // 
            toolStripSpeed.Name = "toolStripSpeed";
            toolStripSpeed.Size = new Size(0, 17);
            // 
            // toolStripSpring1
            // 
            toolStripSpring1.Name = "toolStripSpring1";
            toolStripSpring1.Size = new Size(541, 17);
            toolStripSpring1.Spring = true;
            // 
            // toolStripMode
            // 
            toolStripMode.Name = "toolStripMode";
            toolStripMode.Size = new Size(0, 17);
            // 
            // toolStripZoom
            // 
            toolStripZoom.Name = "toolStripZoom";
            toolStripZoom.Size = new Size(0, 17);
            // 
            // toolStripSpring2
            // 
            toolStripSpring2.Name = "toolStripSpring2";
            toolStripSpring2.Size = new Size(541, 17);
            toolStripSpring2.Spring = true;
            // 
            // toolStripTimeOffset
            // 
            toolStripTimeOffset.Name = "toolStripTimeOffset";
            toolStripTimeOffset.Size = new Size(0, 17);
            // 
            // toolStripStatusDiv2
            // 
            toolStripStatusDiv2.Name = "toolStripStatusDiv2";
            toolStripStatusDiv2.Size = new Size(24, 17);
            toolStripStatusDiv2.Text = "  -  ";
            // 
            // toolStripFrameNumber
            // 
            toolStripFrameNumber.Name = "toolStripFrameNumber";
            toolStripFrameNumber.Size = new Size(0, 17);
            // 
            // pnlOverlay
            // 
            pnlOverlay.BackColor = Color.Red;
            pnlOverlay.Location = new Point(384, 57);
            pnlOverlay.Name = "pnlOverlay";
            pnlOverlay.Size = new Size(200, 100);
            pnlOverlay.TabIndex = 10;
            // 
            // labSpeedIndicator
            // 
            labSpeedIndicator.AutoSize = true;
            labSpeedIndicator.BackColor = Color.Transparent;
            labSpeedIndicator.Font = new Font("Consolas", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labSpeedIndicator.Location = new Point(361, 295);
            labSpeedIndicator.Name = "labSpeedIndicator";
            labSpeedIndicator.Size = new Size(180, 22);
            labSpeedIndicator.TabIndex = 9;
            labSpeedIndicator.Text = "transparentLabel1";
            // 
            // labFPS
            // 
            labFPS.AutoSize = true;
            labFPS.Font = new Font("Consolas", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labFPS.Location = new Point(471, 225);
            labFPS.Name = "labFPS";
            labFPS.Size = new Size(70, 22);
            labFPS.TabIndex = 8;
            labFPS.Text = "15 f/s";
            // 
            // labMarker
            // 
            labMarker.Anchor = AnchorStyles.Bottom;
            labMarker.AutoSize = true;
            labMarker.BackColor = SystemColors.Control;
            labMarker.FlatStyle = FlatStyle.Flat;
            labMarker.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labMarker.ForeColor = Color.Red;
            labMarker.Location = new Point(235, 592);
            labMarker.Name = "labMarker";
            labMarker.Size = new Size(15, 15);
            labMarker.TabIndex = 7;
            labMarker.Text = "^";
            labMarker.Visible = false;
            labMarker.MouseDown += labMarker_MouseDown;
            labMarker.MouseMove += labMarker_MouseMove;
            labMarker.MouseUp += labMarker_MouseUp;
            // 
            // labSpeed
            // 
            labSpeed.AutoSize = true;
            labSpeed.BackColor = Color.DimGray;
            labSpeed.FlatStyle = FlatStyle.Flat;
            labSpeed.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labSpeed.ForeColor = Color.Red;
            labSpeed.Location = new Point(32, 258);
            labSpeed.Name = "labSpeed";
            labSpeed.Size = new Size(15, 15);
            labSpeed.TabIndex = 6;
            labSpeed.Text = "<";
            labSpeed.Visible = false;
            // 
            // pnlVideoFull
            // 
            pnlVideoFull.BackColor = Color.DimGray;
            pnlVideoFull.Controls.Add(pnlVideoZoom);
            pnlVideoFull.Location = new Point(60, 7);
            pnlVideoFull.Name = "pnlVideoFull";
            pnlVideoFull.Size = new Size(261, 200);
            pnlVideoFull.TabIndex = 3;
            pnlVideoFull.MouseClick += pnlVideoFull_MouseClick;
            // 
            // pnlVideoZoom
            // 
            pnlVideoZoom.BackColor = Color.DarkGray;
            pnlVideoZoom.Location = new Point(51, 50);
            pnlVideoZoom.Name = "pnlVideoZoom";
            pnlVideoZoom.Size = new Size(123, 89);
            pnlVideoZoom.TabIndex = 0;
            pnlVideoZoom.MouseClick += pnlVideoFull_MouseClick;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(824, 7);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Size = new Size(318, 276);
            dataGridView1.TabIndex = 4;
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            dataGridView1.Click += dataGridView1_Click;
            // 
            // trackBarJogShuttle
            // 
            trackBarJogShuttle.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trackBarJogShuttle.BackColor = Color.DimGray;
            trackBarJogShuttle.Location = new Point(470, 532);
            trackBarJogShuttle.Name = "trackBarJogShuttle";
            trackBarJogShuttle.Size = new Size(335, 45);
            trackBarJogShuttle.TabIndex = 1;
            trackBarJogShuttle.Click += trackBarJogShuttle_Click;
            trackBarJogShuttle.Scroll += trackBarJogShuttle_Scroll;
            trackBarJogShuttle.MouseDown += trackBarJogShuttle_MouseDown;
            trackBarJogShuttle.MouseUp += trackBarJogShuttle_MouseUp;
            // 
            // videoView1
            // 
            videoView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            videoView1.BackColor = Color.Black;
            videoView1.Location = new Point(3, 0);
            videoView1.MediaPlayer = null;
            videoView1.Name = "videoView1";
            videoView1.Size = new Size(1143, 555);
            videoView1.TabIndex = 5;
            videoView1.Text = "videoView1";
            videoView1.Click += videoView1_Click;
            videoView1.DoubleClick += pnlVIDEO_DoubleClick;
            videoView1.MouseDown += pnlVIDEO_MouseDown;
            videoView1.MouseMove += pnlVIDEO_MouseMove;
            videoView1.MouseUp += pnlVIDEO_MouseUp;
            videoView1.MouseWheel += pnlVIDEO_MouseWheel;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageSize = new Size(16, 16);
            imageList1.TransparentColor = Color.Transparent;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, settingsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1146, 24);
            menuStrip1.TabIndex = 7;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, toolStripSeparator, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Image = (Image)resources.GetObject("openToolStripMenuItem.Image");
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openToolStripMenuItem.Size = new Size(146, 22);
            openToolStripMenuItem.Text = "&Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // toolStripSeparator
            // 
            toolStripSeparator.Name = "toolStripSeparator";
            toolStripSeparator.Size = new Size(143, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(146, 22);
            exitToolStripMenuItem.Text = "E&xit";
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { editMarkersToolStripMenuItem, darkModeToolStripMenuItem, videoNavMenuItem1, infoBoxMenuItem2 });
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(61, 20);
            settingsToolStripMenuItem.Text = "&Settings";
            // 
            // editMarkersToolStripMenuItem
            // 
            editMarkersToolStripMenuItem.Name = "editMarkersToolStripMenuItem";
            editMarkersToolStripMenuItem.Size = new Size(191, 22);
            editMarkersToolStripMenuItem.Text = "Edit Markers...";
            editMarkersToolStripMenuItem.Click += editMarkersToolStripMenuItem_Click;
            // 
            // darkModeToolStripMenuItem
            // 
            darkModeToolStripMenuItem.CheckOnClick = true;
            darkModeToolStripMenuItem.Name = "darkModeToolStripMenuItem";
            darkModeToolStripMenuItem.Size = new Size(191, 22);
            darkModeToolStripMenuItem.Text = "Dark Mode";
            darkModeToolStripMenuItem.CheckedChanged += darkModeToolStripMenuItem_CheckedChanged;
            // 
            // videoNavMenuItem1
            // 
            videoNavMenuItem1.CheckOnClick = true;
            videoNavMenuItem1.Name = "videoNavMenuItem1";
            videoNavMenuItem1.Size = new Size(191, 22);
            videoNavMenuItem1.Text = "Show Video Navigator";
            videoNavMenuItem1.CheckedChanged += videoNavMenuItem1_CheckedChanged;
            // 
            // infoBoxMenuItem2
            // 
            infoBoxMenuItem2.CheckOnClick = true;
            infoBoxMenuItem2.Name = "infoBoxMenuItem2";
            infoBoxMenuItem2.Size = new Size(191, 22);
            infoBoxMenuItem2.Text = "Show Info Panel";
            infoBoxMenuItem2.CheckedChanged += infoBoxMenuItem2_CheckedChanged;
            // 
            // contentsToolStripMenuItem
            // 
            contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
            contentsToolStripMenuItem.Size = new Size(32, 19);
            // 
            // indexToolStripMenuItem
            // 
            indexToolStripMenuItem.Name = "indexToolStripMenuItem";
            indexToolStripMenuItem.Size = new Size(32, 19);
            // 
            // searchToolStripMenuItem
            // 
            searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            searchToolStripMenuItem.Size = new Size(32, 19);
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(6, 6);
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(32, 19);
            // 
            // customizeToolStripMenuItem
            // 
            customizeToolStripMenuItem.Name = "customizeToolStripMenuItem";
            customizeToolStripMenuItem.Size = new Size(32, 19);
            // 
            // optionsToolStripMenuItem
            // 
            optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            optionsToolStripMenuItem.Size = new Size(32, 19);
            // 
            // undoToolStripMenuItem
            // 
            undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            undoToolStripMenuItem.Size = new Size(32, 19);
            // 
            // redoToolStripMenuItem
            // 
            redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            redoToolStripMenuItem.Size = new Size(32, 19);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 6);
            // 
            // cutToolStripMenuItem
            // 
            cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            cutToolStripMenuItem.Size = new Size(32, 19);
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new Size(32, 19);
            // 
            // pasteToolStripMenuItem
            // 
            pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            pasteToolStripMenuItem.Size = new Size(32, 19);
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            selectAllToolStripMenuItem.Size = new Size(32, 19);
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // REH_VLC_Viewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1146, 651);
            Controls.Add(pnlVIDEO);
            Controls.Add(menuStrip1);
            Name = "REH_VLC_Viewer";
            Text = "Richard's VLC Viewer";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            SizeChanged += REH_VLC_Viewer_SizeChanged;
            ((System.ComponentModel.ISupportInitialize)trackBarPlayHead).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarSpeed).EndInit();
            pnlVIDEO.ResumeLayout(false);
            pnlVIDEO.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            pnlVideoFull.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarJogShuttle).EndInit();
            ((System.ComponentModel.ISupportInitialize)videoView1).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button butBegin;
        private Button butSingleBack;
        private Button butPlay;
        private Button butSingleFwd;
        private Button butStop;
        private TrackBar trackBarPlayHead;
        private TrackBar trackBarSpeed;
        private Panel pnlVIDEO;
        private Panel pnlVideoFull;
     // private SemiTransparentPanel pnlVideoFull;
        private Panel pnlVideoZoom;
        private TrackBar trackBarJogShuttle;
        private DataGridView dataGridView1;
        private ImageList imageList1;
        private ToolTip toolTip1;
        private LibVLCSharp.WinForms.VideoView videoView1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator;
        private ToolStripMenuItem contentsToolStripMenuItem;
        private ToolStripMenuItem indexToolStripMenuItem;
        private ToolStripMenuItem searchToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem customizeToolStripMenuItem;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem undoToolStripMenuItem;
        private ToolStripMenuItem redoToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem cutToolStripMenuItem;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem pasteToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem selectAllToolStripMenuItem;
        private OpenFileDialog openFileDialog1;
        private Label labSpeed;
        private Label labMarker;
        private Label labFPS;
        private TransparentLabel labSpeedIndicator;
        private SemiTransparentPanel pnlOverlay;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripTrackFPS;
        private ToolStripStatusLabel toolStripTimeOffset;
        private ToolStripStatusLabel toolStripFrameNumber;
        private ToolStripStatusLabel toolStripSpeed;
        private ToolStripStatusLabel toolStripMode;
        private ToolStripStatusLabel toolStripStatusDiv1;
        private ToolStripStatusLabel toolStripStatusDiv2;
        private ToolStripStatusLabel toolStripSpring1;
        private ToolStripStatusLabel toolStripZoom;
        private ToolStripStatusLabel toolStripSpring2;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem darkModeToolStripMenuItem;
        private ToolStripMenuItem editMarkersToolStripMenuItem;
        private ToolStripMenuItem videoNavMenuItem1;
        private ToolStripMenuItem infoBoxMenuItem2;
    }
}
