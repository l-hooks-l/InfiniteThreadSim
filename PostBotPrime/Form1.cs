using System;
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

namespace designUI
{
    public partial class Form1 : Form
    {
        _Pbox testbox = new _Pbox("@What the fuck did you just fucking say about me, you little bitch? I’ll have you know I graduated top of my class in the Navy Seals, and I’ve been involved in numerous secret raids on Al-Quaeda, and I have over 300 confirmed kills. I am trained in gorilla warfare and I’m the top sniper in the entire US Armed Forces. You are nothing to me but just another target. I will wipe you the fuck out with precision the likes of which has never been seen before on this Earth, mark my fucking words. You think you can get away with saying that shit to me over the Internet? Think again, fucker. As we speak, I am contacting my secret network of spies across the USA and your IP is being traced right now so you better prepare for the storm, maggot. The storm that wipes out the pathetic little thing you call your life. You’re fucking dead, kid. I can be anywhere, anytime, and I can kill you in over seven hundred ways, and that’s just with my bare hands. Not only am I extensively trained in unarmed combat, but I have access to the entire arsenal of the United States Marine Corps and I will use it to its full extent to wipe your miserable ass off the face of the continent, you little shit. If only you could have known what unholy retribution your little “clever” comment was about to bring down upon you, maybe you would have held your fucking tongue. But you couldn’t, you didn’t, and now you’re paying the price, you goddamn idiot. I will shit fury all over you and you will drown in it. You’re fucking dead, kiddo. "
            , 100000, 123456789, new PointF(0, 0), 0);
        _Pbox testbox2 = new _Pbox(@">>63413290 
Why do some do vertical and horizontal push / pull stuff but nobody does vertical / horizontal squats ? Almost nobody even does hinges, it's just push/pull/squat or h.push/v.push/h.pull/v.pull/squats.

Wtf ? "
            , 100000, 123456789, new PointF(0, 0), 1);
        _Pbox testbox3 = new _Pbox(@">>63418245
Any decent program includes hinge and calf work. But even that isn’t enough imo. Calisthenics really suffers from a lack of low body scaleability. There’s not an easy way of working out hips, abductors, and the tibialis with bodyweight.
Though frankly that’s a problem with most male /fit/izens. Not enough lower body focus. "
            , 100000, 123456789, new PointF(0, 0), 2);
        _Pbox testbox4 = new _Pbox(@"In the end of knee raises, would it be a good idea to do another little movement where I contract the abs a bit more and lift my lower body a bit up/forward? "
            , 100000, 123456789, new PointF(0, 0), 0);

        //class declares
        readonly bool Scrolling = true;
        int MsTicks = 50;
        float ticks = 0.0f;
        float tickdelta = 1000 / 60;
        float frameincrease = 1f;
        float frames = 0;
        int buffer = 5;
        PointF Origin = new PointF(0, 0);
        PointF RollingOrigin = new PointF(0, 0);

        // Artkit
        Pen mypen = new Pen(Color.White);
        Pen pen2 = new Pen(Color.White);
        Pen pen3 = new Pen(Color.White);


        public Form1()
        {
           
            InitializeComponent();

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
                frames= frames + frameincrease;
                panel1.Invalidate();
                
            }

        }

        public class _Pbox
        {
            public string Comment { get; private set; }
            public int PostID { get; private set; }
            public int Unix { get; private set; }
            public int ReplyDepth { get; private set; }
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
            _Pbox[] loaderposts = new _Pbox[] { testbox, testbox2, testbox3, testbox4 };

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
                PointF DrawOrigin = new PointF(0, testbox.Dorigin.Y + RollingOrigin.Y);
                loaderposts[i].Dorigin = DrawOrigin;

                //current box to draw
                DrawPost(loaderposts[i], e);
            }
        }

        public void DrawPost(_Pbox POST, PaintEventArgs e)
        {

            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.Bottom | TextFormatFlags.WordBreak;

            //origin for post set
            PointF PostOrigin = new PointF(POST.Dorigin.X + buffer, POST.Dorigin.Y + buffer);


            PostOrigin.Y =+ RollingOrigin.Y;

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

            PointF TextOrigin = new PointF(ImageOrigin.X + ImageSize.Width + buffer, PostOrigin.Y + buffer);

            SizeF PostSize = new SizeF((panel1.Width - PostOrigin.X - 5), 25);

            SizeF TextSize = new SizeF((AntiOriginB.X - TextOrigin.X - buffer), (TextOrigin.Y - AntiOriginB.Y - buffer));
            Size TextSizeInt = new Size((int)(AntiOriginB.X - TextOrigin.X - buffer), (int)(TextOrigin.Y - AntiOriginB.Y - buffer));

          
            Size size = TextRenderer.MeasureText(g, POST.Comment, panel1.Font, TextSizeInt, flags);
            TextSize.Height = TextSize.Height + size.Height;

            if (TextSize.Height < ImageSize.Height)
            {
                PostSize.Height = ImageSize.Height + PostSize.Height;
            }
            else
            {
                PostSize.Height = TextSize.Height + PostSize.Height;
            }

            RectangleF PostBox = new RectangleF(PostOrigin, PostSize);
            Rectangle textbounds = new Rectangle((int)TextOrigin.X,(int)TextOrigin.Y, (int)TextSize.Width, (int)TextSize.Height);

            g.DrawRectangle(mypen, PostOrigin.X, PostOrigin.Y, PostSize.Width, PostSize.Height);
            g.DrawRectangle(mypen, ImageOrigin.X, ImageOrigin.Y, ImageSize.Width, ImageSize.Height);
            //g.DrawRectangle(mypen, TextOrigin.X, TextOrigin.Y, TextSize.Width, TextSize.Height);

           // g.DrawRectangle(mypen, PostOrigin.X, PostOrigin.Y, PostSize.Width, PostSize.Height);
            TextRenderer.DrawText(g, POST.Comment, panel1.Font, textbounds, Color.White, panel1.BackColor, flags);
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
       
    }
            
}
