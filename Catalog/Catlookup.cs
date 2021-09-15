using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using static PostBotPrime.Form1;

namespace Catalog
{
    class Catlookup
    {
        public static WebClient WebClient = new WebClient();
        //public static Thread _thread;
        private const string api_url = @"https://a.4cdn.org/";
        private const string api_img_url = @"https://i.4cdn.org/";
        private static Int32 ReplyThreshold = 30;
        private static string SaveDirectory = @"C:\Users\Nathan\Documents\ChanJson";

        static void Main()
        {
            System.Console.Title = "Board Inspector";
            Menu().GetAwaiter().GetResult();
        }

        private static async Task Menu()
        {
            var board = GetBoard();
            ThreadList _Tlist;
            while (string.IsNullOrEmpty(board)) board = GetBoard();

            Console.WriteLine(" <><><><><><><| Exploring Threads |><><><><><><><> ");
            _Tlist = await LoadCatalog(board);

            if (_Tlist == null)
            {
                Console.WriteLine("|||   Thread List Returned Null...");
            }

            for (int i = 0; i < _Tlist.ThreadArray.Count; i++)
            {
                Console.WriteLine("| ♥ " + _Tlist.ThreadArray[i].weight + " | " + _Tlist.ThreadArray[i].ThreadUrl + " | τ " + _Tlist.ThreadArray[i].Semantic);
            }
            Console.WriteLine("|||   loading threads");
            _LoadedBoardFabric loadedboard = await ThreadLoader(_Tlist);

             LoomedFabric SortedBoardArray = ParsePosts(loadedboard);


            Console.WriteLine("Press any key to save threads to disc  " + SortedBoardArray.displayThreads.Count);
            Console.ReadLine();
            if (SaveDirectory.Equals(string.Empty)) SaveDirectory = $"{Directory.GetCurrentDirectory()}\\{SortedBoardArray.board}";
            else SaveDirectory = $"{SaveDirectory}\\{SortedBoardArray.board}";
            Directory.CreateDirectory(SaveDirectory); //creates location for downloaded images
            foreach (DisplayThread displayThread in SortedBoardArray.displayThreads)
            {
                string filename = displayThread.id.ToString();
                string path = $"{ SaveDirectory }\\{filename}-unsorted";
                Console.WriteLine(filename);
                if(Directory.GetFiles(SaveDirectory).Contains(filename)) //if thread instance is already saved to disk  overwrite
                {
                    SaveLoad.Save(displayThread.sortedthread, path);
                }
                else //new instance of thread  save
                {
                  SaveLoad.Save(displayThread.sortedthread, path);
                }
            }

            Console.WriteLine(" Threads saved to disc");
            Console.ReadLine();
        }

