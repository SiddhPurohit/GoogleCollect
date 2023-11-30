using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] public GameObject coinText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0,20*Time.deltaTime,0);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            PlayerManager.numberofCoins+=1;
            GameObject sc = Instantiate(coinText, transform.position, Quaternion.identity);
            Destroy(sc,0.5f);
            // Debug.Log("Coins:"+PlayerManager.numberofCoins);
            Destroy(this.gameObject);
        }
    }
}
