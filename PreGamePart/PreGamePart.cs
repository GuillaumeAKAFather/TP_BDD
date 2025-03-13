
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;

public class PreGameLoop{

    private MySqlPart sqlPart;
    private int actualPlayerID;

    public void MainLoop(){
        Console.WriteLine("Connecting to server ...");
        sqlPart = new MySqlPart();
        Console.WriteLine("Connection done !");

        Welcome();

        // Console.ForegroundColor = ConsoleColor.Yellow;
        // Console.WriteLine("------------- Bienvenue dans le jeu -------------");
        // Console.ForegroundColor = ConsoleColor.White;
        // Console.WriteLine("1 - Se connecter");
        // Console.WriteLine("2 - S'inscrire");

        // var result = int.Parse(Console.ReadLine());

        // if(result == 1){
        //     sqlPart.HandleSubscription();
        // }


    }   

    private void Welcome(){
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

            while(true){
            Console.WriteLine("Enter the game id you want to join : ");
            string input = Console.ReadLine();
            if(int.TryParse(input, out int gameIdShoo)){
                break;
            }

            
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

}