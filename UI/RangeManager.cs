using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeManager : MonoBehaviour
{
    public GameObject parent;
    public float alpha;
    public InfantryAttackController infantryAttackController;
    float range;
    // Start is called before the first frame update
    void Start()
    {
        alpha = 0f;
        parent = transform.parent.gameObject;
        infantryAttackController = parent.GetComponent<InfantryAttackController>();       
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
        gameObject.transform.rotation = Quaternion.Euler(1, 1, 0);//warld座標で位置を固定
        range = infantryAttackController.soldierRange / 4;
        transform.localScale = new Vector3(range, range, 0);
    }
}
