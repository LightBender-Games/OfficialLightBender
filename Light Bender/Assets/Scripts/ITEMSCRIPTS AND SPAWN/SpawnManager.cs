using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
   public static SpawnManager Instance;
   
   GameObject[] redTeamSpawns;
   GameObject[] blueTeamSpawns;

   void Awake()
   {
      Instance = this;
      redTeamSpawns = GameObject.FindGameObjectsWithTag("RedSpawn");
      blueTeamSpawns = GameObject.FindGameObjectsWithTag("BlueSpawn");
   }
   public Transform GetSpawnpoint(int team)
   {
      return team == 0 ? GetBlueSpawnpoint() : GetRedSpawnpoint();
   }
   public Transform GetRedSpawnpoint()
   {
      return redTeamSpawns[Random.Range(0, redTeamSpawns.Length)].transform;
   }
   public Transform GetBlueSpawnpoint()
   {
      return blueTeamSpawns[Random.Range(0, blueTeamSpawns.Length)].transform;
   }
   
   
}
