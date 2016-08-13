
// CopyRight(C) 2010 Karl Stevens
//  BParse62815.cs is  VCS2012/BCS10/BCS10 DIRECTORY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;  //.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;

//using System.Numeric;
//using BCS10;


/*  dRegs cRegs cNets, dBuss, clks, dlys
 *  dcd, mux
 *  wav
 */ 


/*  blk -- typ, msb, lsb, dstr, oVal
 *  reg : blk -- din, qout setup(), launch()
 *  wav : bctl
 *  
 *  
 *  wav -- 
 *  bCtl -- bIx, cStr, cRpn, dStr, dRpn
 */ 

namespace BCS10
{

    public class Chng
    {
        public int ix, time;
        public int val;
        //public string ln;
        public string name;
        //public Blk cB;
        //public Point pt;
    }

    /*   Events
     *   ff/reg names, CtlStrs, VStrs
     *   clk CtlStrs
     *  
     */


    public class BParse
    {
        /*Event val asgn to node, time rpt 
         *  returns node val as q  
         *   NODE can be FF, NODE, constant, or input that is changed by Event or others output 
         *   
         */
        public class NODE
        {
            public NODE(String s)
            {
                if (char.IsLetterOrDigit(s[0]) || s[0] == '_')
                    name = s.Trim();
                else
                    name = s.Substring(1).Trim();
            }
            public bool nX = false;
            public List<Point> Wave;
            public String name;
            public int qv;
            public int val
            { get { return qv; } set { qv = value; } }
            public bool bv = false;
            public virtual bool q
            { get { return val > 0; } set { } }
            public virtual bool qbar { get; set; }
        }


    public class Event : NODE
    {
        public Event(String s)
            : base(s) { }
        public string name
        { get { return base.name; } }
        //public Pin pin;
        public NODE node;
        public int eVal
        {
            get
            {
                return base.val;
            }
            set
            {
                base.val = value;
            }
        }
        //public int val;
        public int time;
        public int rpt;
        public override bool q
        {
            get
            {
                return this.node.val > 0;
            }
            //set
            //{
            //    base.q = value;
            //}
        }
        public override bool qbar
        {
            get
            {
                return this.node.val == 0;
            }
            //set
            //{
            //    base.qbar = value;
            //}
        }
        //protected List<Point> Wave;
    }


    //public class Pin : NODE
    //{
    //    public Pin(String s)
    //        : base(s) { }
    //    public string name
    //    { get { return base.name; } }
    //    public int val
    //    {
    //        get
    //        {
    //            return base.val;
    //        }
    //        set
    //        {
    //            base.val = value;
    //        }
    //    }
    //    public override bool q
    //    {
    //        get
    //        {
    //            return base.val > 0;
    //        }
    //        //set
    //        //{
    //        //    base.q = value;
    //        //}
    //    }
    //    public override bool qbar
    //    {
    //        get
    //        {
    //            return base.val == 0;
    //        }
    //        //set
    //        //{
    //        //    base.qbar = value;
    //        //}
    //    }
    //    //protected List<Point> Wave;
    //}


    public class FF : NODE
    {
        public FF(String s)
            : base(s) { }
        //public FF(Event eV) : base(eV.name)
        //{
        //    base.q = eV.val;
        //}
        //public bool b { set { base.q = true; } }
        //{ get { return base.name; } }
        int bdin, bq;
        public int din
        { set { bdin = value; } }
        //public int q
        //{ get { return base.q; } }
        public string oncStr, offcStr, ctlXpr;
        public List<NODE> onNs = new List<NODE>();
        public List<char> onops = new List<char>();
        public List<NODE> offNs = new List<NODE>();
        public List<char> offops = new List<char>();
        public override bool q
        {
            get
            {
                return base.qv > 0;
            }
            set
            {
                base.q = value;
            }
        }
        public override bool qbar
        {
            get
            {
                return base.qv == 0;
            }
            set
            {
                base.qbar = value;
            }
        //protected List<Point> Wave;
    }
        public int wavx = -1;
        public int scnx = -1;
        public void clk()
        {
            base.qv = bdin;
        }
        public List<Point> Wave = new List<Point>();
        public LinkedList<bool> Chngs;
    }

    public class REG : NODE
    {
        public REG(String s)
            : base(s) { }
        string name
        { get { return base.name; } }
        int vdin, vq;
        new public int din
        { set { vdin = value; } }
        new public int q
        { get { return vq; } }
        public string ctlStr, ctlXpr;
        //public RegEn rEn;
        int inv, qv;
        void clk()
        {
            vq = vdin;
        }
        public LinkedList<bool> Chngs;
    }
    public class Bus : NODE
    {
        public Bus(String s)
            : base(s) { }
        string name
        { get { return base.name; } }
        public int val;
        public string vStr, ctlStr;
        public NODE[] bArr;
        //public List<Point> Wave
        //{ get { return Wave; } }
        public LinkedList<int> Chngs;
    }

    public class bNet : NODE
    {
        public bNet(string s)
            : base(s) { nX = true; }
        string name
        { get { return base.name; } }
        public bool bV;
        public string bStr;
        public List<NODE> bNs = new List<NODE>();
        public List<char> bops = new List<char>();
        //public List<Point> Wave
        //{ get { return Wave; } }
        public LinkedList<int> Chngs;
    }
    List<bNet> bNets = new List<bNet>();
    //}

        char[] wsp = new char[] { };
        char[] copers1 = new char[] { '+', '-', '!', '~', '=', '(', ')', '?', ':', '&', '|', '^', '*', '/', '<', '>', '%' };  //, '{', '}' '~', };
        char[] copers2 = new char[] { '!', '=', '<', '>' };
        char[] bopers1 = new char[] { '|', '&' };
        string[] bopers2 = new string[] { "|", "|!", "&", "&!" };
        string[] bopers = new string[] {"&", "|", "^", "+", "-", ">", "<", "!=", "<=", "==", ">=", "*", "/", "%", "(", ")" };
        //string[] bopers = new string[] { "&", "|", "^", "+", "-", "&&", "||", ">", "<", "!=", "<=", "==", ">=", "*", "/", "%", "<<", ">>", "!", "~", "(", ")" };
        //int lp = 0;
        public List<List<Point>> waves = new List<List<Point>>();
        public List<String> wavNms = new List<string>();
        public List<Point> wPts = new List<Point>();
        public LinkedListNode<Chng> wavNode;
        //List<Reg> Regs = new List<Reg>();
        List<Event> events = new List<Event>();
        List<Event> eVList = new List<Event>();
        List<FF> ffs = new List<FF>();
        string[] rA;
       List<char> ops;
        //List<Reg> FFs = new List<Reg>();

        public Form3 wform3;
        public ListBox lB1;
        public ListBox lB2;
        public ListBox lB3;
        /**************************/
        /**                      **/
        /**  Global declarations **/
        /**                      **/
        /**************************/

        FileStream fs_in;  // = new FileStream(fPath, FileMode.Open);
        StreamReader sr_in; // = new StreamReader(fs_in);
        public class eClk
        {
            public eClk(string[] tArray)
            {
                name = tArray[0];
                c1_bv = tArray[1];
            }
            public List<FF> on = new List<FF>();
            public List<FF> off = new List<FF>();
            public List<FF> regs = new List<FF>();
            public List<NODE> c1Ns = new List<NODE>();
            public List<char> c1ops = new List<char>();
            public List<NODE> c2Ns = new List<NODE>();
            public List<char> c2ops = new List<char>();
            public string c1_bv;
            public string c1Ctrl;
            public string c2_bv;
            public string c2Ctrl;
            public int ix;
            public string name;
            public List<FF> on_ch, off_ch;
            public List<FF> reg_ch;
        }
        public class clk_def
        {
            public List<int> blks = new List<int>();
            public string c1_bv;
            public String [] c1nms;
            public List<FF> c1regs = new List<FF>();
            public string c2_bv;
            public string[] c2nms;
            public List<FF> c2regs = new List<FF>();
            public int ix;
            public string name;
            public List<FF> on, off;
            public List<NODE> reg;
            public List<FF> on_ch, off_ch;
            public List<FF> reg_ch;
        }
        List<clk_def> clks = new List<clk_def>(64);
        List<eClk> eclks = new List<eClk>();
        //List<Event> events = new List<Event>();
        List<NODE> Regs = new List<NODE>();
        //List<Reg> FFs = new List<Reg>();
        clk_def pc;
        eClk peC;
        string sBs = null;
        //int xOp;
        string[] sScan = new string[64];
        int lineno = 1; /* The current line number being parsed */
        int err_cd;
        string[] sSplit;
        string[] inSplit;
        int bix = 0, sIx1, sIx2, scn_ix;
        //Blk bIx;
        char sep1, sep2;
        public int rn_tm = 0;
        int run = 100, ln_tm = 0, scn_tm = 0;
        char[] sep = { ':', '?', '=', '@'};
        string[] strsep = { "?", "=", "@", ":", "\n", "//", "/*" };
        string[] cmntsep = { "//", "/*", "*/" };
        char[] bkts = { '[', '.', ']' };
        char[] prens = { '(', ')' };
        StringBuilder sb1 = new StringBuilder();
        StringBuilder sb2 = new StringBuilder();
        /*
         *  list of Regs with bool q maxterm q values 
         *  list of Regs with bool q minterm q values 
         *  maxterm returns on first true mintern or at end
         *  minterm returns on first falsr or at end
         *  bq is set at launch, interogated after all launches and stored in event
         */


