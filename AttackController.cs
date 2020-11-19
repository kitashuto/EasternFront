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
    public float searchTime = 0;    //経過時間
    public bool gunUpMotion = true;
    public bool reloadMotion = true;

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
    public float lookDelta = 0f;//SoldierMoveで使うためpublic
    public float shootSpan;
    public float shootDelta;//初期値を-5にしないと敵を向いた瞬間攻撃してしまう。だから5(shootSpan)秒巻き戻してからスタートさせる。
    public float minSpan;
    public float maxSpan;

    public bool accuracyBonus1 = false;
    public bool accuracyBonus2 = false;





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
        shootSpan = GetRandomTime();
        shootDelta = -0.5f;
        probability = Probability();

        //初期値(Mk3)
        attackPower = 35;
        hitRate = 20;
        soldierRange = 3f;
        rifleSE = mk3SE;
        shootEndRag = 1.5f;
        reloadSpan = 4.5f;

        minSpan = 1.5f;
        maxSpan = 1.5f;

        remainingAmmo = 100;
        magazine = 3;
        clip = 3;

        lookSpan = 0.75f;
        holdSpan = 1.5f;
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