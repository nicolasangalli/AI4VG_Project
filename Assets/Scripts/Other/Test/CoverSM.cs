using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverSM : MonoBehaviour
{

    public float reactionTime = 0.2f;
    private FSM fsm;
    private GameObject[] obstacles;

    // Start is called before the first frame update
    void Start()
    {
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        FSMState cover = new FSMState();
        cover.stayActions.Add(FindCover);
        fsm = new FSM(cover);

        StartCoroutine(Patrol());
    }

    private void FindCover()
    {
        
    }

    private IEnumerator Patrol()
    {
        while (true)
        {
            fsm.Update();
            yield return new WaitForSeconds(reactionTime);
        }
    }
}