        char[] xopers = new char[] { '!', '&', '|', '=' };

        //public bool ctrlX(string cXpr)
        //{
        //    Stack<string> oS;
        //    oS = new Stack<string>(cXpr.Split(xopers, StringSplitOptions.RemoveEmptyEntries));
        //    StringBuilder sb = new StringBuilder(cXpr);
        //    return ctrlx(oS, sb);
        //}
        //public bool ctrlx(Stack<String> oS, StringBuilder sb)
        //{
        //    string[] wSp = new string[] { };
        //    int xL = 0, nIx = 0, v1, v2;
        //    bool rslt = false;
        //    //blk vB;
        //    while (oS.Count > 0)  //sb2.ToString().Trim().Length > 0)
        //    {
        //        if (Char.IsLetterOrDigit(oS.Peek()[0]) || oS.Peek()[0] == '_')
        //        {
        //            //vB = get_blk(oS.Peek());
        //            rslt = bval(oS);
        //        }
        //        else
        //            switch (oS.Peek()[0])
        //            {
        //                case '_':
        //                    {
        //                        return bval(sb.ToString().Trim());
        //                    }
        //                    break;
        //                case '=':
        //                    {
        //                        if (sb[xL + 1] == '=')
        //                        {
        //                            //v1 = get_blk(sb2.ToString().Substring(0, xL).Trim()).qout;
        //                            //v2 = get_blk(sb2.ToString().Substring(xL + 2).Trim()).qout;
        //                            //return v1 == v2 ? true : false;
        //                            return true;
        //                        }
        //                    }
        //                    break;
        //                case '!':
        //                    rslt = bval(oS);  //sb2.ToString().Substring(0, xL).Trim());
        //                    break;
        //                case '&':
        //                    oS.Pop();
        //                    rslt = ctrlx(oS, null);
        //                    if (!rslt)
        //                        return false;
        //                    while (oS.Count > 0)
        //                    {
        //                        rslt &= ctrlx(oS, null);
        //                        if (!rslt)
        //                            return false;
        //                        if (oS.Count == 0 || oS.Peek() != "&")
        //                            break;
        //                    }
        //                    return rslt;
        //                    break;
        //                case '|':
        //                    oS.Pop();
        //                    //rslt = bval(oS);
        //                    if (rslt)
        //                        return true;
        //                    while (oS.Count > 0)
        //                    {
        //                        //rslt |= bval(oS);
        //                        //if (rslt)
        //                        //    return true;
        //                        //if (oS.Count > 0)
        //                        //{
        //                        //if(oS.Peek()[0] == '&')
        //                        //{
        //                        rslt |= ctrlx(oS, sb2);
        //                        if (rslt)
        //                            return rslt;
        //                        //}
        //                        //else
        //                        //{
        //                        //    rslt = bval(oS);
        //                        //    if (rslt)
        //                        //        return rslt;
        //                        //    }
        //                        //    rslt |= bval(oS);
        //                        //    //}
        //                        //while (oS.Count > 0)  //   && oS.Peek()[0] == '&')
        //                        //{
        //                        //    rslt &= bval(oS);
        //                        //}
        //                        //    if (rslt)
        //                        //        return rslt;
        //                    }
        //                    return false;
        //                    break;
        //                default:
        //                    break;
        //            }
        //        return rslt; // bval(sb2.ToString().Trim());
        //    }
        //    return false;
        //}
        public int ctrlx(FF ff)
        {
            return ff.qv;
        }

        public int ctrlX(List<NODE> fL, List<char> opL, string cS)
        {
            bool bRslt;
            if (fL[0].nX)
                bRslt = ctrlX(((bNet)fL[0]).bNs, ((bNet)fL[0]).bops, cS)> 0 ? true : false;
            else
                bRslt = fL[0].q;
            if (opL[0] == '!')
                bRslt = !bRslt;
            for(int x = 1, y = 1; x < opL.Count; x++)
            {
                //bRslt = fL[x].q;
                switch (opL[x].ToString())
                {
                    case"!":
                        x++;
                        break;
                    case " ":
                        break;
                    case "|": 
                        if (bRslt) // returns if already true
                            return 1;
                        if (opL[x + 1] == '!')
                            bRslt = fL[y++].qbar;
                        else
                            bRslt = fL[y++].q;
                        break;
                    case "|!":
                        if (bRslt) // returns if already true
                            return 1;
                        if (opL[x + 1] == '!')
                            bRslt = fL[y++].qbar;
                        else
                            bRslt = fL[y++].q;
                        break;
                    case "&":
                        if (opL[x + 1] == '!')
                            bRslt &= fL[y++].qbar;
                        else
                            bRslt &= fL[y++].q;
                        break;
                    case "&!":
                        if (opL[x + 1] == '!')
                            bRslt &= fL[y++].qbar;
                        else
                            bRslt &= fL[y++].q;
                        break;
                }
            }
            return bRslt ? 1 : 0;
        }