        public static LoomedFabric ParsePosts(_LoadedBoardFabric boardfabric) 
        {

            Tree replytree = new Tree();
            
            
            int OPpostID = 0;
            string pureCOM = "";
                string pattern = @"<br>";
                string replacement = " ";
                Regex breakpoints = new Regex(pattern);
            Dictionary<string, int> threadbank = wordBankCompiler(boardfabric.Board);
            List<DisplayThread> finishedThreads = new List<DisplayThread>();

            Console.WriteLine("parse checkpoint 1");
            for (int i = 0; i < boardfabric.LThreads.Count; i++)
            {
                List<_Pbox> postarray = new List<_Pbox>();
                List<_Pbox> SortedDisplayList = new List<_Pbox>();
                string json = boardfabric.LThreads[i].JSON;
                string postCom = "";
                var posts = JObject.Parse(json)["posts"].ToObject<JArray>();
                OPpostID = Int32.Parse(posts[0]["no"].ToString());

                for (int d = 0; d < posts.Count; d++) //thread post loop
                {

                    int postID = Int32.Parse(posts[d]["no"].ToString());
                    int postUnix = Int32.Parse(posts[d]["time"].ToString());
                    if (posts[d]["no"].ToString() != null && posts[d]["com"] != null)
                    {
                        postCom = posts[d]["com"].ToString();

                        string postComBroken = breakpoints.Replace(postCom, replacement);
                        Console.WriteLine(postCom + "| broken > |" + postComBroken);
                        pureCOM = "";

                        if (posts[i]["no"].ToString() != null && posts[i]["com"] != null)  //html agility pack and encoding fix
                        {
                            var htmldoc = new HtmlAgilityPack.HtmlDocument();
                            var html = postComBroken;
                            if (html != null)
                            {

                                htmldoc.LoadHtml(html);
                                var htmlparsed = htmldoc.DocumentNode.InnerText;
                                pureCOM = WebUtility.HtmlDecode(htmlparsed);

                            }

                        }




                    }


                    //post components collected

                    var postweight = PostWeight(threadbank, pureCOM);
                    //vars collected
                    Console.WriteLine("parse checkpoint 2");
                    Console.WriteLine(pureCOM + " pureCOM");
                    replytree = replies(pureCOM, replytree, postID);  //reply tree creation
                    
                    _Pbox postbox = new _Pbox(pureCOM, postID, postUnix, new PointF(0, 0), 0); //idividual post box 
                    postbox.ReplyDepth = replydepth(postbox, replytree);


                    postarray.Add(postbox);

                    if (postweight > 0) //this might break tree building?
                    {
                        //     postarray.Append<_Pbox>(postbox);
                    }


                }
                Console.WriteLine("parse checkpoint 3");

                 //  SortedDisplayList = replysort(postarray, replytree);
                SortedDisplayList = postarray;
            DisplayThread finishedThread = new DisplayThread(OPpostID, boardfabric.Board, SortedDisplayList);

             finishedThreads.Add(finishedThread);      
           
            }

            Console.WriteLine("parse checkpoint 4");


            LoomedFabric loomedFabric = new LoomedFabric(boardfabric.Board, finishedThreads);
            return loomedFabric;

        }

        public static async Task<ThreadList> LoadCatalog(string BOARD)
        {
            var endpoint = string.Join("/", BOARD, "catalog");
            Console.WriteLine("|     loading thread");
            var json = await WebClient.DownloadStringTaskAsync($"{api_url}{endpoint}.json");

            if (string.IsNullOrEmpty(json))
            {
                System.Console.WriteLine("Web request returned null :(");
                return null;
            }

            Console.WriteLine("|      Json OK");

            JArray PageArray = JArray.Parse(json).ToObject<JArray>();
            JArray CompiledThread = new JArray();

            Console.WriteLine("|        ThreadPages OK");

            if (PageArray == null)
            {
                Console.WriteLine("thread parse error");
                return null;
            }
            for (int i = 0; i < PageArray.Count; i++) //turns gross pagearrays to a compiled threadlist
            {
                for (int d = 0; d < PageArray[i]["threads"].Count(); d++)
                {
                    // Console.WriteLine(d + " page-thread-count |" + PageArray[i]["threads"][d]["no"].ToString());
                    var ThreadArray = PageArray[i]["threads"][d].ToObject<JObject>();
                    CompiledThread.Add(ThreadArray);
                }

            }

            Console.WriteLine("|       PreCompile Complete");
            Dictionary<string, int> wordbank = new Dictionary<string, int>();

            wordbank = wordBankCompiler(BOARD);

            List<UnloadedCatalogThreads> TotalThreads = new List<UnloadedCatalogThreads>();

            for (int i = 0; i < CompiledThread.Count; i++) //iterate through every thread in catalog
            {
                if (CompiledThread[i] != null)
                {

                    if (Int32.Parse(CompiledThread[i]["replies"].ToString()) > ReplyThreshold)
                    {
                        var threadID = Int32.Parse(CompiledThread[i]["no"].ToString());
                        var replies = Int32.Parse(CompiledThread[i]["replies"].ToString());
                        var images = Int32.Parse(CompiledThread[i]["images"].ToString());
                        var sematic = CompiledThread[i]["semantic_url"].ToString();
                        var Unixtime = Int32.Parse(CompiledThread[i]["time"].ToString());

                        var weight = ThreadWeight(replies, images, wordbank, sematic);

                        //Console.WriteLine("/" + BOARD + "/thread/" + threadID + ".json | " + weight + " | R: " + replies);
                        var boardstring = "/" + BOARD + "/thread/" + threadID + ".json";
                        var currentThread = new UnloadedCatalogThreads(weight, boardstring, sematic, threadID);

                        TotalThreads.Add(currentThread);
                    }

                }
            }
            var SortedThreads = TotalThreads.OrderByDescending(x => x.weight).ToList();
            var _tlist = new ThreadList(BOARD, SortedThreads, json);
            Console.WriteLine("<><><><><><><| ThreadList Complete |><><><><><><><>");
            return _tlist;
        }

