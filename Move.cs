using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{

    bool moveGo;
    bool moveTorF;
    bool shootEndFlag;
    public bool shootToMove;
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
        if (soldierHP.HP < 1) return;                
        CurrentState = State.Move;
        pos1 = Camera.main.WorldToScreenPoint(transform.localPosition);
        targetPos = pos;
        rotation = Quaternion.LookRotation(Vector3.forward, Input.mousePosition - pos1);

        if (infantryAttackController.shootEnd == false && infantryAttackController.attackMethodName == "AttackMethod")
        {
            MoveMotion();
        }
        if (infantryAttackController.shootEnd == false &&infantryAttackController.attackMethodName == "SniperAttackMethod" && infantryAttackController.lieDownMotion == false)
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

    void Start() 
    {
        soldierHP = gameObject.GetComponent<SoldierHP>();
        infantryAttackController = gameObject.GetComponent<InfantryAttackController>();
        animator = this.GetComponent<Animator>();

        speed = 1.5f;
    }

    void Update()
    {
       

        speedCycle = speed;

        //speedは下限1.5上限3

        if (CurrentState == State.Move && soldierHP.HP >= 1 && infantryAttackController.shootEnd == false && standUpTime == 0)
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



        //p45(shoot == true)だったときの分岐処理。InfantryAttackControllerの射撃ラグを測る処理内にshootToMoveをtrueにするトリガーを設置。このことによりコッキングが終わった後に移動アニメーションが作動する。
        if(shootToMove == true && CurrentState == State.Move)
        {            
            MoveMotion();
            shootToMove = false;
        }
                
    }
}