using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Seriak
{
    public class Bar:Control
    {


        [DefaultValue(100)]
        public int Max { get; set; } = 100;
        [DefaultValue(10)]
        public int value { get; set; } = 10;


        public Bar() : base()
        {
            DoubleBuffered = true;
        }

        protected override void OnAutoSizeChanged(EventArgs e)
        {
            base.OnAutoSizeChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            Graphics gr = e.Graphics;
            Rectangle rect = this.ClientRectangle;

            using (SolidBrush br = new SolidBrush(this.BackColor))
                gr.FillRectangle(br, rect);

            double k = (double)Max / rect.Width;
            int w = (int)(value / (double)k);

            using (SolidBrush br = new SolidBrush(this.ForeColor))

            gr.FillRectangle(br, 0, 0, w, rect.Height);

            
        }
    }
}
