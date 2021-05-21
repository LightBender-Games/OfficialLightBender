﻿using System;
using System.Collections.Generic;
using System.Transactions;
using TMPro;
using UnityEngine;
using Random = System.Random;

namespace Zones
{
    public class ZoneCapture : MonoBehaviour
    {
        [SerializeField] private int controlled = -1; // 0 for blue, 1 for red, -1 for nobody
        private List<PlayerController> playersNear = new List<PlayerController>();
        private List<PlayerController> bluePlayers = new List<PlayerController>();
        private List<PlayerController> redPlayers = new List<PlayerController>();
        private List<PlayerController>[] playersTeam = new List<PlayerController>[2];

        [SerializeField] private double maxValue = 3000;
        private double maxValueOre = 1500;
        private double[] blueTimers = new double[2];
        private double[] redTimers = new double[2]; 
        private double[][] Timers = new double[2][];

        public float Radius = 5;
        public bool playerNear;




        // Update is called once per frame
        void Update()
        {
            if (this == null)
            {
                Debug.LogError("Cannot update time on a timer with no zones assigned !");
            }

            CheckIfPlayersInZone();
            CheckIfPlayersLeave();
            
            if (playerNear)
            {
                (int TeamTryingControl, int playersTryControl) = GetTeamAndPlayersTryControl(bluePlayers, redPlayers);
                Debug.Log("number of playersTryControl: "+playersTryControl);
                if (playersTryControl != 0)
                { // if there are the same number of blue and red players, then do nothing
                    
                    double factor = 1 + 0.5 * (playersTryControl - 1);
                    double timeValue = (Time.deltaTime * 100 * factor);
                    // 1x --> 1 player, 1.5x --> 2 players, 2x --> 3 players and so on
                    if (TeamTryingControl == controlled)
                    {
                        if (Timers[TeamTryingControl][0] == 0)
                        { // timer Capture at 0, then timer Ore
                            if (Timers[TeamTryingControl][1] == 0)
                            { // timer Ore at 0
                                int randInt = GameManager.rand.Next(playersTeam[TeamTryingControl].Count);
                                (playersTeam[TeamTryingControl])[randInt].AddOre(1);
                                int ores = playersTeam[TeamTryingControl][randInt].GetOresHolded();
                                if (ores == 1)
                                {
                                    Debug.Log((playersTeam[TeamTryingControl])[randInt].name + " has got an ore.");
                                }
                                else
                                {
                                    Debug.Log((playersTeam[TeamTryingControl])[randInt].name + " has got "+ores+" ores.");
                                }
                                // should add a visual way to see that the player holds the ore
                                Timers[TeamTryingControl][1] += maxValueOre;
                                // resets the timer Ore
                            }
                            else
                            {
                                Timers[TeamTryingControl][1] = DecreaseTimer(Timers[TeamTryingControl][1],
                                    timeValue);
                            }
                        }
                        else
                        { // first fully bring timer Capture to 0
                            Timers[TeamTryingControl][0] = DecreaseTimer(Timers[TeamTryingControl][0],
                                timeValue);
                            //Timers[TeamTryingControl][0] -= (int)(Time.deltaTime * 100 * factor);
                        }
                    }
                    else
                    { // 2 possibilities :
                      // zone is neutral or
                      // zone is controlled by enemy
                        if ((TeamTryingControl+1)%2 == controlled)
                        { // zone controlled by enemy
                            int teamEnemy = (TeamTryingControl + 1) % 2;
                            if (Timers[teamEnemy][0] < maxValue)
                            { // zone to recapture
                                Timers[teamEnemy][0] = IncreaseTimer(Timers[teamEnemy][0],
                                        timeValue,maxValue);
                            }
                            else
                            { // Timers[teamEnemy][0] == maxValue , so reset neutral
                                Timers[0][1] = maxValueOre;
                                Timers[1][1] = maxValueOre; 
                                // reset ore timers

                                Timers[0][0] = maxValue;
                                Timers[1][0] = maxValue; 
                                // reset capture timers, just to be sure. 

                                controlled = -1;
                            }
                        }
                        else
                        { // zone is neutral
                            //Debug.Log("Test step 0 : timer capture of own team "+Timers[TeamTryingControl][0]);
                            if (Timers[TeamTryingControl][0] == 0)
                            { // zone was neutral and will be controlled by capturing team
                                controlled = TeamTryingControl;
                                
                                int randInt = GameManager.rand.Next(playersTeam[TeamTryingControl].Count);
                                (playersTeam[TeamTryingControl])[randInt].AddOre(1);
                                Debug.Log((playersTeam[TeamTryingControl])[randInt].name + " has got an ore.");
                            }
                            else
                            {
                                //Debug.Log("Test step 1 : timer capture of enemy "+Timers[(TeamTryingControl+1)%2][0]);
                                if (Timers[(TeamTryingControl+1)%2][0] < maxValue)
                                {
                                    Timers[(TeamTryingControl + 1) % 2][0] = IncreaseTimer(
                                        Timers[(TeamTryingControl + 1) % 2][0],
                                        timeValue, maxValue);
                                }
                                else
                                { // timer capture of enemy team is at max, so decrease own timer capture
                                    Timers[TeamTryingControl][0] = DecreaseTimer(Timers[TeamTryingControl][0],
                                        timeValue);
                                }
                            }
                        }
                        
                    }
                }

                /*if (timeValue > 0)
                {
                    timeValue -= Time.deltaTime * 100 * factor;
                }
                else
                {
                    int TeamWhoCaptured = GetTeamMaxPlayers();
                    controlled = TeamWhoCaptured;
                    int randInt = new Random().Next(playersTeam[TeamWhoCaptured].Count);
                    (playersTeam[TeamWhoCaptured])[randInt].SetHasOre(true);
                    Debug.Log((playersTeam[TeamWhoCaptured])[randInt].name + " has got an ore.");
                    // should add a visual way to see that the player holds the ore
                    timeValue += maxValue;
                    // should not reset but rather tend to neutral
                }*/
                // timeValue is in centiseconds

                
            }
            else
            {
                double factor = 1.5;
                // recharges 1.5x as fast
                
                // recharge timers
                if (Timers[0][0] < maxValue)
                {
                    Timers[0][0] = IncreaseTimer(Timers[0][0],
                        (Time.deltaTime * 100 * factor), maxValue);
                }
                else if (Timers[1][0] < maxValue)
                {
                    Timers[1][0] = IncreaseTimer(Timers[1][0],
                        (Time.deltaTime * 100 * factor), maxValue);
                }
            }

            DisplayTimeForPlayers(bluePlayers,redPlayers);
            // this will be difficult. We need to show the timer which has been updated, however, 
            // if a red-player enters a blue-controlled zone with a blue player in it, the blue player should still see the ore timer freezed, while
            // the red player should see the capture timer freezed.
            
            // A player of a certain team can only see the timers of its team, except when recapturing.
            
            
            
        }

