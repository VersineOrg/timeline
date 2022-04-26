using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace timeline;

public class Timeline
{
    public List<string> timeline { get; set; }

    public Timeline(List<BsonDocument> postsBson)
    {
        List<string> postList = new List<string>();
        foreach (BsonDocument post in postsBson)
        {
            string newpost =  Post.PostToJson(post);
            postList.Append(newpost);
        }
        this.timeline = postList;
    }

    public static string TimelineToJson(List<BsonDocument> postsBson)
    {
        Timeline timeline = new Timeline(postsBson);
        string jsonString = JsonConvert.SerializeObject(timeline);
        return jsonString;
    }
}