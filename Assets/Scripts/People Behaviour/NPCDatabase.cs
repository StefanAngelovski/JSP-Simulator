using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCDatabase", menuName = "ScriptableObjects/NPCDatabase", order = 1)]
public class NPCDatabase : ScriptableObject
{
    public List<GameObject> npcPrefabs;
}