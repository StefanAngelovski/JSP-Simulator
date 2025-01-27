using System.Collections.Generic;  

public static class SharedGameData
{
    public static int BusCount { get; set; } = 3; 

    // List of municipalities
    // Default list with 3 random municipalities
    public static List<string> Municipalities { get; set; } = new List<string>
    {
        "Karposh",
        "Aerodrom",
        "Kisela Voda",
        "Bitpazar",
        "Butel",
        "Chair",
        "Gjorce Petrov",
        "Shuto Orizari",
        "Saraj"
    };

    public static readonly Dictionary<int, List<string>> BusRoutes = new Dictionary<int, List<string>>
    {
        // Bus 2
        {2, new List<string> {"Centar", "Karposh", "Taftalidze", "Deksion"} },
        
        // Bus 5
        {5, new List<string> {"Centar", "Aerodrom", "Novo Lisice", "Lisice"} },
        
        // Bus 22
        {22, new List<string> {"Centar", "Karposh", "Taftalidze", "Gjorche Petrov", "Deksion"} },
        
        // Bus 24
        {24, new List<string> {"Centar", "Karposh", "Taftalidze", "Deksion"} },
        
        // Bus 57
        {57, new List<string> {"Centar", "Bitpazar", "Chair", "Butel"} },
    };

    // Currently active bus route
    public static int CurrentBusNumber { get; set; }
    public static List<string> CurrentRoute { get; set; }
    public static int CurrentStopIndex { get; set; } = 0;

    // Initialize a random bus route
    public static void SelectRandomBusRoute()
    {
        var busNumbers = new List<int>(BusRoutes.Keys);
        CurrentBusNumber = busNumbers[UnityEngine.Random.Range(0, busNumbers.Count)];
        CurrentRoute = BusRoutes[CurrentBusNumber];
        CurrentStopIndex = 0;
    }
}
