using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{ 
    void Update()
    {
        //Disables all player objects so they don't overlap with the main player's UI? Maybe? Don't exactly remember the thought process.
        //If you are really curious, comment this out and see what happens.
        foreach(GameObject i in GameObject.FindGameObjectsWithTag("Player"))
        {
            if(i.GetComponent<BoltEntity>().Source != null)
            {
                for(int x = 0; x < i.transform.childCount; x++)
                {
                    i.transform.GetChild(x).gameObject.SetActive(false);
                }
            }
        }
    }
}