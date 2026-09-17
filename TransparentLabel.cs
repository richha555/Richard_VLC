using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;

namespace Richard_VLC
{
    public partial class TransparentLabel : Label
    {
        // Define the Win32 extended window style constant
        private const int WS_EX_TRANSPARENT = 0x20;

        //public TransparentLabel()
        //{
        //    SetStyle(
        //        ControlStyles.UserPaint |
        //        ControlStyles.SupportsTransparentBackColor |
        //        ControlStyles.OptimizedDoubleBuffer |
        //        ControlStyles.AllPaintingInWmPaint,
        //        true);

        //    BackColor = Color.Transparent;
        //}
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TRANSPARENT; // (tells Windows to paint underneath first)
                return cp;
            }
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= WS_EX_TRANSPARENT;
        //        return cp;
        //    }
        //}
        //private void InvalidateEx()
        //{
        //    if (Parent != null)
        //        Parent.Invalidate(Bounds, false);
        //    else
        //        Invalidate();
        //}

        //protected override void OnPaintBackground (PaintEventArgs e)
        //{
        //    //Intentionally do nothing - stops background from drawing
        //    //base.OnPaintBackground(e);
        //}
        protected override void OnPaint(PaintEventArgs e)
        {
            //double angleRadians = Math.Atan2(Height, Width);
            //float angleDegrees = -1 * (float)(angleRadians * 180 / Math.PI);
            //angleDegrees *= 0.9f;
            //e.Graphics.RotateTransform(angleDegrees, MatrixOrder.Append);
            //e.Graphics.TranslateTransform(20, Height - 75, MatrixOrder.Append);
            //e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            //Font font = new Font("Ariel", 50);
            //  e.Graphics.DrawString(Text, font, Brushes.Gray, 1, 2); //Shadow
            Debug.WriteLine($"Transparent Label {this.Name} : {this.Text}");
            using (Brush brush = new SolidBrush(ForeColor)) {
                e.Graphics.DrawString(this.Text, this.Font, brush, this.ClientRectangle);
            }
            base.OnPaint(e);
        }

        //protected override void WndProc(ref Message m)
        //{
        //    if (m.Msg != 0x14 /*WM_ERASEBKGND*/ && m.Msg != 0x0F /*WM_PAINT*/)
        //        base.WndProc(ref m);
        //    else {
        //        if (m.Msg == 0x0F) // WM_PAINT
        //            //base.OnPaint(new PaintEventArgs(Graphics.FromHwnd(Handle), ClientRectangle));
        //            OnPaint(new PaintEventArgs(Graphics.FromHwnd(Handle), ClientRectangle));

        //        DefWndProc(ref m);
        //    }
        //}

    }
}
