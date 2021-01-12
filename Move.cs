using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{

    
    public bool flag;
    public bool standUpToIdle;
    public bool downAnimBool = false;
    public bool downEnd = false;
    public bool shootToDown = false;
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

        flag = true;        
        pos1 = Camera.main.WorldToScreenPoint(transform.localPosition);
        targetPos = pos;
        rotation = Quaternion.LookRotation(Vector3.forward, Input.mousePosition - pos1);

        //歩兵用
        if (infantryAttackController.isReady == false && infantryAttackController.isShooting == false && infantryAttackController.attackMethodName == "AttackMethod")
        {
            Debug.Log("A");
            CurrentState = State.Move;
            MoveMotion();
        }
        else if(infantryAttackController.isReady == true && infantryAttackController.isShooting == false && infantryAttackController.attackMethodName == "AttackMethod")
        {
            Debug.Log("B");
            downAnimBool = true;
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

   



    public void DownMotion()
    {
        if (downAnimBool == true)
        {
            animator.SetTrigger("DownTrigger");
            downEnd = true;
            downAnimBool = false;
        }
        else if(downEnd == true && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            infantryAttackController.isReady = false;
            CurrentState = State.Move;
            MoveMotion();
            downEnd = false;
        }
        else if (downEnd == true && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            infantryAttackController.lookDelta = 0f;
            infantryAttackController.shootDelta = 0f;
        }
    }

    

    void Update()
    {        
        speedCycle = speed;

        //speedは下限1.5上限3

        if (CurrentState == State.Move && infantryAttackController.isShooting == false && downEnd == false && standUpTime == 0)
        {
            if (soldierHP.hp < 1) return;
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);            

            var distance = ((Vector2)transform.position - targetPos).sqrMagnitude;
            if (System.Math.Abs(distance) < 0.01f)
            {
                CurrentState = State.Idle;
                flag = false;
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
        if (CurrentState == State.Ready && flag == true && shootToDown == true)
        {
            Debug.Log("C");
            downAnimBool = true;
            infantryAttackController.isShooting = false;
            shootToDown = false;
        }

        DownMotion();
    }








    public void StandUpToIdle()
    {
        standUpTime += Time.deltaTime;
        if (standUpToIdle == true && CurrentState == State.Move)
        {
            animator.SetTrigger("SatndUpTrigger");
            standUpToIdle = false;
        }

        if (standUpTime > 1.3f)
        {
            MoveMotion();
            standUpTime = 0;
        }

    }
}