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
            butBegin.Location = new Point(3, 566);
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
            butSingleBack.Location = new Point(39, 566);
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
            butPlay.Location = new Point(75, 566);
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
            butSingleFwd.Location = new Point(111, 566);
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
            butStop.Location = new Point(147, 566);
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
            trackBarPlayHead.Location = new Point(186, 566);
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
            trackBarSpeed.Size = new Size(45, 560);
            trackBarSpeed.TabIndex = 1;
            trackBarSpeed.Click += trackBarSpeed_Click;
            trackBarSpeed.Scroll += trackBarSpeed_Scroll;
            trackBarSpeed.MouseDown += trackBarSpeed_MouseDown;
            // 
            // pnlVIDEO
            // 
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
            pnlVIDEO.Size = new Size(1146, 603);
            pnlVIDEO.TabIndex = 3;
            pnlVIDEO.DoubleClick += pnlVIDEO_DoubleClick;
            pnlVIDEO.MouseDown += pnlVIDEO_MouseDown;
            pnlVIDEO.MouseMove += pnlVIDEO_MouseMove;
            pnlVIDEO.MouseUp += pnlVIDEO_MouseUp;
            pnlVIDEO.MouseWheel += pnlVIDEO_MouseWheel;
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
            dataGridView1.Location = new Point(820, 7);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Size = new Size(322, 254);
            dataGridView1.TabIndex = 4;
            dataGridView1.Click += dataGridView1_Click;
            // 
            // trackBarJogShuttle
            // 
            trackBarJogShuttle.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trackBarJogShuttle.BackColor = Color.DimGray;
            trackBarJogShuttle.Location = new Point(465, 534);
            trackBarJogShuttle.Name = "trackBarJogShuttle";
            trackBarJogShuttle.Size = new Size(335, 45);
            trackBarJogShuttle.TabIndex = 1;
            trackBarJogShuttle.Click += trackBarJogShuttle_Click;
            trackBarJogShuttle.Scroll += trackBarJogShuttle_Scroll;
            trackBarJogShuttle.MouseUp += trackBarJogShuttle_MouseUp;
            // 
            // videoView1
            // 
            videoView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            videoView1.BackColor = Color.Black;
            videoView1.Location = new Point(3, 0);
            videoView1.MediaPlayer = null;
            videoView1.Name = "videoView1";
            videoView1.Size = new Size(1143, 560);
            videoView1.TabIndex = 5;
            videoView1.Text = "videoView1";
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
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
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
            openToolStripMenuItem.ImageTransparentColor = Color.Magenta;
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
            ClientSize = new Size(1146, 627);
            Controls.Add(pnlVIDEO);
            Controls.Add(menuStrip1);
            Name = "REH_VLC_Viewer";
            Text = "Richard's VLC Viewer";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)trackBarPlayHead).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarSpeed).EndInit();
            pnlVIDEO.ResumeLayout(false);
            pnlVIDEO.PerformLayout();
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
    }
}
