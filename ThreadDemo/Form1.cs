using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace ThreadDemo
{

    public partial class Form1 : Form
    {
        private MyThread[] threadLst;
        private const int xCell = 30;
        private const int yCell = 30;
        private const int xMax = 25;
        private const int yMax = 20;
        private Random rnd = new Random();

        private void Running(object obj)
        {
            MyThread p = (MyThread)obj;
            try
            {
                while (!p.StopRequested)
                {
                    p.PauseEvent.Wait();
                    if (p.StopRequested)
                        break;

                    Point oldPosition = p.Pos;
                    p.HieuchinhVitri();

                    BeginInvoke(new Action(() =>
                    {
                        DrawThread(p, oldPosition);
                    }));

                    Thread.Sleep(100);
                }
            }
            finally
            {
                p.IsRunning = false;
                p.IsPaused = false;

                BeginInvoke(new Action(() =>
                {
                    using (Graphics g = CreateGraphics())
                    {
                        using (Brush brush = new SolidBrush(BackColor))
                        {
                            g.FillRectangle(brush, xCell * p.Pos.X, yCell * p.Pos.Y, xCell, yCell);
                        }
                    }
                }));
            }
        }

        private void DrawThread(MyThread p, Point oldPosition)
        {
            using (Graphics g = CreateGraphics())
            {
                using (Brush brush = new SolidBrush(BackColor))
                {
                    g.FillRectangle(brush, xCell * oldPosition.X, yCell * oldPosition.Y, xCell, yCell);
                }

                if (p.Pic != null)
                {
                    g.DrawImage(p.Pic, xCell * p.Pos.X, yCell * p.Pos.Y);

                    Color c = p.Pic.GetPixel(1, 1);
                    int yR = c.R > 128 ? 0 : 255;
                    int yG = c.G > 128 ? 0 : 255;
                    int yB = c.B > 128 ? 0 : 255;

                    using (Pen pen = new Pen(Color.FromArgb(yR, yG, yB), 2))
                    {
                        DrawDirectionArrow(g, pen, p);
                    }
                }
            }
        }

        private void DrawDirectionArrow(Graphics g, Pen pen, MyThread p)
        {
            int x;
            int y;
            int x1 = p.Pos.X;
            int y1 = p.Pos.Y;

            if (p.tx >= 0 && p.ty >= 0)
            {
                x = xCell * x1 + xCell - 2;
                y = yCell * y1 + yCell - 2;
                g.DrawLine(pen, x, y, x - 10, y);
                g.DrawLine(pen, x, y, x, y - 10);
            }
            else if (p.tx >= 0 && p.ty < 0)
            {
                x = xCell * x1 + xCell - 2;
                y = yCell * y1 + 2;
                g.DrawLine(pen, x, y, x - 10, y);
                g.DrawLine(pen, x, y, x, y + 10);
            }
            else if (p.tx < 0 && p.ty >= 0)
            {
                x = xCell * x1 + 2;
                y = yCell * y1 + yCell - 2;
                g.DrawLine(pen, x, y, x + 10, y);
                g.DrawLine(pen, x, y, x, y - 10);
            }
            else
            {
                x = xCell * x1 + 2;
                y = yCell * y1 + 2;
                g.DrawLine(pen, x, y, x + 10, y);
                g.DrawLine(pen, x, y, x, y + 10);
            }
        }
        public Form1()
        {
            KeyPreview = true;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Assembly myAssembly = GetType().Assembly;

            threadLst = new MyThread[26];
            for (int i = 0; i < 26; i++)
            {
                threadLst[i] = new MyThread(rnd, xMax, yMax);
                char c = (char)(i + 65);

                using (Stream myStream = myAssembly.GetManifestResourceStream(
                    myAssembly.GetName().Name + ".Resources.Bitmap" + c + ".bmp"))
                {
                    if (myStream != null)
                    {
                        threadLst[i].Pic = new Bitmap(myStream);
                    }
                }
            }

            ClientSize = new Size(xMax * xCell, yMax * yCell);
            Location = new Point(0, 0);
            BackColor = Color.Black;
        }

     

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            int newch = e.KeyValue;

            if (newch < 0x41 || newch > 0x5A)
                return;

            int index = newch - 65;

            if (threadLst == null || threadLst[index] == null)
                return;

            MyThread currentThread = threadLst[index];

            if (e.Control && e.Shift)
            {
                StopThread(currentThread);
            }
            else if (e.Control && e.Alt)
            {
                PauseThread(currentThread);
            }
            else if (e.Control)
            {
                if (currentThread.Worker != null && currentThread.Worker.IsAlive)
                {
                    currentThread.Worker.Priority = ThreadPriority.Lowest;
                    MessageBox.Show("Priority: " + currentThread.Worker.Priority.ToString());
                }
            }
            else if (e.Alt)
            {
                ResumeThread(currentThread);
            }
            else if (e.Shift)
            {
                if (currentThread.Worker != null && currentThread.Worker.IsAlive)
                {
                    currentThread.Worker.Priority = ThreadPriority.Highest;
                    MessageBox.Show("Priority: " + currentThread.Worker.Priority.ToString());
                }
            }
            else
            {
                StartThread(currentThread, newch);
            }
        }

        private void StartThread(MyThread currentThread, int keyCode)
        {
            if (currentThread.IsRunning)
                return;

            currentThread.StopRequested = false;
            currentThread.IsRunning = true;
            currentThread.PauseEvent.Set();

            currentThread.Worker = new Thread(new ParameterizedThreadStart(Running));
            currentThread.Worker.IsBackground = true;

            if (keyCode == 65)
            {
                currentThread.Worker.Priority = ThreadPriority.Highest;
            }
            else
            {
                currentThread.Worker.Priority = ThreadPriority.Lowest;
            }

            currentThread.Worker.Start(currentThread);
        }

        private void PauseThread(MyThread currentThread)
        {
            if (!currentThread.IsRunning)
                return;

            currentThread.PauseEvent.Reset();
            currentThread.IsPaused = true;
        }

        private void ResumeThread(MyThread currentThread)
        {
            if (!currentThread.IsRunning)
                return;

            currentThread.PauseEvent.Set();
            currentThread.IsPaused = false;
        }

        private void StopThread(MyThread currentThread)
        {
            if (!currentThread.IsRunning)
                return;

            currentThread.StopRequested = true;
            currentThread.PauseEvent.Set();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (threadLst == null)
                return;

            foreach (MyThread item in threadLst)
            {
                if (item == null)
                    continue;

                item.StopRequested = true;
                item.PauseEvent.Set();
            }

            foreach (MyThread item in threadLst)
            {
                if (item?.Worker != null && item.Worker.IsAlive)
                {
                    item.Worker.Join(500);
                }
            }
        }
    }
}
