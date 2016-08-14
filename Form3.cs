using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BCS10;

namespace BCS10
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        //public Point[] wPts;
        //public Point[] wave;
        public List<Point[]> f3waves = new List<Point[]>();
        //public LinkedList<Chng> f3chngs = new LinkedList<BCS10.Chng>();
        public List<String> wav_nms;
        public LinkedList<BCS10.Chng> f3Chngs = new LinkedList<BCS10.Chng>();

        private void Form3_Load(object sender, EventArgs e)
        {
        }

        private void Form3_Paint(object sender, PaintEventArgs e)
        {
            System.Drawing.Pen myPen;
            System.Drawing.Graphics formGraphics = this.CreateGraphics();
            myPen = new System.Drawing.Pen(System.Drawing.Color.Red);
            System.Drawing.Font drawFont = new System.Drawing.Font("Consolas", 8);
            System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            //foreach (Chng c in f3chngs)
            //{
            //    wave = c.cB.wPts.ToArray();
            //    if (wave.Length > 1)
            //        formGraphics.DrawLines(myPen, wave);
            //}
            foreach (Point[] wPts in f3waves)
            {
                formGraphics.DrawLines(myPen, wPts);
            }
            float x = 10.0f;
            float y = 00.0f;
            foreach (String s in wav_nms)
                formGraphics.DrawString(s, drawFont, drawBrush, x, y += 20);
            drawFont.Dispose();
            drawBrush.Dispose();
            myPen.Dispose();
            formGraphics.Dispose();
        }
    }
}
