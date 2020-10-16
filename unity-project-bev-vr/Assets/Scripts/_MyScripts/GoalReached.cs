using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalReached : MonoBehaviour
{
    public SessionManager sessionManager;
    public GameObject goalReachedView;

    void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "car"){
            sessionManager.EndSession();
            goalReachedView.SetActive(true);
        }
    }
}
