using System.ComponentModel;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace timeline;

public class Timeline
{
    public static List<BsonDocument> SortTimeline(List<BsonDocument> postsBson, string sort)
    {
        if (sort == "date")
        {
            IEnumerable<BsonDocument> sortedPosts = postsBson.OrderByDescending(document => document.GetElement("date").Value.AsInt64);
            List<BsonDocument> newPostsBson = new List<BsonDocument>();
            foreach (BsonDocument bsonDocument in sortedPosts)
            {
                newPostsBson.Add(bsonDocument);
            }

            return newPostsBson;
        }
        else
        {
           throw new Exception("sort is not a valid arg");
        }
    }
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