using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadDemo
{
    class MyThread
    {
        private const double PI = Math.PI;

        public Thread Worker;
        public bool IsRunning = false;
        public bool IsPaused = false;
        public volatile bool StopRequested = false;
        public ManualResetEventSlim PauseEvent = new ManualResetEventSlim(true);
        public Bitmap Pic;
        internal int Xmax;
        internal int Ymax;
        public Point Pos;
        private double dblGocChay;
        internal double tx;
        internal double ty;

        public MyThread(Random rnd, int xMax, int yMax)
        {
            Xmax = xMax;
            Ymax = yMax;
            Pos = new Point(rnd.Next(0, Xmax), rnd.Next(0, Ymax));
            dblGocChay = ChinhGocChay(rnd.Next(0, 360));
            CapNhatHuong();
        }

        private double ChinhGocChay(double goc)
        {
            if (0 <= goc && goc < 90)
                return 45;
            if (90 <= goc && goc < 180)
                return 135;
            if (180 <= goc && goc < 270)
                return 225;
            return 315;
        }

        private double DoiGocChayX(double goc)
        {
            double newGoc;
            if (goc > 0 && goc < 180)
            {
                newGoc = 180 - goc;
            }
            else
            {
                newGoc = 540 - goc;
            }
            return ChinhGocChay(newGoc);
        }

        private double DoiGocChayY(double goc)
        {
            return ChinhGocChay(360 - goc);
        }

        private void CapNhatHuong()
        {
            tx = 2 * Math.Cos(dblGocChay * PI / 180);
            ty = 2 * Math.Sin(dblGocChay * PI / 180);
        }

        public void HieuchinhVitri()
        {
            int x = Pos.X;
            int y = Pos.Y;

            if (x == 0 || x == Xmax - 1)
            {
                dblGocChay = DoiGocChayX(dblGocChay);
            }

            if (y == 0 || y == Ymax - 1)
            {
                dblGocChay = DoiGocChayY(dblGocChay);
            }

            CapNhatHuong();

            x += (int)tx;
            if (x < 0)
            {
                x = 0;
            }
            else if (x >= Xmax)
            {
                x = Xmax - 1;
            }

            y += (int)ty;
            if (y < 0)
            {
                y = 0;
            }
            else if (y >= Ymax)
            {
                y = Ymax - 1;
            }

            Pos = new Point(x, y);
        }
    }
}
