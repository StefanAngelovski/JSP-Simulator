using UnityEngine;
using System.Collections.Generic;  // Add this line

public static class SharedGameData
{
    public static int BusCount { get; set; } = 3; // Default value is 3

    // List of municipalities
    // Default list with 3 random municipalities
    public static List<string> Municipalities { get; set; } = new List<string>
    {
        "Karposh",
        "Aerodrom",
        "Kisela Voda"
    };
}