        public int Bnew(String fPath)
        {
            int init = 0, lp = 0, sepIx = 0;
            string[] tArray = new string[0];
            string[] cmnts = new String[] { "//", "/*" };
            string ltkn = "", tkn = "";
            List<string> tkns = new List<string>();
            fs_in = new FileStream(fPath, FileMode.Open);
            sr_in = new StreamReader(fs_in);
            string nLn = "";
            // comment +nm !nm =nm ?nm : @ ?
            /* t/n, t/f, dcd/demux, mux/datasel, bus,
             * [+nm, !nm, \c1, \c2] ? exp
             * nm : exp ? exp
             * nm : nm ? exp
             * nm = exp
             *  ff, reg, ram, rom blocks are assigned(clocked), din value enabled by Bool expr
             *  ff/lth din at setup determines launch qout.
             *  bool either types or relational expr
             *  base blk
             *      derived reg, ff, combo, bus, dp, ram, rom, mux, dcd,,,,
             *          setup, launch (parallelism) 
             *          get, set (input, output)
             */
            // 
            //  combo blk uses '=' for continuous and reg blk uses ? for clocked assignment.
            //  \c1 and \c2 pairs define clock domains for subsequent FFs 
            //  inputs only have  get accessors(read only) to evaluate outputs
            //  outputs have get (used by inputs) and set (to assign ouput values)
            //  combos are valid after launch,
            //  FFs set by clock if enabled.
            //  after all setups are called, launches are called to change qout = din
            //  and combo blks resolve
            //  
            List<string> tSL = new List<string>();
            List<char> dL = new List<char>();
            List<char> dList = new List<char>();
            Queue<string> tQ = new Queue<string>();
            Queue<string> dQ = new Queue<string>();
            //List<bNet> bNets = new List<bNet>();
            Event eV = null;
            int vParse = 0;
            char ID = ' ';
            int x = 0;
            //nm : num					// initialize at t = 0
            //nm : num1 @ num2			// value = num1 at t = num2
            //nm : num1 @ num2 : num3		// value = num1 at t = num2 repeats each num3 interval
            //nm : num nm ? exp     nm : num nm = exp
            //net : net1 ? net2			// bool net node value equal to bool net1 if bool net2 is true else false
            //net : exp ? net				// bool net node value equal to exp if bool net2 is true else false
            //net : exp1 ? exp2			// bool net node value equals exp1 if bool exp2 is true else false
            //net ? exp					// bool net node value true if bool exp evaluates to true else false
            //bus = exp					// bus value equqls exp evaluation
            //bus1 = bus2 ? net			// bus1 value equals bus2 value if net is true else 0	
            //bus = exp ? net				// bus equals exp value if net is true else 0
            //+nam ? net					// set ff nam equal true if net is true
            //+nam ? exp					// reset ff nam equal false if exp is true
            //!nam ? net					// set ff nam equal true if net is true
            //!nam ? exp					// reset ff nam equal false if exp is true
            //aa:0x0cf3     tQ.count == 2 after ':'     continue
            //clk:1 @ 1:2   tQ.count == 1 after ':'     tQ.count == 1 after '@'     tQ.count == 2 after ':' continue
            //dmy7:7 @ 14   tQ.count == 1 after ':'     tQ.count == 2 after '@'     continue

            StringBuilder dS = new StringBuilder();
            //nxtTkn(ref tArray, ref sIx1, ref tQ, ref dQ, ref dS, ref sep, ref sb2);
            //sb2.Append(dS.ToString());
            //tArray = sb2.ToString().Split(sep, StringSplitOptions.RemoveEmptyEntries);

            while (sr_in.EndOfStream == false || sb2.ToString().Trim().Length > 0)
            {
                if (sb2.ToString().IndexOfAny(sep) < 0)
                {
                    nxtTkn(ref tArray, ref sIx1, ref tQ, ref dQ, ref dS, ref sep, ref sb2);
                    sb2.Append(dS.ToString());
                }
                tArray = sb2.ToString().Split(sep, 3, StringSplitOptions.RemoveEmptyEntries);
                //nxtTkn(ref tArray, ref sIx1, ref tQ, ref dQ, ref dS,ref sep, ref sb2);
                switch (sb2[sb2.ToString().IndexOfAny(sep)])

                {
                    case ':':  //  name : [reg, net, bus] [?, =] cond [reg, expr, RegEn  Initial or timed event
                        if (tArray.Length < 3)  // need 2 seps for ':'
                        {
                            nxtTkn(ref tArray, ref sIx1, ref tQ, ref dQ, ref dS, ref sep, ref sb2);
                            sb2.Append(dS.ToString());
                            tArray = sb2.ToString().Split(sep, 3, StringSplitOptions.RemoveEmptyEntries);
                        }
                        sIx1 = tArray[0].Length + tArray[1].Length + 1;
                        if (sb2[sIx1] == '@')  // timed event
                        {
                            eV = new Event(tArray[0]);
                            //eV.node = events.Find(n => n.name == eV.name);
                            //if (eV.node == null)
                            //    eV.node = new Event(eV.name);
                            //else
                            //    eV.node = ((Event)eV.node).node;
                            if (xParse(tArray[1], out vParse) == false) // value to be assigned     event value
                            { err_cd = 1; MessageBox.Show(String.Format("Invalid Value Line {0}", lineno)); }
                            eV.eVal = vParse;
                            sb2.Remove(0, tArray[0].Length + tArray[1].Length + 2);  // remoce both ':'  '@'
                            tArray = sb2.ToString().Split(sep, 2, StringSplitOptions.RemoveEmptyEntries);
                            if (xParse(tArray[0], out vParse) == false) // value to be assigned     event value
                            { err_cd = 1; MessageBox.Show(String.Format("Invalid Value Line {0}", lineno)); }
                            eV.time = vParse;
                            addEvent(events, eV);
                            //Regs.Add(eV);
                            tArray = sb2.ToString().Split(sep, 3, StringSplitOptions.RemoveEmptyEntries);
                            sb2.Remove(0, tArray[0].Length);  // remove '@'
                            tArray = sb2.ToString().Split(sep, 3, StringSplitOptions.RemoveEmptyEntries);
                            if (tArray.Length < 2)
                            {
                                nxtTkn(ref tArray, ref sIx1, ref tQ, ref dQ, ref dS, ref sep, ref sb2);
                                sb2.Append(dS.ToString());
                                tArray = sb2.ToString().Split(sep, 3, StringSplitOptions.RemoveEmptyEntries);
                            }
                            tArray = tArray[0].Split(wsp, 2, StringSplitOptions.RemoveEmptyEntries);
                            if (tArray.Length == 1)
                                break;
                            if (xParse(tArray[0], out vParse) == false) // value to be assigned     event value
                            { err_cd = 1; MessageBox.Show(String.Format("Invalid Value Line {0}", lineno)); }
                            eV.rpt = vParse;
                            sb2.Remove(0, sb2.ToString().IndexOf(tArray[1]));
                        }  //end of '@'
                        else
                        {
                            eV = new Event(tArray[0]);
                            if (tArray.Length == 3 && tArray[1].Split(wsp, 2, StringSplitOptions.RemoveEmptyEntries).Length > 1)
                            {
                                sIx1 = tArray[0].Length;
                                if ((tArray = tArray[1].Split(wsp, 2, StringSplitOptions.RemoveEmptyEntries)).Length > 1)
                                {  // initial
                                    // How about endofstream????????????
                                    eV.node = Regs.Find(y => y.name == eV.name);
                                    //if (eV.ff == null)
                                    //{
                                    //    eV.ff = new FF(eV.name);
                                    //    Regs.Add(eV.ff);
                                    //}
                                    sb2.Remove(0, sb2.ToString().IndexOf(tArray[1]));
                                    if (xParse(tArray[0], out vParse) == false) // value to be assigned     event value
                                    { err_cd = 1; MessageBox.Show(String.Format("Invalid Value Line {0}", lineno)); }
                                    eV.eVal = vParse;
                                }
                                addEvent(events, eV);
                            }
                        }  //  end of initialize and timed assigns
                        //sIx1 = sb2.ToString().IndexOfAny(sep);
                        //break;
                        NODE ff;
                        switch (sb2[sIx1])   // conditional assigns 
                        {
                            case '?':  // Boolean
                                {
                                    if (pc.reg == null)
                                        pc.reg = new List<NODE>();
                                    if ((ff = Regs.Find(xx => xx.name == tArray[0])) == null)
                                        Regs.Add(ff = new FF(tArray[0]));
                                        pc.reg.Add(ff);
                                    sb2.Remove(0, tArray[0].Length + tArray[1].Length + tArray[2].Length + 2);
                                }
                                break;
                            case '=':   // Value
                                //((Bus)eV).ctlStr = tArray[0];
                                //((Bus)eV).vStr = tArray[1];
                                break;
                            default:
                                break;
                        }
                        //string str = "aabccdeefgghiijkklmm";
                        //string pattern = "(\\w)\\1";
                        //string replacement = "$1";
                        //Regex rgx = new Regex(pattern);

                        //string result = rgx.Replace(tArray[1], replacement, 5);
                        //Console.WriteLine("Original String:    '{0}'", str);
                        //Console.WriteLine("Replacement String: '{0}'", result); 

                        break;  // end of  name : value assigns
                    case '?':  // name ? Boolean expression
                        string nm = tArray[0];
                        if (tArray[0].TrimStart()[0] == '+')  // shorthand set
                        {
                            if (peC.on == null)
                                peC.on = new List<FF>();
                            if ((ff = (Regs.Find(xx => xx.name == nm.Substring(1)))) == null)
                                ff = new FF(nm.Substring(1));
                            peC.on.Add((FF)ff);
                            ((FF)ff).oncStr = tArray[1];
                            //if (tArray[1].IndexOfAny(bopers2) >= 0)
                            //{
                            //ff.onEn = new RegEn(nm);
                            //cBld(tArray[1], ref ff.onEn.nL, ref ff.onEn.ops);
                            //}
                            sb2.Remove(0, sb2.ToString().IndexOf(tArray[1]) + tArray[1].Length);
                        }
                        else if (tArray[0].TrimStart()[0] == '!')  //  shorthand reset
                        {
                            if (peC.off == null)
                                peC.off = new List<FF>();
                            if ((ff = ((FF)Regs.Find(xx => xx.name == nm.Substring(1)))) == null)
                                Regs.Add(ff = new FF(nm.Substring(1)));
                            peC.off.Add((FF)ff);
                            ((FF)ff).offcStr = tArray[1];
                            //ff.offEn = new RegEn(nm);
                            //cBld(tArray[1], ref ff.offEn.nL, ref ff.offEn.ops);
                            //}
                            //else
                            //    ff.offEn.nL.Add(tArray[1]); 
                            sb2.Remove(0, sb2.ToString().IndexOf(tArray[1]) + tArray[1].Length);
                        }
                        else
                            if (tArray[0] == "\\c1")  // c1, c2 clock pair
                            {//  clock definition using c1, c2 pairs for edges, latches, etc.pc = new clk_def();
                                eclks.Add(peC = new eClk(tArray));
                                //if (tArray[1].IndexOfAny(bopers2) < 0)
                                peC.c1Ctrl = tArray[1].Trim();
                                //else
                                    //cBld(peC.c1Ctrl, ref peC.c1Ns, ref peC.c1ops);
                                sIx1 = sb2.ToString().IndexOf(tArray[1]);
                                sb2.Remove(0, sIx1 + tArray[1].Length);
                                nxtTkn(ref tArray, ref sIx1, ref tQ, ref dQ, ref dS, ref sep, ref sb2);
                                {
                                    sb2.Append(dS.ToString());
                                    tArray = sb2.ToString().Split(sep, 3, StringSplitOptions.RemoveEmptyEntries);
                                }
                                if (sb2[tArray[0].Length] == '?')
                                {
                                    if (tArray[0] != "\\c2")
                                    { }
                                    //if (tArray[1].IndexOfAny(copers1) < 0)
                                    peC.c2Ctrl = tArray[1].Trim();
                                    //else
                                        //cBld(peC.c2Ctrl, ref peC.c2Ns, ref peC.c2ops);
                                    sIx1 = sb2.ToString().IndexOf(tArray[1]);
                                    sb2.Remove(0, sIx1 + tArray[1].Length);
                                }
                                else
                                    MessageBox.Show("clock parse error" + sb1.ToString());
                            }
                            else
                            {
                                bNet bN = new bNet(tArray[0]);
                                bNets.Add(bN);
                                bN.bStr = tArray[1];
                                int vbx = bN.bStr.LastIndexOfAny(copers1);
                                if (vbx > 0 && bN.bStr[vbx] == '!' && (bN.bStr.Substring(vbx).Split(wsp)).Length == 1)
                                {
                                    bN.bStr = bN.bStr.Substring(0, vbx);
                                    sb2.Remove(0, vbx);
                                }
                                else
                                    sb2.Remove(0, tArray[0].Length + tArray[1].Length + 1);
                            }
                        break;
                    case '=':
                        Bus bus = new Bus(tArray[0]);
                        bus.vStr = tArray[1];
                        // check doe '+' and '!' at beginning of new line
                        //nxtTkn(ref tArray, ref sIx1, ref tQ, ref dQ, ref dS, ref sep, ref sb2);
                        //{
                        //    sb2.Append(dS.ToString());
                        //    tArray = sb2.ToString().Split(sep, 3, StringSplitOptions.RemoveEmptyEntries);
                        //}
                        int vIx = bus.vStr.LastIndexOfAny(copers1);
                        if (vIx > 0 && bus.vStr[vIx] == '+' && (bus.vStr.Substring(vIx).Split(wsp)).Length == 1)
                        {
                            bus.vStr = bus.vStr.Substring(0, vIx);
                            sb2.Remove(0, vIx);
                        }
                        else
                        sb2.Remove(0, tArray[0].Length + tArray[1].Length + 1);
                        break;
                    default:
                        break;
                }
                continue;
            }  // input end of stream
            MessageBox.Show(String.Format("{0} lines processed.", lineno));

            /* 
             * if(en) din();  clk.add(reg);
             *  foreach(reg) reg.clk();
             * srFF on on/off, reg on reg
             * reg/ff chooses em ref method based on q
             * puts this on clk ch for clk
            */
            NODE enxt;
            foreach (Event e in events)
            {
                if (e.node == null)
                    e.node = Regs.Find(rnm => rnm.name == e.name);
                //if (e.node != null)
                //    continue;  // Event changes FF
                //e.node.name = events.Find(en => en.node == e.node).name;
            }
                //if (e.pin == null)
            foreach (Event en in events)
            {
                if (en.node != null)// Event changes Pin
                    continue;
                en.node = events.Find(enm => enm.name == en.name).node;
                if (en.node == null)
                {
                    en.node = new NODE(en.name);
                    en.node.val = en.val;
                    continue;
                }
            }
            foreach (bNet bn in bNets)
            {
                cBld(bn.bStr.Trim(), ref bn.bNs, ref bn.bops);
            }

            foreach (eClk clk in eclks)
            {
                cBld(clk.c1Ctrl.Trim(), ref clk.c1Ns, ref clk.c1ops);
                cBld(clk.c2Ctrl.Trim(), ref clk.c2Ns, ref clk.c2ops);
                if (clk.on != null)
                    foreach (FF r in clk.on)
                    {
                        if (r.oncStr == null)
                        { }
                        else
                        cBld(r.oncStr.Trim(), ref r.onNs, ref r.onops);
                    }
                if (clk.off != null)
                    foreach (FF r in clk.off)
                    {
                        if (r.offcStr == null)
                        { }
                        else
                        cBld(r.offcStr.Trim(), ref r.offNs, ref r.offops);
                    }
                //if (clk.reg != null)
                //    foreach (RegEn r in clk.reg)
                //    {
                //        if (r.ctlStr == null)
                //        { }
                //        else
                //r.ctlXpr = bld_rpl(r.ctlStr);
                //}
            }

            while (events.Count != 0 && rn_tm <= run)  // || cascade == 1)
            // Main loop: more events and more run time or maybe asynch
            {

                //foreach (Event e in events)
                //{
                //}
                bool rslt = false;
                if (events[0].time < rn_tm) goto err_stop; // internal error bv1 is event time
                //if (cascade != 1)
                {  //  no asynch event; proceed to next time
                    ln_tm = rn_tm - scn_tm + 1;
                }
                rn_tm = events[0].time; // advance to next event time
                if (rn_tm > run)
                    break; // time to quit
                if (rn_tm > scn_tm + 120) // output screen is full
                    return 'l'; // what for ????
                for (int ex = 0; events.Count > ex; ex++)
                {  //  process events that occur at this time
                    if (events[0].time != rn_tm)
                        break;
                    Event pev = events[0];
                    events.RemoveAt(0);
                   
                    pev.node.val = pev.eVal;
                    if (pev.rpt > 0)
                    {
                        pev.time += pev.rpt; // next time for cyclics
                        addEvent(events, pev);
                    }
                }
                foreach (eClk clk in eclks)  // get new din if(c1_bv) , update q if(c2_bv)
                {
                    if (ctrlX(clk.c1Ns, clk.c1ops, clk.c1Ctrl) == 1)
                    {
                        if (clk.on != null)
                            foreach (FF bc in clk.on) // single bit regs set on
                            {
                                if (bc.qv != 1)
                                {
                                    if (ctrlX(bc.onNs, bc.onops, bc.oncStr) == 1)
                                        {
                                            if (clk.on_ch == null)
                                                clk.on_ch = new List<FF>();
                                            bc.din = 1;
                                            clk.on_ch.Add(bc);
                                        }
                                }
                            }
                        if (clk.off != null)
                            foreach (FF bc in clk.off) // single bit regs reset off
                            {
                                if (bc.qv != 0)
                                {
                                    if (ctrlX(bc.offNs , bc.offops, bc.offcStr) == 1)
                                    {
                                        if (clk.off_ch == null)
                                            clk.off_ch = new List<FF>();
                                        bc.din = 0;
                                        clk.off_ch.Add(bc);
                                    }
                                }
                                //if (clk.reg != null) // multi bit regs load
                                //    foreach (NODE bc in clk.reg)
                                //if (rslt = ctrlX(bc.ctlXpr) ? true : false)
                              
                            }
                    }
                }
                foreach (eClk clk in eclks)  // get new din if(c1_bv) , update q if(c2_bv)
                {
                    if (rslt = (ctrlX(clk.c2Ns, clk.c2ops, clk.c2Ctrl) == 1))
                    {
                        if (clk.on_ch != null)
                            foreach (FF bc in clk.on_ch)
                        {
                            bc.clk();
                            scan(bc, ref bc.scnx, bc.qv, 0);
                        }

                        if (clk.off_ch != null)
                            foreach (FF bc in clk.off_ch)
                            {
                                bc.clk();
                                scan(bc, ref bc.scnx, bc.qv, 0);
                            }

                        if (clk.reg_ch != null)
                        foreach (FF bc in clk.reg_ch)
                        {
                            bc.clk();
                            scan(bc, ref bc.scnx, bc.qv, 0);
                        }
                    }
                    clk.on_ch = null;
                    clk.off_ch = null;
                    clk.reg_ch = null;
                }// clks loop
            }  // events loop

        //public List<List<Point>> waves = new List<List<Point>>();
        //public List<String> wavNms = new List<string>();
        //public List<Point> wPts = new List<Point>();
        //public LinkedListNode<Chng> wavNode;

            //wform3.f3waves = waves;
            wform3.wav_nms = wavNms;
            int xpos = 130, ypos = 20;
            //foreach(wPts in w
            //foreach (Chng c in waves)
            //{
            //    wform3.f3chngs.AddLast(c);
            //}
            scn_out(  0, lB2);
            //end_of_file();
                fs_in.Close();
                return 0;
                err_stop: return -1;
        }

