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
        else if (sort == "cool")
        {
            long dateNow = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalHours;
            foreach (BsonDocument bsonDocument in postsBson)
            {
                TimeSpan time = TimeSpan.FromSeconds(bsonDocument.GetElement("date").Value.AsInt64);
                long votes = bsonDocument.GetElement("upvoter").Value.AsBsonArray.ToList().Count - bsonDocument.GetElement("downvoter").Value.AsBsonArray.ToList().Count;
                if (votes < 0)
                {
                    votes = 1;
                }
                long score = (votes * votes) / (dateNow - time.Hours);
                bsonDocument.Add("score", score);
            }
            IEnumerable<BsonDocument> sortedPosts = postsBson.OrderByDescending(document => document.GetElement("score").Value.AsInt64);
            List<BsonDocument> newCoolPostsBson = new List<BsonDocument>();
            foreach (BsonDocument bsonDocument in sortedPosts)
            {
                bsonDocument.Remove("score");
                newCoolPostsBson.Add(bsonDocument);
            }
            return newCoolPostsBson;
        }
        else
        {
           throw new Exception("sort is not a valid arg");
        }
    }
    public static string TimelineToJson(List<BsonDocument> postsBson)
    {
        string jsonString = "[";
        int l = postsBson.Count;
        for (int i = 0; i < l; i++)
        {
            jsonString += Post.PostToString(postsBson[i]);
            if (i < l - 1)
            {
                jsonString += ", ";
            }
        }

        jsonString += "]";
        return jsonString;
    }
    
}