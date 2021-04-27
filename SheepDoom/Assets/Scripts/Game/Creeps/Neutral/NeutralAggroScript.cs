﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SheepDoom
{
    public class NeutralAggroScript : MonoBehaviour
    {
        [Header("Current Target")]
        public GameObject Attacker;
        public bool LockedOn;

        [Header("Object stats")]
        public float moveSpd;
        public float moveSpdInGame;
        public float attackRange;

        [Header("Attacking object to be instantiated")]
        public GameObject AuraDamageObject;

        [Header("Attack Cooldown")]
        public float attackCooldown;
        public float attackCooldownInGame;

        [Header("Attack Animation Duration(Stops movement for x time")]
        public bool isAttacking;
        public float attackDuration;
        public float attackDurationInGame;

        // Start is called before the first frame update
        void Start()
        {
            //initialize attacking timers
            attackCooldownInGame = attackCooldown;
            attackDurationInGame = -1;
            isAttacking = false;
            moveSpdInGame = moveSpd;
        }

        // Update is called once per frame
        void Update()
        {
            if (attackCooldownInGame >= 0)
            {
                attackCooldownInGame -= Time.deltaTime;
            }

            //dont move when attacking
            if (attackDurationInGame >= 0 && isAttacking)
            {
                moveSpdInGame = 0;
                attackDurationInGame -= Time.deltaTime;
            }

            //not attacking when timer up
            else
            {
                //start moving again
                moveSpdInGame = moveSpd;
                isAttacking = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //if no target, lock onto a new player in range
            if (!LockedOn)
            {
                if (other.CompareTag("Player"))
                {
                    Debug.Log("Player" + other.gameObject.name + " spotted");
                    Attacker = other.gameObject;
                    LockedOn = true;

                }
            }

        }

        private void OnTriggerStay(Collider other)
        {
            //1st priority: lock on to first player
            if (other.CompareTag("Player") && LockedOn && !other.gameObject.GetComponent<PlayerHealth>().isPlayerDead())
            {
                if (other.gameObject != Attacker.gameObject) return;

                Debug.Log("Moving towards " + Attacker.gameObject.name);

                //chase if not in attack range
                if (Vector3.Distance(Attacker.transform.position, transform.position) > attackRange)
                {
                    transform.LookAt(Attacker.transform);
                    this.gameObject.transform.position += transform.forward * moveSpdInGame * Time.deltaTime;
                }

                //if in attack range
                else
                {
                    //attack if cooldown up
                    if (attackCooldownInGame <= 0)
                    {
                        attackDurationInGame = attackDuration;
                        isAttacking = true;
                        Debug.Log("Death ball attacking");
                        Instantiate(AuraDamageObject, this.gameObject.transform.position, this.gameObject.transform.rotation);
                        //reset attack timer
                        attackCooldownInGame = attackCooldown;
                    }
                }
            }

            //2nd priority: if 1st player out of range, change attacker to any other in range
            else if (!LockedOn)
            {
                if (other.CompareTag("Player") && !other.gameObject.GetComponent<PlayerHealth>().isPlayerDead())
                {
                    LockedOn = true;
                    Attacker = other.gameObject;
                    Debug.Log("Moving towards " + Attacker.gameObject.name);

                    if (Vector3.Distance(Attacker.transform.position, transform.position) > attackRange)
                    {
                        transform.LookAt(Attacker.transform);
                        this.gameObject.transform.position += transform.forward * moveSpdInGame * Time.deltaTime;

                    }

                    //if in attack range
                    else
                    {
                        //attack if cooldown up
                        if (attackCooldownInGame <= 0)
                        {
                            attackDurationInGame = attackDuration;
                            Debug.Log("Death ball attacking");
                            isAttacking = true;
                            Instantiate(AuraDamageObject, this.gameObject.transform.position, this.gameObject.transform.rotation);
                            //reset attack timer
                            attackCooldownInGame = attackCooldown;
                        }
                    }
                }


            }



        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && LockedOn)
            {
                Debug.Log("Stopped following " + Attacker.gameObject.name);
                Attacker = null;
                LockedOn = false;
            }
        }
    }

}



