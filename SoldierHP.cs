using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierHP : MonoBehaviour, IDamagable
{
    public int hp = 100;//publicでインスペクターに表示させたくない。方法を見つけること。

    float destroyDelta;
    bool t = false;
    Animator animator;

    public void AddDamage(int damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            this.animator.SetTrigger("DeadTrigger");
            Debug.Log("Soldierが倒された");
            t = true;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        this.animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(hp);
        if (t == true)
        {
            destroyDelta += Time.deltaTime;
            if (destroyDelta > 10f)
            { Destroy(gameObject); }
        }
    }


}
