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
    public bool gunUpMotion = true;
    public bool downMotion;
    public bool reloadMotion;

    public EnemyHP enemyHPScript;
    public int enemyHP;
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
    public bool shootEnd;
    public float shootEndDelta;
    public float shootEndRag;


    public float lookSpan;//15～16行目はUnityの教科書p212のししおどしの時間差計算の引用
    public float holdSpan;
    public float lookDelta;//SoldierMoveで使うためpublic
    public float shootSpan;
    public float shootDelta;//初期値を-5にしないと敵を向いた瞬間攻撃してしまう。だから5(shootSpan)秒巻き戻してからスタートさせる。
    public float minSpan;
    public float maxSpan;

    public bool accuracyBonus1 = false;
    public bool accuracyBonus2 = false;

    public bool attackOrder;
    public bool idleMotion = false;
    public Move move;



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
        hitRate = 50;
        soldierRange = 3f;
        rifleSE = mk3SE;
        shootEndRag = 1.4f;
        reloadSpan = 4.8f;

        minSpan = 1.5f;
        maxSpan = 2f;

        remainingAmmo = 100;
        magazine = 3;
        clip = 3;

        lookSpan = 0.5f;
        holdSpan = 1.25f;
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
            else if (shootRange > soldierRange && soldierMove.CurrentState != Move.State.Move && allAmmo != 0 && attackOrder == true && magazine != 0)
            {
                if (shootEnd == false)
                {
                    downMotion = true;
                    if (downMotion == true)
                    {
                        animator.SetTrigger("DownTrigger");
                        downMotion = false;
                    }
                }
                else
                {
                    if (downMotion == true)
                    {
                        animator.SetTrigger("DownTrigger");
                        downMotion = false;
                    }

                }
                animator.SetTrigger("IdleTrigger");
                lookDelta = 0;
            }
            else if (allAmmo == 0)
            {
                //animator.SetTrigger("DownTrigger");
                //animator.SetTrigger("IdleTrigger");
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
                downMotion = true;
                shootEndDelta = 0;
                shootEnd = false;
                move.shootToMove = true;
                //moveGoサイン。reloadGoサイン。の処理。
            }
        }

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
            enemyHP = enemyHPScript.HP;

            //オブジェクトの距離が近いか、距離0であればオブジェクト名を取得
            //一時変数に距離を格納
            if (enemyHP >= 1)//下のifとまとめてもいいかも
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





}