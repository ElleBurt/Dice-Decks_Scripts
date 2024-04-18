using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class DiceBoxHover : MonoBehaviour, GetValues
{
    public bool animFin = false;
    DiceRoller diceRoller;
    public bool hovered = false;
    public GameObject ItemDesc;
    
    
    [Range(0f, 1f)]
    public float scaleFactor = 1.0f;
    [Range(0f, 1f)]
    public float offsetFactor = 1.0f;
    [Range(0f, 1f)]
    public float offsetDescFactor = 1.0f;

    public bool inCheckout = false;
    public bool marketDice = false;
    
    DiceTemplate dt;

    private GameObject openDesc;

    void Awake(){
        diceRoller = FindObjectOfType<DiceRoller>();
    }

    public bool GetStage(){
        return inCheckout;
    }

    public void SetStage(){
        inCheckout = !inCheckout;
    }

    public Dictionary<string, float> GetValuesAvailable(){
        return new Dictionary<string, float> {{"Buy", dt.basePrice},{"Sell", dt.baseSellValue}};
    }


    private void OnMouseEnter() {
        if(animFin && transform.childCount > 0){
            transform.GetChild(0).position = transform.position + new Vector3(0, 2*offsetFactor, 0);
            hovered = true;

            if(openDesc == null){

                GameObject Desc = GameObject.Instantiate(ItemDesc,(transform.position + new Vector3(0,5.5f*offsetDescFactor,0)),Quaternion.Euler(Vector3.zero));

                Desc.transform.localScale *= scaleFactor;

                GameObject dice = transform.GetChild(0).gameObject;

                dt = dice.GetComponent<DiceRoll>().diceTemplate;
                Desc.transform.Find("Desc").GetComponent<TMP_Text>().text = $"{dt.name}\n{dt.description}";
                Desc.transform.Find("RightSide").Find("SideInfo").Find("High").GetComponent<TMP_Text>().text = $"High: {dt.hiVal}";
                Desc.transform.Find("RightSide").Find("SideInfo").Find("Low").GetComponent<TMP_Text>().text = $"Low: {dt.loVal}";
                Desc.transform.SetParent(dice.transform);
                Desc.transform.Find("Price").GetComponent<TMP_Text>().text = $"Buy: ${dt.basePrice} | Sell: ${dt.baseSellValue}";
                Transform SpecFaces = Desc.transform.Find("RightSide").Find("Faces");

                int index = 0;
                foreach(Transform child in dice.transform){
                    if(Regex.IsMatch(child.name,@"\D") && child.name != "DiceResult" && child.name != "DiceItemDesc(Clone)"){
                        GameObject faceTemp = Instantiate(Resources.Load($"UI/Prefabs/FaceTemp") as GameObject);
                        faceTemp.transform.Find("Face").GetComponent<Image>().sprite = (Resources.Load<Sprite>($"UI/Faces/{child.name}"));
                        faceTemp.transform.SetParent(SpecFaces);
                        faceTemp.transform.localScale = Vector3.one;
                        faceTemp.transform.localRotation = Quaternion.Euler(Vector3.zero);
                        faceTemp.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(index, index,0);
                        index++;
                    }
                }
                
                Desc.transform.localScale *= 1.2f;   
                openDesc = Desc;
            }
            openDesc.transform.LookAt(Camera.main.transform.position);
            openDesc.transform.rotation *= Quaternion.Euler(0,180,0);
        }
        
    }
    private void OnMouseExit() {
        if(animFin && transform.childCount > 0){
            transform.GetChild(0).position = transform.position ;
            hovered = false;
        }
        if(openDesc != null){
            Destroy(openDesc);
        }
        
    }

    void OnMouseDown(){
        if(hovered && !marketDice){
            diceRoller.AddDice(transform.GetChild(0).GetComponent<DiceRoll>().diceTemplate);
            animFin = false;
            transform.parent.GetComponent<DiceBoxController>().closeBox(transform);
        }
    }
}
