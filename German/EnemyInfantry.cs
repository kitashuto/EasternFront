using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;//配列で使用する。

public class EnemyInfantry : MonoBehaviour
{
    EnemyHP enemyHP;

    public AudioClip g98SE;

    public Animator animator;
    public AudioSource aud;


    public Move soldierMove;
    public SoldierHP soldierHP;

    public GameObject nearEnemy;         //最も近いオブジェクト
    public float searchTime;    //経過時間
    public bool upMotion;
    public bool lieDownMotion;
    public bool standUpMotion;
    public bool shootToDown;
    public bool downMotion;
    public bool reloadMotion;

    public SoldierHP soldierHPScript;
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
    public float lieDownSpan;
    public float lookDelta;//SoldierMoveで使うためpublic
    public float shootSpan;
    public float shootDelta;//初期値を-5にしないと敵を向いた瞬間攻撃してしまう。だから5(shootSpan)秒巻き戻してからスタートさせる。
    public float minSpan;
    public float maxSpan;

    public bool accuracyBonus1 = false;
    public bool accuracyBonus2 = false;

    public bool outOfRange;
    public bool idleMotion = false;
    public float idleTimer;
    public Move move;
    public InfantryAnimation infantryAnimation;
    public string attackMethodName;
    public Vector2 targetPos;
    Quaternion rotation;
    public bool moveAnim;
    public bool idleBool;
    public bool attackTrigger;
    public bool isStop;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        DefaultState();
        enemyHP = gameObject.GetComponent<EnemyHP>();
        this.animator = GetComponent<Animator>();
        aud = GetComponent<AudioSource>();
        targetPos = new Vector2(1.3f, -5.5f);
        moveAnim = true;
        speed = 1.25f;
    }

    // Update is called once per frame
    void Update()
    {
        AttackMethod();
        
        ReloadMethod();

        allAmmo = remainingAmmo + magazine;
    }



    public void DefaultState()
    {
        //shootSpan = GetRandomTime();  子クラスのstartに書き写したら初弾が早く撃たれることはなくなった。しかしできるならその処理をこっちに書きたい。
        shootDelta = 0f;
        probability = Probability();

        //初期値(Mk3)
        attackPower = 35;
        hitRate = 0;
        soldierRange = 5f;
        shootEndRag = 1.4f;
        reloadSpan = 4.8f;

        minSpan = 2.5f;
        maxSpan = 4.5f;

        remainingAmmo = 100;
        magazine = 100;
        clip = 100;

        lookSpan = 0f;
        holdSpan = 0.2f;
        lieDownSpan = 2.5f;
    }




    public void MoveMethod()
    {
        if (enemyHP.hp < 1) return;

        if (moveAnim == true)
        {
            animator.SetTrigger("MoveTrigger");
            animator.SetFloat("Speed", 1.25f);
            moveAnim = false;

            lookDelta = 0f;
            shootDelta = 0f;
            upMotion = true;
            lieDownMotion = true;
            outOfRange = false;
            idleBool = false;            
        }       
            rotation = Quaternion.LookRotation(Vector3.forward, targetPos);
            transform.localRotation = rotation;
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }



    



    public void AttackMethod()
    {
        
        if (enemyHP.hp < 1) return;

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
            if (shootRange <= soldierRange && allAmmo != 0 )//!=の後はsoldierMoveではなく元となるSoldierMoveを指定
            {
                if (idleBool == true)
                {
                    animator.SetTrigger("IdleTrigger");
                    idleBool = false;
                }

                this.lookDelta += Time.deltaTime;//Unityの教科書p212,スクリプト13行目
                if (lookDelta > lookSpan && magazine >= 1)
                {
                    var vec1 = (p2 - p1).normalized;
                    if (shootEnd == false)
                    {
                        this.transform.rotation = Quaternion.FromToRotation(Vector3.up, vec1);
                    }
                    if (lookDelta > holdSpan && upMotion == true)
                    {
                        shootSpan = 1.25f;
                        animator.SetTrigger("UpTrigger");
                        upMotion = false;
                    }

                    if (lookDelta > holdSpan)
                    {

                        shootDelta += Time.deltaTime;

                        if (shootDelta > shootSpan)
                        {//発砲モーション付ける                                                                                           
                            animator.SetTrigger("FireTrigger");//このアニメが終わるまではmoveできないようにしたい
                            aud.PlayOneShot(g98SE);

                            if (probability < hitRate)
                            {
                                nearEnemy.GetComponent<IDamagable>().AddDamage(attackPower);
                            }

                            //attackPower = 30;
                            probability = Probability();
                            shootSpan = GetRandomTime();
                            //move.shootToMove = false;//ここにいれないと2発目以降MoveのMoveMotion(2)の処理がうまくいかない

                            shootDelta = 0f;
                            shootEnd = true;

                        }
                    }
                }
                if (outOfRange == false)
                {
                    moveAnim = false;
                    idleBool = true;
                    upMotion = true;
                    outOfRange = true;
                }
            }

            //敵がレンジ外の時
            else if ((shootRange > soldierRange && allAmmo != 0 && magazine != 0) || allAmmo == 0 )
            {
                
                if (outOfRange == true)
                {
                    if (shootToDown == false)
                    {
                        downMotion = true;
                        DownToIdleMotion();                       
                    }
                    else if (shootToDown == true)
                    {
                        DownToIdleMotion();
                        shootToDown = false;
                    }
                    outOfRange = false;
                }

                lookDelta = 0;
                shootDelta = 0;
            }
        }

        else if (nearEnemy == null && downMotion == true)
        {
            DownToIdleMotion();
            downMotion = false;
            lookDelta = 0;
            shootDelta = 0;
        }


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
                shootToDown = true;
                //move.shootToMove = true;
                //moveGoサイン。reloadGoサイン。の処理。
            }
        }

        
    }


   



    public void ReloadMethod()
    {
        if (enemyHP.hp < 1) return;
        //リロード時の行動
        if (magazine == 0 && remainingAmmo >= 1)
        {
            if (soldierMove.CurrentState != Move.State.Move)
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

            soldierHPScript = obs.GetComponent<SoldierHP>();


            //オブジェクトの距離が近いか、距離0であればオブジェクト名を取得
            //一時変数に距離を格納
            if (soldierHPScript.hp >= 1)//下のifとまとめてもいいかも
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



    public void SearchTimeMethod()
    {
        searchTime += Time.deltaTime;

        if (searchTime >= 0.5f)
        {
            string[] tagNameArray = { "Infantry", "Officer" };

            //最も近かったオブジェクトを取得
            nearEnemy = searchTag(gameObject, tagNameArray);

            //経過時間を初期化
            searchTime = 0;

        }
    }


    public virtual float GetRandomTime()
    {
        //数値を継承先クラスで変数にして定義する

        return Random.Range(minSpan, maxSpan);//最初、ここにはminSpan,maxSpanを入れていたが、その場合初期攻撃時に兵士全員が同タイミングで攻撃するようになっていた。しかし、GetRandomRangeに直接floatの値(2.5f,5f)を代入することで、初期攻撃がなくなり、二回目の攻撃から完全なランダムで攻撃するようになった。どういうことかわからないのでまたこの問題に直面したら解決するように。どうやらStartにmin,maxSpanの固定値を入れるのがいけないらしい。GetRandomTimeに入れることで解決。
    }



    public int Probability()
    {
        return Random.Range(1, 101);
    }



    public void DownToIdleMotion()
    {        
        if (downMotion == true)
        {
            animator.SetTrigger("DownTrigger");
            idleMotion = true;
            downMotion = false;
        }

        idleTimer += Time.deltaTime;

        if (idleTimer > 0.08f && idleMotion == true)
        {
            animator.SetTrigger("IdleTrigger");                        
            idleMotion = false;
        }

        
    }



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
