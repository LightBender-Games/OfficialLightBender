
using System.IO;
using Photon.Pun;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
     PhotonView Phv;

     GameObject controller;

     int team;

    void Awake()
    {
        Phv = GetComponent<PhotonView>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (Phv.IsMine)
        {
            CreateController();
        }
    }

    void CreateController() // cree l objet joueur
    {
        Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint(team);
        Debug.Log($"Team number {team} is being instantiated");
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"),spawnpoint.position,spawnpoint.rotation,0,new object[] { Phv.ViewID });
    }

    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        // might add a little delay before respawn
        CreateController();
    }

    public void SetTeam(int Team)
    {
        if (Team == 0 || Team == 1)
        {
            team = Team;
        }
        else
        {
            Debug.Log("PLayerManager: Team number is not valid");
        }
        
    }
    
}
