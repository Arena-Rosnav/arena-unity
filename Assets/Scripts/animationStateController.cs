using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateController : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        string state = getState();
        switch(state){
            case "walking":
                animator.SetBool("isWalking", true);
                break;
            default:
                break;
        }
    }

    string getState(){
        return "walking";
    }
}
