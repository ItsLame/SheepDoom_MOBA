﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SheepDoom
{
    public class BaseCreepHealthBAR : HealthBar
    {
        [SerializeField] private Image foregroundimage;
        [SerializeField] private float updatespeedseconds = 0.5f;

        protected override void Awake()
        {
            base.Awake();
            if(gameObject.CompareTag("TeamCoalitionRangeCreep") || gameObject.CompareTag("TeamConsortiumRangeCreep"))
                GetComponentInParent<LeftMinionBehaviour>().OnHealthPctChanged += HandleHealthChanged;
            else if(gameObject.CompareTag("MegaBoss"))
                GetComponentInParent<MegaBossNewScript>().OnHealthPctChanged += HandleHealthChanged;
        }

        protected override void InitHealthBar()
        {
            P_foregroundImage = foregroundimage;
            P_updateSpeedSeconds = updatespeedseconds;
        }
    }
}

