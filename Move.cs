using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{

    bool moveGo;
    bool moveTorF;

    public float speed;//parentの宣言を消したため、parentであった部分は全てtransformに。またResetメソッドも削除。
    float speedCycle;
    Animator animator;
    SoldierHP soldierHP;
    InfantryAttackController infantryAttackController;

    public enum State { Idle, Ready, Move };
    public State CurrentState { get; private set; }

    Vector2 targetPos;


    public void Forcus()
    {
        CurrentState = State.Ready;
    }

    public void FocusOut()
    {
        CurrentState = State.Idle;
        animator.SetTrigger("IdleTrigger");
    }

    public void MoveTo(Vector2 pos)
    {
        if (soldierHP.HP >= 0)
        {
            CurrentState = State.Move;

            animator.SetTrigger("MoveTrigger");
            infantryAttackController.lookDelta = 0;
            infantryAttackController.shootDelta = -0.5f;
            infantryAttackController.gunUpMotion = true;
            
            var pos1 = Camera.main.WorldToScreenPoint(transform.localPosition);
            var rotation = Quaternion.LookRotation(Vector3.forward, Input.mousePosition - pos1);
            transform.localRotation = rotation;

            targetPos = pos;
        }
    }

    void Start() 
    {
        soldierHP = gameObject.GetComponent<SoldierHP>();
        infantryAttackController = gameObject.GetComponent<InfantryAttackController>();
        animator = this.GetComponent<Animator>();
        
        
    }

    void Update()
    {
       

        speedCycle = speed;

        //speedは下限1.5上限3

        if (CurrentState == State.Move && soldierHP.HP >= 1 && infantryAttackController.shootEnd == false)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);            

            var distance = ((Vector2)transform.position - targetPos).sqrMagnitude;
            if (System.Math.Abs(distance) < 0.01f)
            {
                CurrentState = State.Idle;
                //attackController.lookDelta = 0;//Attackメソッドの繰り返しタイミングはここ。         

                if (infantryAttackController.magazine != 0)
                {
                    animator.SetTrigger("IdleTrigger");                           
                }
                else
                {
                    animator.SetTrigger("ReloadTrigger");
                }
            }

            if (speedCycle > 2.6f)
            {
                speedCycle = 2.6f;
            }
            else if (speedCycle < 1.75f)
            {
                speedCycle = 1.75f;
            }
            animator.SetFloat("Speed", speedCycle * 1.1f); 
        }
        
    }
}