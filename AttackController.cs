using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;//配列で使用する。

public abstract class AttackController : MonoBehaviour
{
    public AudioClip rifleSE;

    public AudioClip mk3SE;
    public AudioClip p14SE;
    public string audioClipName;

    public AudioClip rifleReloadSE;

    public Animator animator;
    public AudioSource aud;


    public Move soldierMove;
    public SoldierHP soldierHP;

    public GameObject nearEnemy;         //最も近いオブジェクト
    public float searchTime;    //経過時間
    public bool upMotion;
    public bool lieDownMotion;
    public bool standUpMotion;
    public bool downMotion = false;
    public bool afterStabMotion;
    public bool beforeStabMotion;
    public bool reloadMotion;

    public EnemyHP enemyHPScript;
    public int probability;//全体で使える百分率

    public int attackPower;
    public int hitRate;
    public float soldierRange;
    public float shootRange;

    //弾薬関係の変数
    public int remainingAmmo;
    public int allAmmo;
    public int magazine;
    public int clip;

    public float reloadSpan;
    public float reloadDelta;


    public float lookSpan;//15～16行目はUnityの教科書p212のししおどしの時間差計算の引用
    public float holdSpan;
    public float lieDownSpan;
    public float lookDelta;//SoldierMoveで使うためpublic
    public float shootSpan;
    public float shootDelta;//初期値を-5にしないと敵を向いた瞬間攻撃してしまう。だから5(shootSpan)秒巻き戻してからスタートさせる。
    public float minSpan;
    public float maxSpan;

    public bool accuracyBonus1 = false;
    public bool accuracyBonus2 = false;

    public bool attackOrder;
    public bool outOfRange;
    public bool outOfStabRange;
    public bool idleMotion;
    public bool isShooting;
    public bool isReady;
    public bool shootAnimBool;
    public float idleTimer;
    public Move move;
    public InfantryAnimation infantryAnimation;
    public string attackMethodName;


    //攻撃の命中精度などに使う百分率
    public int Probability()
    {
        return Random.Range(1, 101);
    }





    //兵士の攻撃間隔のメソッド
    public virtual float GetRandomTime()
    {
        //数値を継承先クラスで変数にして定義する
        
        return Random.Range(minSpan, maxSpan);//最初、ここにはminSpan,maxSpanを入れていたが、その場合初期攻撃時に兵士全員が同タイミングで攻撃するようになっていた。しかし、GetRandomRangeに直接floatの値(2.5f,5f)を代入することで、初期攻撃がなくなり、二回目の攻撃から完全なランダムで攻撃するようになった。どういうことかわからないのでまたこの問題に直面したら解決するように。どうやらStartにmin,maxSpanの固定値を入れるのがいけないらしい。GetRandomTimeに入れることで解決。
    }





    //歩兵初期装備Mk3のステータスメソッド
    public void DefaultState()
    {
        //shootSpan = GetRandomTime();  子クラスのstartに書き写したら初弾が早く撃たれることはなくなった。しかしできるならその処理をこっちに書きたい。
        shootDelta = 0f;
        probability = Probability();

        //初期値(Mk3)
        attackPower = 35;
        hitRate = 25;
        soldierRange = 6f;
        rifleSE = mk3SE;
        reloadSpan = 4.8f;

        minSpan = 4f;
        maxSpan = 5.5f;

        remainingAmmo = 100;
        magazine = 10;
        clip = 10;

        lookSpan = 0f;
        holdSpan = 0.2f;
        lieDownSpan = 2.5f;
    }




    public void WeaponSoundController()
    {
        if (audioClipName == "Mk3SE")
        {

            rifleSE = mk3SE;

        }
        else if (audioClipName == "P14SE")
        {

            rifleSE = p14SE;

        }
    }




