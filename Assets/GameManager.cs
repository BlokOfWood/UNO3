using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{ 
    void Update()
    {
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