        private bool nxtTkn(ref string[] tA, ref int sIx1
                 , ref Queue<string> tQ, ref Queue<string> dQ, ref StringBuilder dS, ref char[] sep, ref StringBuilder sb2)
        {
            //if(sb2.Length > sIx1)
            //    sb2.Remove(0, sIx1 + 1);
            dS.Remove(0, dS.Length);
            while(sr_in.EndOfStream == false)
            {   // sb mt, no delim, cmnts    
                while ((dS.ToString().IndexOfAny(sep)) < 0)  //.IndexOf(':') < 0 && sb.ToString().IndexOf('=') < 0)
                {
                    if (sr_in.EndOfStream)
                        break;
                    lineno++;
                    string srLine = sr_in.ReadLine();
                    sIx2 = srLine.IndexOf("//");
                    switch(sIx2)
                    {
                        case 0:
                            break;
                        case -1:
                            dS.Append(srLine + ' ');
                            break;
                        default:
                            srLine = srLine.Substring(0, srLine.IndexOf("//")).Trim();
                            dS.Append(srLine + ' ');
                            break;
                    }
                }
                return true;
            }
            return false;
        }

        private void mkTevent(int init, int tokenIx, string tkn)
        {

        }
    // end bnew
        public void ram_gen()
            { // do blk.fouts,  allocate en bits and qouts for cexp inputs
                //foreach (Blk bl in blks) // total blks
                {
                    //foreach (blkBak2 bc in bl.fanout) // blks that combine with bl
                    {

                    }
                }
                // form stringbuilder hexinput, or in 1 bits for minterms matches

            }
        private bool xParse(string src, out int xval)
        {
            if (src.Length > 2 && (src[1] == 'x' || src[1] == 'X'))
            {
                if (!Int32.TryParse(src.Substring(2), NumberStyles.HexNumber, null, out xval))
                {
                    MessageBox.Show("num Parse error");
                    return false;
                }
            }
            else
            {
                if (!Int32.TryParse(src, out xval))
                {
                    MessageBox.Show("num Parse error");
                    return false;
                }
            }
            return true;
        }

