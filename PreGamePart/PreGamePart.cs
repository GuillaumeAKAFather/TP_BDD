
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using MongoDB.Driver;

public class PreGameLoop{

    private MySqlPart sqlPart;
    private MongoDbPart mongoDbPart;
    private int actualPlayerID;
    private int actualGameID;

    public async Task MainLoop(){
        Console.WriteLine("Connecting to server sql...");
        sqlPart = new MySqlPart();
        Console.WriteLine("Connection done !");
        Console.WriteLine("Connecting to MongoDB...");
        mongoDbPart = new MongoDbPart();
        Console.WriteLine("Connection done !");

        await Welcome();

    }   

    private async Task Welcome(){
        Console.WriteLine("----------------------Bienvenue dans ce putain de jeu----------------------");
        
        int result = -1;
        while(true){
            Console.WriteLine("1 - S'inscrire");
            Console.WriteLine("2 - Se connecter");

            string input = Console.ReadLine();
            if(int.TryParse(input, out result) && result <= 2 && result >= 1){
                break;
            }

        }
        switch(result){
            case 1 :
            sqlPart.HandleSubscription();
            Welcome();
            break;
            case 2 :
            ConnectToTheGame();
            break;
        }
    }

    private void ConnectToTheGame(){
        Console.WriteLine("Enter your pseudo :");
        string? result = Console.ReadLine();
        int pseudoID = sqlPart.PseudoID(result);
        
        if(pseudoID>-1){
            //Console.WriteLine("Pseudo exists "+ pseudoID);
            Console.WriteLine("Enter your password : ");
            bool goodPasswordEntered = sqlPart.CheckPassword(Console.ReadLine(), pseudoID);
            if(goodPasswordEntered){
                Console.WriteLine("Connected ! ");
                actualPlayerID = pseudoID;
                SelectGameState();

            }else{
                Console.WriteLine("Password incorrect.");
                Welcome();
            }
        }
        else{
            Console.WriteLine("Pseudo does not exists");
            Welcome();
            
        }

    }

    private void SelectGameState(){
        int result = 0;
        while(true){
            Console.WriteLine("1 - Create game");
            Console.WriteLine("2 - Join game");
            Console.WriteLine("3 - Print joinable game");
            string input = Console.ReadLine();
            if(int.TryParse(input, out result) && result >= 1 && result <= 3){
                break;
            }
        }

        switch(result){
            case 1: //Create game
            sqlPart.CreateGame();
            SelectGameState();
            break;
            case 2:// Join game
            int gameIdChoosed = -1;
            while(true){
                Console.WriteLine("Enter the game id you want to join : ");
                string input = Console.ReadLine();
                if(int.TryParse(input, out gameIdChoosed)){
                    break;
                }
            }
            
            if(sqlPart.JoinGame(actualPlayerID, gameIdChoosed)){
                actualGameID = gameIdChoosed;
                Console.WriteLine("Registering first position");
                mongoDbPart.RegisterPlayerMovement(actualPlayerID, actualGameID, Vector3.Zero);
                ChooseGameAction();
            }
        
            break;
            case 3 : // Print joinable game
            List<int> joinableListIds = sqlPart.JoinablePartieList();
            foreach(int id in joinableListIds){
                Console.WriteLine("Joinable game with id : " + id);
            }
            SelectGameState();
            break;
        }
    }

    private void ChooseGameAction(){
        //Print actual player Position
        Vector3 actualPosition = mongoDbPart.GetPlayerPositionByGameId(actualPlayerID, actualGameID);

        Console.WriteLine("Actual position : " + actualPosition);
        int actionChoosed;
        while(true){
            Console.WriteLine("Choose what to do : ");
            Console.WriteLine("1 - Movement ");
            Console.WriteLine("2 - Shoot ");
            string input = Console.ReadLine();
            if(int.TryParse(input, out actionChoosed) && actionChoosed >= 1 && actionChoosed <= 2){
                break;
            }
        }

        switch(actionChoosed){
            case 1 : //Move 

            Vector3 desiredPosition = new Vector3();
            
            while(true){
                Console.WriteLine("x :");
                string value = Console.ReadLine();
                if(int.TryParse(value, out int xValue)){
                    desiredPosition.X = xValue;
                    break;
                }
            }

            while(true){
                Console.WriteLine("y :");
                string value = Console.ReadLine();
                if(int.TryParse(value, out int yValue)){
                    desiredPosition.Y = yValue;
                    break;
                }
            }

            while(true){
                Console.WriteLine("z :");
                string value = Console.ReadLine();
                if(int.TryParse(value, out int zValue)){
                    desiredPosition.Z = zValue;
                    break;
                }
            }

            if(!sqlPart.IsOutsideGameBound(actualGameID, desiredPosition)){
                //Valid move
                mongoDbPart.RegisterPlayerMovement(actualPlayerID, actualGameID, desiredPosition);
            }else{
                //Not valid move
                Console.WriteLine("You try to get out of bound");
            }
            
            ChooseGameAction();

            break;
            case 2: // Shoot
            break;
        }

    }

}