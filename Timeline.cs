using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace timeline;

public class Timeline
{
    public List<BsonObjectId> timeline { get; set; }
    
    public static string TimelineToJson(List<BsonDocument> postsBson)
    {
        string jsonString = "";
        int l = postsBson.Count;
        for (int i = 0; i < l; i++)
        {
            jsonString += Post.PostToString(postsBson[i]);
            if (i < l - 1)
            {
                jsonString += ", ";
            }
        }

        return jsonString;
    }
}