        public void DisplayTimeForPlayers(List<PlayerController> bluePlayers,List<PlayerController> redPlayers)
        {
            /*
             for each team, 3 possiblities :
             - timer capture of said team is 0, meaning we show the timer ore of said team
             - timer capture of said team is greater than 0, meaning we show the timer capture
             - timer capture of said team is max value, in which case, we show the timer capture of the other team, which is either max value or lower.
             */
            double blueShow = GetTeamShow(0);
            double redShow = GetTeamShow(1);
            
            DisplayTimeForTeam(bluePlayers,blueShow);
            DisplayTimeForTeam(redPlayers,redShow);

        }

        double GetTeamShow(int team)
        {
            if (team > 2 || team < 0)
            {
                throw new ArgumentException("GetTeamShow: cannot get show of team lower than 0 or greater than 2!");
            }
            /*
             for each team, 3 possiblities :
             - timer capture of said team is 0, meaning we show the timer ore of said team
             - timer capture of said team is greater than 0, meaning we show the timer capture
             - timer capture of said team is max value, in which case, we show the timer capture of the other team, which is either max value or lower.
             */

            if (Timers[team][0] == 0)
            {
                return Timers[team][1]; 
                // timer ore of team
            }

            if (Timers[team][0] == maxValue)
            {
                return Timers[(team + 1) % 2][0]; 
                // timer capture of other team
            }

            return Timers[team][0];
            // timer capture of team
        }

