using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class TB_BDD{

    static string connectionString = "mongodb://127.0.0.1:27017";
    static string databaseName ="crash_test_db";
    static string collectionName = "player";

    static void Main(string[] args){
        PreGameLoop preGameLoop = new PreGameLoop();
        preGameLoop.MainLoop();


        MongoClient client = new MongoClient(connectionString);
        IMongoDatabase db = client.GetDatabase(databaseName);
        var collection = db.GetCollection<Player>(collectionName);

        Player person = new Player{FirstName = "Naruto", LastName = "Uzumaki"};
        //await collection.InsertOneAsync(person)

    }

}


public class Player{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id {get ; set;}
    public string FirstName{get;set;}
    public string LastName{get;set;}
    
}