﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace SheepDoom
{
    public class Character1 : NetworkBehaviour
    {
        // Attack/skill related prefabs
        [SerializeField]
        private GameObject normalAtkProjectile, normalSpecial, altSpecial, normalUlti, altUlti;
        private GameObject firedProjectile;

        [SerializeField]
        private Transform spawnPoint, aoespawnPoint;

        //For Alma Casting Special And Ultimate(Enable Cancelling)
        private bool isCasting, isCastingUltimate, isCastingComplete, isCastingUltimateComplete;
        [SerializeField]
        private float castTime, castTimeUltimate;
        private float castTimeInGame = 0, castTimeInGameUltimate = 0;
        [SerializeField] private Vector3 lastPos;
        public float dist;
        private bool whichulti;
        private float timeRemaining = 2f;

        //Animation
        [SerializeField] public NetworkAnimator networkAnimator;

        [Client]
        public void normalAtk()
        {
            CmdNormalAtk();
        }

        [Command]
        void CmdNormalAtk()
        {
            //firedProjectile = Instantiate(normalAtkProjectile, spawnPoint.position, spawnPoint.rotation);
            //firedProjectile.GetComponent<PlayerProjectileSettings>().SetOwnerProjectile(gameObject);

            firedProjectile = Instantiate(normalAtkProjectile, transform);
            firedProjectile.GetComponent<PlayerProjectileSettings>().SetOwnerProjectile(gameObject);
            firedProjectile.transform.SetParent(null, false);
            firedProjectile.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

            NetworkServer.Spawn(firedProjectile, connectionToClient);
        }

        [Client]
        public void SpecialAtk(bool _isAltSpecial)
        {
            if (_isAltSpecial)
            {
                startCasting();
            }
            else if (!_isAltSpecial)
            {
                CmdSpecialAtk(_isAltSpecial);
            }
        }
        [Client]
        void startCasting()
        {
            networkAnimator.SetTrigger("AlmaPlantingMine");
            isCasting = true;
            castTimeInGame = castTime;
            //set transform
            lastPos = transform.position;
        }

        [Command]
        void CmdSpecialAtk(bool _isAltSpecial)
        {
            if (!_isAltSpecial)
            {
                //firedProjectile = Instantiate(normalSpecial, spawnPoint.position, spawnPoint.rotation);
                firedProjectile = Instantiate(normalSpecial, transform);
                firedProjectile.GetComponent<PlayerProjectileSettings>().SetOwnerProjectile(gameObject);
                firedProjectile.transform.SetParent(null, false);
                firedProjectile.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

                NetworkServer.Spawn(firedProjectile, connectionToClient);
            }
            else if (_isAltSpecial)
            {
                //firedProjectile = Instantiate(altSpecial, spawnPoint.position, spawnPoint.rotation);
                firedProjectile = Instantiate(altSpecial, transform);
                firedProjectile.GetComponent<PlayerProjectileSettings>().SetOwnerProjectile(gameObject);
                firedProjectile.transform.SetParent(null, false);
                firedProjectile.transform.SetPositionAndRotation(aoespawnPoint.position, aoespawnPoint.rotation);

                NetworkServer.Spawn(firedProjectile, connectionToClient);
            }
        }

        [Client]
        public void UltiAtk(bool _isAltUlti)
        {
            if (_isAltUlti)
            {
                whichulti = true;
                StartCastingUltimate();
            }
            else if (!_isAltUlti)
            {
                whichulti = false;
                StartCastingUltimate();
            }


        }
        [Client]
        void StartCastingUltimate()
        {
            networkAnimator.SetTrigger("AlmaUltimate");
            timeRemaining = 2;
            isCastingUltimate = true;
            castTimeInGame = castTime;
            //set transform
            lastPos = transform.position;
        }

        [Command]
        void CmdUltiAtk(bool _isAltUlti)
        {
            if (!_isAltUlti)
                //firedProjectile = Instantiate(normalUlti, spawnPoint.position, spawnPoint.rotation);
                firedProjectile = Instantiate(normalUlti, transform);
            else if (_isAltUlti)
                //firedProjectile = Instantiate(altUlti, spawnPoint.position, spawnPoint.rotation);
                firedProjectile = Instantiate(altUlti, transform);

            firedProjectile.GetComponent<PlayerProjectileSettings>().SetOwnerProjectile(gameObject);
            firedProjectile.transform.SetParent(null, false);
            firedProjectile.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

            NetworkServer.Spawn(firedProjectile, connectionToClient);
        }
        private void Update()
        {
            if (isClient && hasAuthority)
            {
                // ======================================== for casting timer calculations for Mines ================================== 
                //start the casting 
                if (isCasting)
                {
                    dist = Vector3.Distance(lastPos, transform.position);
                    Debug.Log("Casting......");
                    castTimeInGame -= Time.deltaTime;
                    Debug.Log("Cast time left: " + castTimeInGame);

                    if (dist > 0.01)
                    {
                        networkAnimator.SetTrigger("CastCancelToRun");
                        Debug.Log("Player Moved, stopping incantation");
                        isCasting = false;

                    }

                }

                //when casting is complete
                if (isCasting && castTimeInGame <= 0)
                {
                    networkAnimator.SetTrigger("CastCancel");
                    Debug.Log("Casting complete");
                    isCastingComplete = true;
                    isCasting = false;

                }


                if (isCastingComplete)
                {
                    Debug.Log("isCastingComplete 2: " + isCastingComplete);
                    CmdSpecialAtk(true);
                    isCastingComplete = false;
                }

                //if interrupted by cc (stun)
                if (isCasting && gameObject.GetComponent<CharacterMovement>().isStopped)
                {
                    networkAnimator.SetTrigger("CastCancel");
                    Debug.Log("Casting failed");
                    isCasting = false;
                }

                //if interrupted by cc (sleep)
                if (isCasting && gameObject.GetComponent<CharacterMovement>().isSleeped)
                {
        //            networkAnimator.SetTrigger("CastCancel");
                    Debug.Log("Casting failed");
                    isCasting = false;
                }
                // ======================================== for casting timer calculations for Ultimate ================================== 
                //start the casting 
                if (isCastingUltimate)
                {
                    dist = Vector3.Distance(lastPos, transform.position);
                    castTimeInGame -= Time.deltaTime;

                    if (dist > 1)
                    {
              //          networkAnimator.SetTrigger("CastCancelToRun");
                        isCastingUltimate = false;

                    }

                }

                //when casting is complete
                if (isCastingUltimate && castTimeInGame <= 0)
                {
                    if (whichulti == true)
                    {
                        CmdUltiAtk(true);
                    }
                    else if (whichulti == false)
                    {
                        CmdUltiAtk(false);
                    }
                    isCastingUltimate = false;
                    isCastingUltimateComplete = true;


                }
                if (isCastingUltimateComplete)
                {
                    dist = Vector3.Distance(lastPos, transform.position);
                    Debug.Log("Time remaining" + timeRemaining);
                    timeRemaining -= Time.deltaTime;
                    if (timeRemaining <= 0)
                    {
              //          networkAnimator.SetTrigger("CastCancel");
                        isCastingUltimateComplete = false;
                    }
                    if (timeRemaining > 0)
                    {
                        if (dist > 1)
                        {
                            Debug.Log("i moved");
                            networkAnimator.SetTrigger("CastCancelToRun");
                            isCastingUltimateComplete = false;

                        }

                    }


                }

                //if interrupted by cc (stun)
                if (isCastingUltimate && gameObject.GetComponent<CharacterMovement>().isStopped)
                {
                    networkAnimator.SetTrigger("CastCancel");
                    isCastingUltimate = false;
                }

                //if interrupted by cc (sleep)
                if (isCastingUltimate && gameObject.GetComponent<CharacterMovement>().isSleeped)
                {
                    networkAnimator.SetTrigger("CastCancel");
                    isCastingUltimate = false;
                }
            }
        }
    }
}
