﻿using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

namespace SheepDoom
{
    public class PlayerObj : NetworkBehaviour
    {
        public static PlayerObj instance; // only use when outside of gameobject, but currently only works on client
        public Client ci;

        [Header("Player profile")]
        [SyncVar(hook = nameof(OnNameUpdate))] private string syncName; 
        [SerializeField]
        [SyncVar] private string matchID;
        [SyncVar(hook = nameof(OnTeamUpdate))] private int teamIndex = 0;
        [SyncVar] private bool isHost = false;
        [SyncVar] private bool isReady = false;
        [SyncVar] private bool isLockedIn = false;
        [SyncVar] public string heroName = string.Empty;

        #region Get
        public string GetPlayerName()
        {
            return syncName;
        }

        public string GetMatchID()
        {
            return matchID;

        }
        public int GetTeamIndex()
        {
            return teamIndex;
        }

        public bool GetIsHost()
        {
            return isHost;
        }

        public bool GetIsReady()
        {
            return isReady;
        }

        public string GetHeroName()
        {
            return heroName;
        }

        public bool GetIsLockedIn()
        {
            return isLockedIn;
        }

        #endregion

        #region Set

        public void SetPlayerName(string name)
        {
            if(hasAuthority)
                CmdSetName(name);
        }

        [Command]
        private void CmdSetName(string name)
        {
            syncName = name;
        }

        public void SetMatchID(string _matchID)
        {
            matchID = _matchID;
        }
       
        public void SetTeamIndex(int _teamIndex)
        {
            teamIndex = _teamIndex;
        }

        public void SetIsHost(bool _isHost)
        {
            isHost = _isHost;
        }

        public void SetIsReady(bool _isReady)
        {
            isReady = _isReady;
        }

        public void SetHeroName(string _heroName)
        {
            heroName = _heroName;
        }

        public void SetIsLockedIn(bool _isLockedIn)
        {
            isLockedIn = _isLockedIn;
        }

        #endregion
        
        private void OnNameUpdate(string prev, string next)
        {
            if (!hasAuthority) return;
            if (gameObject.CompareTag("lobbyPlayer"))
                MainMenu.instance.SetPlayerName(next);
        }

        void OnTeamUpdate(int oldIndex, int newIndex)
        {
            if (!hasAuthority) return;
            CmdSetClientTeamIndex(newIndex);
        }

        [Command]
        void CmdSetClientTeamIndex(int _newIndex)
        {
            ci.GetComponent<SpawnManager>().P_playerTeamIndex = _newIndex;
        }

        private void InitializePlayerObj(Client _ci)
        {
            if(_ci != null)
            {
                instance = _ci.GetComponent<SpawnManager>().GetPlayerObj().GetComponent<PlayerObj>();

                if(instance != null)
                    Debug.Log("PlayerObj initialized!");
                else
                    return;
            }
        }

        [Server]
        public void RemoveFromGame(string _matchID, bool _isExitGame) // adjusted for sudden dc and clean exit
        {
            if (_matchID == matchID)
            {
                Match gameMatch = MatchMaker.instance.GetMatches()[_matchID];
                if(_isExitGame)
                    gameMatch.GetSDSceneManager().UnloadScenes(ci.GetComponent<NetworkIdentity>().connectionToClient, matchID, false, false); // unload on client

                if (gameMatch.GetPlayerObjList().Contains(gameObject))
                    gameMatch.GetPlayerObjList().Remove(gameObject); // remove from match player list

                if (gameMatch.GetHeroesList().Contains(ci.GetComponent<SpawnManager>().GetPlayerObj()))
                {
                    gameMatch.GetHeroesList().Remove(ci.GetComponent<SpawnManager>().GetPlayerObj()); // remove from match hero list
                    if(_isExitGame)
                        NetworkServer.Destroy(ci.GetComponent<SpawnManager>().GetPlayerObj()); // destroy it if it exits cleanly, else dont need to destroy
                }

                // move back to main menu scene on server
                if(_isExitGame) // only need to move if it exits cleanly
                {
                    gameMatch.GetSDSceneManager().MoveToNewScene(ci.gameObject, SceneManager.GetSceneAt(0));
                    gameMatch.GetSDSceneManager().MoveToNewScene(gameObject, SceneManager.GetSceneAt(0));
                }
            }
            else
                Debug.Log("WARNING PLAYER IS IN THE WRONG MATCH!! matchID: " + _matchID + " player matchID: " + matchID);
        }

