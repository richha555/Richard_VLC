namespace Richard_VLC
{
    partial class frmMarkers
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridView1 = new DataGridView();
            colFrameNum = new DataGridViewTextBoxColumn();
            colTimeOffset = new DataGridViewTextBoxColumn();
            colName = new DataGridViewTextBoxColumn();
            colDescr = new DataGridViewTextBoxColumn();
            colColor = new DataGridViewComboBoxColumn();
            colStartStop = new DataGridViewComboBoxColumn();
            colMarkerGuid = new DataGridViewTextBoxColumn();
            butNew = new Button();
            butLeft = new Button();
            butRight = new Button();
            butDel = new Button();
            butGoTo = new Button();
            butClose = new Button();
            butHere = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colFrameNum, colTimeOffset, colName, colDescr, colColor, colStartStop, colMarkerGuid });
            dataGridView1.Location = new Point(1, 1);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(799, 415);
            dataGridView1.TabIndex = 0;
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
            dataGridView1.RowEnter += dataGridView1_RowEnter;
            // 
            // colFrameNum
            // 
            colFrameNum.HeaderText = "Frame Number";
            colFrameNum.Name = "colFrameNum";
            // 
            // colTimeOffset
            // 
            colTimeOffset.HeaderText = "Time Offset";
            colTimeOffset.Name = "colTimeOffset";
            // 
            // colName
            // 
            colName.HeaderText = "Label";
            colName.Name = "colName";
            // 
            // colDescr
            // 
            colDescr.HeaderText = "Description";
            colDescr.Name = "colDescr";
            // 
            // colColor
            // 
            colColor.HeaderText = "Color";
            colColor.Name = "colColor";
            colColor.Resizable = DataGridViewTriState.True;
            colColor.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colStartStop
            // 
            colStartStop.HeaderText = "Start/Stop";
            colStartStop.Name = "colStartStop";
            colStartStop.Resizable = DataGridViewTriState.True;
            colStartStop.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colMarkerGuid
            // 
            colMarkerGuid.HeaderText = "GUID";
            colMarkerGuid.Name = "colMarkerGuid";
            colMarkerGuid.Visible = false;
            // 
            // butNew
            // 
            butNew.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            butNew.BackColor = Color.Honeydew;
            butNew.Location = new Point(10, 422);
            butNew.Name = "butNew";
            butNew.Size = new Size(101, 41);
            butNew.TabIndex = 1;
            butNew.Text = "New Marker @ Current Pos";
            butNew.UseVisualStyleBackColor = false;
            butNew.Click += butNew_Click;
            // 
            // butLeft
            // 
            butLeft.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            butLeft.Location = new Point(183, 422);
            butLeft.Name = "butLeft";
            butLeft.Size = new Size(85, 41);
            butLeft.TabIndex = 1;
            butLeft.Text = "<= Move Marker Left";
            butLeft.UseVisualStyleBackColor = true;
            butLeft.MouseDown += butLeft_MouseDown;
            butLeft.MouseUp += butLeft_MouseUp;
            // 
            // butRight
            // 
            butRight.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            butRight.Location = new Point(377, 422);
            butRight.Name = "butRight";
            butRight.Size = new Size(85, 41);
            butRight.TabIndex = 1;
            butRight.Text = "Move => Marker Right";
            butRight.UseVisualStyleBackColor = true;
            butRight.MouseDown += butRight_MouseDown;
            butRight.MouseUp += butRight_MouseUp;
            // 
            // butDel
            // 
            butDel.Anchor = AnchorStyles.Bottom;
            butDel.BackColor = Color.MistyRose;
            butDel.Location = new Point(484, 422);
            butDel.Name = "butDel";
            butDel.Size = new Size(71, 41);
            butDel.TabIndex = 1;
            butDel.Text = "Remove Marker";
            butDel.UseVisualStyleBackColor = false;
            butDel.Click += butDel_Click;
            // 
            // butGoTo
            // 
            butGoTo.Anchor = AnchorStyles.Bottom;
            butGoTo.Location = new Point(577, 422);
            butGoTo.Name = "butGoTo";
            butGoTo.Size = new Size(71, 41);
            butGoTo.TabIndex = 1;
            butGoTo.Text = "Goto Marker";
            butGoTo.UseVisualStyleBackColor = true;
            butGoTo.Click += butGoTo_Click;
            // 
            // butClose
            // 
            butClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            butClose.DialogResult = DialogResult.Cancel;
            butClose.Location = new Point(717, 422);
            butClose.Name = "butClose";
            butClose.Size = new Size(71, 41);
            butClose.TabIndex = 1;
            butClose.Text = "Close";
            butClose.UseVisualStyleBackColor = true;
            butClose.Click += butClose_Click;
            // 
            // butHere
            // 
            butHere.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            butHere.Location = new Point(274, 422);
            butHere.Name = "butHere";
            butHere.Size = new Size(97, 41);
            butHere.TabIndex = 1;
            butHere.Text = "Move Marker to Current Pos";
            butHere.UseVisualStyleBackColor = true;
            butHere.Click += butHere_Click;
            // 
            // frmMarkers
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 466);
            Controls.Add(butClose);
            Controls.Add(butGoTo);
            Controls.Add(butDel);
            Controls.Add(butRight);
            Controls.Add(butHere);
            Controls.Add(butLeft);
            Controls.Add(butNew);
            Controls.Add(dataGridView1);
            Name = "frmMarkers";
            Text = "frmMarkers";
            Load += frmMarkers_Load;
            Resize += frmMarkers_Resize;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridView1;
        private Button butNew;
        private Button butLeft;
        private Button butRight;
        private Button butDel;
        private Button butGoTo;
        private Button butClose;
        private Button butHere;
        private DataGridViewTextBoxColumn colFrameNum;
        private DataGridViewTextBoxColumn colTimeOffset;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colDescr;
        private DataGridViewComboBoxColumn colColor;
        private DataGridViewComboBoxColumn colStartStop;
        private DataGridViewTextBoxColumn colMarkerGuid;
    }
}