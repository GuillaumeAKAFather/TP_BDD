
using Microsoft.VisualBasic;

public class PreGameLoop{

    private MySqlPart sqlPart;

    public void MainLoop(){

        sqlPart = new MySqlPart();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("------------- Bienvenue dans le jeu -------------");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("1 - Se connecter");
        Console.WriteLine("2 - S'inscrire");

        var result = int.Parse(Console.ReadLine());

        if(result == 1){
            sqlPart.HandleSubscription();
        }


    }   



}