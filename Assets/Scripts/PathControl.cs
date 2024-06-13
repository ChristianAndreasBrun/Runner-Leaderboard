using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathControl : MonoBehaviour
{
    GameManager manager;


    public void Init(GameManager manager)
    {
        this.manager = manager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            manager.GetRnadomNewPath(gameObject);
        }
    }
}