        public void addEvent(List<Event> events, Event x)
        {
            int ix = 0;
            foreach (Event e in events)
            {
                if (e.time >= x.time)
                    break;
                ix++;
            }
                events.Insert(ix, x);
        }
        public class rop { public string oper; public int prec; public int opcw;}
        public StringBuilder rpb = new StringBuilder();
        public Stack<rop> ostk = new Stack<rop>();
        //pop the top operator from the operator-stack and write it to output. 
        //If (the top of the operator-stack is a '(' ), then a parentheses-balancing error has occurred. Complain bitterly.
        //  {
        //Choose: 
        //if token is an operand, then write it to output. 
        //qops = opT3.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);


        //Repeat these steps as long as input tokens are available. 

        //Choose: 
        //if token is an operand, then write it to output. 
        //else if token is an operator, then: 
        //while ( (token's precedence) ≤ (precedence of the operator on top of the operator-stack) ): 
        //pop the top operator from the operator-stack and write it to output. 
        //push the token onto the operator-stack. 
        //else if token is '(', then push it onto the operator-stack (with precedence -1). 
        //else if token is ')', then: 
        //while (the top of the operator-stack is not a '(' ): 
        //pop the top operator from the operator-stack and write it to output. 
        //if the operator-stack becomes empty, then a parentheses-balancing error has occurred. Complain bitterly. 
        //pop the '(' off the operator-stack; discard it and the token. 
        //else some token error has occurred. Abandon the conversion. 
        //Repeat these steps as long as input tokens are available. 
        //While (the operator-stack is not empty): 
        //pop the top operator from the operator-stack and write it to output. 
        //If (the top of the operator-stack is a '(' ), then a parentheses-balancing error has occurred. Complain bitterly. 
        //Close the input and the output. 


        //void get_field()  // (int f1, int f2)
        //{
        //    char c;
        //    if ((c = sIn[xOp + 1]) != ']')
        //    {// nx = (int)get_digits(); if (err_cd != 0) goto err;
        //        xOp += sScan[xT++].Length;
        //        if (nx < 0 || nx > 15) { err_cd = 17; goto err; }
        //        if (c == ']')
        //            ny = nx;
        //        else
        //        {
        //            if ((c = sIn[xOp++]) != ':') { err_cd = 17; goto err; }
        //            // ny = (int)get_digits(); if (err_cd != 0) goto err;
        //            xOp += sScan[xT++].Length;
        //            c = sIn[xOp++];
        //            if (c != ']' || ny > 15 || ny < 0) { err_cd = 18; goto err; }
        //        }
        //    }
        //    else
        //    {
        //        nx = 0;
        //        ny = 15;
        //    }
        //    //cd = more();
        //    c = sIn[xOp++];
        //    return;

        //err:
        //    MessageBox.Show("field specification error");
        //    return;
        //}

        int open_files(string fPath)
        {
            fs_in = new FileStream(fPath, FileMode.Open);
            sr_in = new StreamReader(fs_in);
            return 0;
        }


        enum prec
        {
            eq = 1, qm, oo, nn, o, xo, n, ee, lg, srl, pm, md, un
        };

        public bool cprec(ref rop sx)
        {

            switch (sx.oper)
            {
                case "&": sx.prec = (int)prec.n; return true;
                case "|": sx.prec = (int)prec.o; return true;
                case "^": sx.prec = (int)prec.xo; return true;
                case "+": sx.prec = (int)prec.pm; return true;
                case "-": sx.prec = (int)prec.pm; return true;
                case "&&": sx.prec = (int)prec.nn; return true;
                case "||": sx.prec = (int)prec.oo; return true;
                case ">": sx.prec = (int)prec.lg; return true;
                case "<": sx.prec = (int)prec.lg; return true;
                case "!=": sx.prec = (int)prec.ee; return true;
                case "<=": sx.prec = (int)prec.ee; return true;
                case "==": sx.prec = (int)prec.ee; return true;
                case ">=": sx.prec = (int)prec.ee; return true;
                case "*": sx.prec = (int)prec.md; return true;
                case "/": sx.prec = (int)prec.md; return true;
                case "%": sx.prec = (int)prec.md; return true;
                case "<<": sx.prec = (int)prec.srl; return true;
                case ">>": sx.prec = (int)prec.srl; return true;
                case "!":
                case "~": sx.prec = (int)prec.un; return true;
                case "(": sx.prec = -1; return false;
                case "=": sx.prec = -1; return true;
                default: MessageBox.Show("Missed op "); return false;
            }
        }  // end cp

        
        public bool bval(string bnm)
        {
            Stack<String> oS = new Stack<string> (bnm.Split(wsp, StringSplitOptions.RemoveEmptyEntries));
            return bval(oS);
        }
        public bool bval(Stack<String> oS)
        {
            int bqout;
            bool rslt = false;
            //RegEn r;
            String cnot = oS.Peek() == "!" ? oS.Pop() : null;
            if (Char.IsDigit(oS.Peek()[0]) && xParse(oS.Pop(), out bqout))
                rslt = bqout > 0 ? true : false;
            else if (Char.IsLetter(oS.Peek()[0]))
                //if((rslt = (r = Regs.Find(x => x.name == (oS.Peek().Trim()))) != null))
                {
                }
                //rslt = (((Reg)oS.Pop()) > 0 ? true : false;
           return cnot == "!" ? !rslt : rslt;
        }
    

        string seq;
        bool cond, fcond;

        public int valX(string oper, Stack<int> dstk)
        {
            {
                int bop = 0;
                bop = dstk.Pop();
                switch (oper)
                {
                    case ("*"): dstk.Push(dstk.Pop() * bop); seq += " product " + dstk.Peek().ToString(); break;
                    case ("/"): dstk.Push(dstk.Pop() / bop); seq += " quotient " + dstk.Peek().ToString(); break;
                    case ("%"): dstk.Push(dstk.Pop() % bop); seq += " modulus " + dstk.Peek().ToString(); break;
                    case ("+"): dstk.Push(dstk.Pop() + bop); seq += " sum  " + dstk.Peek().ToString(); break;
                    case ("-"): dstk.Push(dstk.Pop() - bop); seq += " rmndr " + dstk.Peek().ToString(); break;
                    case ("<<"): dstk.Push(dstk.Pop() << bop); seq += " lsft " + dstk.Peek().ToString(); break;
                    case (">>"): dstk.Push(dstk.Pop() >> bop); seq += " rsft " + dstk.Peek().ToString(); break;
                    case ("&"): dstk.Push(dstk.Pop() & bop); seq += " band " + dstk.Peek().ToString(); break;
                    case ("|"): dstk.Push(dstk.Pop() | bop); seq += " bor " + dstk.Peek().ToString(); break;
                    case ("^"): dstk.Push(dstk.Pop() ^ bop); seq += " bxor " + dstk.Peek().ToString(); break;
                    case ("<"): seq += " less: cond " + ((cond = ((dstk.Pop() < bop) ? true : false)).ToString()); return (cond) ? 1 : 0; break;
                    case (">"): seq += " gtr: cond " + ((cond = ((dstk.Pop() > bop) ? true : false)).ToString()); return (cond) ? 1 : 0; break;
                    case ("=="): seq += " eql: cond " + ((cond = ((dstk.Pop() == bop) ? true : false)).ToString()); return (cond) ? 1 : 0; break;
                    case ("!="): seq += " cond " + ((cond = ((dstk.Pop() != bop) ? true : false)).ToString()); return (cond) ? 1 : 0; break;
                    case ("<="): seq += " leq: cond " + ((cond = ((dstk.Pop() <= bop) ? true : false)).ToString()); return (cond) ? 1 : 0; break;
                    case (">="): seq += " geq: cond " + ((cond = ((dstk.Pop() >= bop) ? true : false)).ToString()); return (cond) ? 1 : 0; break;
                    default:
                        break;
                }
            }
            return dstk.Pop();
        }

