﻿using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace EnemyAI
{
    public class AIController : MonoBehaviourPunCallbacks,IDamageable
    {
        public NavMeshAgent agent;
        public GameObject[] walkpoints;
        private int nbWalkpoints = 4;

        public GameObject[] enemies;
        public string enemyTeam;
        
        public const float maxHealth = 100f;
        public float currentHealth = maxHealth;

        public GameObject lastShooter;
        
        private bool alreadyAttacked;
        private int framesUntilAttack = 60;

        private bool canRespawn = true;

        public float respawnTime = 5;
        
        // Weapons
        [SerializeField]  Item[] items;
        PhotonView Phv;
        Renderer[] visuals;
        public int team;
        int itemIndex;
        int previousItemIndex = -1;

        private SingleShotAI SingleShotAI;
        
        void Awake()
        {
            walkpoints = GameObject.FindGameObjectsWithTag("CHECKPOINT");
            enemies = GameObject.FindGameObjectsWithTag(enemyTeam);
            agent = GetComponent<NavMeshAgent>();
            Phv = GetComponent<PhotonView>();
        }
            
        void Start()
        {
            if (Phv.IsMine)
            {
                EquipItem(0);
                SingleShotAI = (SingleShotAI) items[0];
                SingleShotAI = (SingleShotAI) items[0];
            }
            NextWalkPoint();
            visuals = GetComponentsInChildren<Renderer>();
        }
        
        void Update()
        {
            Vector3 DistanceTillCheckpoint = transform.position - agent.destination;
            if (DistanceTillCheckpoint.magnitude < 3f)
            {
                NextWalkPoint();
            }

            (bool InSight, GameObject target) = GetTarget();
            if (InSight)
            {
                Attack(target);
            }
        }

        (bool, GameObject) GetTarget()
        {
            float distanceToNearest = 5f;
            GameObject Nearest = null;
            foreach (var player in enemies)
            {
                Vector3 distance = player.transform.position - agent.destination;
                if (distance.magnitude < distanceToNearest)
                {
                    Nearest = player;
                    distanceToNearest = distance.magnitude;
                }
            }

            return (Nearest != null, Nearest);
        }

        void NextWalkPoint()
        {
            int i = Random.Range(0, nbWalkpoints);
            agent.SetDestination(walkpoints[i].transform.position);
            
            // RaycastHit rch;
            // if (Physics.Raycast(walkpoint.position, walkpoint.up, out rch, 100))
            // {
            //     agent.SetDestination(rch.transform.position);
            // }
        }
        
        /// <summary>
        /// Attack
        ///
        /// </summary>
        /// <param name="enemy"></param>
        /// 
        void Attack(GameObject enemy)
        {
            if (alreadyAttacked)
            {
                framesUntilAttack--;
            }
            else
            {
                transform.LookAt(enemy.transform);
                items[0].Use();
            }
        }
        
        void EquipItem(int _index)
        {
            if (_index == previousItemIndex)
                return;
        
            itemIndex = _index;

            items[itemIndex].itemGameObject.SetActive(true);

            if (previousItemIndex != -1)
            {
                items[previousItemIndex].itemGameObject.SetActive(false);
            }

            previousItemIndex = itemIndex;
        
            if (Phv.IsMine)
            {
                Hashtable hash = new Hashtable {{"itemindex", itemIndex}};
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            }
        }
        
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (!Phv.IsMine && targetPlayer == Phv.Owner)
            {
                EquipItem(0);
            }
        }

        
        void SetRenderers(bool state)
        {
            foreach (var renderer in visuals)
            {
                renderer.enabled = state;
            }
        }
        
        IEnumerator Respawn(float respawnWaitTime)
        {
            canRespawn = false;
            SetRenderers(false);
            currentHealth = 100;
            PlayerManager.scores[(team+1)%2] += 1;
            //Debug.Log((team+1)%2);
            PlayerManager.UpdateScores();
            GetComponent<AIController>().enabled = false;
            Transform spawn = SpawnManager.instance.GetTeamSpawn(team);
            transform.position = spawn.position;
            transform.rotation = spawn.rotation;
            GetComponent<AIController>().enabled = true;
            
            SendChatMessage("System",
                lastShooter.name +" killed " + name);
            
            yield return new WaitForSeconds(respawnWaitTime);     
            
            
            SetRenderers(true);
            canRespawn = true;
        }

        public void TakeDamage(float damage)
        {
            Phv.RPC("RPC_TakeDamage", RpcTarget.All,damage);
        }

        private void SendChatMessage(string sender, string message)
        {
            Phv.RPC("SendChat",RpcTarget.All,sender,message);
        }
        
        [PunRPC]
        void RPC_TakeDamage(float damage)
        {
            if (!Phv.IsMine)
                return;

            currentHealth -= damage;

            if (currentHealth <= 0 && canRespawn)
            {
                StartCoroutine(Respawn(respawnTime));
            }
        }
        
        [PunRPC]
        void SendChat(string sender, string message)
        {
            ChatMessage m = new ChatMessage(sender,message);

            GameManager.chatMessages.Insert(0, m);
            if(GameManager.chatMessages.Count > 8)
            {
                GameManager.chatMessages.RemoveAt(GameManager.chatMessages.Count - 1);
            }

            Chat.chatMessages = GameManager.chatMessages;
            // responsible for the synchronisation of all messages
        }
    }
}
