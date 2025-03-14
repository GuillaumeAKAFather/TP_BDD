using System.Configuration;
using System.Data.SqlTypes;
using System.Numerics;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
public class MySqlPart{

    MySqlConnection connection;
    MySqlDataReader reader;


    public MySqlPart(){
        string connectString = "server=81.1.20.23;uid=EtudiantJvd;pwd='!?CnamNAQ01?!';database=USRS6N_1;port=3306";
        connection = new MySqlConnection();
        connection.ConnectionString = connectString;
        connection.Open();

    }

    


    public void HandleSubscription(){

        string player_pseudo = string.Empty;
        string player_name = string.Empty;
        string player_lastName = string.Empty;
        int player_age = 0;
        string player_adress = string.Empty;
        string player_pswd=  string.Empty;


        
        Console.WriteLine("Enter your pseudo : ");
        player_pseudo = Console.ReadLine();
        
        Console.WriteLine("Enter a password : ");
        player_pswd = Console.ReadLine();

        Console.WriteLine("Enter your first name : ");
        player_name = Console.ReadLine();

        Console.WriteLine("Enter your last name : ");
        player_lastName = Console.ReadLine();

        while(true){
            Console.WriteLine("Enter your age : ");
            string input = Console.ReadLine();
            if(int.TryParse(input, out int result)){
                break;
            }
            player_age = result;
        }

        Console.WriteLine("Enter your mail : ");
        player_adress = Console.ReadLine();

        

        string sql = $"insert into Gr6_Joueurs ( pseudo, motDePasse, prenom, nom, email, age) VALUES ('{player_pseudo}', '{player_pswd}', '{player_name}' , '{player_lastName}', '{player_adress}' , '{player_age}');";
        ExecuteSql(sql);

        Console.WriteLine("Subscription done !");
    }



    public int PseudoID(string pseudoEntered){
        string sql = $"select joueurId from Gr6_Joueurs where pseudo='{pseudoEntered}'";
        using (MySqlDataReader result = GetSqlResults(sql)){
                if(result.Read()){
                return result.GetInt32(0);
            }
        }
        return -1;
        
    }
    public bool CheckPassword(string password, int playerID){
        string sql = $"select joueurId from Gr6_Joueurs where motDePasse='{password}'";
        using(MySqlDataReader result = GetSqlResults(sql)){
            while(result.Read()){
                var res = (int)result["joueurID"];
                if (res == playerID){
                    return true;
                }
            }
        }

        return false;
    }

    public List<int> JoinablePartieList(){
        List<int> partyIds = new List<int>();
        string sql = "select partieID from Gr6_Partie where etat='en cours'";
        using(MySqlDataReader result = GetSqlResults(sql)){
            while(result.Read()){
                var res = (int)result["partieID"];
                partyIds.Add(res);
            }
        }

        
        return partyIds;
    }

    public void CreateGame(){
        string sql = "insert into Gr6_Partie (dimensionCube, duree, etat, jeuId) values (5,60,'en cours',3); SELECT LAST_INSERT_ID()";
        int generatedPrimaryKey = -1;
        using (MySqlDataReader result = GetSqlResults(sql)){
            //Game created
            if(result.Read()){
                generatedPrimaryKey = result.GetInt32(0);
                Console.WriteLine("Key of created game : " + generatedPrimaryKey);
            }
        }

        GenerateEnemyShips(generatedPrimaryKey);

        Console.WriteLine("Game created !");

    }

    private void GenerateEnemyShips(int partieID){
        //Get game size;
        string sql = $"select dimensionCube from Gr6_Partie where partieId={partieID}";
        int gameSize = -1;
        using (MySqlDataReader result = GetSqlResults(sql)){
            if(result.Read()){
                gameSize= result.GetInt32(0);
            }
        }

        if (gameSize == -1){
            return;
        }

        //Generate line enemy
        Random rand = new Random();
        for (int i = 0; i < 10; i++)
        {
            int shipSize = rand.Next(1,gameSize);
            Vector3 shipPosition= new Vector3( rand.Next(0,gameSize - shipSize),rand.Next(0,gameSize - shipSize),rand.Next(0,gameSize - shipSize));
            string jsonShipPosition = JsonConvert.SerializeObject(shipPosition);
            //Console.WriteLine("Create ship with size : " + shipSize +" at position :" + shipPosition);
            string addShipSql = $"insert into `USRS6N_1`.`Gr6_Vaisseau` ( `partieId`, `typeVaisseau`, `taille`, `position`) VALUES ('{partieID}', 'segment', '{shipSize}', '{jsonShipPosition}')";
            ExecuteSql(addShipSql);
        }

    }

