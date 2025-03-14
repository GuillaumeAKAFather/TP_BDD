
using System.Drawing;
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
    Vector3 backupPosition;

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
        Console.WriteLine("----------------------Bienvenue dans ce jeu----------------------");
        
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
            case 1: //Move 
            HandleMovement();
            break;
            case 2: // Shoot
            HandleShoot();
            break;
        }

    }

    private void HandleShoot(){
        Vector3 desiredShootDirection = new Vector3();

        while (true)
        {
            Console.WriteLine("x : (beetwen -1 an 1)");
            string value = Console.ReadLine();
            if (int.TryParse(value, out int xValue) && xValue >= -1 && xValue <= 1)
            {
                desiredShootDirection.X = xValue;
                break;
            }
        }

        while (true)
        {
            Console.WriteLine("y : (beetwen -1 an 1)");
            string value = Console.ReadLine();
            if (int.TryParse(value, out int yValue) && yValue >= -1 && yValue <= 1)
            {
                desiredShootDirection.Y = yValue;
                break;
            }
        }

        while (true)
        {
            Console.WriteLine("z : (beetwen -1 an 1)");
            string value = Console.ReadLine();
            if (int.TryParse(value, out int zValue) && zValue >= -1 && zValue <= 1)
            {
                desiredShootDirection.Z = zValue;
                break;
            }
        }

        if(desiredShootDirection == Vector3.Zero){
            Console.WriteLine("Invalid shoot direction ");
            ChooseGameAction();
            return;
        }

        
        mongoDbPart.RegisterPlayerShoot(actualPlayerID, actualGameID, desiredShootDirection, backupPosition);

        CheckEnemyHit(backupPosition, desiredShootDirection);
        ChooseGameAction();

    }

    private void CheckEnemyHit(Vector3 fromPosition, Vector3 shootDirection){

        Dictionary<int, MySqlPart.EnemyShipDataStructure> shipInParty = sqlPart.GetEnemyShipDataByGameId(actualGameID);
        Dictionary<int, List<Vector3>> hitablePoint = GenerateHitablePoint(shipInParty);

        Vector3 currentMissilePosition = fromPosition;

        bool cut = false;
        while(true){
            currentMissilePosition += shootDirection;
            //Console.WriteLine("missile Position :"  + currentMissilePosition);

            foreach(var point in hitablePoint){
                
                if(point.Value.Contains(currentMissilePosition)){
                    //Touch a ship
                    Console.WriteLine("Touch a ship at position " + currentMissilePosition);
                    sqlPart.AddPlayerPoint(actualGameID, actualPlayerID);
                    sqlPart.AddDestroyedPart(actualGameID, currentMissilePosition);
                    ChooseGameAction();
                    break;
                }
                
            }

            
            
            if(currentMissilePosition.X < 0 || currentMissilePosition.X >= 5 ||
             currentMissilePosition.Y < 0 || currentMissilePosition.Y >= 5 || 
             currentMissilePosition.Z < 0 || currentMissilePosition.Z >= 5 ||cut){
                //Console.WriteLine("missile LAST Position :"  + currentMissilePosition);
                Console.WriteLine("Missile missed");
                ChooseGameAction();
                return;

            }

        }


    }
    
    private Dictionary<int, List<Vector3>> GenerateHitablePoint(Dictionary<int, MySqlPart.EnemyShipDataStructure> shipInParty){
        Dictionary<int, List<Vector3>> dict = new Dictionary<int, List<Vector3>>();
        
        foreach (var data in shipInParty)
        {
            MySqlPart.EnemyShipDataStructure shipData = data.Value;
            int taille = shipData.taille;
            Vector3 position = shipData.position;
            string typeVaisseau = shipData.typeVaisseau;

            dict[data.Key] = ComputeShipSize(position, taille, typeVaisseau);

            // int key = data.Key;
            //Console.WriteLine($"Ship taille {shipData.taille}, ship position {shipData.position} ");
        }


        return dict;
    }

    private List<Vector3> ComputeShipSize(Vector3 shipPosition, int taille, string typeVaisseau){

        List<Vector3> shipSizePoints = new List<Vector3>();
        //List<Vector3> unavailablePoint = sqlPart.GetUnavailablePoint(actualGameID);
        
        switch(typeVaisseau){
            case "segment":
            shipSizePoints.Add(shipPosition);
            for (int i = 0; i < taille; i++)
            {
                var position = new Vector3(  shipPosition.X + i, shipPosition.Y, shipPosition.Z );
                // if(unavailablePoint.Contains(position))
                //     continue;
                shipSizePoints.Add(position);
            }
            break;
        }

        return shipSizePoints;

    }

    private void HandleMovement()
    {
        Vector3 desiredPosition = new Vector3();

        while (true)
        {
            Console.WriteLine("x :");
            string value = Console.ReadLine();
            if (int.TryParse(value, out int xValue))
            {
                desiredPosition.X = xValue;
                break;
            }
        }

        while (true)
        {
            Console.WriteLine("y :");
            string value = Console.ReadLine();
            if (int.TryParse(value, out int yValue))
            {
                desiredPosition.Y = yValue;
                break;
            }
        }

        while (true)
        {
            Console.WriteLine("z :");
            string value = Console.ReadLine();
            if (int.TryParse(value, out int zValue))
            {
                desiredPosition.Z = zValue;
                break;
            }
        }

        if (!sqlPart.IsOutsideGameBound(actualGameID, desiredPosition))
        {
            //Valid move
            mongoDbPart.RegisterPlayerMovement(actualPlayerID, actualGameID, desiredPosition);
            backupPosition = desiredPosition;
        }
        else
        {
            //Not valid move
            Console.WriteLine("You try to get out of bound");
        }

        ChooseGameAction();
    }
}