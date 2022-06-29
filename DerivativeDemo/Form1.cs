using System;
using System.Collections.Generic;
using System.Drawing;
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
        PointF convertToPBCoord(float x, float y)
        {
            float newX = x + centerX;
            float newY = y * -1 + centerY;

            return new PointF(newX, newY);
        }

        TrackBar derivativePointBar;
        public Form1()
        {
            InitializeComponent();

            Graph = new PictureBox()
            {
                Location = new Point(0, 0),
                BackColor = Color.AliceBlue,
                Size = this.ClientSize
            };

            centerX = Graph.Width / 2;
            centerY = Graph.Height - 100;

            Controls.Add(Graph);

            map = new Bitmap(Graph.Width, Graph.Height);
            graphics = Graphics.FromImage(map);

            points.Add(new Point(0, 0));

            float xDiff = inverseF(centerX);

            leftmostX = centerX - xDiff;
            rightmostX = centerX + xDiff;

            Label derivativeLabel = new Label()
            {
                Text = "Use arrow keys to have more precise control\nUse mouse to slide more freely",
                AutoSize = true,
                Location = new Point(0, 50)
            };
            Graph.Controls.Add(derivativeLabel);
            derivativePointBar = new TrackBar()
            {
                Location = new Point(0, derivativeLabel.Bottom),
                TickStyle = TickStyle.None,
                TickFrequency = 1,
                Minimum = (int)(-xDiff + circleRadius),
                Maximum = (int)(xDiff - circleRadius),
                Value = 0,
                AutoSize = true
            };
            Graph.Controls.Add(derivativePointBar);
            derivativePointBar.ValueChanged += DerivativePointBar_ValueChanged;
            DerivativePointBar_ValueChanged(this, null);

            DrawGraph();
        }
        float circleRadius = 5f;
        private void DerivativePointBar_ValueChanged(object sender, EventArgs e)
        {
            float x = centerX + derivativePointBar.Value;

            graphics.Clear(Graph.BackColor);
            DrawBackgroundGraph();
            DrawGraph();

            float newX = x - centerX;
            PointF clickedPoint = new PointF(newX, f(newX));

            float slope = fprime(clickedPoint.X);

            float b = clickedPoint.Y - slope * clickedPoint.X;

            string output = $"Location of point: X:{clickedPoint.X}, Y:{clickedPoint.Y}\nEquation of line: y = {Math.Round(slope, 4)}*x + {Math.Round(b, 4)}";
            DrawOuputStr(output);

            float leftmostY = centerY - ((leftmostX - centerX) * slope + b);
            float rightmostY = centerY - ((rightmostX - centerX) * slope + b);

            graphics.DrawLine(slopePen, new PointF(leftmostX, leftmostY), new PointF(rightmostX, rightmostY));

            graphics.FillEllipse(Brushes.Orange, new RectangleF(x - circleRadius, centerY - clickedPoint.Y - circleRadius, circleRadius * 2, circleRadius * 2));

            Graph.Image = map;
        }

        Pen linePen = new Pen(Brushes.Red, 3);
        Pen slopePen = new Pen(Brushes.Blue, 3);
        Pen backgroundGraphPen = new Pen(Brushes.Black, 2);
        void DrawGraph()
        {
            for (float x = -centerX; x < centerX; x++)
            {
                PointF point = convertToPBCoord(x, f(x));

                graphics.DrawLine(linePen, points[points.Count - 1], point);

                points.Add(point);
            }

            Graph.Image = map;
        }

        void DrawBackgroundGraph()
        {
            graphics.DrawLine(backgroundGraphPen, new PointF(0, centerY), new PointF(Graph.Width, centerY));
            graphics.DrawLine(backgroundGraphPen, new PointF(centerX, 0), new PointF(centerX, Graph.Height));
        }
        void DrawOuputStr(string output)
        {
            graphics.DrawString(output, this.Font, Brushes.Black, new PointF(0, 0));
        }
        public float f(float x)
            => (float)Math.Pow(x, 2) / 10f;

        public float fprime(float x)
            => (float)x / 5;
        public float inverseF(float y)
            => (float)Math.Sqrt(y * 10);
    }
}