        //public int do_op(string oper, Stack<object> dstk)
        //{
        //    {
        //        int bop = 0;
        //        bop = (int)dstk.Pop();
        //        switch (dstk.Peek().GetType().Name)
        //        {
        //            case "int":

        //                break;
        //        }
        //        switch (oper)
        //        {
        //            case ("*"):
        //                dstk.Push((int)dstk.Pop() * (int)dstk.Pop());
        //                seq += " product " + dstk.Peek().ToString();
        //                break;
        //            case ("/"):
        //                dstk.Push(dstk.Pop() / bop);
        //                seq += " quotient " + dstk.Peek().ToString();
        //                break;
        //            case ("%"):
        //                dstk.Push(dstk.Pop() % bop);
        //                seq += " modulus " + dstk.Peek().ToString();
        //                break;
        //            case ("+"):
        //                dstk.Push(dstk.Pop() + bop);
        //                seq += " sum  " + dstk.Peek().ToString();
        //                break;
        //            case ("-"):
        //                dstk.Push(dstk.Pop() - bop);
        //                seq += " rmndr " + dstk.Peek().ToString();
        //                break;
        //            case ("<<"):
        //                dstk.Push(dstk.Pop() << bop);
        //                seq += " lsft " + dstk.Peek().ToString();
        //                break;
        //            case (">>"):
        //                dstk.Push(dstk.Pop() >> bop);
        //                seq += " rsft " + dstk.Peek().ToString();
        //                break;
        //            case ("&"):
        //                dstk.Push(dstk.Pop() & bop);
        //                seq += " band " + dstk.Peek().ToString();
        //                break;
        //            case ("|"):
        //                dstk.Push(dstk.Pop() | bop);
        //                seq += " bor " + dstk.Peek().ToString();
        //                break;
        //            case ("^"):
        //                dstk.Push(dstk.Pop() ^ bop);
        //                seq += " bxor " + dstk.Peek().ToString();
        //                break;
        //            //case (ops.less):
        //            //    seq += " less: cond " + ((cond = ((aop < bop) ? true : false)).ToString());
        //            //    fcond = (cond) ? false : true;
        //            //    break;
        //            //case (ops.gtr):
        //            //    seq += " gtr: cond " + ((cond = ((aop > bop) ? true : false)).ToString());
        //            //    fcond = (cond) ? false : true;
        //            //    break;
        //            //case (ops.eql):
        //            //    seq += " eql: cond " + ((cond = ((aop == bop) ? true : false)).ToString());
        //            //    fcond = (cond) ? false : true;
        //            //    break;
        //            //case (ops.neq):
        //            //    seq += " cond " + ((cond = ((aop != bop) ? true : false)).ToString());
        //            //    fcond = (cond) ? false : true;
        //            //    break;
        //            //case (ops.leq):
        //            //    seq += " leq: cond " + ((cond = ((aop <= bop) ? true : false)).ToString());
        //            //    fcond = (cond) ? false : true;
        //            //    break;
        //            //case (ops.geq):
        //            //    seq += " geq: cond " + ((cond = ((aop >= bop) ? true : false)).ToString());
        //            //    fcond = (cond) ? false : true;
        //            //    break;
        //            default:
        //                break;
        //        }
        //    }
        //    return dstk.Pop();
        //}

        //Choose: 
        //if token is an operand, then write it to output. 
        //else if token is an operator, then: 
        //while ( (token's precedence) ≤ (precedence of the operator on top of the operator-stack) ): 
        //pop the top operator from the operator-stack and write it to output. 
        //push the token onto the operator-stack. 
        //else if token is '(', then push it onto the operator-stack (with precedence -1). 
        //else if token is ')', then: 
        //while (the top of the operator-stack is not a '(' ): 
        //pop the top operator from the operator-stack and write it to output. 
        //if the operator-stack becomes empty, then a parentheses-balancing error has occurred. Complain bitterly. 
        //pop the '(' off the operator-stack; discard it and the token. 
        //else some token error has occurred. Abandon the conversion. 
        //Repeat these steps as long as input tokens are available. 
        //While (the operator-stack is not empty): 
        //pop the top operator from the operator-stack and write it to output. 
        //If (the top of the operator-stack is a '(' ), then a parentheses-balancing error has occurred. Complain bitterly. 
        //Close the input and the output. 

        //Repeat these steps as long as input tokens are available. 


        public string bld_rpl(string exp)
        {
            string[] xops;
            string[] fld;
            string opT;
            opT = "";
            string sv;
            rop sx2;
            StringBuilder xsb;
            int lp = 0, opIx;
            xops = exp.Split(copers1, 2, StringSplitOptions.RemoveEmptyEntries);
            if (exp[xops[0].Length - 1] == '[')
            {
                fld = exp.Split(bkts, 3, StringSplitOptions.RemoveEmptyEntries);
                if (fld.Length > 1)
                {
                    xsb = new StringBuilder(fld[0].Trim());
                    if (ixf1[2] != "0")
                        xsb.Append(" >> " + Int32.Parse(ixf1[2]));
                    return xsb.ToString();
                }
            }
            xsb = new StringBuilder(exp);
            xops = xsb.ToString().Split(bopers, 2, StringSplitOptions.RemoveEmptyEntries);
            rpb = new StringBuilder(xops[0] + " ");
            opIx = xops[0].Length;
           while(xops.Length > 1)  // more opers to handle
           {
               switch (xsb[xops[0].Length].ToString())
               {
                   case "!":
                       {
                           sx2 = new rop();
                           sx2.oper = "!";
                           cprec(ref sx2);
                           ostk.Push(sx2);
                       }
                       break;
                   case "~":
                       {
                           sx2 = new rop();
                           sx2.oper = "~";
                           cprec(ref sx2);
                           ostk.Push(sx2);
                       }
                       break;
                   case "(":
                       {
                           sx2 = new rop();
                           sx2.oper = "(";
                           sx2.prec = -1;
                           ostk.Push(sx2);
                       }
                       break;
                   case ")":
                       //while (the top of the operator-stack is not a '(' ): 
                       //pop the top operator from the operator-stack and write it to output.
                       //pop the '(' off the operator-stack; discard it and the token.  
                       while (ostk.Peek().oper != "(")
                       {
                           if (ostk.Count == 0)
                               MessageBox.Show("unbalanced parens");
                           rpb.Append(ostk.Pop().oper + " ");
                       }
                       ostk.Pop();
                       if (ostk.Count > 0 && ostk.Peek().oper == "~")
                           rpb.Append(ostk.Pop().oper);
                       break;
                   default:
                       {
                           sx2 = new rop();
                           sx2.oper = xsb[xops[0].Length].ToString();
                           if (cprec(ref sx2))
                               //while ( (token's precedence) ≤ (precedence of the operator on top of the operator-stack) ): 
                               //pop the top operator from the operator-stack and write it to output. 
                               //push the token onto the operator-stack. 
                               while (ostk.Count > 0 && sx2.prec <= ostk.Peek().prec)
                               {
                                   opT += (sv = ostk.Peek().oper);
                                   rpb.Append(" " + ostk.Pop().oper + " ");
                               }
                           ostk.Push(sx2);
                       }
                       break;
               }  // end of ctkn switch 

               xsb.Remove(0, xops[0].Length + 1);
               xops = xsb.ToString().Split(bopers, 3, StringSplitOptions.RemoveEmptyEntries);
               rpb.Append(xops[0] + " ");
           } // while xsb.length > 0
           //Repeat these steps as long as input tokens are available.
           //rpb.Append(xops[0]);
           //While (the operator-stack is not empty):
           while (ostk.Count > 0)
               if (ostk.Peek().oper != "(")
                   rpb.Append(" " + (sv = ostk.Pop().oper) + " ");
               else
                   ostk.Pop();
           if (ostk.Count > 0 && ostk.Peek().oper == "~")
               rpb.Append(ostk.Pop().oper + " ");
           return rpb.ToString();  // rpl[rpl.Count - 1];
        }

