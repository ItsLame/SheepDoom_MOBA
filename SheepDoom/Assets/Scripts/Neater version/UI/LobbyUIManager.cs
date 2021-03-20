﻿using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;
using System.Collections;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

namespace SheepDoom
{
    //set matchid and players in match
    public class LobbyUIManager : NetworkBehaviour
    {
        public static LobbyUIManager instance;
        [Header("Lobby UI Manager Setup")]
        [SyncVar] [SerializeField] private GameObject matchIDText;
        [SyncVar] [SerializeField] private GameObject team1GameObject;
        [SyncVar] [SerializeField] private GameObject team2GameObject;
        [SyncVar] private string matchID = string.Empty;
        [SyncVar] private int matchIndex = 0;
        public SyncList<PlayerObj> playersInLobby = new SyncList<PlayerObj>();
        
        #region Properties
        
        public GameObject P_matchIDText
        {
            get {return matchIDText;}
            set {matchIDText = value;}
        }

        public GameObject P_team1GameObject
        {
            get {return team1GameObject;}
            set {team1GameObject = value;}       
        }

        public GameObject P_team2GameObject
        {
            get {return team2GameObject;}
            set {team2GameObject = value;}
        }

        public string P_matchID
        {
            get {return matchID;}
            set {matchID = value;}
        }

        public int P_matchIndex
        {
            get {return matchIndex;}
            set {matchIndex = value;}
        }

        #endregion

        private void Start()
        {
            if(isServer)
                ServerStartSetting();
            if(isClient)
                ClientStartSetting();
        }

        public void ServerStartSetting()
        {
            //begin setting matchID
            StartCoroutine(SetUI_MatchID());
        }

        public void ClientStartSetting()
        {
            //if matchIDText UI on client's side does not match syncvar's
            while(P_matchIDText.GetComponent<Text>().text != P_matchID)
            {
                P_matchIDText.GetComponent<Text>().text = P_matchID;
            }

            StartCoroutine(SetUI_Team());
        }

        IEnumerator SetUI_MatchID()
        {
            //get matchid from lobby manager first (from server)
            if(P_matchID == string.Empty)
            {
                P_matchID = LobbyManager.instance.GetMatchID();
                P_matchIndex = LobbyManager.instance.GetMatchIndex();

                while(P_matchID == null)
                {
                    //Debug.Log();
                    yield return "matchID still null!";
                }

                Debug.Log("matchID not null anymore!");
            }

            //set matchID to syncvar variable (matchIDText UI)
            P_matchIDText.GetComponent<Text>().text = P_matchID;
        }

        IEnumerator SetUI_Team()
        {
            yield return "firstly, ";

            //to retrieve early birds but not working yet
            //PlayerObj.instance.GetComponent<PlayerLobbyUI>().AddPlayerToList(instance, P_team1GameObject, P_team2GameObject);

            Debug.Log("playerObj added into synclist, now transforming...");

            if(PlayerObj.instance.GetTeamIndex() == 1)
            {
                Debug.Log("updating team1 UI");
                PlayerObj.instance.GetComponent<PlayerLobbyUI>().PlayerSetParent(P_team1GameObject.transform);
            }
            if(PlayerObj.instance.GetTeamIndex() == 2)
            {
                Debug.Log("updating team2 UI");
                PlayerObj.instance.GetComponent<PlayerLobbyUI>().PlayerSetParent(P_team2GameObject.transform);
            }
        }

        #region Start & Stop Callbacks

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer() { }

        /// <summary>
        /// Invoked on the server when the object is unspawned
        /// <para>Useful for saving object data in persistent storage</para>
        /// </summary>
        public override void OnStopServer() { }

        /// <summary>
        /// Called on every NetworkBehaviour when it is activated on a client.
        /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
        /// </summary>
        public override void OnStartClient() { }

        /// <summary>
        /// This is invoked on clients when the server has caused this object to be destroyed.
        /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
        /// </summary>
        public override void OnStopClient() { }

        /// <summary>
        /// Called when the local player object has been set up.
        /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
        /// </summary>
        public override void OnStartLocalPlayer() { }

        /// <summary>
        /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
        /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
        /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included, this will be called on the client that owns the object.</para>
        /// </summary>
        public override void OnStartAuthority() { }

        /// <summary>
        /// This is invoked on behaviours when authority is removed.
        /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
        /// </summary>
        public override void OnStopAuthority() { }

        #endregion
    }
}
