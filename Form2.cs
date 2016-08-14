using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BCS10
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        public List<Point> segs2 = new List<Point>();
        public List<Point[]> wave = new List<Point[]>();
        public List<String> wnams = new List<string>();

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            System.Drawing.Pen myPen;
            System.Drawing.Graphics formGraphics = this.CreateGraphics();
            myPen = new System.Drawing.Pen(System.Drawing.Color.Red);
            System.Drawing.Font drawFont = new System.Drawing.Font("Consolas", 8);
            System.Drawing.SolidBrush drawBrush = new
                System.Drawing.SolidBrush(System.Drawing.Color.Black);

            foreach (Point[] w in wave)
            //foreach (Point s in w)
            {
                formGraphics.DrawLines(myPen, w);
            }
            float x = 10.0f;
            float y = 00.0f;
            foreach (string s in wnams)
                formGraphics.DrawString(s, drawFont, drawBrush, x, y += 20);
            drawFont.Dispose();
            drawBrush.Dispose();
            myPen.Dispose();
            formGraphics.Dispose();

        }
    }
}