        void DisplayTimeForTeam(List<PlayerController> PlayersOfTeam, double time)
        {
            double seconds = Mathf.FloorToInt((float) (time / 100));
            double centiseconds = Mathf.FloorToInt((float) (time % 100));
            
            for (int i = 0; i < PlayersOfTeam.Count; i++)
            {
                PlayersOfTeam[i].transform.Find("Canvas").GetComponentsInChildren<TextMeshProUGUI>()[0].text =
                    string.Format("{0:00}:{1:00}", seconds, centiseconds);
            }
        }


        double DecreaseTimer(double timer, double decreaseValue)
        {
            if (timer-decreaseValue < 0)
            {
                return 0;
            }

            return timer - decreaseValue;
        }

        double IncreaseTimer(double timer, double increaseValue, double maxTimerValue)
        {
            if (timer + increaseValue > maxTimerValue)
            {
                return maxTimerValue;
            }

            return timer + increaseValue;
        }

        void CheckIfPlayersInZone()
        {
            for (int i = 0; i < PlayerManager.players.Count; i++)
            {
                float dist = Vector3.Distance(PlayerManager.players[i].transform.position, this.transform.position);
                if (dist < Radius)
                {
                    AddPlayerNear(PlayerManager.players[i]);
                }
            }
        }

        void CheckIfPlayersLeave()
        {
            for (int i = 0; i < playersNear.Count; i++)
            {
                float dist = Vector3.Distance(playersNear[i].transform.position, this.transform.position);
                if (dist > Radius)
                {
                    RemovePlayerNear(playersNear[i]);
                }
            }
        }

        
        public void AddPlayerNear(PlayerController player)
        {
            //if (player.gameObject.CompareTag("Player"))
            if (!playersNear.Contains(player))
            {
                Debug.Log("Successfully added a new player near");
                player.transform.Find("Canvas").transform.Find("Timer").GetComponent<TextMeshProUGUI>().gameObject.SetActive(true);
                playersNear.Add(player);
                playersTeam[player.GetTeam()].Add(player);
                // add to bluePlayers or redPlayers
                
                DisplayTimeForPlayers(bluePlayers,redPlayers);
                // show timer
            }

            SetPlayerNear(true);
        }

        public void RemovePlayerNear(PlayerController player)
        {
            //if (player.gameObject.CompareTag("Player"))
            TextMeshProUGUI timer = player.transform.Find("Canvas").transform.Find("Timer").GetComponent<TextMeshProUGUI>();
            timer.text = "";
            timer.gameObject.SetActive(false);
            // hide timer
            playersNear.Remove(player);
            playersTeam[player.GetTeam()].Remove(player);
            if (playersNear.Count == 0)
            {
                SetPlayerNear(false);
            }
        }

        public void SetPlayerNear(bool playerNear)
        {
            this.playerNear = playerNear;
        }


        private (int team, int playersIncrementTimer) GetTeamAndPlayersTryControl(List<PlayerController> bluePlayers,
            List<PlayerController> redPlayers)
        {
            Debug.Log("GetTeamAndPlayersTryControl ");
            Debug.Log("redPlayersCount: " +redPlayers.Count);
            Debug.Log("bluePlayersCount: "+bluePlayers.Count);
            int team;
            int playersTimer;
            if (redPlayers.Count > bluePlayers.Count)
            {
                team = 1; // red;
                playersTimer = redPlayers.Count - bluePlayers.Count;
            }
            else
            {
                team = 0; // blue
                playersTimer = bluePlayers.Count - redPlayers.Count;
            }
            // if bluePlayers.count == redPlayers.count, then no matter the team, 
            // playersIncrementTimer will be 0, so no change in timer, 
            // and that is what we want.

            return (team, playersTimer);
        }

        private void Awake()
        {
            playersTeam[0] = bluePlayers;
            playersTeam[1] = redPlayers;

            blueTimers[0] = maxValue;
            blueTimers[1] = maxValueOre;
            redTimers[0] = maxValue;
            redTimers[1] = maxValueOre;
            
            Timers[0] = blueTimers;
            Timers[1] = redTimers;
        }
    }
}
    

