using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Richard_VLC
{
    public partial class SemiTransparentPanel : Panel
    {
        private const int WS_EX_TRANSPARENT = 0x20;
        private int _alpha = 125; // 0 (fully transparent) to 255 (fully opaque)

        [DefaultValue(125)]
        public int Alpha
        {
            get => _alpha;
            set { _alpha = Math.Clamp(value, 0, 255); Invalidate(); }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TRANSPARENT; // (tells Windows to paint underneath first)
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw the semi-transparent background color
            using (var brush = new SolidBrush(Color.FromArgb(_alpha, BackColor))) {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
            base.OnPaint(e);
        }
    }
}
