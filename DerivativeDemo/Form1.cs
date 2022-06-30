using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DerivativeDemo
{
    public partial class Form1 : Form
    {
        Graphics graphics;
        Bitmap map;
        PictureBox Graph;

        List<PointF> points = new List<PointF>();

        float leftmostX;
        float rightmostX;

        float centerX;
        float centerY;
        float ogY;

        private float a = 1 / 10f;
        float h = 0;
        float k = 0;

        TrackBar derivativePointBar;
        NumericUpDown aUpDown;
        public Form1()
        {
            InitializeComponent();

            Graph = new PictureBox()
            {
                Location = new Point(0, 0),
                BackColor = Color.AliceBlue,
                Size = this.ClientSize
            };
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            Label derivativeLabel = new Label()
            {
                Text = "Tangent point location:",
                AutoSize = true,
                Location = new Point(0, 80)
            };

            centerX = Graph.Width / 2;
            centerY = Graph.Height - 100;
            ogY = centerY;

            Controls.Add(Graph);

            map = new Bitmap(Graph.Width, Graph.Height);
            graphics = Graphics.FromImage(map);

            points.Add(new Point(0, 0));

            Graph.Controls.Add(derivativeLabel);
            derivativePointBar = new TrackBar()
            {
                Location = new Point(0, derivativeLabel.Bottom),
                TickStyle = TickStyle.None,
                TickFrequency = 1,
                Value = 0,
                AutoSize = true
            };
            derivativePointBar.ValueChanged += DerivativePointBar_ValueChanged;


            Graph.Controls.Add(derivativePointBar);


            Label aLabel = new Label()
            {
                Location = new Point(0, derivativePointBar.Bottom),
                AutoSize = true,
                Text = "A: "
            };
            Graph.Controls.Add(aLabel);
            aUpDown = new NumericUpDown()
            {
                Location = new Point(aLabel.Right, derivativePointBar.Bottom),
                Minimum = 0.01M,
                Maximum = 1.5M,
                DecimalPlaces = 2,
                Increment = 0.01M,
                Value = (decimal)a,
                AutoSize = true
            };
            Graph.Controls.Add(aUpDown);
            aUpDown.ValueChanged += AUpDown_ValueChanged;

            Draw();
        }

        float changeAmount = 3f;

        [DllImport("user32")]
        public static extern int GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        public static extern int GetKeyboardState(byte[] keystate);

        byte[] keys = new byte[256];
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

            GetKeyboardState(keys);

            if ((keys[(int)Keys.A] & 128) == 128)
            {
                h -= changeAmount;
            }
            if ((keys[(int)Keys.D] & 128) == 128)
            {
                h += changeAmount;
            }
            if ((keys[(int)Keys.W] & 128) == 128)
            {
                k += changeAmount;
            }
            if ((keys[(int)Keys.S] & 128) == 128)
            {
                k -= changeAmount;
            }
            Draw();
        }

        private void AUpDown_ValueChanged(object sender, EventArgs e)
        {
            a = (float)aUpDown.Value;
            Draw();
        }

        void Draw()
        {
            graphics.Clear(Graph.BackColor);
            DrawBackgroundGraph();
            DrawGraph();

            derivativePointBar.Minimum = (int)((leftmostX - (centerX + h)));
            derivativePointBar.Maximum = (int)((rightmostX - (centerX + h)));
            Text = $"Use arrow keys to move the graph around.";

            float x = centerX + derivativePointBar.Value + h;
            float newX = x - centerX;
            PointF clickedPoint = new PointF(newX, f(newX));

            float slope = fprime(clickedPoint.X);

            float b = clickedPoint.Y - slope * clickedPoint.X;

            string output = $"Location of point: X:{clickedPoint.X}, Y:{clickedPoint.Y}\nEquation of line: y = {Math.Round(slope, 4)}*x + {Math.Round(b, 4)}\nSlope at point = Slope of line = {Math.Round(slope, 4)}\nEquation of Parabola: y = {a}(x - {h})^2 + {k}";
            DrawOuputStr(output);

            float leftmostY = centerY - ((leftmostX - centerX) * slope + b);
            float rightmostY = centerY - ((rightmostX - centerX) * slope + b);

            graphics.DrawLine(slopePen, new PointF(leftmostX, leftmostY), new PointF(rightmostX, rightmostY));

            graphics.FillEllipse(Brushes.Orange, new RectangleF(x - circleRadius, centerY - clickedPoint.Y - circleRadius, circleRadius * 2, circleRadius * 2));

            Graph.Image = map;
        }

        float circleRadius = 5f;
        private void DerivativePointBar_ValueChanged(object sender, EventArgs e)
        {
            Draw();
        }

        Pen linePen = new Pen(Brushes.Red, 3);
        Pen slopePen = new Pen(Brushes.Blue, 1);
        Pen backgroundGraphPen = new Pen(Brushes.Black, 2);


        void DrawGraph()
        {
            leftmostX = float.MaxValue;
            rightmostX = float.MaxValue;

            points.Clear();
            for (float x = 0; x < Graph.Width; x++)
            {
                float y = f(x - centerX);
                PointF point = new PointF(x, centerY - y);

                if (point.Y >= 0 && leftmostX == float.MaxValue)
                {
                    leftmostX = x;
                }
                if (point.Y <= 0 && leftmostX != float.MaxValue && rightmostX == float.MaxValue)
                {
                    rightmostX = x;
                }

                if (points.Count > 0)
                {
                    graphics.DrawLine(linePen, points[points.Count - 1], point);
                }

                points.Add(point);
            }

            Graph.Image = map;
        }

        void DrawBackgroundGraph()
        {
            graphics.DrawLine(backgroundGraphPen, new PointF(0, ogY), new PointF(Graph.Width, ogY));
            graphics.DrawLine(backgroundGraphPen, new PointF(centerX, 0), new PointF(centerX, Graph.Height));
        }
        void DrawOuputStr(string output)
        {
            graphics.DrawString(output, this.Font, Brushes.Black, new PointF(0, 0));
        }
        public float f(float x)
            => a * (float)Math.Pow(x - h, 2) + k;

        public float fprime(float x)
            => 2 * a * (x - h);
        public float inverseF(float y)
            => (float)Math.Sqrt((y - k) / a) + h;
    }
}