using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapSM : MonoBehaviour
{

    public float reactionTime = 0.1f;
    public GameObject directionalLight;

    private FSM fsm;
    private List<GameObject> activeSentinels = new List<GameObject>();

    void Start()
    {
        FSMState quiet = new FSMState();
        quiet.enterActions.Add(LightOn);

        FSMState alarm = new FSMState();
        alarm.enterActions.Add(LightOff);

        FSMTransition t1 = new FSMTransition(SentinelActive);
        FSMTransition t2 = new FSMTransition(NoSentinelActive);

        quiet.AddTransition(t1, alarm);
        alarm.AddTransition(t2, quiet);

        fsm = new FSM(quiet);

        StartCoroutine(Patrol());
    }

    private void LightOn()
    {
        if(directionalLight.activeInHierarchy == false)
        {
            directionalLight.SetActive(true);
        }
    }

    private void LightOff()
    {
        if(directionalLight.activeInHierarchy)
        {
            directionalLight.SetActive(false);
        }
    }

    private bool SentinelActive()
    {
        if(activeSentinels.Count > 0)
        {
            return true;
        }
        return false;
    }

    private bool NoSentinelActive()
    {
        return !SentinelActive();
    }

    public void AddActiveSentinel(GameObject sentinel)
    {
        activeSentinels.Add(sentinel);
    }

    public void RemoveActiveSentinel(GameObject sentinel)
    {
        activeSentinels.Remove(sentinel);
    }

    public List<GameObject> GetActiveSentinels()
    {
        return activeSentinels;
    }

    private IEnumerator Patrol()
    {
        while(true)
        {
            fsm.Update();
            yield return new WaitForSeconds(reactionTime);
        }
    }

}
