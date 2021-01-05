using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildManager : MonoBehaviour
{
    public GameObject rangeObject;
    public RangeManager rangeManager;
    public bool rangeSwitching;
    // Start is called before the first frame update
    void Start()
    {
        rangeObject = transform.Find("range").gameObject;
        rangeManager = rangeObject.GetComponent<RangeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(rangeSwitching == true)
        {
            rangeManager.alpha = 1f;
        }
        else
        {
            rangeManager.alpha = 0;
        }
    }
}