        public static string GetBoard()
        {
            Console.WriteLine(@"                                                ");
            Console.WriteLine(@"  / _ \_____________________________/`/\+-/\'\'\");
            Console.WriteLine(@"\_\(_)/_/ Goblin CodeWorks Presents -+-    -+-+-");
            Console.WriteLine(@" _//o\\_   ImageboardSpider  Mk.1   \'\/+-\/`/`/");
            Console.WriteLine(@"  /   \                              \/-+--\/`/ ");
            Console.WriteLine(@"                                                ");
            Console.WriteLine(@"              Choose a Board                    ");
            return System.Console.ReadLine();
        }

        public static Tree repliesnew(string postContent, Tree replytree, int postID)
        {
            Console.WriteLine(postContent + " content");
            //IDictionary<int,int> postreplies = new IDictionary<int,int>();
         //   replytree.addNode(postID);

            //  Regex IDfind = new Regex();

            if (postContent.Contains(">>") && postContent.Contains(" "))
            {
                int Start, End;
                int data = 1;
                string datacheck = "";
                int lastreply = 1;
                bool crossthread = false;
                Start = postContent.IndexOf(">>", 0) + 2;


                do
                {
                    End = postContent.IndexOf(" ", Start);
                    datacheck = postContent.Substring(Start, End - Start);

                    if (datacheck.IndexOf(">") != -1)
                    {
                        Console.WriteLine("crossthread detected");
                        crossthread = true;

                    }
                    data = Int32.Parse(postContent.Substring(Start, End - Start));
                    Console.WriteLine(data + " data");




                    if (replytree.getNode(data) == null && data != 0) //new unique reply
                    {
                        Node newroot = new Node();
                        newroot.id = data;
                       // newroot.a(data);
                        replytree.addRoot(newroot);
                        lastreply = data;

                    }
                    else if (replytree.getNode(data) != null && data != lastreply) //parent node contained in tree already
                    {
                        Node newnode = new Node();
                        newnode.id = data;
                        newnode.addParent(data);
                        replytree.addNode(newnode);
                        lastreply = data;
                    }

                    Start = postContent.IndexOf(">>", End) + 2;
                    crossthread = false;

                }
                while (Start != 1);

               //  return replytree;
            }
       //     replytree.addNode(postID); //end amend
            return replytree;
        }

        public static Tree replies(string postContent, Tree replytree, int postID)
        {
            string pattern = "(?<=>>)(((?<!>>>)[0-9]))+";
            Regex ReplyFinder = new Regex(pattern,  RegexOptions.IgnoreCase);
            Node postNode = new Node();
            postNode.id = postID;
            MatchCollection matches = ReplyFinder.Matches(postContent);
            if (matches.Count != 0)
            {
               replytree.addNode(postNode);
                for (int i = 0; i < matches.Count; i++)
                {
                    
                    //postNode.addParent(Int32.Parse(matches[i].ToString())); //should be reply's postid
                    postNode.addParent(Int32.Parse(matches[i].Value));
                }

            }
            else
            {
                replytree.addRoot(postNode); //add node to reply tree
            }
            return replytree;
        }

        public static List<_Pbox> replysort(List<_Pbox> unsortedposts,Tree replytree)
        {
            List<_Pbox> SortedPosts = new List<_Pbox>();

            foreach (_Pbox upost in unsortedposts)
            {


                if(upost.ReplyDepth == 0) //if post is original
                {
                   
                    SortedPosts.Add(upost);
                    //loop through all children and amend them to sorted posts
                   SortedPosts = childloop(upost.PostID, replytree, SortedPosts, unsortedposts);
                }
                else //if the post is a reply
                {
                    
                }

            }

            return SortedPosts;
        }
        public static int replydepth(_Pbox post, Tree replytree)
        {
            var depth = 0;
            var depthmax = 0;
            if (replytree.getNode(post.PostID) != null)
            {
           Node node = replytree.getNode(post.PostID);
            if(node.getparents() != null) //node has parents and not tree root
            {
                    if (node.getparents().Count != 0)
                    {


                        List<Node> parents = node.getparents();
                        foreach (Node node1 in parents)
                        {

                            depth = node1.replydepth;

                            if (depth > depthmax)
                            {
                                depthmax = depth;
                            }

                        }
                        depthmax = depthmax + 1;
                        return depthmax;
                    }
            }
            }

            return 0;  


        }
        public static List<_Pbox> childloop(int post, Tree replytree, List<_Pbox> SortedPosts, List<_Pbox> UnsortedPosts)
        {

            if (replytree.getNode(post) != null)
            { 
                if (replytree.getNode(post).getchildren() != null)//if there is children
                {
                    foreach (Node n in replytree.getNode(post).getchildren()) //for each child
                    {
                        
                        _Pbox pluckedpost = UnsortedPosts.Find(x => x.PostID == n.id);
                        UnsortedPosts.Remove(pluckedpost);

                        SortedPosts.Add(pluckedpost);  //add child to sorted list, check this child for children
                        //SortedPosts.Append<_Pbox>(UnsortedPosts[n.id]);
                        //add this child to sorted list
                        SortedPosts = childloop(n.id, replytree, SortedPosts, UnsortedPosts); //check this child for children
                    }
                }
            }
            return SortedPosts;
        }

        public static int ThreadWeight(Int32 replycount, Int32 imagecount, Dictionary<string, int> WordBank, string semantic)
        {
            int collectiveMulti = 1;
            foreach (KeyValuePair<string, int> kvp in WordBank)
            {
                var keywords = kvp.Key;
                Regex RX = new Regex(@keywords);
                
                if (RX.IsMatch(semantic) == true)
                {
                    collectiveMulti = kvp.Value + collectiveMulti;
                 //   Console.WriteLine("collective multi " + collectiveMulti);
                }

            }

            var replydiff = replycount - imagecount;
            var preMulti = replydiff ^ 3 / 2;
            var trueweight = preMulti * (1 + collectiveMulti / 4);

            return trueweight;
        }

        public static int PostWeight(Dictionary<string, int> WordBank, string content)
        {
            int collectiveMulti = 1;
            foreach (KeyValuePair<string, int> kvp in WordBank)
            {
                var keywords = kvp.Key;
                Regex RX = new Regex(@keywords);
 
                if (RX.IsMatch(content) == true)
                {
                    MatchCollection matches = RX.Matches(content);
                    collectiveMulti = (kvp.Value*matches.Count) + collectiveMulti;
                    //   Console.WriteLine("collective multi " + collectiveMulti);
                }

            }

            var replydiff = 10;
            var preMulti = replydiff ^ 3 / 2;
            var trueweight = preMulti * (1 + collectiveMulti / 4);

            return trueweight;
        }

        public static async Task<_LoadedBoardFabric> ThreadLoader(ThreadList _Tlist)
        {

            List<LoadedThread> LThreadArray = new List<LoadedThread>();

            for(int i = 0; i < 2; i++)
            {
                await Task.Delay(15000);
                var urlendpoint = _Tlist.ThreadArray[i].ThreadUrl;
                var _semantic = _Tlist.ThreadArray[i].Semantic;
                var _weight = _Tlist.ThreadArray[i].weight;
                var Tid = _Tlist.ThreadArray[i].ThreadID;
                var json = await WebClient.DownloadStringTaskAsync($"{api_url}{urlendpoint}"); //_Tlist.ThreadArray.Count
                //var json = "test";
                Console.WriteLine("thread loaded test");
                LoadedThread lthread = new LoadedThread(urlendpoint, _semantic, json, _weight);
                LThreadArray.Add(lthread);
            }
             _LoadedBoardFabric boardthreads = new _LoadedBoardFabric(_Tlist.Board, LThreadArray);
            return boardthreads;

        }

       
        public static Dictionary<string, int> wordBankCompiler(string boardId)
        {
            Dictionary<string, int> TD = new Dictionary<string, int>();

            // base WorkBank for semantic titles

            // Good Posts
           // TD.Add("?", 3);
            TD.Add("prove", 2);
            TD.Add("how", 2);
            TD.Add("why", 2);
            TD.Add("when", 2);
            TD.Add("talk", 2);
            TD.Add("discussion", 2);
            TD.Add("does", 2);
            TD.Add("be-me", 2);
            TD.Add("greentext", 20);

            // Filter Posts
            TD.Add("general", -90);

            //Bad Words
            TD.Add("nigger", -50);
            TD.Add("Nigger", -50);
            TD.Add("kike", -50);
            TD.Add("Kike", -50);

            // board specific additions
            /*  switch (boardId)
                     {

                         case "g":

                      // Good Posts
                      TD.Add("good", 3);

                      // Filter Posts
                      TD.Add("filter", -1);

                      //Bad Words
                      TD.Add("badword", -5);


                             break;
                         case "x":

                      // Good Posts
                      TD.Add("good", 3);

                      // Filter Posts
                      TD.Add("filter", -1);

                      //Bad Words
                      TD.Add("badword", -5);


                      break;
                  case "x":

                      // Good Posts
                      TD.Add("good", 3);

                      // Filter Posts
                      TD.Add("filter", -1);

                      //Bad Words
                      TD.Add("badword", -5);


                      break;

              }
           */

            Console.WriteLine(" ♥ Wordbank Created ♥ ");

            return TD;



        }

    }
    public class UnloadedCatalogThreads
    {
        public int weight { get; private set; }
        public string ThreadUrl { get; private set; }
        public string Semantic { get; private set; }
        public int ThreadID { get; private set; }
        public UnloadedCatalogThreads(int Weight, string Threadurl, string semantic, int threadid)
        {
            weight = Weight;
            ThreadUrl = Threadurl;
            Semantic = semantic;
            ThreadID = threadid;
        }
    }
    public class ThreadList
    {
        public string Board { get; private set; }
        public List<UnloadedCatalogThreads> ThreadArray { get; private set; }
        public string JSON { get; private set; }