    public struct EnemyShipDataStructure{
        public int partieID;
        public int taille;
        public Vector3 position;
        public string typeVaisseau;
    }
    public Dictionary<int, EnemyShipDataStructure> GetEnemyShipDataByGameId(int partieID){
        string sql = $"select * from Gr6_Vaisseau v where v.partieId='{partieID} and not exists(select 1 from Gr6_DestroyedPart d where d.data = v.data)'";
        Dictionary<int, EnemyShipDataStructure> resultDataStructure = new Dictionary<int, EnemyShipDataStructure>();

        using (MySqlDataReader result = GetSqlResults(sql)){
            while(result.Read()){
                resultDataStructure[ (int)result["vaisseauId"]] = new EnemyShipDataStructure{
                    partieID = (int)result["partieId"],
                    taille = (int)result["taille"],
                    position = JsonConvert.DeserializeObject<Vector3>((string)result["position"]),
                    typeVaisseau = (string)result["typeVaisseau"],
                };
            }
        }

        return resultDataStructure;

    }

    public bool IsOutsideGameBound(int gameId, Vector3 position){
        string sql = $"select dimensionCube from Gr6_Partie where partieId={gameId}";
        int gameSize = -1;
        using (MySqlDataReader result = GetSqlResults(sql)){
            if(result.Read()){
                gameSize= result.GetInt32(0);
            }
        }
        
        return position.X < 0 || position.X >= gameSize ||
        position.Y < 0 || position.Y >= gameSize ||
        position.Z < 0 || position.Z >= gameSize;
    }

    private MySqlDataReader GetSqlResults(string sql){
        MySqlCommand command = new MySqlCommand(sql, connection);
        return command.ExecuteReader();
    }

    private void ExecuteSql(string sql){
        MySqlCommand command = new MySqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public bool JoinGame(int joueurID, int partieID){

        //Check if game exists and is not finished
        Console.WriteLine("Check party available");
        string sql = $"select etat from Gr6_Partie where partieId ={partieID}";
        using (MySqlDataReader result = GetSqlResults(sql)){
            if(result.Read()){
                
            }else{
                // Handle later, party does not exist or is finished
                return false;
            }
        }

        //VictoryField NANI ?
        string joinSessionSql = $"INSERT INTO `USRS6N_1`.`Gr6_Session` (`partieId`, `joueurId`) VALUES ('{partieID}', '{joueurID}')";
        ExecuteSql(joinSessionSql);
        return true;
    }

    public void AddPlayerPoint(int gameId , int playerId){
        string sql = $"update Gr6_Session set score = score + 1 WHERE partieId = '{gameId}' AND joueurId = '{playerId}'";
        ExecuteSql(sql);
    }

    public void AddDestroyedPart(int gameId, Vector3 destroyedPosition ){
        string json = JsonConvert.SerializeObject(destroyedPosition);
        string destroyPartSql = $"insert into `USRS6N_1`.`Gr6_DestroyedPart` (`partDestroyed`, `gameId`) VALUES ('{json}', '{gameId}')";
        ExecuteSql(destroyPartSql);
    }
    public List<Vector3> GetUnavailablePoint(int gameId){
        List<Vector3> unavailablePoint = new List<Vector3>();
        string sql = $"select * form Gr6_DestroyedPart where gameId='{gameId}'";

        using (MySqlDataReader result = GetSqlResults(sql)){
            while(result.Read()){
                unavailablePoint.Add( JsonConvert.DeserializeObject<Vector3>((string)result["partDestroyed"]));
            }
        }

        return unavailablePoint;
    }
}