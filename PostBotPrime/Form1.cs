﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Drawing.Drawing2D;
using System.IO;
using Newtonsoft.Json;

namespace PostBotPrime
{
    public partial class Form1 : Form
    {
        _Pbox testbox = new _Pbox(@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam gravida sem sem, vel suscipit velit convallis eleifend. Fusce quis tempus lectus, nec euismod odio. Phasellus tristique, sapien a semper sagittis, lectus nunc vulputate felis, a facilisis dui arcu ut augue.", 100000, 123456789, new PointF(0, 0), 0);
        _Pbox testbox2 = new _Pbox(@" fsregserserfserfserfserfrefserfserfseffdfsfxsefxsefxsxsxsxsxrexscxddddddddddddddddddddddddddddddddddddddddddddddddddddd
Wtf ? "
            , 100000, 123456789, new PointF(0, 0), 1);
        _Pbox testbox3 = new _Pbox(@">>63418245
Any decent program includes hinge and calf wor enough lower body focus. "
            , 100000, 123456789, new PointF(0, 0), 2);
        _Pbox testbox4 = new _Pbox(@"In the end of knee raises, would it be a good idea to do another little movement where I contract the abs a bit more and lift my lower body a bit up/forward? "
            , 100000, 123456789, new PointF(0, 0), 0);
        _Pbox testbox5 = new _Pbox(@">>63418245
Any decent program includes hinge and calf wor aieo aieo goblin aieo goblin ody focus. "
    , 100000, 123456789, new PointF(0, 0), 2);

        public string LoadDirectory = @"C:\Users\Nathan\Documents\ChanJson";



        //class declares
        bool Scrolling = false;
        bool loaded = false;
        int MsTicks = 50;
        float ticks = 0.0f;
        float tickdelta = 1000 / 60;
        float frameincrease = 2f;
        float frames = 0;
        float stopbuffer = 400;
        int buffer = 5;
        PointF Origin = new PointF(0, 0);
        PointF RollingOrigin = new PointF(0, 0);
        List<PointF> scrollpoints = new List<PointF>();
        _Pbox[] loadedthread;

        SpeechSynthesizer Voice1 = new SpeechSynthesizer();
        
        
        int p = 0;
        int f = 0;

        // Artkit
        Pen mypen = new Pen(Color.FromArgb(255,106,0,128), 2f);
       LinearGradientBrush LGbrush = new LinearGradientBrush(new Point(0,0),new Point(100,100),
       Color.FromArgb(255, 152, 255, 152),   // 
       Color.FromArgb(255, 152, 152, 255));  

        LinearGradientBrush BGbrush = new LinearGradientBrush(new Point(1000, 1000),new Point(0, 0),
       Color.FromArgb(255, 255, 0, 255),   // Opaque red
       Color.FromArgb(255, 152, 152, 255));  // Opaque blue)

     LinearGradientBrush Vanbrush = new LinearGradientBrush(new Point(0, 0), new Point(500, 500),
Color.FromArgb(255, 152, 255, 152),   // 
Color.FromArgb(255, 152, 152, 255));

        LinearGradientBrush Van2brush = new LinearGradientBrush(new Point(0, 0), new Point(1000, 500),
Color.FromArgb(255, 152, 255, 152),   // 
Color.FromArgb(255, 152, 152, 255));

        Font stylefont;
        

        //Pen lgpen = new Pen(LGbrush);
        Pen mypen2 = new Pen(Color.FromArgb(255, 106, 0, 128));
        private PointF DrawOrigin;

        public Form1()
        {
            

            var files = Directory.GetFiles(LoadDirectory);
            if (files.Length != 0)
            {
                loadedthread = Loadfile(files[f]);
            }
            else
            {
                loadedthread = new _Pbox[] { testbox, testbox2,testbox3,testbox4,testbox5 };
            }
            InitializeComponent();


          Font tempfont = new Font(panel1.Font,FontStyle.Regular);
          stylefont = tempfont;
           

            System.Timers.Timer Ftimer = new System.Timers.Timer(MsTicks);
            Ftimer.Elapsed += OnTimedEvent;
            Ftimer.AutoReset = true;
            Ftimer.Enabled = true;
            SetDoubleBuffered(panel1);

 

        }
        
        private void OnTimedEvent(object source, ElapsedEventArgs t)
        {
            ticks = ticks + MsTicks;
            if(ticks > tickdelta)
            {
                ticks -= tickdelta;
                // text scroll
                if (Scrolling == true)
                {
                frames= frames + frameincrease;
                panel1.Invalidate();
                
                }

            }

        }

        public class _Pbox
        {
            public string Comment { get; private set; }
            public int PostID { get; private set; }
            public int Unix { get; private set; }
            public int ReplyDepth { get; set; }
            public PointF Dorigin { get; set; }

            public _Pbox(string Com, int postid, int unix, PointF draworigin, int replydepth)
            {
                Comment = Com;
                PostID = postid;
                Unix = unix;
                Dorigin = draworigin;
                ReplyDepth = replydepth;
            }

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            //test array
            _Pbox[] loaderposts = loadedthread;

            Graphics g = e.Graphics;
            Point origin = new Point(0, 0);
            Size formsize = new Size(this.Width, this.Height);
            Rectangle bg = new Rectangle(origin, formsize);


            g.FillRectangle(BGbrush, bg);


            if (Scrolling == true)
            {
                RollingOrigin.Y = (0 + buffer) - (frames/1);
            }
            else
            {
                RollingOrigin.Y = (0 + buffer);
            }


            //drawing posts area
            for (int i = 0; i < loaderposts.Length; i++) 
            {
                //origin set
              DrawOrigin = new PointF(0, testbox.Dorigin.Y + RollingOrigin.Y);  

                
               
                
                //current box to draw
                DrawPost(loaderposts[i], e);
            }
            loaded = true;
        }

        public void DrawPost(_Pbox POST, PaintEventArgs e)
        {

            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.VerticalCenter;

            //origin for post set
            PointF PostOrigin = new PointF(POST.Dorigin.X + buffer, POST.Dorigin.Y + buffer);


            PostOrigin.Y =+ RollingOrigin.Y;

            //inital post origin objects
            if (loaded == false)
            {
                scrollpoints.Add(PostOrigin);

            }


            //if scrolling check if we passed the next checkpoint
            if (Scrolling == true && loaded == true && p < 0) // p less then 0 for inital post
            {
                if (-RollingOrigin.Y > (scrollpoints[p + 1].Y - stopbuffer))
                {
                    Scrolling = false;
                }
            }
            else if (Scrolling == true && loaded == true && p <= scrollpoints.Count) // midrolling
            {
                if (-RollingOrigin.Y > scrollpoints[p - 1].Y - stopbuffer)
                {
                    Scrolling = false;
                }

            }
            else if (loaded == true && p == scrollpoints.Count - 1) // endstop
            {
                if (-RollingOrigin.Y > scrollpoints[p-1].Y - stopbuffer)
                {
                    Scrolling = false;
                }
            }

            //inital check for replychain object
            if (POST.ReplyDepth > 0)
            {
                PostOrigin.X = PostOrigin.X + (POST.ReplyDepth * 25);
                //create reply chain graphic TO DO

            }


            PointF AntiOrigin = new PointF(panel1.Width, PostOrigin.Y);
            PointF AntiOriginB = new PointF(AntiOrigin.X - buffer, AntiOrigin.Y - buffer);

            PointF ImageOrigin = new PointF(PostOrigin.X + buffer, PostOrigin.Y + buffer);
            SizeF ImageSize = new SizeF(125, 125);

            PointF HeaderOrigin = new PointF(ImageOrigin.X + ImageSize.Width + buffer, PostOrigin.Y + buffer);
            SizeF HeaderSize = new SizeF((AntiOriginB.X - HeaderOrigin.X - buffer), (HeaderOrigin.Y - AntiOriginB.Y - buffer));
            Size HeaderSizeInt = new Size((int)(AntiOriginB.X - HeaderOrigin.X - buffer), (int)(HeaderOrigin.Y - AntiOriginB.Y - buffer));


             string header = "No. " + POST.PostID.ToString() + "                  Unix. " + POST.Unix.ToString();

            Size hsize = TextRenderer.MeasureText(g, header, panel1.Font, HeaderSizeInt, flags);
            HeaderSize.Height = HeaderSize.Height + hsize.Height;

            PointF TextOrigin = new PointF(ImageOrigin.X + ImageSize.Width + buffer, PostOrigin.Y + HeaderSize.Height + buffer);
           

            SizeF PostSize = new SizeF((panel1.Width - PostOrigin.X - 5), 25);

            SizeF TextSize = new SizeF((AntiOriginB.X - TextOrigin.X - buffer), (TextOrigin.Y - AntiOriginB.Y - buffer));
            Size TextSizeInt = new Size((int)(AntiOriginB.X - TextOrigin.X - buffer), (int)(TextOrigin.Y - AntiOriginB.Y - buffer));

           
          
            Size size = TextRenderer.MeasureText(g, POST.Comment, panel1.Font, TextSizeInt, flags);
           

            TextSize.Height = TextSize.Height + size.Height;
           

            if ((TextSize.Height + HeaderSize.Height) < ImageSize.Height)
            {
                PostSize.Height = ImageSize.Height + PostSize.Height;
            }
            else
            {
                PostSize.Height = TextSize.Height + HeaderSize.Height + PostSize.Height;
            }

            RectangleF PostBox = new RectangleF(PostOrigin, PostSize);
            Rectangle textbounds = new Rectangle((int)TextOrigin.X,(int)TextOrigin.Y, (int)TextSize.Width, (int)TextSize.Height);
            Rectangle headerbounds = new Rectangle((int)HeaderOrigin.X, (int)HeaderOrigin.Y, (int)HeaderSize.Width, (int)HeaderSize.Height);

            g.FillRectangle(Van2brush, PostOrigin.X, PostOrigin.Y, PostSize.Width, PostSize.Height);
            g.DrawRectangle(mypen, PostOrigin.X, PostOrigin.Y, PostSize.Width, PostSize.Height);

            g.DrawRectangle(mypen, ImageOrigin.X, ImageOrigin.Y, ImageSize.Width, ImageSize.Height);
            //g.DrawRectangle(mypen, TextOrigin.X, TextOrigin.Y, TextSize.Width, TextSize.Height);
            g.DrawRectangle(mypen, HeaderOrigin.X, HeaderOrigin.Y, HeaderSize.Width, HeaderSize.Height);


            // g.DrawRectangle(mypen, PostOrigin.X, PostOrigin.Y, PostSize.Width, PostSize.Height);
            TextRenderer.DrawText(g, POST.Comment, stylefont, textbounds, mypen2.Color, Color.Transparent, flags);
            TextRenderer.DrawText(g, header, stylefont, headerbounds, mypen2.Color, Color.Transparent, flags);

            //next posts origin set
            RollingOrigin.Y = (RollingOrigin.Y + PostBox.Height + buffer);

        }
        public static void SetDoubleBuffered(Control control)
        {
            // set instance non-public property with name "DoubleBuffered" to true
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        public static void Save(_Pbox[] sorted, string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.Write(JsonConvert.SerializeObject(sorted));
            }

        }
        public _Pbox[] Loadfile(string path)
        {
            using (var reader = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<_Pbox[]>(reader.ReadToEnd());
            }

        }

        public void ReadNextPost(object sender, EventArgs e)
        {
            _Pbox[] loaderposts = new _Pbox[] { testbox, testbox2, testbox3, testbox4 , testbox5};
            
            if (p < loaderposts.Length-1)
            {
                panel1.Invalidate();
             
                 Voice1.SpeakAsync(loaderposts[p].Comment);
                    Scrolling = true;
                 p++;
       
            }
            else if (p < loaderposts.Length)
            {
                

                 Voice1.SpeakAsync(loaderposts[p].Comment);
                Scrolling = true;
                p++;
            }

                Voice1.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(ReadNextPost);        
        }

       private void Form1_Load(object sender, EventArgs e)
        {
           // SpeechSynthesizer Voice1 = new SpeechSynthesizer();
            Voice1.SetOutputToDefaultAudioDevice();
            Voice1.SelectVoiceByHints(VoiceGender.Male);
            Voice1.Rate = 4;
            _Pbox[] loaderposts = new _Pbox[] { testbox, testbox2, testbox3, testbox4, testbox5 };
           

            //start first post, then event
            Voice1.SpeakAsync("");
            Voice1.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(ReadNextPost);


        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Point origin = new Point(0, 0);
            Size formsize = new Size(this.Width, this.Height);
            Rectangle bg = new Rectangle(origin, formsize);


         g.FillRectangle(BGbrush,bg);

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Point origin = new Point(0, 0);
            Size formsize = new Size((this.panel3.Width - 1), (this.panel3.Height - 1));
            Size formborder = new Size(this.panel3.Width, this.panel3.Height);
            Rectangle bg = new Rectangle(origin, formborder);
            Rectangle brd = new Rectangle(origin, formsize);
            g.FillRectangle(Vanbrush, bg);
            g.DrawRectangle(mypen, brd);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Point origin = new Point(0, 0);
            Size formsize = new Size((this.panel2.Width-1), (this.panel2.Height-1));
            Size formborder = new Size(this.panel2.Width, this.panel2.Height);
            Rectangle bg = new Rectangle(origin, formborder);
            Rectangle brd = new Rectangle(origin, formsize);

            g.FillRectangle(Vanbrush, bg);
           // g.DrawRectangle(mypen2, bg);
            g.DrawRectangle(mypen, brd);

        }
    }

  

}
