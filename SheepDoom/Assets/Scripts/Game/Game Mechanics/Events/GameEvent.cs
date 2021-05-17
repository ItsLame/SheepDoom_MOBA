﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace SheepDoom
{
    public class GameEvent : NetworkBehaviour
    {
        public Text AnnouncerText;

        [SyncVar] public string whoKilled;
        [SyncVar] public string gotKilled;

        [SyncVar] public bool isNeutral = false;
        [SyncVar] public bool isBoss = false;
        [SyncVar] public bool isMinion = false;

        // Start is called before the first frame update
        void Start()
        {
            GameObject gameEventText = FindMe.instance.P_GameEvent;
            AnnouncerText = gameEventText.GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            if (CompareTag("Player") && GetComponent<PlayerHealth>().isPlayerDead())
            {
                if (isServer)
                {
                    if (isNeutral)
                        RpcAnnouncers(gotKilled + " has been slain by a neutral");
                    else if (isBoss)
                        RpcAnnouncers("The mega boss has taken " + gotKilled + "'s life");
                    else if (isMinion)
                        RpcAnnouncers(gotKilled + " has been slain by a minion");
                    else
                        RpcAnnouncers(whoKilled + " killed " + gotKilled);
                }
            }
        }
        [Server]
        public void AnnounceBossDeath(string defeater, float teamID)
        {
            if (teamID == 1)
                RpcAnnouncers("Consortium's: " + defeater + " has slain the mega boss");
            else
                RpcAnnouncers("Coalation's: " + defeater + " has slain the mega boss");
        }

        [ClientRpc]
        void RpcAnnouncers(string announcement)
        {
            AnnouncerText.GetComponent<AnnouncerTextScript>().ResetText(3);
            AnnouncerText.text = announcement;
        }
    }
}