        public ThreadList(string board, List<UnloadedCatalogThreads> Threads, string json)
        {
            Board = board;
            ThreadArray = Threads;
            JSON = json;

        }



    }
    public class LoadedThread
    {
        public string Url { get; private set; }
        public string Semantic { get; private set; }
        public string JSON { get; private set; }
        public int Weight { get; private set; }
        public LoadedThread(string url, string semantic, string json, int weight)
        {
            Url = url;
            Semantic = semantic;
            JSON = json;
            Weight = weight;
        }

    }
    public class _LoadedBoardFabric
    {
        public string Board { get; private set; }
        //public string Semantic { get; private set; }
        public List<LoadedThread> LThreads { get; private set; }
        public _LoadedBoardFabric(string board, List<LoadedThread> _loadedthread)
        {
            Board = board;
            LThreads = _loadedthread;
        }

    }
    public class LoomedFabric
    {
        public string board { get; set; }
        public List<DisplayThread> displayThreads {get; set;}
        public LoomedFabric(string Board, List<DisplayThread> DisplayThreads)
        {
            board = Board;
            displayThreads = DisplayThreads;
        }

    }
    public class Node
    {
        public int id { get; set; }

        string message { get; set; }

        List<Node> parent { get; set; }

        List<Node> children { get; }

