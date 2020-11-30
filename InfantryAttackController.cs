using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;//配列で使用する。


public class InfantryAttackController : AttackController
{
    
    float time;
    

    // Start is called before the first frame update
    void Start()
    {
        attackOrder = true;
        this.animator = GetComponent<Animator>();
        this.aud = GetComponent<AudioSource>();

        soldierMove = gameObject.GetComponent<Move>();
        soldierHP = gameObject.GetComponent<SoldierHP>();
        move = gameObject.GetComponent<Move>();

        DefaultState();
        shootSpan = GetRandomTime();
        nearEnemy = null;
    }

    // Update is called once per frame
    public void Update()
    {
        if (attackOrder == true)
        {
            AttackMethod();
            ReloadMethod();

        }
        else if (attackOrder == false)
        {
            
            time += Time.deltaTime;
            
                if (idleMotion == true)
                {
                    animator.SetTrigger("IdleTrigger");
                    idleMotion = false;
                }
                lookDelta = 0f;
                shootDelta = 0f;
                gunUpMotion = true;
                nearEnemy = null;
            
            if (time > 0.01f)
            {
                
                
                time = 0;
                attackOrder = true;
                
            }
        }
        WeaponSoundController();
    }




    




    
}
