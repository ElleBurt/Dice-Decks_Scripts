using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class DiceDisplay : MonoBehaviour
{

    private Vector3 basePos;
    private Quaternion baseRot;
    private Vector3 hoverOffset = new Vector3(0f, 2f, 0f);
    private bool MouseOver = false;

    public Transform dice;
    public GameObject ItemDesc;

    private GameObject openDesc;

    private bool soundPlayed = false;

    // Start is called before the first frame update
    void Awake()
    {
        basePos = transform.position;
        
    }

    //play noise on mouse over
    void OnMouseOver(){
        MouseOver = true;
        if(!soundPlayed && dice != null){
            StartCoroutine(PlaySound());
            soundPlayed = true;
        }        
    }

    void OnMouseExit(){
        MouseOver = false;
        soundPlayed = false;
    }

    //if dice added to display fix rotation
    public void DiceAdded(GameObject Dice){
        dice = Dice.transform;
        dice.rotation = new Quaternion(0,0,0,0);
    }
    public void DiceRemoved(){
        dice = null;
    }

    //move dice up or down
    void Update(){

        if(dice != null){
            
            if(!MouseOver){
                dice.position = basePos;
                Destroy(openDesc);
            }else{
                dice.position = basePos+hoverOffset;
                
                if(openDesc == null){
                    GameObject Desc = GameObject.Instantiate(ItemDesc,(transform.position + new Vector3(-3,5.5f,0)),Quaternion.identity);
                    GameObject dice = transform.GetChild(0).gameObject;
                    DiceTemplate dt = dice.GetComponent<DiceRoll>().diceTemplate;
                    Desc.transform.Find("Desc").GetComponent<TMP_Text>().text = $"{dt.name}\n{dt.description}";
                    Desc.transform.Find("RightSide").Find("SideInfo").Find("High").GetComponent<TMP_Text>().text = $"Highest: {dt.hiVal}";
                    Desc.transform.Find("RightSide").Find("SideInfo").Find("Low").GetComponent<TMP_Text>().text = $"Lowest: {dt.loVal}";
                    Desc.transform.SetParent(dice.transform);

                    Transform SpecFaces = Desc.transform.Find("RightSide").Find("Faces");

                    int index = 0;
                    foreach(Transform child in dice.transform){
                        if(Regex.IsMatch(child.name,@"\D") && child.name != "DiceResult" && child.name != "ItemDesc(Clone)"){
                            GameObject faceTemp = Instantiate(Resources.Load($"UI/Prefabs/FaceTemp") as GameObject);
                            faceTemp.transform.Find("Face").GetComponent<Image>().sprite = (Resources.Load<Sprite>($"UI/Faces/{child.name}"));
                            faceTemp.transform.SetParent(SpecFaces);
                            faceTemp.transform.localScale = Vector3.one;
                            faceTemp.transform.localRotation = Quaternion.Euler(Vector3.zero);
                            faceTemp.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(index, index,0);
                            index++;
                        }
                    }
                    

                    openDesc = Desc;
                }
                openDesc.transform.LookAt(Camera.main.transform.position);
                openDesc.transform.rotation *= Quaternion.Euler(0,180,0);
            }
        }
        
        
    }

    //play "tunked" sound
    IEnumerator PlaySound(){
        transform.GetComponentInParent<AudioSource>().Play();
        yield return null;
    }
}
