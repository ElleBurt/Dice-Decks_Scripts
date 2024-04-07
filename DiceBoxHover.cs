using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class DiceBoxHover : MonoBehaviour
{
    public bool animFin = false;
    DiceRoller diceRoller;
    private bool hovered = false;
    public GameObject ItemDesc;

    private GameObject openDesc;

    void Awake(){
        diceRoller = FindObjectOfType<DiceRoller>();
    }
    private void OnMouseEnter() {
        if(animFin){
            transform.GetChild(0).position = transform.position + new Vector3(0, 2, 0);
            hovered = true;

            if(openDesc == null){
                GameObject Desc = GameObject.Instantiate(ItemDesc,(transform.position + new Vector3(0,5,0)),Quaternion.Euler(Vector3.zero));
                GameObject dice = transform.GetChild(0).gameObject;
                DiceTemplate dt = dice.GetComponent<DiceRoll>().diceTemplate;
                Desc.transform.Find("Desc").GetComponent<TMP_Text>().text = $"{dt.name}\n{dt.description}";
                Desc.transform.Find("RightSide").Find("SideInfo").Find("High").GetComponent<TMP_Text>().text = $"Highest: {dt.hiVal}";
                Desc.transform.Find("RightSide").Find("SideInfo").Find("Low").GetComponent<TMP_Text>().text = $"Lowest: {dt.loVal}";

                Transform SpecFaces = Desc.transform.Find("RightSide").Find("Faces");

                int index = 0;
                foreach(Transform child in dice.transform){
                    if(Regex.IsMatch(child.name,@"\D") && child.name != "DiceResult"){
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
        if(animFin){
            transform.GetChild(0).position = transform.position ;
            hovered = false;
        }
        if(openDesc != null){
            Destroy(openDesc);
        }
        
    }
    void Update(){
        if(hovered && Input.GetMouseButtonDown(0)){
            diceRoller.AddDice(transform.GetChild(0).GetComponent<DiceRoll>().diceTemplate);
            animFin = false;
            transform.parent.GetComponent<DiceBoxController>().closeBox(transform);
        }
    }
}
