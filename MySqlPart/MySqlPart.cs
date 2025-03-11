using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

public class MySqlPart{

    MySqlConnection connection;
    MySqlCommand command;
    MySqlDataReader reader;


    public MySqlPart(){
        string connectString = "server=localhost;uid=root;pwd=qkvdig;database=tb_bdd";
        connection = new MySqlConnection();
        connection.ConnectionString = connectString;
        connection.Open();
    }


    public void HandleSubscription(){
        string player_name = string.Empty;
        string player_age = string.Empty;
        string player_adress = string.Empty;

        while(player_name == string.Empty){
            Console.WriteLine("Enter your name");
            player_name = Console.ReadLine();
        }

        while(player_age == string.Empty){
            Console.WriteLine("Enter your age");
            player_age = Console.ReadLine();

        }

        while(player_adress == string.Empty){
            Console.WriteLine("Enter your adress");
            player_adress = Console.ReadLine();
        }

        string sql = $"INSERT INTO tb_bdd.player_data ( player_name, player_age, player_adress) VALUES ('{player_name}', {player_age}, '{player_adress}');";

        command = new MySqlCommand(sql, connection);
        command.ExecuteReader();
    }

    public void CreateParty(){
        string sql = $"INSERT INTO tb_bdd.game_data (game_size) VALUES (7);";
        command = new MySqlCommand(sql, connection);
       
        
    }

}