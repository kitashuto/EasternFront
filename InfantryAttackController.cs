using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;//配列で使用する。


public class InfantryAttackController : AttackController
{
    public bool attackOrder;
    public bool idleMotion = false;
    float time;

    // Start is called before the first frame update
    void Start()
    {
        attackOrder = true;
        this.animator = GetComponent<Animator>();
        this.aud = GetComponent<AudioSource>();
        soldierMove = gameObject.GetComponent<Move>();
        soldierHP = gameObject.GetComponent<SoldierHP>();
        DefaultState();
        nearEnemy = null;
    }

    // Update is called once per frame
    public void Update()
    {
        if (attackOrder == true)
        {
            AttackMethod();
        }
        else if (attackOrder == false)
        {
            time += Time.deltaTime;
                if (idleMotion == true)
                {
                    animator.SetTrigger("IdleTrigger");
                    idleMotion = false;
                }
                lookDelta = 0;
                shootDelta = -0.5f;
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

    public void Reload()
    {
        if (magazine != 0 && remainingAmmo < 1) return;
        if (soldierMove.CurrentState == Move.State.Move)
        {
            reloadDelta = 0;
            return;
        }
        reloadDelta += Time.deltaTime;
        // gunUpMotion = false;←これ必要かわからない
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
            lookDelta = 0;
            shootDelta = -0.5f;
            magazine += clip;
            remainingAmmo -= clip;
            reloadDelta = 0;
        }
    }

    public void AttackMethod()
    {
        // HPが無いときは攻撃できない
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
        // 敵がいなければ攻撃できない
        if (nearEnemy == null)
        {
          lookDelta = 0;
          Reload();
          return;
        }
        //射撃時の行動
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
        allAmmo = remainingAmmo + magazine;
        Debug.Log("残弾数" + allAmmo);

        //リロード時の行動
        Reload();

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
                //moveGoサイン。reloadGoサイン。の処理。
            }
        }
    }
}