        public Tree tree { get; set; }
        public int replydepth { get; set; }


        public Node()
        {
            this.children = new List<Node>();
            this.parent = new List<Node>();
            this.replydepth = 0;
        }

        public void addParent(int id) // gets parent node from parent id, then adds this node as parents child
        {
            //int is id of the replyed to post, ie this nodes parent
            if(this.tree.getNode(id) != null)
            {
           Node parentnode = this.tree.getNode(id);
            parentnode.id = id;
            this.parent.Add(parentnode);
            int index = this.parent.IndexOf(parentnode);
            this.parent[index].children.Add(this);
            }
 

        }
        public List<Node> getparents() //grabs node from nodeid dictionary
        {
            if (parent != null)
            {
                return parent;
            }
            return null;

        }
        public List<Node> getchildren() //grabs node from nodeid dictionary
        {
            if (children != null)
            {
                return children;
            }
            return null;
        }
    }

    public class Tree
    {
        List<Node> roots;
        Dictionary<int, Node> node_dict;

        public Tree()
        {
            roots = new List<Node>();
            node_dict = new Dictionary<int, Node>();
        }

        public void addNode(Node node) //adds node to node dict and sets its active tree
        {
            node_dict.Add(node.id, node);
            node.tree = this;
        }

        public Node getNode(int id) //grabs node from nodeid dictionary
        {
            if(node_dict.ContainsKey(id))
            {
                return node_dict[id];
            }
            return null;
        }

