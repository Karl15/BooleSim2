using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using BCS10;

namespace BCS10
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        String fPath;
        String dPath;
        FileStream log, fs_in;
        StreamReader sr_in;
        StreamWriter lwrt;
        int chgix = 0;
        public BParse pb3 = new BParse();
        Form2 wform2 = new Form2();
        Form3 wform3 = new Form3();
        
        public Form frm1;
        public List<Point> segs = new List<Point>();
        public List<Point> rPts = new List<Point>();
        public List<LinkedList<Point>> wav2 = new List<LinkedList<Point>>();
        System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Red);

        private void Form1_Load_1(object sender, EventArgs e)
        {
            int num = 0xE70;
            int comp = ~num;
            int comp2 = comp + 1;
            openFileDialog1.ShowDialog();
            //segs = new List<Point>();
            //Point[] wave = new Point[3];
            //segs.Add(new Point(10, 30));
            //segs.Add(new Point(20, 30));
            //segs.Add(new Point(20, 20));
            //segs.Add(new Point(30, 20));
            //segs.Add(new Point(40, 20));
            //segs.Add(new Point(40, 30));
            //segs.Add(new Point(50, 30));
            ////pp = new Point[segs.Count];
            //pp = new Point[pb3.pts.Count];
            //int x = 0;
            //foreach (Point p in pb3.pts)
            //    pp[x++] = p;

        }

        private void openFileDialog1_FileOk_1(object sender, CancelEventArgs e)
        {
            fPath = openFileDialog1.FileName;

            dPath = Path.GetDirectoryName(fPath);
           // pb3.lB1 = listBox1;
            pb3.lB2 = listBox2;
            pb3.lB3 = listBox3;
            pb3.wform3 = wform3;
            //listBox1.Hide();
            //listBox2.Hide();
            pb3.lB2.Items.Clear();
            listBox3.Hide();
            button1.Hide();
            //wform2 = new Form2();
            //wform3 = new Form3();
            //pb3.wform3 = wform3;
            pb3.Bnew(fPath);  //, this);
            //foreach(Blk b in pb3.blks)
            //{

            //}
            //wform2.wavnms
            wform2.segs2 = pb3.pts;
            wform2.wave = pb3.scrLn;
            wform2.wnams = pb3.wnam;
            wform2.Show();
            wform3.f3waves = pb3.scrLn;
            wform3.Show();
            

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nSelIx = listBox2.SelectedIndex;
            if (nSelIx != -1)
            {
                //listBox1.Hide();
                listBox3.Show();
                button2.Show();
                int nLen = listBox2.SelectedItem.ToString().Length;
                chgix = 0;
                pb3.action(nSelIx, ref chgix, nLen);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //listBox3.Hide();
            //listBox1.Show();private void button1_Click(object sender, System.EventArgs e)
            //wform.setu
            log = new FileStream("cycle_log", FileMode.Create);
            lwrt = new StreamWriter(log);
            //fs_in = new FileStream(fPath, FileMode.Open);
            //sr_in = new StreamReader(fs_in);
            //foreach (string s in listBox1.Items)
            //{

            //    lwrt.Write(String.Format("{0}", s));  // {0}", s, sr_in.ReadLine()));
            //    lwrt.WriteLine();
            //}
            //while (!sr_in.EndOfStream)
            //{

            //    lwrt.Write(String.Format("{0,-40}", sr_in.ReadLine()));
            //    lwrt.WriteLine();
            //}
            lwrt.Flush();
            lwrt.Close();
            //listBox1.Show();
            //button2.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int nSelIx = listBox2.SelectedIndex;
            if (nSelIx != -1)
            {
                int nLen = listBox2.SelectedItem.ToString().Length;
                pb3.action(nSelIx, ref chgix, nLen);
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            System.Drawing.Pen myPen;
            myPen = new System.Drawing.Pen(System.Drawing.Color.Red);
            //fg = this.CreateGraphics();
            //foreach (Point[] s in segs)
            //    fg.DrawLines(myPen, s);
            System.Drawing.Graphics formGraphics = this.CreateGraphics();
            Point[] wave = new Point[3];
            wave[0].X = 10;
            wave[0].Y = 50;
            wave[1].X = 100;
            wave[1].Y = 50;
            wave[2].X = 100;
            wave[2].Y = 30;
            formGraphics.DrawLines(myPen, wave);
            String lbs;
            lbs = pb3.scn[pb3.scn.Count - 1].ln;
            System.Drawing.Font drawFont = new System.Drawing.Font("Consolas", 8);
            System.Drawing.SolidBrush drawBrush = new
                System.Drawing.SolidBrush(System.Drawing.Color.Black);
            float x = 10.0f;
            float y = 20.0f;
            formGraphics.DrawString(lbs, drawFont, drawBrush, x, y);
            drawFont.Dispose();
            drawBrush.Dispose();
            formGraphics.Dispose();
            myPen.Dispose();
           
        }
        }
}
