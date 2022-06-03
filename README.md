# timeline

exemples with Maxime's db (Bob, Alice and Jeanmi)
see appsettings

TIMELINE has 2 endpoints :
1- "GET" : "/getTimeline" :
    given the token retrieves all id's friend's posts and returns a list of posts.
    
    exemple:
 
given:
{
"token":"ew0KICAiYWxnbyI6ICJIUzI1NiIsDQogICJ0eXBlIjogIkpXVCINCn0=.ew0KICAiaWQiOiAiNjI2OTU5NDBhZTNjNDU0ZjM1NGFlMmYwIiwNCiAgImV4cCI6ICIwIg0KfQ==.Yjk/P1pkPz9/BD8/Pwc7XTw/Pz8/Pz8oPz8qJXUwGDI="
}
//token is Alice's

returns:
{
	"status": "success",
	"message": "timeline created",
	"data": "{\"UserId\":\"626958c217581b173d7b7a3c\",\"Message\":\"salut les potes\"}, {\"UserId\":\"626958c217581b173d7b7a3c\",\"Message\":\"oe c'est bob\"}, {\"UserId\":\"62696bd3ec4e5f3255c6964d\",\"Message\":\"Salut moi c'est Jeanmi\"}"
}
//Alice is friend with Bob and Jeanmi, so "/getTimeline" returns all the posts from Bob and Jeanmi
    
2- "GET" : "/getPostsfromFriend" :
    given a token and an id checks if the id is a friends of the user associated with token, if he is retrieves all the post specifically from that friend.
    
    exemple:
    
given: 
{
"token":"ew0KICAiYWxnbyI6ICJIUzI1NiIsDQogICJ0eXBlIjogIkpXVCINCn0=.ew0KICAiaWQiOiAiNjI2OTU5NDBhZTNjNDU0ZjM1NGFlMmYwIiwNCiAgImV4cCI6ICIwIg0KfQ==.Yjk/P1pkPz9/BD8/Pwc7XTw/Pz8/Pz8oPz8qJXUwGDI=",
	"friendId":"62696bd3ec4e5f3255c6964d"
} 
//token is Alice's and id is Jeanmi.

returns: 
{
	"status": "success",
	"message": "retrieved posts from fren",
	"data": "{\"UserId\":\"62696bd3ec4e5f3255c6964d\",\"Message\":\"Salut moi c'est Jeanmi\"}"
}
//returns the one and only post from Jeanmi
