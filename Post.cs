using System.Net;
using System.Text;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace timeline;

public class Post
{
    public BsonObjectId UserId { get; set; }
    public String Message { get; set; }
    

    public Post(BsonDocument document)
    {
        this.UserId = document.GetElement("userId").Value.AsObjectId;
        this.Message = document.GetElement("message").Value.AsString;;
    }

    public static string PostToJson(BsonDocument document)
    {
        Post post = new Post(document);
        string jsonString = JsonConvert.SerializeObject(post);
        return jsonString;
    }
}