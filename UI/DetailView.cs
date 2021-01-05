using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailView : MonoBehaviour
{
    public string[] tagNameArray = { "Infantry", "Officer" ,"Sniper" , "Marksman"};
    public UIManager ui;
    public int soldierHp;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hit2d = Physics2D.Raycast(ray.origin, ray.direction);
        foreach (string tagName in tagNameArray)
        {
            if (hit2d.collider != null && hit2d.collider.gameObject.CompareTag(tagName))
            {
                var s = hit2d.collider.gameObject.GetComponent<ChildManager>();
                if (s != null && Input.GetKey(KeyCode.C)) //Iキーが押されていれば
                {
                    s.rangeSwitching = true;
                    //Debug.Log("表示");
                }
                else
                {
                    s.rangeSwitching = false;
                    //Debug.Log("非表示");
                }

                var hpScript = hit2d.collider.gameObject.GetComponent<SoldierHP>();
                soldierHp = hpScript.hp;
                ui.UpdateHP(soldierHp);
            }
        }
    }
}
