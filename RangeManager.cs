using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeManager : MonoBehaviour
{
    public GameObject parent;
    InfantryAttackController infantryAttackController;
    float range;
    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent.gameObject;
        infantryAttackController = parent.GetComponent<InfantryAttackController>();       
    }

    // Update is called once per frame
    void Update()
    {
        range = infantryAttackController.soldierRange / 4;
        transform.localScale = new Vector3(range, range, 0);
    }
}
