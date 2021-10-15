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
using System.Drawing.Drawing2D;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace PostBotPrime
{

    public partial class Form1 : Form
    {
 

        public string LoadDirectory = @"C:\chanjson";
        public string ImageDirectory = @"";
        public string IconDirectory = @"C:\Users\Nathan\Pictures\Icons";
        public string MusicDirectory = @"";

        public int Mode = 0; //default loading mode, 0 custom playlist/ 1 top weights/ 2 pre decided playlist directory at random

        //class declares
        bool Scrolling = false;
        bool loaded = false;
        int MsTicks = 50;
        float ticks = 0.0f;
        float tickdelta = 1000 / 60;
        float frameincrease = 1f;
        float frames = 0;
        float stopbuffer = 400;
        int buffer = 5;
        PointF Origin = new PointF(0, 0);
        PointF RollingOrigin = new PointF(0, 0);
        List<PointF> scrollpoints = new List<PointF>();
        public List<_Pbox> loadedthread;
        public Dictionary<int, float> ThreadPoints;
        public string CB = "init";

        Dictionary<string,SpeechSynthesizer> loadedvoices = new Dictionary<string,SpeechSynthesizer>();
        SpeechSynthesizer activevoice = new SpeechSynthesizer();
        public static Dictionary<string, Image> loadedimages = new Dictionary<string, Image>();

        Image IconImage;
        float ScrollCord = 0.00f;
        int ScrollingMulti = 1;


        int CurP = 0;
        int UPThresh = 4;
        int LOWThresh = 4;
        int UPbumper = 0;
        int LOWbumper = 0;
        int depththresh = 100;

        int p = 0;
        int f = 0;
        int b = 0;



        Rectangle bg;


        // Artkit
        Pen mypen = new Pen(Color.FromArgb(255,106,0,128), 2f);
       LinearGradientBrush LGbrush = new LinearGradientBrush(new Point(0,0),new Point(100,100),
       Color.FromArgb(255, 152, 255, 152),   // 
       Color.FromArgb(255, 152, 152, 255));  

        LinearGradientBrush BGbrush = new LinearGradientBrush(new Point(1000, 1000),new Point(0, 0),
       Color.FromArgb(255, 255, 0, 255),   // Opaque red
       Color.FromArgb(255, 152, 152, 255));  // Opaque blue)

     LinearGradientBrush Vanbrush = new LinearGradientBrush(new Point(0, 0), new Point(500, 500),
Color.FromArgb(255, 252, 255, 152),   // 
Color.FromArgb(255, 152, 152, 255));

        LinearGradientBrush Van2brush = new LinearGradientBrush(new Point(0, 0), new Point(1000, 500),
Color.FromArgb(255, 152, 255, 152),   // 
Color.FromArgb(255, 152, 152, 255));

        Font stylefont;
        int checker = 0;

        //Pen lgpen = new Pen(LGbrush);
        Pen mypen2 = new Pen(Color.FromArgb(255, 106, 0, 128));
        private PointF DrawOrigin;

        public Form1()
        {

            loadedthread = LoadNewThread();
          
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
                frames= frames + (frameincrease*ScrollingMulti );
                panel1.Invalidate();
                //    panel2.Invalidate();
                //    panel3.Invalidate();
                
                }

            }

        }

        
        [Serializable] public class _Pbox
        {

            public string Data { get; private set; }
            public string SpokenData { get; set; }
            public int PostID { get; private set; }
            public int Unix { get; private set; }
            public int ReplyDepth { get; set; }
            public PointF Dorigin { get; set; }
            public PointF EndOrigin { get; set; }
            public bool hasExt { get; set; }
            public string Imagepath { get; set; }
            public int Weight { get; set; }
            public string Board { get; set; }
            public string voicesetting { get; set; }

            
          public _Pbox(string Com,string spokencom, int postid, int unix, PointF draworigin, PointF endorigin, int replydepth, bool ext,string imagepath, int weight, string board,string Voice)
            {

                Data = Com;
                SpokenData = spokencom;
                PostID = postid;
                Unix = unix;
                Dorigin = draworigin;
                EndOrigin = endorigin;
                ReplyDepth = replydepth;
                hasExt = ext;
                Weight = weight;
                Board = board;
                Imagepath = imagepath;
               voicesetting = Voice;
            }
            public void setvoice(string voice)
            {
                voicesetting = voice;
            }
            public string getVoice()
            {
                if(voicesetting != null)
                {
                    return voicesetting;
                }
                return null;
            }
 
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            //test array
            //List<_Pbox> loaderposts = loadedthread;
               Graphics g = e.Graphics;

            if (loaded != true) //init 
            {
                ThreadPoints = GrabThreadPoints(loadedthread, e); // create threadpaoint array

                loadedimages = GrabImages(loadedthread);

                Rectangle rect = new Rectangle(0,0,panel1.Width,panel1.Height);
                Region bounds = new Region(rect);
                g.Clip = bounds;

            Point origin = new Point(0, 0);
            Size formsize = new Size(this.Width, this.Height);
            bg = new Rectangle(origin, formsize);

            }



            g.FillRectangle(BGbrush, bg);

            if(p < 10 && frames > 5000)
            {
                //frames way too high for current position  --  reset
                frames = 0;

            }
            // if p is greater then loadedposts.count load up next thread
            if(p >= loadedthread.Count())
            {
                loadedthread = LoadNewThread();
                ThreadPoints = GrabThreadPoints(loadedthread, e); // create threadpaoint array
                Scrolling = false;
                RollingOrigin.Y = 0;
                frames = 0;
                loaded = false;
                ScrollingMulti = 1;
                frames = 0;
                panel1.Invalidate();
                panel1.Refresh();
            }

            if (loadedthread[p].Dorigin.Y - stopbuffer > (ScrollCord + frames)) //current post location vs scrolling cordinate 
            {
                Scrolling = true;
                if (((loadedthread[p].Dorigin.Y - stopbuffer) - (ScrollCord + frames)) > 200)
                {
                    ScrollingMulti = ScrollingMulti*2;
                }
              //  if (loadedthread[1].Dorigin.Y > HurryUp)
                {
                    // add hurry up to origin to snap back to current post
                }
            }
           else
            {
                Scrolling = false;
                ScrollingMulti = 1;
            }
           

            for (int i = LOWbumper; i < UPbumper; i++) // draw loaded posts
            {

                DrawPost1(loadedthread[i], e);

            }

            loaded = true;

        }

        private List<_Pbox> LoadNewThread()
        {
            List<_Pbox> returnlist = new List<_Pbox>();
            switch(Mode)
            {
                case 0:
                    // case 0 custom playlist
                      Directory.CreateDirectory($"{LoadDirectory}//Defaultplaylist");
                     var files0 = Directory.GetFiles($"{LoadDirectory}//Defaultplaylist");

                    if(files0.Length == 0)
                    {
                        // no files in thread directory

                    }

                    if (f > files0.Length)
                    {
                        //reset f to fit new directory
                        f = 0;
                    }

                    returnlist = Loadfile(files0[f]);
                    f++;

                    break;
                case 1:
                    // case 1 top playlist
                    var files1 = Directory.GetFiles($"{LoadDirectory}//Defaultplaylist");
                    if (files1.Length == 0)
                    {
                        // no files in thread directory

                    }

                    if (f > files1.Length)
                    {
                        //reset f to fit new directory
                        f = 0;
                    }

                    returnlist = Loadfile(files1[f]);
                    f++;

                    break;
                case 2:
                    //case 2 predecided playlist; grab from random directory
                    var files2 = Directory.GetFiles($"{LoadDirectory}//Defaultplaylist");
                    if (files2.Length == 0)
                    {
                        // no files in thread directory

                    }

                    if (f > files2.Length)
                    {
                        //reset f to fit new directory
                        f = 0;
                    }

                    returnlist = Loadfile(files2[f]);
                    f++;
                    break;

            }
            p = 0;
            loaded = false;
            RollingOrigin = new PointF(0, 0);
            return returnlist;
        }

        private Dictionary<string, Image> GrabImages(List<_Pbox> loadedthread)
        {
            Dictionary<string, Image> imgbank = new Dictionary<string, Image>();
            BinaryFormatter formatter = new BinaryFormatter();            
            foreach(_Pbox post in loadedthread)
            {
               if(post.hasExt != true) //post has no extension
                {
                    continue;
                }

                FileStream Imagestream = File.OpenRead(post.Imagepath);
                Image image = Image.FromStream(Imagestream);

                if(imgbank.ContainsKey(post.Imagepath))  //image already added
                {
                    continue;
                }
                else
                {
                    imgbank.Add(post.Imagepath,image);
                }


            }

            return imgbank;
            
        }

        private Dictionary<int, float> GrabThreadPoints(List<_Pbox> loadedthread, PaintEventArgs e)
        {

            Dictionary<int, float> PostPoints = new Dictionary<int, float>();
            b = 0;
            PointF PostOrigin = new PointF(0 + buffer, 0);
            for (int i = 0; i < loadedthread.Count; i++)
            {
            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.VerticalCenter;

                //origin for post set


                PostOrigin.X = (0 + buffer);
                if (loadedthread[i].ReplyDepth > 0) //replydepth
                {
                    
                    if (PostOrigin.X < depththresh)
                    {
                        PostOrigin.X = PostOrigin.X + (loadedthread[i].ReplyDepth * 8);
                    }
                    else
                    {
                        PostOrigin.X = PostOrigin.X + depththresh;
                    }
                    //create reply chain graphic TO DO

                }


                PointF AntiOrigin = new PointF(panel1.Width, PostOrigin.Y);
                PointF AntiOriginB = new PointF(AntiOrigin.X - buffer, AntiOrigin.Y - buffer);

                PointF ImageOrigin = new PointF(PostOrigin.X + buffer, PostOrigin.Y + buffer);
                SizeF ImageSize = new SizeF(0, 0);
                if (loadedthread[i].hasExt == true)
                {
                    ImageSize = new SizeF(125, 125);
                }


                PointF HeaderOrigin = new PointF(ImageOrigin.X + ImageSize.Width + buffer, PostOrigin.Y + buffer);
                SizeF HeaderSize = new SizeF((AntiOriginB.X - HeaderOrigin.X - buffer), (HeaderOrigin.Y - AntiOriginB.Y - buffer));
                Size HeaderSizeInt = new Size((int)(AntiOriginB.X - HeaderOrigin.X - buffer), (int)(HeaderOrigin.Y - AntiOriginB.Y - buffer));


                string header = "No. " + loadedthread[i].PostID.ToString() + "                  Unix. " + loadedthread[i].Unix.ToString();

                Size hsize = TextRenderer.MeasureText(e.Graphics, header, panel1.Font, HeaderSizeInt, flags);
                HeaderSize.Height = HeaderSize.Height + hsize.Height;

                PointF TextOrigin = new PointF(ImageOrigin.X + ImageSize.Width + buffer, PostOrigin.Y + HeaderSize.Height + buffer);


                SizeF PostSize = new SizeF((panel1.Width - PostOrigin.X - 5), 25);

                SizeF TextSize = new SizeF((AntiOriginB.X - TextOrigin.X - buffer), (TextOrigin.Y - AntiOriginB.Y - buffer));
                Size TextSizeInt = new Size((int)(AntiOriginB.X - TextOrigin.X - buffer), (int)(TextOrigin.Y - AntiOriginB.Y - buffer));



                Size size = TextRenderer.MeasureText(e.Graphics, loadedthread[i].Data, panel1.Font, TextSizeInt, flags);


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
                Rectangle textbounds = new Rectangle((int)TextOrigin.X, (int)TextOrigin.Y, (int)TextSize.Width, (int)TextSize.Height);
                Rectangle headerbounds = new Rectangle((int)HeaderOrigin.X, (int)HeaderOrigin.Y, (int)HeaderSize.Width, (int)HeaderSize.Height);










                RollingOrigin.Y = (RollingOrigin.Y + PostBox.Height + buffer);
                // PostOrigin.Y = +RollingOrigin.Y;
                loadedthread[i].Dorigin = PostOrigin;
                loadedthread[i].EndOrigin = RollingOrigin;

                PostPoints.Add(b, RollingOrigin.Y);
                PostOrigin.Y =+ RollingOrigin.Y;
                b++;

            }
            return PostPoints;

        }

        public void DrawPost1(_Pbox Post, PaintEventArgs e)
        {
           

            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.VerticalCenter;

            //origin for post set
            PointF PostOrigin = new PointF(Post.Dorigin.X, Post.Dorigin.Y - frames);


            PostOrigin.X = (0 + buffer);
            if (Post.ReplyDepth > 0) //replydepth
            {

                if (PostOrigin.X < depththresh)
                {
                    PostOrigin.X = PostOrigin.X + (Post.ReplyDepth * 8);
                }
                else
                {
                    PostOrigin.X = PostOrigin.X + depththresh;
                }
                //create reply chain graphic TO DO

            }


            PointF AntiOrigin = new PointF(panel1.Width, PostOrigin.Y);
            PointF AntiOriginB = new PointF(AntiOrigin.X - buffer, AntiOrigin.Y - buffer);

            PointF ImageOrigin = new PointF(PostOrigin.X + buffer, PostOrigin.Y + buffer);
            SizeF ImageSize = new SizeF(0, 0);
            if (Post.hasExt == true)
            {
            ImageSize = new SizeF(125, 125);
            }


            PointF HeaderOrigin = new PointF(ImageOrigin.X + ImageSize.Width + buffer, PostOrigin.Y + buffer);
            SizeF HeaderSize = new SizeF((AntiOriginB.X - HeaderOrigin.X - buffer), (HeaderOrigin.Y - AntiOriginB.Y - buffer));
            Size HeaderSizeInt = new Size((int)(AntiOriginB.X - HeaderOrigin.X - buffer), (int)(HeaderOrigin.Y - AntiOriginB.Y - buffer));


            string header = "No. " + Post.PostID.ToString() + "                  Unix. " + Post.Unix.ToString();

            Size hsize = TextRenderer.MeasureText(e.Graphics, header, panel1.Font, HeaderSizeInt, flags);
            HeaderSize.Height = HeaderSize.Height + hsize.Height;

            PointF TextOrigin = new PointF(ImageOrigin.X + ImageSize.Width + buffer, PostOrigin.Y + HeaderSize.Height + buffer);


            SizeF PostSize = new SizeF((panel1.Width - PostOrigin.X - 5), 25);

            SizeF TextSize = new SizeF((AntiOriginB.X - TextOrigin.X - buffer), (TextOrigin.Y - AntiOriginB.Y - buffer));
            Size TextSizeInt = new Size((int)(AntiOriginB.X - TextOrigin.X - buffer), (int)(TextOrigin.Y - AntiOriginB.Y - buffer));



            Size size = TextRenderer.MeasureText(e.Graphics, Post.Data, panel1.Font, TextSizeInt, flags);


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
            Rectangle textbounds = new Rectangle((int)TextOrigin.X, (int)TextOrigin.Y, (int)TextSize.Width, (int)TextSize.Height);
            Rectangle headerbounds = new Rectangle((int)HeaderOrigin.X, (int)HeaderOrigin.Y, (int)HeaderSize.Width, (int)HeaderSize.Height);

            e.Graphics.FillRectangle(Van2brush, PostOrigin.X, PostOrigin.Y, PostSize.Width, PostSize.Height);
            e.Graphics.DrawRectangle(mypen, PostOrigin.X, PostOrigin.Y, PostSize.Width, PostSize.Height);

            e.Graphics.DrawRectangle(mypen, ImageOrigin.X, ImageOrigin.Y, ImageSize.Width, ImageSize.Height); //post image bounding box
            if(Post.hasExt)
            {
            e.Graphics.DrawImage(loadedimages[Post.Imagepath] ,ImageOrigin.X, ImageOrigin.Y, ImageSize.Width, ImageSize.Height);
            }



            //g.DrawRectangle(mypen, TextOrigin.X, TextOrigin.Y, TextSize.Width, TextSize.Height);
            e.Graphics.DrawRectangle(mypen, HeaderOrigin.X, HeaderOrigin.Y, HeaderSize.Width, HeaderSize.Height);


            // g.DrawRectangle(mypen, PostOrigin.X, PostOrigin.Y, PostSize.Width, PostSize.Height);
            TextRenderer.DrawText(e.Graphics, Post.Data, stylefont, textbounds, mypen2.Color, Color.Transparent, flags);
            TextRenderer.DrawText(e.Graphics, header, stylefont, headerbounds, mypen2.Color, Color.Transparent, flags);


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

           
          
            Size size = TextRenderer.MeasureText(g, POST.Data, panel1.Font, TextSizeInt, flags);
           

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
            TextRenderer.DrawText(g, POST.Data, stylefont, textbounds, mypen2.Color, Color.Transparent, flags);
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
        public List<_Pbox> Loadfile(string path)
        {


       /*     using (var reader = new StreamReader(path))
            {        
                var temp2 = JsonConvert.DeserializeObject<IEnumerable<_Pbox>>(reader.ReadToEnd()).ToList();

                return temp2;
            } */


          /*  System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(sorted.GetType());
            Stream stream = File.Open(path, FileMode.Open);
            List<_Pbox> loadedfile = (List<_Pbox>)x.Deserialize(stream);

            stream.Close();
            return loadedfile; */

            Stream stream = File.Open(path, FileMode.Open);              //binary load
            BinaryFormatter formatter = new BinaryFormatter();
            List<_Pbox> loadedfile = (List<_Pbox>)formatter.Deserialize(stream);
            List<_Pbox> freshlist = new List<_Pbox>();
            foreach(_Pbox post in loadedfile)
            {
                //setting images to refrenced list

                //if post does not have an image tag set one from image directory


            }
            foreach (_Pbox post in loadedfile)
            {
                int copiedid = post.PostID;
                string copieddata = post.Data;
                string cspoken = post.SpokenData;
                int copieddepth = post.ReplyDepth;
                int cunix = post.Unix;
                bool cext = post.hasExt;
                string cimg = post.Imagepath;
                int weight = post.Weight;
                string board = post.Board;
                string voice = post.voicesetting;
                _Pbox freshpost = new _Pbox(copieddata,cspoken,copiedid,cunix,new PointF(0,0),new PointF(0,0),copieddepth,cext,cimg,weight,board,voice);
                freshpost.voicesetting = voice;
                freshlist.Add(freshpost);
            }

            stream.Close();
            return freshlist;

        }
        public Dictionary<string,SpeechSynthesizer> BuildSS()
        {
            SpeechSynthesizer Male1 = new SpeechSynthesizer();
            SpeechSynthesizer Male2 = new SpeechSynthesizer();
            SpeechSynthesizer Male3 = new SpeechSynthesizer();
            SpeechSynthesizer Female1 = new SpeechSynthesizer();
            SpeechSynthesizer Female2 = new SpeechSynthesizer();
            SpeechSynthesizer Female3 = new SpeechSynthesizer();
            SpeechSynthesizer special1 = new SpeechSynthesizer();
            SpeechSynthesizer special2 = new SpeechSynthesizer();

            Male1.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(ReadNextPost);
            Male2.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(ReadNextPost);
            Male3.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(ReadNextPost);
            Female1.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(ReadNextPost);
            Female2.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(ReadNextPost);
            Female3.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(ReadNextPost);
            special1.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(ReadNextPost);
            special2.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(ReadNextPost);

            Male1.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Adult);
            Male2.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Teen);
            Male3.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Senior);
            Female1.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
            Female2.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Teen);
            Female3.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Senior);
            special1.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Child);
            special2.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
            Dictionary<string,SpeechSynthesizer> voices = new Dictionary<string, SpeechSynthesizer>();

            voices.Add("Male1",Male1);
            voices.Add("Male2", Male2);
            voices.Add("Male3", Male3);
            voices.Add("Female1", Female1);
            voices.Add("Female2", Female2);
            voices.Add("Female3", Female3);
            voices.Add("Special1", special1);
            voices.Add("Special2", special2);
            voices.Add("default", Male1);
            foreach(KeyValuePair<string,SpeechSynthesizer> kvp in voices)
            {
                kvp.Value.Rate = 10;
                kvp.Value.SetOutputToDefaultAudioDevice() ;
            }
            loadedvoices = voices;
            return voices;
        }
        public SpeechSynthesizer GetSynth(string voice,Dictionary<string,SpeechSynthesizer> loadedvoicelist)
        {

            if(loadedvoicelist.ContainsKey(voice))
            {
                return loadedvoicelist[voice];
            }
            else
            {
                SpeechSynthesizer defaultvoice = new SpeechSynthesizer();
                defaultvoice.SelectVoiceByHints(VoiceGender.Female);
                return defaultvoice;
            }
        }


        public void ReadNextPost(object sender, EventArgs e)
        {

            // SpeechSynthesizer ActiveVoice = GetSynth(loadedthread[p].getVoice(),loadedvoices);
            string voice = loadedthread[p].voicesetting;

            SpeechSynthesizer temps = GetSynth(voice,loadedvoices);
            activevoice.SelectVoiceByHints(temps.Voice.Gender, temps.Voice.Age);


           // activevoice = GetSynth(voice, loadedvoices);
            if (p < loadedthread.Count) //midroll posts
            {

            
                if (loadedthread[p].Data != null)
                {
                    activevoice.SpeakAsync(loadedthread[p].SpokenData);
                }

            }
            if (p >= loadedthread.Count ) //last thread post;
            {
                if(loadedthread[p] != null)
                {
                    activevoice.SpeakAsync(loadedthread[p].SpokenData);
                    //load new thread at this point
                }
            }

            LOWbumper = p - LOWThresh;
            if (LOWbumper < 0)
            {
                LOWbumper = 0;
            }
             UPbumper = p + UPThresh;
            if (UPbumper > loadedthread.Count)
            {
                UPbumper = loadedthread.Count;
            }
 p++;
            panel1.Invalidate();

        }

       private void Form1_Load(object sender, EventArgs e)
        {
            // SpeechSynthesizer Voice1 = new SpeechSynthesizer();
            BuildSS();

            activevoice = GetSynth(loadedthread[p].voicesetting, loadedvoices);
            activevoice.SetOutputToDefaultAudioDevice();
         //  activevoice.SelectVoiceByHints(VoiceGender.Male);
            //activevoice.Rate = 4;
            // _Pbox[] loaderposts = new _Pbox[] { testbox, testbox2, testbox3, testbox4, testbox5 };
            LOWbumper = p - LOWThresh;
            if (LOWbumper < 0)
            {
                LOWbumper = 0;
            }
            UPbumper = p + UPThresh;
            if (UPbumper > loadedthread.Count)
            {
                UPbumper = loadedthread.Count;
            }

            //start first post, then event
            activevoice.SpeakAsync("");
        //    activevoice.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(ReadNextPost);


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

            //sidebar
            Graphics g = e.Graphics;
            Point origin = new Point(0, 0);
            Size formsize = new Size((this.panel3.Width - 1), (this.panel3.Height - 1));
            Size formborder = new Size(this.panel3.Width, this.panel3.Height);
            Rectangle bg = new Rectangle(origin, formborder);
            Rectangle brd = new Rectangle(origin, formsize);

  /*          if (loaded != true)
            {

            if (IconDirectory.Equals(string.Empty)) IconDirectory = $"{Directory.GetCurrentDirectory()}\\{loadedthread[0].Board}";
            else IconDirectory = $"{IconDirectory}\\{loadedthread[0].Board}";

               // Directory.CreateDirectory(IconDirectory);
                var files = Directory.GetFiles(IconDirectory);

                 
                if (files.Length == 0)
                {

                }
                else
                {
                    IconImage = Image.FromFile(files[0]);
                }


            }


            if (IconImage != null)
            {
                g.DrawImage(IconImage, bg);
            } */

            g.FillRectangle(Van2brush, bg);
            g.DrawRectangle(mypen, brd);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            //icon top right
            Graphics g = e.Graphics;
            Point origin = new Point(0, 0);
            Size formsize = new Size((this.panel2.Width-1), (this.panel2.Height-1));
            Size formborder = new Size(this.panel2.Width, this.panel2.Height);
            Rectangle bg = new Rectangle(origin, formborder);
            Rectangle brd = new Rectangle(origin, formsize);


            if (loaded != true)
            {

                if (IconDirectory.Equals(string.Empty)) IconDirectory = $"{Directory.GetCurrentDirectory()}\\{loadedthread[0].Board}";
                else IconDirectory = $"{IconDirectory}";
               
               // Directory.CreateDirectory(IconDirectory);
                var files = Directory.GetFiles(IconDirectory);
                Random rng = new Random();
                var c = rng.Next(0, files.Length);

                if (files.Length == 0)
                {

                }
                else
                {
                    IconImage = Image.FromFile(files[c]);
                }


            }


            if (IconImage != null)
            {
                g.DrawImage(IconImage, bg);
            }



          //  g.FillRectangle(Vanbrush, bg);
           // g.DrawRectangle(mypen2, bg);
            g.DrawRectangle(mypen, brd);





        }
    }

  
}
