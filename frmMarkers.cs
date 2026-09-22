using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using LibVLCSharp.Shared;

namespace Richard_VLC
{
    public partial class frmMarkers : Form
    {
        public const int colGuid = 6;
        public event EventHandler Editor_NewMarker_at_CurrPos;
        public event EventHandler Editor_Move_Marker_to_CurrPos;
        public event EventHandler Editor_Move_Marker_Left_1;
        public event EventHandler Editor_Move_Marker_Right_1;
        public event EventHandler Editor_Remove_Marker;
        public event EventHandler Editor_GoTo_Marker;
        public event EventHandler Editor_Change_Marker_Color;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Form MainForm { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public cVideoMarkers MarkerList { get; set; }

        public cVideoMarker? Current_Marker = null;

        public bool _loading_data = true;

        public int last_form_width = -1;
        public frmMarkers()
        {
            InitializeComponent();
        }

        public void Display_MarkerList()
        {
            string[] colors = AppConstants.MarkerColors.ToArray<string>();
            string[] start_stop = { ">>>", "<<<", "" };

            _loading_data = true;

            var hold_cell = this.dataGridView1.CurrentCell;

            cVideoMarker? hold_marker = null;
            int hold_col = -1;
            int hold_row = -1;

            if (hold_cell != null) {
                hold_marker = Marker_on_Row(hold_cell.RowIndex);
                hold_col = hold_cell.ColumnIndex; // keep cursor in same column
            }

            int sel_row = -1;

            this.dataGridView1.Rows.Clear();
            foreach (cVideoMarker marker in this.MarkerList.markers) {
                DataGridViewRow row = new DataGridViewRow();

                row.CreateCells(this.dataGridView1);
                row.Cells[0].Value = marker.Position;
                row.Cells[1].Value = marker.Offset;
                row.Cells[2].Value = marker.Title;
                row.Cells[3].Value = marker.Description;

                DataGridViewComboBoxCell comboBoxColor = (row.Cells[4] as DataGridViewComboBoxCell);
                comboBoxColor.Items.AddRange(colors);

                row.Cells[4].Value = marker.Color.Name;

                DataGridViewComboBoxCell comboBoxSS = (row.Cells[5] as DataGridViewComboBoxCell);
                comboBoxSS.Items.AddRange(start_stop);

                row.Cells[5].Value = start_stop[(int)marker.StartStop];

                row.Cells[colGuid].Value = marker.MarkerGUID;

                row.HeaderCell.Value = string.Format("{0:000}", marker.MarkerID);

                int rowidx = this.dataGridView1.Rows.Add(row);

                if (this.Current_Marker != null) {
                    if (marker.MarkerGUID.Equals(this.Current_Marker.MarkerGUID)) {
                        Debug.WriteLine($"Display_MarkerList({rowidx}) - select # {marker.MarkerID} - {marker.Position} [ {marker.MarkerGUID.ToString()} ]");
                        sel_row = rowidx;
                    }
                } else if (hold_marker != null) {
                    if (marker.MarkerGUID.Equals(hold_marker.MarkerGUID)) {
                        sel_row = rowidx;
                    }
                }
            }

            if (sel_row >= 0) {
                if (hold_col < 0) hold_col = 0;
                this.dataGridView1.CurrentCell = this.dataGridView1[hold_col, sel_row];
            }
            _loading_data = false;
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void butNew_Click(object sender, EventArgs e)
        {
            //  create new marker at current position

            this.Editor_NewMarker_at_CurrPos?.Invoke(this, EventArgs.Empty);

        }

        private void butLeft_Click(object sender, EventArgs e)
        {
            this.Editor_Move_Marker_Left_1?.Invoke(this, EventArgs.Empty);
        }

        private void butHere_Click(object sender, EventArgs e)
        {
            this.Editor_Move_Marker_to_CurrPos?.Invoke(this, EventArgs.Empty);
        }

        private void butRight_Click(object sender, EventArgs e)
        {
            this.Editor_Move_Marker_Right_1?.Invoke(this, EventArgs.Empty);
        }

        private void butDel_Click(object sender, EventArgs e)
        {
            this.Editor_Remove_Marker?.Invoke(this, EventArgs.Empty);
        }

        private void butGoTo_Click(object sender, EventArgs e)
        {
            this.Editor_GoTo_Marker?.Invoke(this, EventArgs.Empty);
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (_loading_data) return;

            cVideoMarker? marker = Marker_on_Row(e.RowIndex);
            if (marker == null) { return; }
            this.Current_Marker = marker;

            Debug.WriteLine($"dataGridView1_RowEnter({e.RowIndex}) - select # {marker.MarkerID} - {marker.Position} [ {marker.MarkerGUID.ToString()} ]");
        }

        private cVideoMarker? Marker_on_Row(int row_num)
        {
            if (row_num < 0) return null;
            //string hdr = "";
            //try {
            //    hdr = this.dataGridView1.Rows[row_num].HeaderCell.Value as string;
            //} catch (Exception ex) { hdr = ""; }
            //if (string.IsNullOrWhiteSpace(hdr)) { return null; }
            //int id = -1;
            //if (!int.TryParse(hdr, out id)) id = -1;
            //cVideoMarker? marker = this.MarkerList.markers.FirstOrDefault(x => x.MarkerID == id);
            object? obj = this.dataGridView1[colGuid, row_num].Value;
            if (obj is Guid guid) {
                cVideoMarker? marker = this.MarkerList.markers.FirstOrDefault(x => x.MarkerGUID.Equals(guid));
                if (marker == null || marker == default(cVideoMarker)) { return null; }
                return marker;
            } else {
                return null;
            }
        }

        private int Row_with_Marker(cVideoMarker? marker)
        {
            if (marker == null) return -1;
            for (int r = 0; r < this.dataGridView1.RowCount; r++) {
                object? obj = this.dataGridView1[colGuid, r].Value;
                if (obj is Guid guid) {
                    if (guid.Equals(marker.MarkerGUID)) {
                        return r;
                    }
                }
            }
            return -1;
        }


        private void frmMarkers_Resize(object sender, EventArgs e)
        {
            if (this.last_form_width < 0) {
                this.last_form_width = this.Width;
                return;
            }

            Debug.WriteLine($"frmMarkers_Resize - Width = {this.Width}");

            int min_size = this.MinimumSize.Width; // width of all buttone + margin of 10

            int max_space = this.Width - min_size;  // space to divide over B, G, E & F

            // A butNew B butLeft C butHere D butRight E butDel F butGoTo G butClose H

            int A = 10;
            int C = 10;
            int D = 10;
            int H = 10;

            int big_spc = (int)(2.0 * (double)max_space / 3.0);
            int sm_spc = max_space - big_spc;

            int B = (int)(0.5 * (double)big_spc);
            int G = B;

            int E = (int)(0.5 * (double)sm_spc);
            int F = E;

            if (this.Width < 718) {
                E = 10; F = 10;
                big_spc = max_space - E - F;
                B = (int)(0.5 * (double)big_spc);
                G = B;
            }

            B = Math.Max(10, B); G = B;
            E = Math.Max(10, E); F = E;

            int delta = this.Width - this.last_form_width;
            // this.butNew.Left
            this.butLeft.Left = this.butNew.Left + this.butNew.Width + B;
            this.butHere.Left = this.butLeft.Left + this.butLeft.Width + C;
            this.butRight.Left = this.butHere.Left + this.butHere.Width + D;
            this.butDel.Left = this.butRight.Left + this.butRight.Width + E;
            this.butGoTo.Left = this.butDel.Left + this.butDel.Width + F;
            // this.butClose
            this.last_form_width = this.Width;
        }

        private void frmMarkers_Load(object sender, EventArgs e)
        {
            this.last_form_width = this.Width;

            int max_wdt =
                10 +
                this.butNew.Width + 10 +
                this.butLeft.Width + 10 +
                this.butHere.Width + 10 +
                this.butRight.Width + 10 +
                this.butDel.Width + 10 +
                this.butGoTo.Width + 10 +
                this.butClose.Width +
                10;
            max_wdt = Math.Max(679, max_wdt);
            int max_hgt = 160;
            this.MinimumSize = new System.Drawing.Size(max_wdt, max_hgt);

            // A butNew B butLeft C butHere D butRight E butDel F butGoTo G butClose H

            this.dataGridView1.RowHeadersWidth = 71;
            this.colFrameNum.Width = 63;
            this.colName.Width = 89;
            this.colDescr.Width = 261;

            dataGridView1.TopLeftHeaderCell.Value = "#";
            dataGridView1.TopLeftHeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView1.EnableHeadersVisualStyles = false;

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(45, 45, 48);

            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            dataGridView1.RowHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(45, 45, 48);

            dataGridView1.RowHeadersDefaultCellStyle.ForeColor =
                Color.White;

            _loading_data = false;
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_loading_data) return;

            bool res = false;

            int rowidx = e.RowIndex;
            int colidx = e.ColumnIndex;

            cVideoMarker? marker = Marker_on_Row(rowidx);
            if (marker == null) { return; }

            object? val = this.dataGridView1[colidx, rowidx].Value;

            string sval = "";
            if (val != null) { sval = val.ToString(); }

            switch (colidx) {
                case 0:
                    double pos;
                    if (val != null) {
                        if (double.TryParse(sval, out pos)) {
                            marker.Position = pos;
                            res = true;
                        }
                    }
                    if (!res) {
                        // invalid position entered
                        _loading_data = true;
                        this.dataGridView1[colidx, rowidx].Value = marker.Position;
                        _loading_data = false;
                    }
                    break;
                case 1:
                    res = marker.ParseOffset(sval);
                    if (!res) {
                        // invalid offset entered
                        _loading_data = true;
                        this.dataGridView1[colidx, rowidx].Value = marker.Offset;
                        _loading_data = false;
                    }
                    break;
                case 2:
                    if (!string.IsNullOrWhiteSpace(sval)) {
                        marker.Title = sval;
                        res = true;
                    }
                    if (!res) {
                        // empty title is not allowed
                        _loading_data = true;
                        this.dataGridView1[colidx, rowidx].Value = marker.Title;
                        _loading_data = false;
                    }
                    break;
                case 3:
                    marker.Description = sval;
                    res = true;
                    break;
                case 4:
                    res = marker.ParseColor(sval);
                    if (res) {
                        this.Editor_Change_Marker_Color?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                case 5:
                    if (sval.Contains(">")) {
                        marker.StartStop = eStartStop.BEGIN;
                        res = true;
                        // clear any other start markers
                        foreach (cVideoMarker m in this.MarkerList.markers) {
                            if (m.StartStop == eStartStop.BEGIN) {
                                if (!m.MarkerGUID.Equals(marker.MarkerGUID)) {
                                    m.StartStop = eStartStop.NONE;
                                    // col,row => ""
                                    int r = Row_with_Marker(m);
                                    if (r >= 0) {
                                        _loading_data = true;
                                        this.dataGridView1[colidx, r].Value = "";
                                        _loading_data = false;
                                    }
                                }
                            }
                        }
                    } else if (sval.Contains("<")) {
                        marker.StartStop = eStartStop.END;
                        res = true;
                        // clear any other end markers
                        foreach (cVideoMarker m in this.MarkerList.markers) {
                            if (m.StartStop == eStartStop.END) {
                                if (!m.MarkerGUID.Equals(marker.MarkerGUID)) {
                                    m.StartStop = eStartStop.NONE;
                                    // col,row => ""
                                    int r = Row_with_Marker(m);
                                    if (r >= 0) {
                                        _loading_data = true;
                                        this.dataGridView1[colidx, r].Value = "";
                                        _loading_data = false;
                                    }
                                }
                            }
                        }
                    } else if (string.IsNullOrWhiteSpace(sval)) {
                        marker.StartStop = eStartStop.NONE;
                        res = true;
                    } else {
                        // user entered invalid value ...
                        _loading_data = true;
                        this.dataGridView1[colidx, rowidx].Value = "";
                        _loading_data = false;
                    }
                    break;
                //case 6:
                //    marker.MarkerGUID = sval;
                //    break;
            }
        }
    }
}
