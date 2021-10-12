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
using Shell32;

namespace Catalog
{
    
    class Catlookup
    {
        public static WebClient WebClient = new WebClient();
        //public static Thread _thread;
        private const string api_url = @"https://a.4cdn.org/";
        private const string api_img_url = @"https://i.4cdn.org/";
        private static Int32 ReplyThreshold = 30;
        private static string SaveDirectory = @"C:\chanjson";
        private static string notags = "";    //undefined name for images without image partner
        private static string imagepath = @"C:\Users\Nathan\Pictures\Icons"; //folder tagged images hangout in
        public static int ichecker = 0;
        public static int perpostweightthresh = 50;

       // public static  Shell32.Shell shell = new Shell32.Shell();
       // public static Shell32.Folder objFolder;

       // objFolder = shell.NameSpace(imagepath);
       //[STAThreadAttribute]
        static void Main()
        {
            System.Console.Title = "Board Inspector";
            Menu().GetAwaiter().GetResult();
        }
        //[STAThread]
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
            Directory.CreateDirectory(SaveDirectory); //creates location for downloaded threads
            foreach (DisplayThread displayThread in SortedBoardArray.displayThreads)
            {
                string filename = displayThread.id.ToString();
                int weight = displayThread.TWeight;
                string sub = WebUtility.UrlEncode(displayThread.Sub);
                Console.WriteLine(sub);
                string path = $"{ SaveDirectory }\\{weight}---{sub}";
                Console.WriteLine(path);
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


            
            
            int OPpostID = 0;
            string OPSub = "";
         //   string pureCOM = "";
                string pattern = @"<br>";
                string replacement = " \r\n";
                Regex breakpoints = new Regex(pattern);
            Dictionary<string, int> threadbank = wordBankCompiler2(boardfabric.Board);
            Dictionary<string, string> keybank = imgtagCompiler();
            List<DisplayThread> finishedThreads = new List<DisplayThread>();
           // objFolder = GetShell32NameSpaceFolder(imagepath);
           // objFolder = shell.NameSpace(imagepath);

            Console.WriteLine("parse checkpoint 1");
            for (int i = 0; i < boardfabric.LThreads.Count; i++)
            {

                Tree replytree = new Tree();
                List<_Pbox> postarray = new List<_Pbox>();
                List<_Pbox> SortedDisplayList = new List<_Pbox>();
                string json = boardfabric.LThreads[i].JSON;
                string postCom = "postcom";
                var posts = JObject.Parse(json)["posts"].ToObject<JArray>();
                OPpostID = Int32.Parse(posts[0]["no"].ToString());




                if (posts[0]["sub"] != null)
                {
                OPSub = posts[0]["sub"].ToString();
                }
                else
                {
                    OPSub = "Discussion-Thread";
                }


                for (int d = 0; d < posts.Count; d++) //thread post loop
                {
               // ichecker++;
                    string pureCOM = "";
                    string SpokenCom = "";
                    int postID = Int32.Parse(posts[d]["no"].ToString());
                    int postUnix = Int32.Parse(posts[d]["time"].ToString());
                    bool postImage = false;
                    string hash = "E";
                    string imgdefinition = "E";
                    Console.WriteLine("pre image");
                    if (posts[d]["ext"] != null) //IF POST HAS EXTENSION
                    {
                        postImage = true;
                        hash = posts[d]["md5"].ToString();

                        foreach (string path in Directory.GetFiles($"{imagepath}\\verified"))
                        {

                            //check this hash against every image in the sfw database
                            string ext = Path.GetExtension(path);
                            string imgpath = $"{imagepath}\\{hash}{ext}";

                            if (path == imgpath)
                            {
                                imgdefinition = imgpath;
                            }



                                //image is not in directory/ not ok
                                //send image to processing pile?

                            
                            
                        }
                    }
                    Console.WriteLine("image finished");

                    if (posts[d]["no"].ToString() != null && posts[d]["com"] != null)
                    {
                        postCom = posts[d]["com"].ToString();

                        string postComBroken = breakpoints.Replace(postCom, replacement);
                       // Console.WriteLine(postCom + "| broken > |" + postComBroken);
                        pureCOM = "";

                        if (posts[d]["no"].ToString() != null && posts[d]["com"] != null)  //html agility pack and encoding fix
                        {
                            var htmldoc = new HtmlAgilityPack.HtmlDocument();
                            var html = postComBroken;
                            if (html != null)
                            {

                                htmldoc.LoadHtml(html);
                                var htmlparsed = htmldoc.DocumentNode.InnerText;
                                pureCOM = WebUtility.HtmlDecode(htmlparsed);
                                Console.WriteLine("textparse start");
                               // SpokenCom = SpokenFix(pureCOM);
                                Console.WriteLine("textparse finished");
                            }

                        }




                    }
                    string imgtag = "";

                    if(postImage == true)
                    { 

                    
                       
                        if (imgdefinition == "E") //md5 hash not found earlier
                            {

                             imgtag = PostImageTag(keybank,pureCOM);         
                        
                            }
                        else   //md5 hash found set hash path to imgtag
                            {
                            imgtag = imgdefinition;
                            }
                    }
                   

                    //post components collected

                    var postweight = PostWeight(threadbank, pureCOM);
                    //vars collected
                    //   Console.WriteLine("parse checkpoint 2");
                    // Console.WriteLine(pureCOM + " pureCOM");
                    Console.WriteLine("pre replies");
                    replytree = replies(pureCOM, replytree, postID, postweight);  //reply tree creation adds id and weight to node
                    Console.WriteLine("replies finished");



                    //walk current posts weight down reply tree from child to root

                    _Pbox postbox = new _Pbox(pureCOM,SpokenCom, postID, postUnix, new PointF(0, 0), new PointF(0, 0), 0, postImage,imgtag,postweight,boardfabric.Board); //idividual post box 
                                                                                                                                                                          // ichecker++;
                    Console.WriteLine("replydepth start");
                    var depth = replydepth(postbox, replytree);
                    Console.WriteLine("replydepth finish");
                    replytree.getNode(postID).replydepth = depth;
                    postbox.ReplyDepth = depth;



                   // postarray.Add(postbox);

                    if (postweight > 0) //this might break tree building?
                    {
                        //     postarray.Append<_Pbox>(postbox);
                        postarray.Add(postbox);
                    }
                    ichecker++;
                    Console.WriteLine(ichecker);

                }
                Console.WriteLine("parse checkpoint 3");

     //              SortedDisplayList = replysort(postarray, replytree);
                SortedDisplayList = Sortbyweight(postarray, replytree);
               // SortedDisplayList = postarray;
              
            DisplayThread finishedThread = new DisplayThread(OPpostID, boardfabric.Board, SortedDisplayList, OPSub, boardfabric.LThreads[i].Weight);

             finishedThreads.Add(finishedThread);      
           
            }

           // Console.WriteLine("parse checkpoint 4");


            LoomedFabric loomedFabric = new LoomedFabric(boardfabric.Board, finishedThreads);
            return loomedFabric;

        }

