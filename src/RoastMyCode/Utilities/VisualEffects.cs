using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace RoastMyCode.Utilities
{
    public static class VisualEffects
    {
        public static void AddGlowEffect(Control control, Color color)
        {
            control.Paint += (s, e) =>
            {
                using (var brush = new SolidBrush(color))
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, control.Width, control.Height);
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                }
            };
        }

        public static void AddFadeInAnimation(Control control, int duration = 200)
        {
            control.Visible = true;
            control.BackColor = Color.FromArgb(0, 0, 0, 0);
            
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 10;
            timer.Tick += (s, e) =>
            {
                if (control.BackColor.A < 255)
                {
                    control.BackColor = Color.FromArgb(control.BackColor.A + 5, control.BackColor.R, control.BackColor.G, control.BackColor.B);
                }
                else
                {
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        public static void AddLoadingSpinner(PictureBox pictureBox, Color color)
        {
            pictureBox.Image = null;
            pictureBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                float radius = Math.Min(pictureBox.Width, pictureBox.Height) / 2;
                float centerX = pictureBox.Width / 2;
                float centerY = pictureBox.Height / 2;
                float angle = DateTime.Now.Millisecond / 16.66f; // 60 FPS

                using (var brush = new SolidBrush(color))
                {
                    for (int i = 0; i < 12; i++)
                    {
                        float opacity = 1 - (angle % 360) / 360;
                        using (var brushAlpha = new SolidBrush(Color.FromArgb((int)(opacity * 255), color)))
                        {
                            float x = centerX + (float)Math.Cos(Math.PI * 2 * i / 12 + angle * Math.PI / 180) * radius * 0.7f;
                            float y = centerY + (float)Math.Sin(Math.PI * 2 * i / 12 + angle * Math.PI / 180) * radius * 0.7f;
                            e.Graphics.FillEllipse(brushAlpha, x - 2, y - 2, 4, 4);
                        }
                        angle += 30;
                    }
                }
            };
        }
    }
}
