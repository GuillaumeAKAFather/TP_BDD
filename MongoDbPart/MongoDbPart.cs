using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Numerics;
public class MongoDbPart{

    private MongoClient client;
    private IMongoDatabase db;
    
    private const string deplacement_joueur ="Gr6_DeplacementJoueur";
    private const string tirs_joueurs ="Gr6_JoueurTirs";

    public MongoDbPart(){

        //Connecto to mongoDb
        string connectionString ="mongodb://AdminLJV:!!DBLjv1858**@81.1.20.23:27017/";
        string databaseName = "USRS6N_2025";
        

        client = new MongoClient(connectionString);
        db = client.GetDatabase(databaseName);

        // IMongoCollection<PlayerMovement> collection = db.GetCollection<PlayerMovement>(collectionName);
        // PlayerMovement pmovement = new PlayerMovement();
        // collection.InsertOne(pmovement);

        // var result = collection.Find<PlayerMovement>( x => x.playerID == 0);

        // foreach(var res in result.ToList()){
        //     Console.Write(res.moveTo);
        // }
    }


    public void RegisterPlayerMovement(int playerID, int gameId, Vector3 newPosition){
        IMongoCollection<PlayerMovement> movementCollection = db.GetCollection<PlayerMovement>(deplacement_joueur);
        movementCollection.InsertOneAsync(new PlayerMovement{
            playerId = playerID, 
            moveTo = newPosition, 
            gameId = gameId
        });
    }

    public int GetPlayerShootAmountByGameId(int playerID, int gameId){
        IMongoCollection<PlayerShootData> playerShootCollection = db.GetCollection<PlayerShootData>(tirs_joueurs);
        return playerShootCollection.Find<PlayerShootData>( x => x.playerId == playerID && x.gameId == gameId).ToList().Count;
    }

    public List<Vector3> GetPlayerMovementListByGameId(int playerId, int gameId){
        List<Vector3> playerMovementList = new List<Vector3>();
        
        IMongoCollection<PlayerMovement> movementCollection = db.GetCollection<PlayerMovement>(deplacement_joueur);
        List<PlayerMovement> result = movementCollection.Find<PlayerMovement>(x => x.playerId == playerId && x.gameId == gameId).ToList();
        
        foreach(PlayerMovement playerMovement in result){
            playerMovementList.Add(playerMovement.moveTo);
        }

        return playerMovementList;
    }

    public void RegisterPlayerShoot(int playerID, int gameId, Vector3 shootDirection){
        IMongoCollection<PlayerShootData> playerShootCollection = db.GetCollection<PlayerShootData>(tirs_joueurs);
        playerShootCollection.InsertOneAsync(new PlayerShootData{
            gameId = gameId,
            playerId = playerID,
            shootDirection = shootDirection,
        });
    }

    public class PlayerMovement{
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id {get;set;}
        public int playerId{get;set;}
        public int gameId{get;set;}
        public Vector3 moveTo{get;set;}
    }

    public class PlayerShootData{
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id {get;set;}
        public int playerId;
        public int gameId{get;set;}
        public Vector3 shootDirection{get;set;}
    }
}

