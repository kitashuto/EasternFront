using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfantryAnimation : MonoBehaviour
{
    Animator animator;
    AttackController attackController;
    [SerializeField] RuntimeAnimatorController[] controller = default;
    public string course;
    public bool t;


    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        course = "Mk3";
    }

    // Update is called once per frame
    void Update()
    {
        if (course == "Mk3" )
        {

            animator.runtimeAnimatorController = controller[0];
                        
        }
        else if (course == "P14" )
        {

            animator.runtimeAnimatorController = controller[1];            
            
        }
        else if (course == "Mk3Sniper")
        {

            animator.runtimeAnimatorController = controller[2];

        }
        else if (course == "P14Sniper")
        {

            animator.runtimeAnimatorController = controller[3];

        }
    }
}
