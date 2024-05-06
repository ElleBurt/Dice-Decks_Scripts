using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BoosterHover : MonoBehaviour
{
    public BoosterTemplate boosterTemp;
    public GameObject ItemDesc;
    public GameObject openDesc;
    public Transform Booster;


    public bool Selected = false;

    Coroutine sellAnimCoro = null;

    public Quaternion baseRot;

    

    void OnMouseEnter(){
        

        if(transform.childCount > 0){
            Vector3 pos = transform.position + boosterTemp.positionOffset;
            transform.GetChild(0).position = pos + new Vector3(0,1,0);

            if(openDesc == null){

                GameObject Desc = GameObject.Instantiate(ItemDesc,(transform.position + new Vector3(0,5.5f*boosterTemp.offsetFactor,0)),Quaternion.Euler(Vector3.zero));

                Desc.transform.localScale *= boosterTemp.scaleFactor;

                Desc.transform.Find("Desc").GetComponent<TMP_Text>().text = $"{boosterTemp.name}\n{boosterTemp.description}";
                Desc.transform.SetParent(transform);
                Desc.transform.Find("Sell").GetChild(0).GetComponent<TMP_Text>().text = $"Buy: ${boosterTemp.basePrice}";
 
                openDesc = Desc;
                openDesc.transform.LookAt(Camera.main.transform.position);
                openDesc.transform.rotation *= Quaternion.Euler(0,180,0);
            }
        }
       
    }

    void OnMouseDown(){
        Selected = !Selected;

        openDesc.transform.Find("Sell").GetComponent<BoosterBuyScript>().selected = Selected;
        openDesc.transform.Find("Sell").GetComponent<BoosterBuyScript>().SwapColor();

        if(sellAnimCoro != null){
            StopCoroutine(sellAnimCoro);
            sellAnimCoro = null;
        }

        if(Selected){
            sellAnimCoro = StartCoroutine(SellAnim());
        }
    }

    void OnMouseExit(){
        
        if(transform.childCount > 0 && !Selected){
            Vector3 pos = transform.position + boosterTemp.positionOffset;
            transform.GetChild(0).position = pos;
            if(openDesc != null){
                Destroy(openDesc);
            }
        }
        
        
        
        
    }

    private IEnumerator SellAnim(){

        float TimeValue = 0.05f;
        float Elapsed = 0f;
        bool flip = false;
        int flipCount = 0;
        int maxFlips = 6;
        float angleChange = 10f;
        Quaternion endVal;

        while(Selected && Booster != null){
            if(flip){
                endVal = baseRot * Quaternion.Euler(angleChange,0,0);
            }else{
                endVal = baseRot * Quaternion.Euler(-angleChange,0,0);
            }

            if(flipCount == maxFlips){
                endVal = baseRot;
            }


            Booster.rotation = Quaternion.Slerp(Booster.rotation, endVal,  Elapsed / TimeValue);

            Elapsed += Time.deltaTime;
            
            if(Elapsed > TimeValue && flipCount == maxFlips){
                yield return new WaitForSeconds(1.5f);
                flipCount = 0;
            }else if(Elapsed > TimeValue){
                flip = !flip;
                Elapsed = 0f;
                flipCount++;
            }

            yield return null;
        }
    }

}
