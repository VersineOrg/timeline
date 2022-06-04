using System.Net;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;

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

        public static async Task HandleIncomingConnections(EasyMango.EasyMango postDatabase, EasyMango.EasyMango userDatabase)
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
                    dynamic body = JsonConvert.DeserializeObject(bodyString)!;
                
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
                            if (userDatabase.GetSingleDatabaseEntry("_id", new ObjectId(id), out BsonDocument user))
                            {
                                List<BsonValue> tempFriends = user.GetElement("friends").Value.AsBsonArray.ToList();
                                List<string> friends = new List<string>();
                                
                                foreach (BsonValue friend in tempFriends)
                                {
                                    friends.Add(friend.AsString);
                                }

                                List<BsonDocument> postsBson = new List<BsonDocument>();

                                foreach (string friendid in friends)
                                {
                                    postDatabase.GetMultipleDatabaseEntries("userId", friendid,
                                        out List<BsonDocument> friendPostsBson);
                                    foreach (BsonDocument friendBson in friendPostsBson)
                                    {
                                        postsBson.Add(friendBson);
                                    }
                                }
                                Response.Success(resp, "timeline created", Timeline.TimelineToJson(postsBson));
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
                else if (req.HttpMethod == "GET" && req.Url?.AbsolutePath == "/getPostsfromFriend")
                {
                    StreamReader reader = new StreamReader(req.InputStream);
                    string bodyString = await reader.ReadToEndAsync();
                    dynamic body = JsonConvert.DeserializeObject(bodyString)!;
                
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
                            if (userDatabase.GetSingleDatabaseEntry("_id", new ObjectId(id), out BsonDocument user))
                            {
                                List<BsonValue> tempFriends = user.GetElement("friends").Value.AsBsonArray.ToList();

                                bool isFriend = false;
                                foreach (BsonValue friend in tempFriends)
                                {
                                    if (friend.AsString == friendId)
                                    {
                                        isFriend = true;
                                    }
                                }
                                if (isFriend)
                                {
                                    userDatabase.GetSingleDatabaseEntry("_id", new ObjectId(id),
                                        out BsonDocument friend);
                                    postDatabase.GetMultipleDatabaseEntries("userId", friendId,
                                        out List<BsonDocument> friendPostsBson);
                                    Response.Success(resp, "retrieved posts from fren", Timeline.TimelineToJson(friendPostsBson));
                                }
                                else
                                {
                                    Response.Fail(resp, "id is not a user's friend");
                                }
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
            string databaseName = config.GetValue<String>("databaseName");
            string collectionName = config.GetValue<String>("collectionName");
        
            string databaseName1 = config.GetValue<String>("databaseName1");
            string collectionName1 = config.GetValue<String>("collectionName1");
        
            // Create a new EasyMango database
            EasyMango.EasyMango postdb = new EasyMango.EasyMango(connectionString,databaseName,collectionName);
            EasyMango.EasyMango userdb = new EasyMango.EasyMango(connectionString,databaseName1,collectionName1);
            
            // Create a Http server and start listening for incoming connections
            string url = "http://*:" + config.GetValue<String>("Port") + "/";
            Listener = new HttpListener();
            Listener.Prefixes.Add(url);
            Listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections(postdb, userdb);
            listenTask.GetAwaiter().GetResult();
        
            // Close the listener
            Listener.Close();
        }
    }