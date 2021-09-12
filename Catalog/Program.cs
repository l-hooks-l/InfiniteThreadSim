using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Catalog
{
    class Program
    {
        public static WebClient WebClient = new WebClient();
        public static Thread _thread;
        private const string api_url = @"https://a.4cdn.org/";
        private const string api_img_url = @"https://i.4cdn.org/";


      /*  static void Main(string[] args)
        {
            System.Console.Title = "Top Posts";
            Menu().GetAwaiter().GetResult();

        } */

        
       
        public static async Task<Thread> LoadThread(string url)
        {
            var endpoint = string.Join("/", url.Remove(0, url.LastIndexOf('.')).Split('/'), 1, 3);
            var apiUrl = $"{api_url}{endpoint}.json";
            Console.WriteLine("loading thread");
            var json = await WebClient.DownloadStringTaskAsync($"{api_url}{endpoint}.json");

            if (string.IsNullOrEmpty(json))
            {
                System.Console.WriteLine("Web request returned null :(");
                return null;
            }

            Console.WriteLine("json OK");
            var posts = JObject.Parse(json)["posts"].ToObject<JArray>();
            if (posts == null)
            {
                // parsing error
                Console.WriteLine("post parse error");
                return null;
            }
            
            IDictionary<int, int> totalreplies = new Dictionary<int, int>();
            var id = posts[0]["no"].ToString();
            var semantic = posts[0]["semantic_url"].ToString();
            var subject = posts[0]["sub"] == null ? semantic : posts[0]["sub"].ToString();

            Console.WriteLine("Checking |" + posts.Count + "| Posts");

            for (int i = 0; i < posts.Count - 2; i++) //iterate through every post in a thread
            {

                int postid = Int32.Parse(posts[i]["no"].ToString());
                //Console.WriteLine(postid + " postid");
                if (posts[i]["com"] == null)
                {
                    Console.WriteLine("Out Of Posts");
                    
                }
                else
                {
                    if (posts[i]["com"] != null)
                    {
                        string postContent = posts[i]["com"].ToString();
                    
                   // Console.WriteLine(postid);
                    Console.WriteLine(postid + " : " + postContent);
                    totalreplies = replies(postContent, totalreplies);
                    }
                }
                
            }
            Console.WriteLine("prejson");
            var thread = new Thread(url, id, subject, semantic, totalreplies, json);
            //_thread = thread;
            Console.WriteLine("thread object created");
            return thread;




        }

        public static IDictionary<int, int> replies(string postContent, IDictionary<int, int> postreplies)
        {
                int Start, End;
                int data = 1;

            if (postContent.IndexOf(">&gt;&gt;", 0) == -1 || postContent.IndexOf("</a><br>", 0) == -1)
            {
               // Console.WriteLine("no reply found");
                return postreplies;

            }
            else
            {
                Start = postContent.IndexOf(">&gt;&gt;", 0) + 9;
                End = postContent.IndexOf("</a><br>", Start);
                //int parsecheck = Int32.Parse(postContent.Substring(Start, End - Start));

                if (Int32.Parse(postContent.Substring(Start, End - Start)) != null && 0 <= Int32.Parse(postContent.Substring(Start, End - Start)))
                {
                    data = Int32.Parse(postContent.Substring(Start, End - Start));

                    if (postreplies.ContainsKey(data) == false) //new unique reply
                    {
                        postreplies.Add(data, 1);
                    }
                    else if (postreplies.ContainsKey(data) == true) //repeated reply
                    {
                        postreplies[data]++;

                    }






                }
                //data = Int32.Parse(postContent.Substring(Start, End - Start));
            }
           /*     Start = postContent.IndexOf(">&gt;&gt;", 0) + 9;
            Console.WriteLine(Start + " start");
            End = postContent.IndexOf("</a><br>", Start);
            Console.WriteLine(End + " end");
               
            //Console.WriteLine("start " + Start + " end " + End);
            if (End - Start >= 0 && Start != 8)
            {
                data = Int32.Parse(postContent.Substring(Start, End - Start));
                


                //Console.WriteLine("reply start | data " + data);

                if (postreplies.ContainsKey(data) == false && End != -1) //new unique reply
                {

                    postreplies.Add(data, 1);


                }
                else if (postreplies.ContainsKey(data) == true && End != -1) //repeated reply
                {
                    postreplies[data]++;

                }
            }
            
            else
            {
                Console.WriteLine("no reply found");
            }
            */
            return postreplies;
            //Console.WriteLine("reply finished");
        }

        

        private static async Task Menu()
        {
            var url = URLPrompt();
            Thread _thread;
            while (string.IsNullOrEmpty(url)) url = URLPrompt();
            System.Console.Write("Path (leave empty for working dir): ");
            var Path = System.Console.ReadLine();

            System.Console.WriteLine("||| Reading Thread |||");
            _thread = await LoadThread(url);
           
            if (_thread == null)
            {
                System.Console.WriteLine("||| Thread Loading Error |||");
                System.Console.ReadKey(true);
                return;
            }
            Console.WriteLine("thread loaded OK");

            System.Console.Title = $"ImageBoard Inspector + {_thread.Subject}";
            Console.WriteLine(_thread.Postreplies.Count);
            Console.WriteLine(_thread.Id);
            Console.WriteLine(_thread.Url);
            var Replies = _thread.Postreplies;

            var posts = JObject.Parse(_thread.JSON)["posts"].ToObject<JArray>();

            for (int i = 0; i < posts.Count - 1 ; i++)
            {
               // Console.WriteLine(i);
                foreach (KeyValuePair<int, int> kvp in _thread.Postreplies)
                {
                    if (Int32.Parse(posts[i]["no"].ToString()) == kvp.Key && posts[i]["no"].ToString()  != null && posts[i]["com"] != null)
                    {
                        //Console.WriteLine("post-to-string " + posts[i]["com"].ToString());
                         var htmldoc = new HtmlAgilityPack.HtmlDocument();
                        var html = posts[i]["com"].ToString();
                        if (html != null)
                        {
                            
                            htmldoc.LoadHtml(html);

                            var htmlparsed = htmldoc.DocumentNode.InnerText;
                            string decoded = WebUtility.HtmlDecode(htmlparsed);

                            Console.WriteLine("{0} | Replies {1} | com : {2}", kvp.Key, kvp.Value, decoded);
                        }
                        else
                        {
                            string empty = "empty com";
                         //   Console.WriteLine("{0} | Replies {1} | com : {2}", kvp.Key, kvp.Value, empty);


                        }
                    }
                }
            }
            //var sorteddict = from entry in _thread.Postreplies orderby entry.Value ascending select entry;
            //foreach (KeyValuePair<int, int> kvp in _thread.Postreplies)
           // {
                
           //     System.Console.WriteLine("Post Id : {0}, Replies : {1} : com : {}", kvp.Key, kvp.Value);

               
          //  }

            System.Console.WriteLine($"{Environment.NewLine}Press any key to continue");
            System.Console.ReadKey(true);
        }
        
        private static string URLPrompt()
        {
            System.Console.WriteLine("Thread Url: ");
            return System.Console.ReadLine();
        }
/*
        public class Tree
        {
            List<Node> roots;
            Dictionary<int, Node> node_dict;

            public Tree()
            {
                roots = new List<Node>();
                node_dict = new Dictionary<int, Node>();
            }

            public void addNode(Node node)
            {
                node_dict.Add(node.id, node);
               // node.tree = this;
            }

            public Node getNode(int id)
            {
                return node_dict[id];
            }

            public void addRoot(Node node)
            {
                this.addNode(node);
                this.roots.Add(node);
            }

        }

   /*     public class Node
        {
            public int id { get; set; }

            string message { get; set; }

            Node parent { get; set; }

            List<Node> children { get; }

            public Tree tree { get; set; }

            public Node()
            {
                this.children = new List<Node>();
            }

            public void addParent(int id)
            {
                Node parent = this.tree.getNode(id);
                this.parent = parent;
                this.parent.children.Add(this);
            }
        } */

        public class Thread
        {
            public string Url { get; private set; }
            public string Id { get; private set; }
            public string Subject { get; private set; }
            public string SematicSubject { get; private set; }
            public IDictionary<int,int> Postreplies { get; private set; }
            public string JSON { get; private set; }

            public Thread(string url, string id, string subject, string semantic, IDictionary<int,int> postReplies, string json)
            {
                this.Url = url;
                Id = id;
                Subject = subject;
                SematicSubject = semantic;
                Postreplies = postReplies;
                JSON = json;
            }

        }






















    }
}