        [Server]
        private void RemoveFromCharSelect(string _matchID) // only for sudden dc, no exit button for char select scene as theres only 30s
        {
            if(_matchID == matchID)
            {
                Debug.Log("BB");
                Match charSelectMatch = MatchMaker.instance.GetMatches()[_matchID];
                charSelectMatch.GetCharacterSelectUIManager().P_gameStarted = true; // so match wont start

                if (charSelectMatch.GetPlayerObjList().Contains(gameObject))
                    charSelectMatch.GetPlayerObjList().Remove(gameObject);

                foreach (GameObject _player in charSelectMatch.GetPlayerObjList())
                {
                    Debug.Log("CC");
                    charSelectMatch.GetSDSceneManager().UnloadScenes(_player.GetComponent<PlayerObj>().ci.GetComponent<NetworkIdentity>().connectionToClient, _matchID, false, true);
                    charSelectMatch.GetSDSceneManager().MoveToNewScene(_player.GetComponent<PlayerObj>().ci.gameObject, SceneManager.GetSceneAt(0));
                    charSelectMatch.GetCharacterSelectUIManager().PlayerLeftCharSelect(_player); 
                }

                charSelectMatch.GetPlayerObjList().Clear();
                MatchMaker.instance.ClearMatch(_matchID, false, true, false);
            }
        }

        [Server]
        private void RemoveFromLobby(string _matchID) // only for sudden dc so far
        {
            if(_matchID == matchID)
            {
                Match lobbyMatch = MatchMaker.instance.GetMatches()[_matchID];
                // adjust team counts
                if (teamIndex == 1)
                    lobbyMatch.MinusTeam1Count();
                else if (teamIndex == 2)
                    lobbyMatch.MinusTeam2Count();
                // check statuses
                if(isReady) // applicable for non-hosts only
                {
                    isReady = false;
                    lobbyMatch.MinusCountReady();
                }

                if(isHost) // only applicable if player is host
                {
                    lobbyMatch.MinusCountReady(); // need to repeat ready count here, intentional, because host by default already ++ready count
                    isHost = false;
                    // host is always 1st player in the list, so when leave, pass to next player, list re-numbers itself when it changes in size
                    // next player settings
                    GameObject nextHostPlayer = null;
                    if (lobbyMatch.GetPlayerObjList().Count > 1)
                    {
                        nextHostPlayer = lobbyMatch.GetPlayerObjList()[1];

                        if (nextHostPlayer.GetComponent<PlayerObj>().GetIsReady())
                            nextHostPlayer.GetComponent<PlayerObj>().SetIsReady(false);

                        nextHostPlayer.GetComponent<PlayerObj>().SetIsHost(true);
                        lobbyMatch.AddCountReady();
                        lobbyMatch.GetLobbyUIManager().PlayerLeftLobby(nextHostPlayer);
                    }
                    else
                        MatchMaker.instance.ClearMatch(_matchID, true, false, false);
                }

                if (lobbyMatch.GetPlayerObjList().Contains(gameObject)) 
                    lobbyMatch.GetPlayerObjList().Remove(gameObject);
            }
        }

        #region Start & Stop Callbacks

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer() 
        {
            ci = Client.ReturnClientInstance(connectionToClient);
            InitializePlayerObj(ci);
        }

        /// <summary>
        /// Invoked on the server when the object is unspawned
        /// <para>Useful for saving object data in persistent storage</para>
        /// </summary>
        public override void OnStopServer()
        {
            if (MatchMaker.instance.GetMatches().ContainsKey(matchID)) // meaning match is still ongoing when player disconnects
            {
                if (gameObject.scene == MatchMaker.instance.GetMatches()[matchID].GetScenes()[0]) 
                    RemoveFromLobby(matchID);
                else if(gameObject.scene == MatchMaker.instance.GetMatches()[matchID].GetScenes()[1])
                {
                    Debug.Log("AA");
                    RemoveFromCharSelect(matchID);
                }
                else if(gameObject.scene == MatchMaker.instance.GetMatches()[matchID].GetScenes()[2])
                {
                    RemoveFromGame(matchID, false); // sudden disconnect, not clean exit

                    if (MatchMaker.instance.GetMatches()[matchID].GetPlayerObjList().Count == 0 && MatchMaker.instance.GetMatches()[matchID].GetHeroesList().Count == 0)
                        MatchMaker.instance.ClearMatch(matchID, false, false, true);
                }
            }
        }

        /// <summary>
        /// Called on every NetworkBehaviour when it is activated on a client.
        /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
        /// </summary>
        public override void OnStartClient() // this is running twice for some reason...
        {
            if(hasAuthority)
            {
                ci = Client.ReturnClientInstance();
                InitializePlayerObj(ci);
            }
        }

        /// <summary>
        /// This is invoked on clients when the server has caused this object to be destroyed.
        /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
        /// </summary>
        public override void OnStopClient() 
        {

        }

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
        public override void OnStartAuthority() 
        {
            
        }

        /// <summary>
        /// This is invoked on behaviours when authority is removed.
        /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
        /// </summary>
        public override void OnStopAuthority() { }

        #endregion
    }
}