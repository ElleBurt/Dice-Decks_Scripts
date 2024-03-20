using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHover : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseOver(){
        Vector3 pos = transform.GetChild(0).gameObject.GetComponent<CardController>().basePos;
        transform.GetChild(0).position = pos + new Vector3(0,3,0);
    }
    void OnMouseExit(){
        Vector3 pos = transform.GetChild(0).gameObject.GetComponent<CardController>().basePos;
        transform.GetChild(0).position = pos;
    }
}
