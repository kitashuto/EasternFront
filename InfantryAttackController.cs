using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;//配列で使用する。


public class InfantryAttackController : AttackController
{
    public bool attackOrder;
    public bool idleMotion = false;
    float time;
    Move move;

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

    public void ReloadMethod()
    {
        if (soldierHP.HP < 1) return;
        //リロード時の行動
        if (magazine == 0 && remainingAmmo >= 1)
        {
            if (soldierMove.CurrentState != Move.State.Move)
            {
                reloadDelta += Time.deltaTime;


                //gunUpMotion = false;←これ必要かわからない
                if (reloadMotion == true)
                {
                    animator.SetTrigger("DownTrigger");
                    //animator.SetTrigger("ReloadTrigger");
                    reloadMotion = false;
                }
                if (reloadDelta > reloadSpan)
                {
                    animator.SetTrigger("IdleTrigger");
                    gunUpMotion = true;
                    lookDelta = 0f;
                    shootDelta = 0f;
                    magazine += clip;
                    remainingAmmo -= clip;
                    reloadDelta = 0;
                }
            }
            else
            {
                reloadDelta = 0;
            }

        }
    }


    public void AttackMethod()
    {
        if (soldierHP.HP < 1) return;
        
        //ここからセミオート攻撃モーション(State.Move以外)]

        searchTime += Time.deltaTime;

        if (searchTime >= 0.5f)
        {
                string[] tagNameArray = { "Enemy", "EnemyOfficer" };

                //最も近かったオブジェクトを取得
                nearEnemy = searchTag(gameObject, tagNameArray);

                //経過時間を初期化
                searchTime = 0;

        }



        //射撃時の行動
        if (nearEnemy != null)
        {
            Vector2 p1 = transform.position;
            Vector2 p2 = nearEnemy.transform.position;
            Vector2 dir = p2 - p1;
            shootRange = dir.magnitude;


            AccuracyBonus();
                
            if (shootRange <= soldierRange && soldierMove.CurrentState != Move.State.Move && allAmmo != 0 && attackOrder == true)//!=の後はsoldierMoveではなく元となるSoldierMoveを指定
            {

                this.lookDelta += Time.deltaTime;//Unityの教科書p212,スクリプト13行目
                if (lookDelta > lookSpan && magazine >= 1)
                {
                    var vec1 = (p2 - p1).normalized;
                    this.transform.rotation = Quaternion.FromToRotation(Vector3.up, vec1);

                    if (lookDelta > holdSpan && gunUpMotion == true)
                    {
                        animator.SetTrigger("UpTrigger");
                        gunUpMotion = false;
                    }

                    if (lookDelta > holdSpan)
                    {

                        
                        //animator.SetTrigger("ReadyTrigger");//moveすることによってこのアニメを終わらせたい
                        shootDelta += Time.deltaTime;
                           
                        if (shootDelta > shootSpan)
                        {//発砲モーション付ける                                                                                           
                            animator.SetTrigger("FireTrigger");//このアニメが終わるまではmoveできないようにしたい
                            aud.PlayOneShot(this.rifleSE);

                            if (probability < hitRate)
                            {
                                nearEnemy.GetComponent<IDamagable>().AddDamage(attackPower);
                            }
                                
                            //attackPower = 30;
                            probability = Probability();
                            shootSpan = GetRandomTime();
                            move.shootToMove = false;//ここにいれないと2発目以降MoveのMoveMotion(2)の処理がうまくいかない

                            shootDelta = 0f;
                            shootEnd = true;

                        }
                            
                    }
                }
            }
                
            else if (allAmmo == 0)
            {
                animator.SetTrigger("DownTrigger");
                animator.SetTrigger("IdleTrigger");
                lookDelta = 0;
            }

        }
        else
        {
            lookDelta = 0;
        }
        allAmmo = remainingAmmo + magazine;
        Debug.Log("残弾数" + allAmmo);


        

        //射撃後のラグを測る
        if (shootEnd == true)
        {
            shootEndDelta += Time.deltaTime;
            if (shootEndDelta > shootEndRag)
            {
                magazine--;
                reloadMotion = true;
                shootEndDelta = 0;
                shootEnd = false;
                move.shootToMove = true;
                //moveGoサイン。reloadGoサイン。の処理。
            }
        }
        
    }
}
