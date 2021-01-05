using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{

    
    public bool shootEndFlag;
    public bool standUpToIdle;
    public float shootEndTime;
    public float standUpTime;
    Vector3 pos1;
    Quaternion rotation;

    public float speed;//parentの宣言を消したため、parentであった部分は全てtransformに。またResetメソッドも削除。
    float speedCycle;
    Animator animator;
    SoldierHP soldierHP;
    InfantryAttackController infantryAttackController;

    public enum State { Idle, Ready, Move };
    public State CurrentState { get; private set; }

    Vector2 targetPos;



    void Start()
    {
        soldierHP = gameObject.GetComponent<SoldierHP>();
        infantryAttackController = gameObject.GetComponent<InfantryAttackController>();
        animator = this.GetComponent<Animator>();
        speed = 2f;
    }


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
        if (soldierHP.hp < 1) return;

        shootEndFlag = true;
        pos1 = Camera.main.WorldToScreenPoint(transform.localPosition);
        targetPos = pos;
        rotation = Quaternion.LookRotation(Vector3.forward, Input.mousePosition - pos1);

        if (infantryAttackController.isShooting == false && infantryAttackController.attackMethodName == "AttackMethod")
        {
            CurrentState = State.Move;
            MoveMotion();
        }
        if (infantryAttackController.attackMethodName == "SniperAttackMethod" && infantryAttackController.lieDownMotion == false)
        {
            StandUpToIdle();
        }
        
                    
    }

    public void MoveMotion()
    {
        
        animator.SetTrigger("MoveTrigger");
        infantryAttackController.lookDelta = 0f;
        infantryAttackController.shootDelta = 0f;
        infantryAttackController.upMotion = true;        
        infantryAttackController.lieDownMotion = true;
        infantryAttackController.outOfRange = false;//これがないと射程外に自分から移動して出て行った後に一瞬gunUpMotionが出て銃を上げた後idleになる
        transform.localRotation = rotation;
    }

    public void StandUpToIdle()
    {        
        standUpTime += Time.deltaTime;
        if (standUpToIdle == true && CurrentState == State.Move)
        {
            animator.SetTrigger("SatndUpTrigger");
            standUpToIdle = false;
        }

        if(standUpTime > 1.3f)
        {
            MoveMotion();
            standUpTime = 0;
        }            
        
    }

    

    void Update()
    {

        Debug.Log(CurrentState);
        speedCycle = speed;

        //speedは下限1.5上限3

        if (CurrentState == State.Move && soldierHP.hp >= 1 && infantryAttackController.isShooting == false && standUpTime == 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);            

            var distance = ((Vector2)transform.position - targetPos).sqrMagnitude;
            if (System.Math.Abs(distance) < 0.01f)
            {
                CurrentState = State.Idle;
                shootEndFlag = false;
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

            if (speedCycle > 2.8f)
            {
                speedCycle = 2.8f;
            }
            else if (speedCycle < 2.2f)
            {
                speedCycle = 2.2f;
            }
            animator.SetFloat("Speed", speedCycle * 0.9f); 
        }



        //p45(shoot == true)だったときの分岐処理。InfantryAttackControllerの射撃ラグを測る処理内にshootToMoveをtrueにするトリガーを設置。このことによりコッキングが終わった後に移動アニメーションが作動する。
        if(CurrentState == State.Ready && shootEndFlag == true && infantryAttackController.isShooting == false)
        {
            CurrentState = State.Move;
            MoveMotion();
        }
                
    }
}