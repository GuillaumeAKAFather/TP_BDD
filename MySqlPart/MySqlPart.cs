using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

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
        string sql = "insert into Gr6_Partie (dimensionCube, duree, etat, jeuId) values (3,60,'en cours',3)";
        using (MySqlDataReader result = GetSqlResults(sql)){
            Console.WriteLine("Game created");
        }
        
    }

    public void JoinGame(int gameIdToJoin){
        
    }

    private MySqlDataReader GetSqlResults(string sql){
        MySqlCommand command = new MySqlCommand(sql, connection);
        return command.ExecuteReader();
    }

    private void ExecuteSql(string sql){
        MySqlCommand command = new MySqlCommand(sql, connection);
        command.ExecuteReader();
    }


}