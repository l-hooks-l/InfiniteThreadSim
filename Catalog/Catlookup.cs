using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Catalog
{
    class Catlookup
    {
        public static WebClient WebClient = new WebClient();
        //public static Thread _thread;
        private const string api_url = @"https://a.4cdn.org/";
        private const string api_img_url = @"https://i.4cdn.org/";
        private static Int32 ReplyThreshold = 30;
        private static string SaveDirectory = "";

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
               ParsePosts(loadedboard);

            for (int i = 0; i < loadedboard.LThreads.Count; i++)
            {
                Console.WriteLine(@"***************" + loadedboard.LThreads[i].Semantic + " | " + loadedboard.LThreads[i].JSON);
                


            }
        }

        public static async Task<> ParsePosts(_LoadedBoardFabric boardfabric)
        {

            Tree replytree = new Tree();


            for (int i = 0; i < boardfabric.LThreads.Count; i++)
            {


                string json = boardfabric.LThreads[i].JSON;
                var posts = JObject.Parse(json)["posts"].ToObject<JArray>();
                string postCom = posts[i]["com"].ToString();
                int postID = Int32.Parse(posts[i]["no"].ToString());
                int postUnix = Int32.Parse(posts[i]["time"].ToString());
              // string postCom = posts[i]["com"].ToString();




                replytree = replies(postCom, replytree);
                _Pbox postbox = new _Pbox(postCom, postID, postUnix, new PointF(0,0), 0);


            }


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

        public static Tree replies(string postContent, Tree replytree)
        {
            //IDictionary<int,int> postreplies = new IDictionary<int,int>();
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





                    if (replytree.getNode(data) == null && data != 0) //new unique reply
                    {
                        Node newroot = new Node();
                        newroot.id = data;
                        newroot.addParent(data);
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
            return replytree;
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


        public static async Task<_LoadedBoardFabric> ThreadLoader(ThreadList _Tlist)
        {

            List<LoadedThread> LThreadArray = new List<LoadedThread>();

            for(int i = 0; i < 5; i++)
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
    public class Node
    {
        public int id { get; set; }

        string message { get; set; }

        List<Node> parent { get; set; }

        List<Node> children { get; }

        public Tree tree { get; set; }

        public Node()
        {
            this.children = new List<Node>();
            this.parent = new List<Node>();
        }

        public void addParent(int id) // gets parent node from parent id, then adds this node as parents child
        {
            Node parent = this.tree.getNode(id);
            this.parent[id] = parent;
            this.parent[id].children.Add(this);
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
            return node_dict[id];
        }

        public void addRoot(Node node) // adds node to root list, and adds node to node dictionary
        {
            this.addNode(node);
            this.roots.Add(node);
            node.tree = this;
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

}