        public void cBld(string ctlStr, ref List<NODE> rL, ref List<char> ops)
        {
            string[] rA;
            string sNm;
            int opix = 0, val = 0;
            NODE nODE;
            if (ctlStr[0] == '!')
            {
                ops.Add('!');
                opix = 1;
            rA = ctlStr.Substring(1).Split(bopers1, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                ops.Add(' '); // ctrlX() needs ops.count > 0
                rA = ctlStr.Split(bopers1, StringSplitOptions.RemoveEmptyEntries);
            }
            //opix += rA[0].Length;  // rA[0].Length;
            foreach (string s in rA)
            {
                opix += s.Length;
                if (opix < ctlStr.Length)
                    ops.Add(ctlStr[opix]);
                opix += 1;
                if (s[0] == '!')
                {
                    ops.Add('!');
                    sNm = s.Substring(1).TrimEnd();
                }
                else
                    sNm = s;
                nODE = (Regs.Find(xx => xx.name == sNm.Trim()));
                if (nODE == null)
                {
                    nODE = (events.Find(xx => xx.name == sNm.Trim()));
                    if (nODE == null)
                    {
                        nODE = (bNets.Find(xx => xx.name == sNm.Trim()));
                        if (nODE == null)
                        {
                            nODE = new Event(s);
                            if (char.IsDigit(s.Trim()[0]))
                            {
                                xParse(s, out val);
                                nODE.val = val;
                                ((Event)nODE).node = new NODE(s);
                                ((Event)nODE).node.val = val;
                            }
                        }
                    }
                }
                rL.Add(nODE);
                //opix += s.Length;
                //if (opix < ctlStr.Length)
                //    ops.Add(ctlStr[opix]);
                //opix += 1;
            }
            ops.Add(' ');
        }  // @ EOS bld ctlXpr
        //  peime k !j  & |
        //  '|' return first true call '&'
        //  '&' return first false or '|'

        public class scrn
        {
            public string ln; public string name; public int tm; public int scn_ix;
            public long seek_pos; public List<tPts> trace;
        }
        public struct tPts { public int time; public int val;};
        //   scrn[] scn = new scrn[128];
        public List<scrn> scn = new List<scrn>();
        scrn s_p;

        public void scan(FF ff, ref int scn_ix, int val, long seek_ch)
        {
            int ln_tm = (int)rn_tm;
            if (scn.Count > 0 && scn_ix >= 0 && scn[(int)scn_ix].name == ff.name)
            {
                s_p = scn[(int)scn_ix];
            }
            else
            {
                if (ff.wavx == -1)
                {
                    ff.wavx = waves.Count;
                    waves.Add(ff.Wave);
                }

                s_p = new scrn();
                s_p.ln = new string('.', ln_tm);
                s_p.ln += (val > 0) ? "/" : "\\";
                s_p.tm = rn_tm;
                tPts vt = new tPts();
                vt.time = rn_tm;
                vt.val = val;
                scn_ix = scn.Count;
                s_p.name = ff.name;
                s_p.scn_ix = scn.Count;
                s_p.trace = new List<tPts>();
                s_p.trace.Add(vt);
                scn.Add(s_p);
                return;
            }
            if (s_p.ln.Length < ln_tm)
                s_p.ln = s_p.ln.PadRight(ln_tm, s_p.ln[s_p.ln.Length - 1] == '/' ? '+' : '_');
            s_p.ln += (val != 0 ? '/' : '\\');
            s_p.tm = ln_tm;
            tPts vt2 = new tPts();
            vt2.time = rn_tm;
            vt2.val = val;
            s_p.trace.Add(vt2);
            s_p.seek_pos = seek_ch;
            scn[(int)scn_ix] = s_p;
            return;
        }

        string lbs;
        public List<Point> pts;
        public List<Point[]> scrLn;
        public List<String> wnam;

        //char scn_out(ref byte mode, int ln_tm, ListBox pLBox, ref byte pCode)
        char scn_out( int ln_tm, ListBox pLBox)
        {
            int nssAr = 0;
            int units;
            int tens;
            int tens_ct;
            int length;
            String lbadd = "**Click facility for first change then next change button**";
            lB2.Items.Add(lbadd);
            nssAr++;
            tens = scn_tm % 100;
            units = tens % 10;
            tens /= 10;
            byte cTens = (byte)tens;
            String sLine;
            String sChar;
            byte[] sCh = new byte[128];
            lbadd = "**Time**   ";
            lbadd += scn_tm;
            lB2.Items.Add(lbadd);
            nssAr++;
            sLine = "                    ";
            for (tens_ct = units, length = 0; length < 100; length++)
            {
                sChar = String.Format("{0}", tens);
                sLine += sChar;
                if (tens_ct++ == 9)
                {
                    if (tens++ == 9) tens = 0;
                    tens_ct = 0;
                }
            }
            lB2.Items.Add(sLine);
            sLine = "                    ";
            for (length = 0; length < 100; length++)
            {
                sChar = String.Format("{0}", units);
                sLine += sChar;
                if (units++ == 9) units = 0;
            }
            lB2.Items.Add(sLine);
            string lno;
            int ypos = 0, lmax = rn_tm * 5 + 130;
            scrLn = new List<Point[]>();
            wnam = new List<string>();
            foreach (scrn so in scn)
            {
                pts = new List<Point>();
                lno = so.ln;
                if (lno.Length < ln_tm)
                {
                    if (lno[lno.Length - 1] == '\\') lno += '.';
                    if (lno[lno.Length - 1] == '/') lno += '-';
                }
                if (so.name != null)
                {
                    lbs = String.Format("{0}{1}", so.name.PadRight(20, '.'), lno.PadRight(ln_tm, lno[lno.Length - 1]));
                    lB2.Items.Add(lbs);
                    //if (so.name == "clk")
                    {
                        int endx = 0;
                        for (int ix = 20, xpos = 130; lbs.Length > ix; ix++, xpos += 5)
                        {
                            //if (lno.Length > lmax)
                            //    lmax = lbs.Length;
                            if (lbs[ix] == '/')
                            {
                                if (endx == ypos + 30)
                                    pts.Add(new Point(xpos, 30 + ypos));
                                else
                                {
                                    pts.Add(new Point(xpos, 20 + ypos));
                                    pts.Add(new Point(xpos, 30 + ypos));
                                    pts.Add(new Point(xpos, 20 + ypos));
                                }
                                pts.Add(new Point(xpos, 20 + ypos));
                                endx = ypos + 20;
                            }
                            else if (lbs[ix] == '\\')
                            {
                                pts.Add(new Point(xpos, 20 + ypos));
                                pts.Add(new Point(xpos, 30 + ypos));
                                endx = ypos + 30;
                            }
                        }
                        if (lbs.Length < 120)
                            pts.Add(new Point(lmax, endx));
                    }
                    scrLn.Add(pts.ToArray());
                    wnam.Add(lbs.Substring(0, 20));
                }
                ypos += 20;  // advance to next line
            }

            return 'l';
        }

        public void action(int nSel, ref int nChar, int nTime)
        {
            String sFac = "";
            int find_ix;
            int time_ix;
            char[] log_op = new char[4];
            char[] log_nm = new char[32];
            find_ix = nSel - 4;

            //long seek_ptr = scn[find_ix].seek_pos;
            string[] sScan = new string[16];
            char[] sep = new char[1];
            sep[0] = ' ';
            time_ix = scn_tm + nTime - 17;
            if (nChar == 0)
            {
                if (scn[find_ix].trace == null)
                {
                    sFac = String.Format("No history is available for {0}", scn[find_ix].name);
                    lB3.Items.Insert(0, sFac);
                }
                else
                {
                    sFac = String.Format("{0} first changed at {1,4} val = 0x{2,4:X}",
                        scn[find_ix].name, scn[find_ix].trace[nChar].time, scn[find_ix].trace[nChar++].val);
                    lB3.Items.Insert(0, sFac);
                }
            }
            else
            {
                if (nChar < scn[find_ix].trace.Count)
                    sFac = String.Format("{0}       changed at {1,4} val = 0x{2,4:X} ", scn[find_ix].name, scn[find_ix].trace[nChar].time, scn[find_ix].trace[nChar++].val);
                else sFac = "no more changes";
                lB3.Items.Insert(0, sFac);
            }
        }
        string[] ixf1, ixf2;
        int ix1, ix2;
        uint[] nMsk = new uint[33]  {0x0000, 0x0001, 0x0003, 0x0007, 0x000f,
                                               0x001f, 0x003f, 0x007f, 0x00ff,
                                               0x01ff, 0x03ff, 0x07ff, 0x0fff,
                                               0x1fff, 0x3fff, 0x7fff, 0xffff,
                                               0x0001ffff, 0x0003ffff, 0x0007ffff, 0x000fffff,
                                               0x001fffff, 0x003fffff, 0x007fffff, 0x00ffffff,
                                               0x01ffffff, 0x03ffffff, 0x07ffffff, 0x0fffffff,
                                               0x1fffffff, 0x3fffffff, 0x7fffffff, 0xffffffff};
    };

    //    //Choose: 
    //    //if token is an operand, then write it to output. 
    //      //else if token is an operator, then: 
    //          //while ( (token's precedence) ≤ (precedence of the operator on top of the operator-stack) ): 
    //              //pop the top operator from the operator-stack and write it to output. 
    //          //push the token onto the operator-stack. 
    //    //else if token is '(', then push it onto the operator-stack (with precedence -1). 
    //    //else if token is ')', then: 
    //    //while (the top of the operator-stack is not a '(' ): 
    //    //pop the top operator from the operator-stack and write it to output. 
    //    //if the operator-stack becomes empty, then a parentheses-balancing error has occurred. Complain bitterly. 
    //    //pop the '(' off the operator-stack; discard it and the token. 
    //    //else some token error has occurred. Abandon the conversion. 
    //    //Repeat these steps as long as input tokens are available. 
    //    //While (the operator-stack is not empty): 
    //    //pop the top operator from the operator-stack and write it to output. 
    //    //If (the top of the operator-stack is a '(' ), then a parentheses-balancing error has occurred. Complain bitterly. 
    //    //Close the input and the output. 