    public void AttackMethod()
    {
        attackMethodName = "AttackMethod";
        if (soldierHP.hp < 1) return;

        //ここからセミオート攻撃モーション(State.Move以外)]

        SearchTimeMethod();

        //射撃時の行動
        if (nearEnemy != null)
        {
            Vector2 p1 = transform.position;
            Vector2 p2 = nearEnemy.transform.position;
            Vector2 dir = p2 - p1;
            shootRange = dir.magnitude;


            AccuracyBonus();

            //敵がレンジ内の時
            if (shootRange <= soldierRange && shootRange > 1.2f && soldierMove.CurrentState != Move.State.Move && allAmmo != 0 )//!=の後はsoldierMoveではなく元となるSoldierMoveを指定
            {                
                this.lookDelta += Time.deltaTime;//Unityの教科書p212,スクリプト13行目
                if (lookDelta > lookSpan && magazine >= 1)
                {
                    var vec1 = (p2 - p1).normalized;
                    
                    //this.transform.rotation = Quaternion.FromToRotation(Vector3.up, vec1);
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, vec1);
                    // 現在の回転情報と、ターゲット方向の回転情報を補完する
                    transform.rotation = Quaternion.Slerp(this.transform.rotation, rotation, 0.1f);
                    
                    if (lookDelta > holdSpan && upMotion == true)
                    {
                        shootSpan = 1.25f;
                        animator.SetTrigger("UpTrigger");
                        upMotion = false;
                        isReady = true;
                        downMotion = true;
                    }

                    if (lookDelta > holdSpan)
                    {
                        shootDelta += Time.deltaTime;

                        if (shootDelta > shootSpan && isShooting == false)
                        {//発砲モーション付ける      
                            animator.SetTrigger("FireTrigger");//このアニメが終わるまではmoveできないようにしたい
                            aud.PlayOneShot(this.rifleSE);                            
                            shootAnimBool = true;
                            if (probability < hitRate)
                            {
                                nearEnemy.GetComponent<IDamagable>().AddDamage(attackPower);
                            }

                            //attackPower = 30;
                            probability = Probability();
                            shootSpan = GetRandomTime();
                            isShooting = true;//ここにいれないと2発目以降MoveのMoveMotion(2)の処理がうまくいかない

                        }
                        else if (shootDelta > shootSpan)
                        {
                            shootDelta = 0f;
                        }
                        else if (shootAnimBool == true && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                        {                            
                            magazine--;
                            isShooting = false;
                            move.shootToDown = true;
                            shootAnimBool = false;                           
                        }

                    }
                }
                if (outOfRange == false)
                {
                    upMotion = true;
                    outOfRange = true;
                }
            }

            //敵がレンジ外の時
            else if ((shootRange > soldierRange && soldierMove.CurrentState != Move.State.Move && allAmmo != 0 && magazine != 0)|| allAmmo == 0 )
            {
                if (shootAnimBool == true && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                {
                    magazine--;
                    isShooting = false;
                    shootAnimBool = false;
                }

                if (isShooting == false)
                {
                    if (outOfRange == true)
                    {
                        DownToIdleMotion();                        
                    }
                    lookDelta = 0;
                    shootDelta = 0;
                }                
            }





            //近接攻撃範囲内の時
            if (shootRange <= 1.2f && soldierMove.CurrentState != Move.State.Move)//!=の後はsoldierMoveではなく元となるSoldierMoveを指定
            {
                this.lookDelta += Time.deltaTime;//Unityの教科書p212,スクリプト13行目
                if (lookDelta > lookSpan)
                {
                    var vec1 = (p2 - p1).normalized;

                    //this.transform.rotation = Quaternion.FromToRotation(Vector3.up, vec1);
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, vec1);
                    // 現在の回転情報と、ターゲット方向の回転情報を補完する
                    transform.rotation = Quaternion.Slerp(this.transform.rotation, rotation, 0.1f);

                    if (lookDelta > holdSpan && beforeStabMotion == true)
                    {
                        shootSpan = 1.25f;
                        animator.SetTrigger("BeforeStabTrigger");
                        beforeStabMotion = false;
                        afterStabMotion = true;
                    }

                    if (lookDelta > holdSpan)
                    {
                        shootDelta += Time.deltaTime;

                        if (shootDelta > shootSpan && isShooting == false)
                        {//発砲モーション付ける      
                            animator.SetTrigger("StabTrigger");//このアニメが終わるまではmoveできないようにしたい
                            aud.PlayOneShot(this.rifleSE);
                            shootAnimBool = true;
                            if (probability < hitRate)
                            {
                                nearEnemy.GetComponent<IDamagable>().AddDamage(attackPower * 3);
                            }

                            //attackPower = 30;
                            probability = Probability();
                            shootSpan = GetRandomTime();
                            isShooting = true;//ここにいれないと2発目以降MoveのMoveMotion(2)の処理がうまくいかない

                        }
                        else if (shootDelta > shootSpan)
                        {
                            shootDelta = 0f;
                        }
                        else if (shootAnimBool == true && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                        {
                            magazine--;
                            isShooting = false;
                            shootAnimBool = false;
                        }

                    }
                }
                if (outOfStabRange == false)
                {
                    beforeStabMotion = true;
                    outOfStabRange = true;
                }
            }

            //近接攻撃範囲外の時
            else if (shootRange == 1.2f && soldierMove.CurrentState != Move.State.Move)
            {
                if (shootAnimBool == true && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                {
                    magazine--;
                    isShooting = false;
                    shootAnimBool = false;
                }
                if (isShooting == false)
                {
                    if (outOfStabRange == true)
                    {
                        DownToIdleMotion();
                    }
                    lookDelta = 0;
                    shootDelta = 0;
                }
            }
        }
        



        else if (nearEnemy == null && upMotion == false)
        {            
            DownToIdleMotion();
            lookDelta = 0;
            shootDelta = 0;
        }

        
    }





   
    public void DownToIdleMotion()
    {
        if (downMotion == true)
        {
            animator.SetTrigger("DownTrigger");            
            idleMotion = true;
            downMotion = false;
        }
        else if(idleMotion == true && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            isReady = false;
            outOfRange = false;
            idleMotion = false;
        }
    }





    public void StabToIdleMotion()
    {
        if (afterStabMotion == true)
        {
            animator.SetTrigger("AfterStabTrigger");
            idleMotion = true;
            afterStabMotion = false;
        }

        idleTimer += Time.deltaTime;

        if (idleTimer > 0.08f && idleMotion == true)
        {
            animator.SetTrigger("IdleTrigger");
            idleTimer = 0;
            idleMotion = false;
        }
    }





    public void StandUpMotion()
    {
        if (standUpMotion == true)
        {
            animator.SetTrigger("StandUpTrigger");
            idleMotion = true;
            standUpMotion = false;
        }

        idleTimer += Time.deltaTime;

        if (idleTimer > 1.3f && idleMotion == true)
        {
            animator.SetTrigger("IdleTrigger");
            idleMotion = false;
        }
    }



    public void ReloadMethod()
    {
        if (soldierHP.hp < 1) return;
        //リロード時の行動
        if (magazine == 0 && remainingAmmo >= 1)
        {
            if (move.CurrentState != Move.State.Move && move.flag != true)
            {
                reloadDelta += Time.deltaTime;


                //upMotion = false;←これ必要かわからない
                if (downMotion == true)
                {
                    animator.SetTrigger("DownTrigger");
                    reloadMotion = true;
                    downMotion = false;
                }
                if (reloadDelta > 0.3f && reloadMotion == true)
                {
                    animator.SetTrigger("ReloadTrigger");
                    isReady = false;
                    reloadMotion = false;
                }
                if (reloadDelta > reloadSpan)
                {
                    animator.SetTrigger("IdleTrigger");
                    upMotion = true;
                    beforeStabMotion = true;
                    reloadMotion = true;//ここをtrueにしないと二回目以降のreloadMotionがtrueにならず、そのためリロード時にレンジ外に出ると正しくリロードされない
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





    




    public GameObject searchTag(GameObject nowObj, string[] tagNameArray)//後にIEnumerableに変えたほうがいいかも。qiita記事参照。
    {
        float tmpDis = 0;           //距離用一時変数
        float nearDis = 0;          //最も近いオブジェクトの距離
        //string nearObjName = "";    //オブジェクト名称
        GameObject targetEnemy = null; //オブジェクト

        GameObject[] enemyObs = new GameObject[] { };//空のゲームオブジェクト、複数ある。
        foreach (string tagName in tagNameArray)
        {
            GameObject[] obs = GameObject.FindGameObjectsWithTag(tagName);
            enemyObs = enemyObs.Concat(obs).ToArray();//IEnumerableの場合はToArrayを全て変える。 
        }
        //タグ指定されたオブジェクトを配列で取得する
        foreach (GameObject obs in enemyObs)
        {
            //自身と取得したオブジェクトの距離を取得
            tmpDis = Vector2.Distance(obs.transform.position, nowObj.transform.position);

            enemyHPScript = obs.GetComponent<EnemyHP>();


            //オブジェクトの距離が近いか、距離0であればオブジェクト名を取得
            //一時変数に距離を格納
            if (enemyHPScript.hp >= 1 || (isShooting == true && enemyHPScript.hp < 1 && enemyHPScript.dead == true))//下のifとまとめてもいいかも
            {
                //obsのレンジが武器の射程に入ったときにランク分けしてtargetenemyに入れる
                //if(soldierrange > obsの距離){ランクの高い敵をtargetenemyに入れる }
                if (nearDis == 0 || nearDis > tmpDis)
                {
                    nearDis = tmpDis;
                    //nearObjName = obs.name;
                    targetEnemy = obs;
                }
            }
            else if(isShooting == false && enemyHPScript.hp < 1)//これがないと敵が死んだ後にisShooting == trueが来る毎に死体撃ちを続けてしまう
            {
                enemyHPScript.dead = false;
            }
        }

        //最も近かったオブジェクトを返す
        //return GameObject.Find(nearObjName);
        return targetEnemy;

    }





    //射程ボーナスメソッド
    public virtual void AccuracyBonus()
    {
        if (shootRange <= soldierRange * 1 / 3)//距離比例の数式を使うとロングレンジキャラが近距離戦でも強くなってしまう(レンジが長い分同距離戦において短距離武器より命中率が上がってしまう)ため、ボツにした。
        {
            if (!accuracyBonus1)
            {
                accuracyBonus1 = true;
                hitRate += 20;
            }
        }
        else
        {
            if (accuracyBonus1)
            {
                accuracyBonus1 = false;
                hitRate -= 20;
            }
        }

        if (shootRange <= soldierRange * 2 / 3)
        {
            if (!accuracyBonus2)
            {
                accuracyBonus2 = true;
                hitRate += 15;
            }
        }
        else
        {
            if (accuracyBonus2)
            {
                accuracyBonus2 = false;
                hitRate -= 15;
            }
        }
    }

    public void SearchTimeMethod()
    {
        searchTime += Time.deltaTime;

        if (searchTime >= 0.5f)
        {
            string[] tagNameArray = { "EnemyInfantry", "EnemyOfficer" };

            //最も近かったオブジェクトを取得
            nearEnemy = searchTag(gameObject, tagNameArray);

            //経過時間を初期化
            searchTime = 0;

        }
    }


    //狙撃手用攻撃メソッド
    public void SniperAttackMethod()
    {
        attackMethodName = "SniperAttackMethod";
        if (soldierHP.hp < 1) return;

        //ここからセミオート攻撃モーション(State.Move以外)]

        SearchTimeMethod();

        //射撃時の行動
        if (nearEnemy != null)
        {
            Vector2 p1 = transform.position;
            Vector2 p2 = nearEnemy.transform.position;
            Vector2 dir = p2 - p1;
            shootRange = dir.magnitude;


            AccuracyBonus();

            //敵がレンジ内の時
            if (soldierRange * 2 / 3 < shootRange && shootRange <= soldierRange && soldierMove.CurrentState != Move.State.Move && allAmmo != 0)//!=の後はsoldierMoveではなく元となるSoldierMoveを指定
            {

                this.lookDelta += Time.deltaTime;//Unityの教科書p212,スクリプト13行目
                if (lookDelta > lookSpan && magazine >= 1)
                {
                    var vec1 = (p2 - p1).normalized;


                    this.transform.rotation = Quaternion.FromToRotation(Vector3.up, vec1);

                    if (lookDelta > lookSpan && lieDownMotion == true)
                    {
                        animator.SetTrigger("LieDownTrigger");
                        lieDownMotion = false;
                        shootSpan = 1f;
                    }

                    if (lookDelta > lieDownSpan)
                    {


                        //animator.SetTrigger("ReadyTrigger");//moveすることによってこのアニメを終わらせたい
                        shootDelta += Time.deltaTime;

                        if (shootDelta > shootSpan)
                        {//発砲モーション付ける                                                                                           
                            animator.SetTrigger("ProneFireTrigger");//このアニメが終わるまではmoveできないようにしたい
                            aud.PlayOneShot(this.rifleSE);

                            if (probability < hitRate)
                            {
                                nearEnemy.GetComponent<IDamagable>().AddDamage(attackPower);
                            }

                            //attackPower = 30;
                            probability = Probability();
                            shootSpan = GetRandomTime();

                            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                            {
                                shootDelta = 0f;
                                Debug.Log("aaaaaaaaaaaaaaaaaaaaaaaa");
                                magazine--;
                                downMotion = true;
                                //moveGoサイン。reloadGoサイン。の処理。
                            }

                        }
                    }
                }
                if (outOfRange == false)
                {
                    lieDownMotion = true;
                    outOfRange = true;
                }
            }
            if (soldierRange * 2 / 3 >= shootRange && soldierMove.CurrentState != Move.State.Move && allAmmo != 0)//!=の後はsoldierMoveではなく元となるSoldierMoveを指定
            {

                this.lookDelta += Time.deltaTime;//Unityの教科書p212,スクリプト13行目
                if (lookDelta > lookSpan && magazine >= 1)
                {
                    var vec1 = (p2 - p1).normalized;


                    this.transform.rotation = Quaternion.FromToRotation(Vector3.up, vec1);

                    if (lookDelta > holdSpan && upMotion == true)
                    {
                        animator.SetTrigger("UpTrigger");
                        upMotion = false;
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

                            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                            {
                                shootDelta = 0f;
                                Debug.Log("aaaaaaaaaaaaaaaaaaaaaaaa");
                                magazine--;
                                downMotion = true;
                                //moveGoサイン。reloadGoサイン。の処理。
                            }


                        }
                    }
                }

            }

            //敵がレンジ外の時
            else if ((shootRange > soldierRange && soldierMove.CurrentState != Move.State.Move && allAmmo != 0 && magazine != 0) || allAmmo == 0)
            {
                if (outOfRange == true)
                {
                    DownToIdleMotion();
                    isShooting = false;
                    outOfRange = false;
                }

                lookDelta = 0;
                shootDelta = 0;
                lookDelta = 0;
                shootDelta = 0;
            }
        }

        else if (nearEnemy == null && (downMotion == true || standUpMotion == true))
        {
            StandUpMotion();
            downMotion = false;
            standUpMotion = false;
            lookDelta = 0;
            shootDelta = 0;
        }



    }




    public void SniperReloadMethod()
    {
        if (soldierHP.hp < 1) return;


        //リロード時の行動
        if (magazine == 0 && remainingAmmo >= 1)
        {
            if (soldierMove.CurrentState != Move.State.Move)
            {
                reloadDelta += Time.deltaTime;
                if (soldierRange * 2 / 3 < shootRange && shootRange <= soldierRange)
                {
                    if (reloadDelta > 0.3f && reloadMotion == true)
                    {
                        animator.SetTrigger("ProneReloadTrigger");
                        reloadMotion = false;
                    }
                    if (reloadDelta > reloadSpan)
                    {
                        lookDelta = 0f;
                        shootDelta = 0f;
                        magazine += clip;
                        remainingAmmo -= clip;
                        reloadDelta = 0;
                    }
                }
                else if (soldierRange * 2 / 3 >= shootRange)
                {
                    if (downMotion == true)
                    {
                        animator.SetTrigger("DownTrigger");
                        reloadMotion = true;
                        downMotion = false;
                    }
                    if (reloadDelta > 0.3f && reloadMotion == true)
                    {
                        animator.SetTrigger("ReloadTrigger");
                        reloadMotion = false;
                    }
                    if (reloadDelta > reloadSpan)
                    {
                        animator.SetTrigger("IdleTrigger");
                        upMotion = true;
                        lookDelta = 0f;
                        shootDelta = 0f;
                        magazine += clip;
                        remainingAmmo -= clip;
                        reloadDelta = 0;
                    }
                }

            }
            else
            {
                reloadDelta = 0;
            }

        }
    }




}