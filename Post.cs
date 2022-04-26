using System.Net;
using System.Text;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace timeline;

public class Post
{
    public String UserId { get; set; }
    public String Message { get; set; }
    
    public Post(string id, string password)
    {
        this.UserId = id;
        this.Message = password;
    }

    public Post(BsonDocument document)
    {
        this.UserId = document.GetElement("userId").Value.AsString;
        this.Message = document.GetElement("message").Value.AsString;;
    }

    public static string PostToJson(BsonDocument document)
    {
        Post post = new Post(document);
        string jsonString = JsonConvert.SerializeObject(post);
        return jsonString;
    }
}