        public static string SpokenFix(string pureCOM)
        {
            string pattern = "(?<=>>)(((?<!>>>)[0-9]))+";
            string urlpattern = @"(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$";
            Regex Replyremover = new Regex(pattern);
            Regex urlremover = new Regex(urlpattern);


            string tempcom = Replyremover.Replace(pureCOM," ");
            string Spoken = urlremover.Replace(tempcom, " ");
            return Spoken;
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

        public static Tree replies(string postContent, Tree replytree, int postID, int postW)
        {
            string pattern = "(?<=>>)(((?<!>>>)[0-9]))+";
            Regex ReplyFinder = new Regex(pattern,  RegexOptions.IgnoreCase);
            Node postNode = new Node();
            postNode.id = postID;
            postNode.replyweight = postW;
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

        public static List<_Pbox> Sortbyweight(List<_Pbox> unsorted, Tree replytree)
        {
            List<_Pbox> SortedPosts = new List<_Pbox>();
            List<Dictionary<int, int>> paths = new List<Dictionary<int, int>>();


            foreach (Node Root in replytree.getRoots()) //for each original root post
            {
                List<Dictionary<int, int>> pathwaylist = new List<Dictionary<int, int>>(); //this nodes combined pathway dict
                Dictionary<int, int> cpath = new Dictionary<int, int>();

                List<Dictionary<int, int>> pathwaysfromroot = ContPath(Root, replytree, pathwaylist, cpath); //grab all pathways in list



                //ratio each pathway and keep top 3
               List<Dictionary<int,int>> ratioedpaths = RatioMethod(pathwaysfromroot);
                SortedPosts = PathstoBox(ratioedpaths,unsorted);

            }

            return SortedPosts;

        }
        public static List<_Pbox> PathstoBox(List<Dictionary<int,int>> toppaths, List<_Pbox> unsorted)
        {
            Dictionary<int, bool> visitedposts = new Dictionary<int, bool>();
            List<_Pbox> sortedarray = new List<_Pbox>();
            foreach(Dictionary<int,int> pathway in toppaths)
            {
                int cache = 0;
                 foreach (KeyValuePair<int,int> entry in pathway)
                 {
                    int id = 0; //indicates how many posts down the path we are


                    if(visitedposts.ContainsKey(entry.Key)) // post has already been added to array
                    {
                        if(id == 0) //added and first disregard
                        {



                        }
                        else //added and in trasit do nothing
                        {

                        }


                    }
                    else //the post has not been added to array
                    {
                        if(id == 0) //first node in path  no cache yet n1
                        {
                            visitedposts.Add(entry.Key,true);

                            _Pbox tempbox = unsorted.Find(x => x.PostID == entry.Key);

                            _Pbox submitbox = new _Pbox(tempbox.Data,tempbox.SpokenData,tempbox.PostID,tempbox.Unix,new PointF(0,0), new PointF(0, 0),tempbox.ReplyDepth,tempbox.hasExt,tempbox.Imagepath, tempbox.Weight,tempbox.Board);

                            sortedarray.Add(submitbox);


                        }
                        else //new node add current and edited cache node
                        {

                           visitedposts.Add(entry.Key, true);


                            _Pbox tempbox = unsorted.Find(x => x.PostID == cache);

                            _Pbox submitbox = new _Pbox(tempbox.Data, " " , tempbox.PostID, tempbox.Unix, new PointF(0, 0), new PointF(0, 0), tempbox.ReplyDepth, tempbox.hasExt, tempbox.Imagepath, tempbox.Weight, tempbox.Board);

                            sortedarray.Add(submitbox);



                            _Pbox tempbox2 = unsorted.Find(x => x.PostID == entry.Key);

                           _Pbox submitbox2 = new _Pbox(tempbox2.Data, tempbox2.SpokenData, tempbox2.PostID, tempbox2.Unix, new PointF(0, 0), new PointF(0, 0), tempbox2.ReplyDepth, tempbox2.hasExt, tempbox2.Imagepath, tempbox2.Weight, tempbox2.Board);

                           sortedarray.Add(submitbox2);

                        }
                    }

                    //add this post to cache for next post
                    cache = entry.Key;

                    id++;
                 }
            }
            return sortedarray;
            //iterate through paths

            //add elements of path to Pbox array, add to visited lookup

            //if current node has been added and its the first, just ignore

            //if current node has been added and its not first on list, store in buffer, dont add to p array

            //if current node is not added, and is not first on the list, add buffer post without spoken text, then current post

            //if current node is not added, and it is the first on the list, add current node to p array contunie


        }

        public static List<Dictionary<int, int>> NodePaths(Node cnode, Tree replytree, List<Dictionary<int, int>> pathwaylist, Dictionary<int, int> RP)
        {
            Dictionary<int, int> rollingpath = new Dictionary<int, int>();
            Dictionary<int, int> mempath = null;
            if(RP != null)
            {
                rollingpath = RP;
            }


            List<Node> kids = cnode.getchildren();
            if (rollingpath.ContainsKey(cnode.id))
            {
                //node already exists in pathway, cycle detected

//                pathwaylist.Add(rollingpath); //add completed path to pathlist
                rollingpath.Clear();

            }
            else
            {

            rollingpath.Add(cnode.id, cnode.replyweight);// add parent node to pathway

            }



            if(cnode.getchildren().Count() == 0) // get children returns an empty list c0 termination
            {
                pathwaylist.Add(new Dictionary<int,int>(rollingpath)); //add completed path to pathlist
              //  rollingpath.Clear();
                if(mempath != null)
                {
                rollingpath = mempath;
                }


            }
            else //if there are children
            {
                mempath = rollingpath;
            }

         //       pathwaylist.Add(new Dictionary<int, int>(rollingpath));
            foreach (Node child in kids) // create cN new dictonary paths
            {


                pathwaylist = NodePaths(child, replytree, pathwaylist, rollingpath);


            }

            return pathwaylist;

        }


        public static List<Dictionary<int,int>> ContPath(Node cnode, Tree replytree, List<Dictionary<int, int>> pathwaymapreturn,Dictionary<int,int> currentpath)
        {
            if(currentpath.ContainsKey(cnode.id))
            {
                //cycle detected break path
                pathwaymapreturn.Add(new Dictionary<int, int>(currentpath));
                return pathwaymapreturn;

            }


            currentpath.Add(cnode.id, cnode.replyweight);// adds this node to the current path

            List<Node> kids = cnode.getchildren();

            if (kids.Count() == 0) // get children returns an empty list c0 termination
            {

                pathwaymapreturn.Add(new Dictionary<int,int>(currentpath));
                return pathwaymapreturn;
            }
            else if (kids.Count == 1) //c1
            {
                foreach (Node kid in kids)
                {
                    pathwaymapreturn = ContPath(kid, replytree, pathwaymapreturn, currentpath);
                    return pathwaymapreturn;
                }


            }

            //cn fork off from current path
            var fork = new Dictionary<int, int>(currentpath);
            foreach(Node kid in kids)
            {

                pathwaymapreturn = ContPath(kid, replytree, pathwaymapreturn, fork);

            }

            return pathwaymapreturn;

        }

        public static List<Dictionary<int, int>> RatioMethod( List<Dictionary<int, int>> bestpaths)
        {
            List<Dictionary<Dictionary<int, int>, float>> unsortedpaths = new List<Dictionary<Dictionary<int, int>, float>>();
            SortedDictionary<int, float> temp = new SortedDictionary<int, float>();
            int pathid = 0;
            foreach(Dictionary<int,int> path in bestpaths) //for each path
            {
                var totalweight = 0;

                foreach(KeyValuePair<int,int> kvp in path) //add all weights vs dictory length
                {
                    totalweight += kvp.Value;

                }
                var length = path.Count();
                var totalheft = length * perpostweightthresh;

                float ratio = (float) totalweight /(totalheft + length) ;
                
               // SortedDictionary<int, float> temp = new SortedDictionary<int, float>();


                temp.Add(pathid, ratio);
                pathid++;

            }
            List<Dictionary<int, int>> top3 = new List<Dictionary<int, int>>();

            var presort = temp.OrderByDescending(x => x.Value).ToList();
            //var presort = from entry in unsortedpaths orderby entry.Values ascending select entry.;
           //var presort = unsortedpaths.OrderByDescending(x => x.Values);

            for(int i = 0; i < 3;i++)
            {
                top3.Add(bestpaths[presort[i].Key]);
            }


            return top3;
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
                        //UnsortedPosts.Remove(pluckedpost);
                        if (pluckedpost != null)
                        {
                            SortedPosts.Add(pluckedpost);  //add child to sorted list, check this child for children
                        }
                        
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
        public static string PostImageTag(Dictionary<string, string> keytags, string content)
        {

            string[] posspaths = { };

            Random rnd = new Random();

            foreach (KeyValuePair<string, string> kvp in keytags)
            {
                var keywords = kvp.Key;

                Regex RX = new Regex(@keywords, RegexOptions.IgnoreCase);
                

                if (RX.IsMatch(content) == true)
                {
                    MatchCollection matches = RX.Matches(content);

                    foreach (Match match in matches)
                    {

                        //keytag match found, add random file from kvp.value path

                       string[] dirfiles = Directory.GetFiles(kvp.Value);
                        if(dirfiles.Length > 0) //more then 0 files in dir
                        {
                            //random files from directoryn to possible path  Faster
                            int c = rnd.Next(0,dirfiles.Length);
                            posspaths.Append<string>(dirfiles[c]);

                            //add all paths in directory to possible paths
                            foreach(string dpath in dirfiles)
                            {
                            posspaths.Append<string>(dpath);
                            }

                        }

                        //add current path to possible paths array
                        //kvp  text/imgtag  add all files with tag to posspath array
                     /*   foreach(ShellFolderItem item in objFolder.Items())
                        {
                            if(item.ExtendedProperty("ImgTag") == "")
                            {
                                //the image does not contain a img tag
                            }
                            else
                            {
                                string tags = item.ExtendedProperty("ImgTag");

                                if(RX.IsMatch(tags) == true)
                                {

                                MatchCollection tagmatch = RX.Matches(tags);

                                    foreach(Match match1 in tagmatch)
                                    {
                                        //add every tag match's path to possible paths
                                        posspaths.Append<string>(item.Path);

                                    }
                                }

                            }
                        }
                        */

                    }


                }

            }

            //draw one path out of path array 
            if (posspaths.Length > 0)
            {
                int chance = rnd.Next(0, posspaths.Length);
                string Path = posspaths[chance];
                return Path;
            }
            else
            {


                //no tag paths to choose, pick path from defaut path
               string[] defaultpath = Directory.GetFiles($"{imagepath}\\default");


                if(defaultpath.Length <= 0)
                {
                return "error no files in default";
                }
                else
                {
                  int i =  rnd.Next(0, defaultpath.Length - 1);
                    return defaultpath[i];
                }
            }
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

            for(int i = 0; i < 1; i++)
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

        public static Dictionary<string, int> wordBankCompiler2(string boardId)
        {
            Dictionary<string, int> TD = new Dictionary<string, int>();

            // wordbank for post weighing

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
        public static Dictionary<string, string> imgtagCompiler()
        {
            //ideally this should use an images tags to decide all from one directory, very slow iteration
            //image path workaround but costs more file storeage for duplicate images

            //creates dict of keywords and directories paths full of relevant images

            //keyword : path-directory
            Dictionary<string, string> TD = new Dictionary<string, string>();

            TD.Add("frog",$"{imagepath}\\frog");

   
            Console.WriteLine(" ♥ TagBank Created ♥ ");

            foreach (KeyValuePair<string,string> kvp in TD)
            {
                Directory.CreateDirectory(kvp.Value);
                
                Console.WriteLine($" ♥ Tag Directory Created {kvp.Value} ♥ ");
            }

            Directory.CreateDirectory($"{imagepath}\\default");
            Directory.CreateDirectory($"{imagepath}\\verified");

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
        //public int Weight { get; set; }

        public ThreadList(string board, List<UnloadedCatalogThreads> Threads, string json)
        {
            Board = board;
            ThreadArray = Threads;
            JSON = json;
          //  Weight = weight;

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
        public int replyweight { get; set; }


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
        public List<Node> getRoots()
        {
            if(roots != null)
            {
                return roots;
            }
            return null;
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
        public string Sub { get; set; }
        public int TWeight { get; set; }

        public DisplayThread(int ID, string Board, List<_Pbox> SortedThread, string sub,int weight)
        {
            id = ID;
            board = Board;
            sortedthread = SortedThread;
            Sub = sub;
            TWeight = weight;
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



