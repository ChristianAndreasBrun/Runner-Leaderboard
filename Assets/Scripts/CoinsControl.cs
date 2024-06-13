using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinsControl : MonoBehaviour
{
    public float percent;
    public bool anyCoins;
    public List<GameObject> coins;


    private void Start()
    {
        float canCreate = Random.Range(0f, 100f);
        if (canCreate < percent)
        {
            if (anyCoins)
            {
                GameObject[] GetTotalCoins = Resources.LoadAll<GameObject>("Coins");
                Instantiate(GetTotalCoins[Random.Range(0, GetTotalCoins.Length)], transform);
            }
            else
            {
                Instantiate(coins[Random.Range(0, coins.Count)], transform);
            }
        }
    }
}
