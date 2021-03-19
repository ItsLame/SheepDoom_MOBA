﻿using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SheepDoom
{
    public delegate void LobbyEvent();
    // The gameobject this script is attached to should only be spawned as a prefab on the server
    public class LobbyManager : NetworkBehaviour
    {
        public event LobbyEvent lobbyEvent;
        public static LobbyManager instance;
        // these will activate on host/join game, not on starting of application
        [Header("MultiScene Setup")]
        [Scene]
        public string lobbyScene;
        [SyncVar] public string matchID = string.Empty;
        //private Transform UIPlayerParentTeam1;
        //private Transform UIPlayerParentTeam2;

        [SyncVar] public int matchIndex = 0;
        [SyncVar] private int team1Count = 0;
        [SyncVar] private int team2Count = 0;

        #region Properties
        
        public int myTeam1Count
        {
            get
            {
                return team1Count;
            }
            set
            {
                team1Count = value;
                if(lobbyEvent != null)
                    lobbyEvent();
            }
        }

        public int myTeam2Count
        {
            get
            {
                return team2Count;
            }
            set
            {
                team2Count = value;
                if(lobbyEvent != null)
                    lobbyEvent();
            }
        }

        #endregion

        void Start()
        {
            
        }

        void Update()
        {
            
        }

        #region Get

        public string GetMatchID()
        {
            return matchID;
        }

        public int GetMatchIndex()
        {
            return matchIndex;
        }

        public int GetTeam1Count()
        {
            return team1Count;
        }

        public int GetTeam2Count()
        {
            return team2Count;
        }

        #endregion

        #region Set

        public void SetMatchID(string _matchID)
        {
            matchID = _matchID;
        }

        public void SetMatchIndex(int _matchIndex)
        {
            matchIndex = _matchIndex;
        }

        public void SetTeam1Count(int _team1Count)
        {
            team1Count = _team1Count;
        }

        public void SetTeam2Count(int _team2Count)
        {
            team2Count = _team2Count;
        }

        #endregion

        public void StartLobbyScene()
        {
            StartCoroutine(ServerLoadSubScenes());
        }

        IEnumerator ServerLoadSubScenes()
        {
            // load lobby scenes
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(lobbyScene, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
                yield return null;
            // before, i used GetSceneAt(i) within a for loop that will loop only ONCE (intentional because I wasn't sure how GetSceneAt works, 
            // the i always resulted in 1 (intentional), so i kept retrieving and adding the same scene
            // now, I matched the GetSceneAt with the number of matches~scenes (a set) generated, so it will retrieve the correct scene
            Scene newLobbyScene = SceneManager.GetSceneAt(MatchMaker.instance.GetMatchCount()); 
            Debug.Log("Loaded scene name: " + newLobbyScene.name);
            // before, matchID correct, but passed in the wrong scene
            // now, matchID correct, passed in the correct scene after resolving the GetSceneAt problem
            MatchMaker.instance.GetLobbyScenes().Add(matchID, newLobbyScene); 
            SceneManager.MoveGameObjectToScene(gameObject, MatchMaker.instance.GetLobbyScenes()[matchID]);
        }

        public void StartGame()
        {

        }

        public void SwitchTeam()
        {

        }
        
        #region Start & Stop Callbacks

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer() 
        {
            instance = this;
        }

  

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