using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BoosterHover : MonoBehaviour
{
    public BoosterTemplate boosterTemp;
    public GameObject ItemDesc;
    private GameObject openDesc;

    public float scaleFactor = 1.0f;
    public float offsetFactor = 1.0f;

    void OnMouseDown(){

    }

    void OnMouseEnter(){
        

        if(transform.childCount > 0){
            Vector3 pos = transform.position + boosterTemp.positionOffset;
            transform.GetChild(0).position = pos + new Vector3(0,1,0);

            if(openDesc == null){

                GameObject Desc = GameObject.Instantiate(ItemDesc,(transform.position + new Vector3(0,5.5f*offsetFactor,0)),Quaternion.Euler(Vector3.zero));

                Desc.transform.localScale *= scaleFactor;

                Desc.transform.Find("Desc").GetComponent<TMP_Text>().text = $"{boosterTemp.name}\n{boosterTemp.description}";
                Desc.transform.SetParent(transform);
                Desc.transform.Find("Price").GetComponent<TMP_Text>().text = $"Buy: ${boosterTemp.basePrice}";

                
                Desc.transform.localScale *= 1.2f;   
                openDesc = Desc;
                
                openDesc.transform.LookAt(Camera.main.transform.position);
                openDesc.transform.rotation *= Quaternion.Euler(0,180,0);
            }
        }
       
    }
    void OnMouseExit(){
        
        if(transform.childCount > 0){
            Vector3 pos = transform.position + boosterTemp.positionOffset;
            transform.GetChild(0).position = pos;
        }
        
        
        if(openDesc != null){
            Destroy(openDesc);
        }
        
    }
}
