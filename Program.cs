using System.Net;
using circles;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;
using VersineUser;


namespace timeline;

// Default Schema for a Http Response
public partial class Response
{
    public String success { get; set; }
    public String message { get; set; }
}


class HttpServer
    {
        
        public static HttpListener? Listener;

        public static async Task HandleIncomingConnections(EasyMango.EasyMango postDatabase, EasyMango.EasyMango userDatabase, EasyMango.EasyMango circleDatabase)
        {

            // Connect to the MongoDB Database
            /* 
            string connectionString = config.GetValue<String>("MongoDB");
            MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);
            MongoClient client = new MongoClient(settings);
            IMongoDatabase database = client.GetDatabase("UsersDB");
            BsonClassMap.RegisterClassMap<User>();
            IMongoCollection<User> collection = database.GetCollection<User>("users");
            Console.WriteLine("Database connected");
            */
            
            
            
            while (true)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await Listener?.GetContextAsync()!;

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.Url?.ToString());
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                
                if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/getTimeline")
                {
                    StreamReader reader = new StreamReader(req.InputStream);
                    string bodyString = await reader.ReadToEndAsync();
                    dynamic body;
                    try
                    {
                        body = JsonConvert.DeserializeObject(bodyString)!;
                    }
                    catch
                    {
                        Response.Fail(resp, "bad request");
                        resp.Close();
                        continue;
                    }
                
                    string token;

                    try
                    {
                        token = ((string) body.token).Trim();
                    }
                    catch
                    {
                        token = "";
                    }

                    if (!(String.IsNullOrEmpty(token)))
                    {
                        string id = WebToken.GetIdFromToken(token);
                        if (!id.Equals(""))
                        {
                            if (userDatabase.GetSingleDatabaseEntry("_id", new ObjectId(id), out BsonDocument userBson))
                            {
                                User user = new User(userBson);
                                List<BsonObjectId> friendsIdList = user.friends;
                                List<BsonDocument> postsBson = new List<BsonDocument>();
                                foreach (BsonObjectId friendId in friendsIdList)
                                {
                                    postDatabase.GetMultipleDatabaseEntries("userId", friendId,
                                        out List<BsonDocument> friendPostsBson);
                                    foreach (BsonDocument postBson in friendPostsBson)
                                    {
                                        Post post = new Post(postBson);
                                        bool userinCircle = false;
                                        foreach (BsonValue circleId in post.Circles)
                                        {
                                            try
                                            {
                                                if (circleDatabase.GetSingleDatabaseEntry("_id", (circleId.AsObjectId),
                                                        out BsonDocument circleBson)) ;
                                                {
                                                    Circle circle = new Circle(circleBson);
                                                    if (circle.users.Contains(new ObjectId(id)))
                                                    {
                                                        userinCircle = true;
                                                    }
                                                }
                                            }
                                            catch {
                                                userinCircle = false;
                                            }
                                            
                                        }
                                        if (userinCircle || post.Circles.Count == 0)
                                        {
                                            postsBson.Add(postBson);
                                        }
                                    }
                                }
                                List<BsonDocument> sortedPosts = Timeline.SortTimeline(postsBson, "date");
                                Response.Success(resp, "retrieved posts from fren", Timeline.TimelineToJson(sortedPosts));                            }
                            else
                            {
                                Response.Fail(resp,"user doesn't exist");
                            }
                        }
                        else
                        {
                            Response.Fail(resp,"invalid token");
                        }
                    }
                    else
                    {
                        Response.Fail(resp,"invalid body");
                    }
                }
                else if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/getCoolTimeline")
                {
                    StreamReader reader = new StreamReader(req.InputStream);
                    string bodyString = await reader.ReadToEndAsync();
                    dynamic body;
                    try
                    {
                        body = JsonConvert.DeserializeObject(bodyString)!;
                    }
                    catch
                    {
                        Response.Fail(resp, "bad request");
                        resp.Close();
                        continue;
                    }
                
                    string token;

                    try
                    {
                        token = ((string) body.token).Trim();
                    }
                    catch
                    {
                        token = "";
                    }

                    if (!(String.IsNullOrEmpty(token)))
                    {
                        string id = WebToken.GetIdFromToken(token);
                        if (!id.Equals(""))
                        {
                            if (userDatabase.GetSingleDatabaseEntry("_id", new ObjectId(id), out BsonDocument userBson))
                            {
                                User user = new User(userBson);
                                List<BsonObjectId> friendsIdList = user.friends;
                                List<BsonDocument> postsBson = new List<BsonDocument>();
                                foreach (BsonObjectId friendId in friendsIdList)
                                {
                                    postDatabase.GetMultipleDatabaseEntries("userId", friendId,
                                        out List<BsonDocument> friendPostsBson);
                                    foreach (BsonDocument postBson in friendPostsBson)
                                    {
                                        Post post = new Post(postBson);
                                        bool userinCircle = false;
                                        foreach (BsonValue circleId in post.Circles)
                                        {
                                            try 
                                            {
                                                if (circleDatabase.GetSingleDatabaseEntry("_id", (circleId.AsObjectId),
                                                        out BsonDocument circleBson)) ;
                                                {
                                                    Circle circle = new Circle(circleBson);
                                                    if (circle.users.Contains(new ObjectId(id)))
                                                    {
                                                        userinCircle = true;
                                                    }
                                                }
                                            }
                                            catch {
                                                userinCircle = false;
                                            }
                                        }
                                        if (userinCircle || post.Circles.Count == 0)
                                        {
                                            postsBson.Add(postBson);
                                        }
                                    }
                                }
                                List<BsonDocument> sortedPosts = Timeline.SortTimeline(postsBson, "cool");
                                Response.Success(resp, "retrieved posts from fren", Timeline.TimelineToJson(sortedPosts));                            }
                            else
                            {
                                Response.Fail(resp,"user doesn't exist");
                            }
                        }
                        else
                        {
                            Response.Fail(resp,"invalid token");
                        }
                    }
                    else
                    {
                        Response.Fail(resp,"invalid body");
                    }
                }
                else if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/getPostsfromFriend")
                {
                    StreamReader reader = new StreamReader(req.InputStream);
                    string bodyString = await reader.ReadToEndAsync();
                    dynamic body;
                    try
                    {
                        body = JsonConvert.DeserializeObject(bodyString)!;
                    }
                    catch
                    {
                        Response.Fail(resp, "bad request");
                        resp.Close();
                        continue;
                    }

                    string token;
                    string friendId;

                    try
                    {
                        token = ((string) body.token).Trim();
                        friendId = ((string) body.friendId).Trim();
                    }
                    catch
                    {
                        token = "";
                        friendId = "";
                    }

                    if (!(String.IsNullOrEmpty(token)))
                    {
                        string id = WebToken.GetIdFromToken(token);
                        if (!id.Equals(""))
                        {
                            if (userDatabase.GetSingleDatabaseEntry("_id", new ObjectId(id), out BsonDocument userBson))
                            {
                                User user = new User(userBson);
                                List<BsonObjectId> friendsIdList = user.friends;
                                if (friendsIdList.Contains(new ObjectId(friendId)))
                                {
                                    List<BsonDocument> postsBson = new List<BsonDocument>();
                                    postDatabase.GetMultipleDatabaseEntries("userId", new ObjectId(friendId),
                                        out List<BsonDocument> friendPostsBson);
                                    foreach (BsonDocument postBson in friendPostsBson)
                                    {
                                        Post post = new Post(postBson);
                                        bool userinCircle = false;
                                        foreach (BsonValue circleId in post.Circles)
                                        {
                                            try
                                            {

                                                if (circleDatabase.GetSingleDatabaseEntry("_id", (circleId.AsObjectId),
                                                        out BsonDocument circleBson)) ;
                                                {
                                                    Circle circle = new Circle(circleBson);
                                                    if (circle.users.Contains(new ObjectId(id)))
                                                    {
                                                        userinCircle = true;
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                                userinCircle = false;
                                            }
                                        }

                                        if (userinCircle || post.Circles.Count == 0)
                                        {
                                            postsBson.Add(postBson);
                                        }
                                    }

                                    List<BsonDocument> sortedPosts = Timeline.SortTimeline(friendPostsBson, "date");
                                    Response.Success(resp, "retrieved posts from fren",
                                        Timeline.TimelineToJson(sortedPosts));
                                }
                                else
                                {
                                    Response.Fail(resp, "id is not a user's friend");
                                }
                            }
                            else
                            {
                                Response.Fail(resp, "user doesn't exist");
                            }
                        }
                        else
                        {
                            Response.Fail(resp, "invalid token");
                        }
                    }
                    else
                    {
                        Response.Fail(resp, "invalid body");
                    }
                }
                else if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/getSelfPosts")
                {
                    StreamReader reader = new StreamReader(req.InputStream);
                    string bodyString = await reader.ReadToEndAsync();
                    dynamic body;
                    try
                    {
                        body = JsonConvert.DeserializeObject(bodyString)!;
                    }
                    catch
                    {
                        Response.Fail(resp, "bad request");
                        resp.Close();
                        continue;
                    }
                
                    string token;

                    try
                    {
                        token = ((string) body.token).Trim();
                    }
                    catch
                    {
                        token = "";
                    }

                    if (!(String.IsNullOrEmpty(token)))
                    {
                        string id = WebToken.GetIdFromToken(token);
                        if (!id.Equals(""))
                        {
                            if (userDatabase.GetSingleDatabaseEntry("_id", new ObjectId(id), out BsonDocument userBson))
                            {
                                if(postDatabase.GetMultipleDatabaseEntries("userId", new ObjectId(id),
                                       out List<BsonDocument> selfPostsBson));
                                List<BsonDocument> sortedPosts = Timeline.SortTimeline(selfPostsBson, "date");
                                Response.Success(resp, "retrieved posts from self", Timeline.TimelineToJson(sortedPosts));
                            }
                            else
                            {
                                Response.Fail(resp,"user doesn't exist");
                            }
                        }
                        else
                        {
                            Response.Fail(resp,"invalid token");
                        }
                    }
                    else
                    {
                        Response.Fail(resp,"invalid body");
                    }
                }
                else
                {
                    Response.Fail(resp, "404");
                }
                    
                resp.Close();    
            }
        }


        public static void Main(string[] args)
        {
            IConfigurationRoot config =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true)
                    .AddEnvironmentVariables()
                    .Build();
            
            string connectionString = config.GetValue<String>("connectionString");
            string postDatabaseName = config.GetValue<String>("postDatabaseName");
            string postCollectionName = config.GetValue<String>("postCollectionName");
            string userDatabaseName = config.GetValue<String>("userDatabaseName");
            string userCollectionName = config.GetValue<String>("userCollectionName");
            string circleDatabaseName = config.GetValue<String>("circleDatabaseName");
            string circleCollectionName = config.GetValue<String>("circleCollectionName");
            
            // Create a new EasyMango database
            EasyMango.EasyMango postdb = new EasyMango.EasyMango(connectionString,postDatabaseName,postCollectionName);
            EasyMango.EasyMango userdb = new EasyMango.EasyMango(connectionString,userDatabaseName,userCollectionName);
            EasyMango.EasyMango circledb = new EasyMango.EasyMango(connectionString,circleDatabaseName,circleCollectionName);
            
            // Create a Http server and start listening for incoming connections
            string url = "http://*:" + config.GetValue<String>("Port") + "/";
            Listener = new HttpListener();
            Listener.Prefixes.Add(url);
            Listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections(postdb, userdb, circledb);
            listenTask.GetAwaiter().GetResult();
        
            // Close the listener
            Listener.Close();
        }
    }