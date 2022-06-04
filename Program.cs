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
        foreach (BsonDocument postBson in postsBson)
        {
            Post newpost = new Post(postBson);
            string postString = newpost.ToJson();
            postList.Add(postString);
        }
        this.timeline = postList; 
       
    }

    public static string TimelineToJson(List<BsonDocument> postsBson)
    {
        string jsonString = "";
        int l = postsBson.Count;
        for (int i = 0; i < l; i++)
        {
            jsonString += Post.PostToJson(postsBson[i]);
            if (i < l - 1)
            {
                jsonString += ", ";
            }
        }

        return jsonString;
    }
}