        public void addRoot(Node node) // adds node to root list, and adds node to node dictionary
        {
            this.addNode(node);
            this.roots.Add(node);
            node.tree = this;
        }

    }

 //   [Serializable()]
  /*   public class _Pbox
    {
        public string Data { get; private set; }
        public int PostID { get; private set; }
        public int Unix { get; private set; }
        public int ReplyDepth { get; set; }
        public PointF Dorigin { get; set; }

        public _Pbox(string Com, int postid, int unix, PointF draworigin, int replydepth)
        {
            Data = Com;
            PostID = postid;
            Unix = unix;
            Dorigin = draworigin;
            ReplyDepth = replydepth;
        }

    } */
    public class DisplayThread
    {
        public int id { get; set; }
        public string board { get; set; }
        public List<_Pbox> sortedthread { get; set; }
        public DisplayThread(int ID, string Board, List<_Pbox> SortedThread)
        {
            id = ID;
            board = Board;
            sortedthread = SortedThread;
        }
    }
    public class SaveLoad
    {


        public static void Save(List<_Pbox> sorted, string path)
        {

            /*     using (var writer = new StreamWriter(path)) //json save
                 {
                     var temp = JsonConvert.SerializeObject(sorted);


                     writer.Write(JsonConvert.SerializeObject(sorted));
                 }*/


            Stream stream = File.Open(path, FileMode.Create); //binary save
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, sorted);
            stream.Close(); 


        }
        public List<_Pbox> Load( string path, List<_Pbox> sorted)
        {
           /* System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(sorted.GetType());
            Stream stream = File.Open(path, FileMode.Open);
            List<_Pbox> loadedfile = (List<_Pbox>)x.Deserialize(stream);           //xml load

            stream.Close();
            return loadedfile; 
           */

              Stream stream = File.Open(path, FileMode.Open);              //binary load
              BinaryFormatter formatter = new BinaryFormatter();
              List<_Pbox> loadedfile = (List<_Pbox>)formatter.Deserialize(stream);
              stream.Close();
              return loadedfile; 

            /*     using (var reader = new StreamReader(path))
           {
                                                                                 //json load

               var temp2 = JsonConvert.DeserializeObject<IEnumerable<_Pbox>>(reader.ReadToEnd()).ToList();

               return temp2;




           } */


        }
    }

}



