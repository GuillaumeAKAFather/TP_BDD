using System.Configuration;
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

        if (gameSize < 1){
            return;
        }

        //Generate line enemy
        Random rand = new Random();
        List<Vector3> shipPositions = new List<Vector3>();

        for (int i = 0; i < 10; i++)
        {
            int shipSize = rand.Next(1, gameSize);
            Vector3 shipPosition;

            // Ensure the new ship position does not overlap with existing ones
            bool positionValid;
            do
            {
                positionValid = true;
                shipPosition = new Vector3(rand.Next(0, gameSize - shipSize), rand.Next(0, gameSize - shipSize), rand.Next(0, gameSize - shipSize));

                foreach (var existingPosition in shipPositions)
                {
                    if (Vector3.Distance(shipPosition, existingPosition) < shipSize)
                    {
                        positionValid = false;
                        break;
                    }
                }
            } while (!positionValid);

            shipPositions.Add(shipPosition);
            string jsonShipPosition = JsonConvert.SerializeObject(shipPosition);
            string addShipSql = $"insert into `USRS6N_1`.`Gr6_Vaisseau` ( `partieId`, `typeVaisseau`, `taille`, `position`) VALUES ('{partieID}', 'segment', '{shipSize}', '{jsonShipPosition}')";
            ExecuteSql(addShipSql);
        }
    }


    private MySqlDataReader GetSqlResults(string sql){
        MySqlCommand command = new MySqlCommand(sql, connection);
        return command.ExecuteReader();
    }

    private void ExecuteSql(string sql){
        MySqlCommand command = new MySqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public void JoinGame(int joueurID, int partieID){

        //Check if game exists and is not finished
        Console.WriteLine("Check party available");
        string sql = $"select etat from Gr6_Partie where partieId ={partieID}";
        using (MySqlDataReader result = GetSqlResults(sql)){
            if(result.Read()){
                Console.WriteLine("Party existes ");
            }else{
                // Handle later, party does not exist or is finished
            }
        }

        //VictoryField NANI ?
        string joinSessionSql = $"INSERT INTO `USRS6N_1`.`Gr6_Session` (`partieId`, `joueurId`) VALUES ('{partieID}', '{joueurID}')";
        Console.WriteLine(joinSessionSql);
        ExecuteSql(joinSessionSql);
    }
}