    //Repeat these steps as long as input tokens are available. 

    //    //Choose: 
    //    //if token is an operand, then write it to output. 
    //      //else if token is an operator, then: 
    //          //while ( (token's precedence) ≤ (precedence of the operator on top of the operator-stack) ): 
    //              //pop the top operator from the operator-stack and write it to output. 
    //          //push the token onto the operator-stack. 
    //    //else if token is '(', then push it onto the operator-stack (with precedence -1). 
    //    //else if token is ')', then: 
    //    //while (the top of the operator-stack is not a '(' ): 
    //    //pop the top operator from the operator-stack and write it to output. 
    //    //if the operator-stack becomes empty, then a parentheses-balancing error has occurred. Complain bitterly. 
    //    //pop the '(' off the operator-stack; discard it and the token. 
    //    //else some token error has occurred. Abandon the conversion. 
    //    //Repeat these steps as long as input tokens are available. 
    //    //While (the operator-stack is not empty): 
    //    //pop the top operator from the operator-stack and write it to output. 
    //    //If (the top of the operator-stack is a '(' ), then a parentheses-balancing error has occurred. Complain bitterly. 
    //    //Close the input and the output. 

    //Repeat these steps as long as input tokens are available. 

    //foreach (string s in ipts)
    //{
    //    x <<= 1;
    //    x |= 1;
    //}
    //msk = x;
    //y = 0;
    //for (x = 1; x <= msk; x++)
    //{
    //    y = 0;
    //    for (mx = 1; mx < msk; mx <<= 1)
    //    {
    //        if ((mx & x) == mx)
    //        {
    //            bsb.Append(ipts[y]);
    //        }
    //        y++;
    //    }
    //    //bsb.Remove(0, bsb.Length);
    //    bsb.Append(" ");
    //}

    //public bool ctrlx(string cx)
    //{
    //    //bool bx = true, aop = false, bop = false; // let false '&' return false.
    //    string[] bterm;
    //    Stack<bool> bstk = new Stack<bool>();
    //    bterm = cx.Split(wsp, StringSplitOptions.RemoveEmptyEntries); //   new char[] { '&', '|' }, 
    //    if (bterm.Length == 1)
    //        return bval(bterm[0]);
    //    foreach (string s in bterm)
    //    {
    //        switch (s[0])
    //        {
    //            case '_':
    //                return ctrlx(get_blk(s, false).cexp);
    //                break;
    //            case '!':
    //                {
    //                    if (bstk.Pop() == true)
    //                        bstk.Push(false);
    //                    else
    //                        bstk.Push(true);
    //                }
    //                break;
    //            case '&':
    //                {
    //                    bstk.Push(bstk.Pop() && bstk.Pop());
    //                }
    //                break;
    //            case '|':
    //                {
    //                    if (bstk.Pop() == true)
    //                    {
    //                        while (bstk.Count > 0)
    //                            bstk.Pop();
    //                        return true;
    //                    }
    //                }
    //                break;
    //            default:
    //                while (Char.IsLetterOrDigit(s[0]))
    //                {
    //                    Stack<int> dstk = new Stack<int>();
    //                    bstk.Push(get_blk(s, false).qout);
    //                }
    //                //lse
    //                {
    //                    do_op(s, bstk) > 0 ? true : false;
    //                    return true;
    //                }
    //                break;
    //        }
    //    }
    //    return bstk.Pop();
    //}

    //public int datx(string dexp)
    //{
    //    string[] bnms;
    //    Stack<int> dstk = new Stack<int>();
    //    int opix = 0;
    //    //bterm = cx.Split(new char[] { '&', '|', '^' }, StringSplitOptions.RemoveEmptyEntries); //   new char[] { '&', '|' }, 
    //    //StringBuilder bsB = new StringBuilder(dexp);
    //    opix = dexp.IndexOfAny(copers1);  // .ToString()
    //    if (opix < 0)
    //    {
    //        if (dexp[0] == '_')
    //            return datx(get_blk(dexp).dexp);
    //        else if (Char.IsDigit(dexp[0]))
    //            return Int32.Parse(dexp);
    //        else
    //            return get_blk(dexp).qout;
    //    }
    //    bnms = dexp.Split(wsp, StringSplitOptions.RemoveEmptyEntries);
    //    foreach (string s in bnms)
    //    {
    //        if (Char.IsLetterOrDigit(s[0]))
    //            if (dexp[0] == '_')
    //            {
    //                dstk.Push(datx(get_blk(s).dexp));
    //            }
    //            else
    //                dstk.Push(get_blk(s).qout);
    //        else
    //        {
    //            if (s[0] == '!')
    //            {
    //                dstk.Push(get_blk(s.Substring(1)).qout > 0 ? 1 : 0);
    //            }
    //            else if (s[0] == '~')
    //            {
    //                dstk.Push(~dstk.Pop());
    //            }
    //            else
    //            {
    //                dstk.Push(do_op(s, dstk));
    //                switch (s)
    //                {
    //                    case ("*"): dstk.Push(dstk.Pop() * dstk.Pop()); seq += " product " + dstk.Peek().ToString(); break;
    //                    case ("/"): dstk.Push(dstk.Pop() / dstk.Pop()); seq += " quotient " + dstk.Peek().ToString(); break;
    //                    case ("%"): dstk.Push(dstk.Pop() % dstk.Pop()); seq += " modulus " + dstk.Peek().ToString(); break;
    //                    case ("+"): dstk.Push(dstk.Pop() + dstk.Pop()); seq += " sum  " + dstk.Peek().ToString(); break;
    //                    case ("-"): dstk.Push(dstk.Pop() - dstk.Pop()); seq += " rmndr " + dstk.Peek().ToString(); break;
    //                    case ("<<"): dstk.Push(dstk.Pop() << dstk.Pop()); seq += " lsft " + dstk.Peek().ToString(); break;
    //                    case (">>"): dstk.Push(dstk.Pop() >> dstk.Pop()); seq += " rsft " + dstk.Peek().ToString(); break;
    //                    case ("&"): dstk.Push(dstk.Pop() & dstk.Pop()); seq += " band " + dstk.Peek().ToString(); break;
    //                    case ("|"): dstk.Push(dstk.Pop() | dstk.Pop()); seq += " bor " + dstk.Peek().ToString(); break;
    //                    case ("^"): dstk.Push(dstk.Pop() ^ dstk.Pop()); seq += " bxor " + dstk.Peek().ToString(); break;
    //                    case ("<"): seq += " less: cond " + ((cond = ((dstk.Pop() < dstk.Pop()) ? true : false)).ToString()); return (cond) ? 1 : 0; break;
    //                    case (">"): seq += " gtr: cond " + ((cond = ((dstk.Pop() > dstk.Pop()) ? true : false)).ToString()); return (cond) ? 1 : 0; break;
    //                    case ("=="): seq += " eql: cond " + ((cond = ((dstk.Pop() == dstk.Pop()) ? true : false)).ToString()); return (cond) ? 1 : 0; break;
    //                    case ("!="): seq += " cond " + ((cond = ((dstk.Pop() != dstk.Pop()) ? true : false)).ToString()); return (cond) ? 1 : 0; break;
    //                    case ("<="): seq += " leq: cond " + ((cond = ((dstk.Pop() <= dstk.Pop()) ? true : false)).ToString()); return (cond) ? 1 : 0; break;
    //                    case (">="): seq += " geq: cond " + ((cond = ((dstk.Pop() >= dstk.Pop()) ? true : false)).ToString()); return (cond) ? 1 : 0; break;
    //                    default:
    //                        break;
    //                }
    //            }
    //        }
    //    }
    //    return dstk.Pop();
    //}
    /*  nm ? bool               Boolean net one or more nodes
     *  nm = value              Arithmetic net one or more nodes
     *  nm = value ? bool       Bus value gated
     *  \c1 ? boolenable        Delimits clock domain c1 on, off, reg din assign
     *  \c2 ? boolenable        Clocks din to q when boolgate is true
     *  nm : bool ? boolgate    Clocked FF conditional boolgate
     *  +nm: boolgate           Clocked shorthand make FF true
     *  !nm: boolgate           Clocked shorthand make FF false
     *  nm : val ? boolgate     Clocked register conditional boolgate
     *  
     *  FFs, Regs
     *  Boole nets, value nets, muxes, decoders Data busses, Clocks
     */

}
    
