using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class DiceDisplay : MonoBehaviour
{

    ScoreCards scoreCards;

    private Vector3 basePos;
    private Quaternion baseRot;
    private Vector3 hoverOffset = new Vector3(0f, 2f, 0f);
    public bool MouseOver = false;

    public Transform dice;
    public GameObject ItemDesc;

    public GameObject openDesc;

    private bool soundPlayed = false;

    public bool Selected = false;
    private Coroutine sellAnimCoro;
    // Start is called before the first frame update
    void Awake()
    {
        basePos = transform.position;
        scoreCards = FindObjectOfType<ScoreCards>();
    }


    private void OnMouseDown() {
        Selected = !Selected;

        openDesc.transform.Find("Sell").GetComponent<DiceHeldInfoScript>().selected = Selected;
        openDesc.transform.Find("Sell").GetComponent<DiceHeldInfoScript>().SwapColor();

        if(sellAnimCoro != null){
            StopCoroutine(sellAnimCoro);
            sellAnimCoro = null;
        }

        if(Selected){
            sellAnimCoro = StartCoroutine(SellAnim());
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

        while(Selected){
            if(flip){
                endVal = baseRot * Quaternion.Euler(angleChange,0,0);
            }else{
                endVal = baseRot * Quaternion.Euler(-angleChange,0,0);
            }

            if(flipCount == maxFlips){
                endVal = baseRot;
            }

            dice.rotation = Quaternion.Slerp(dice.rotation, endVal,  Elapsed / TimeValue);

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


    //play noise on mouse over
    void OnMouseOver(){
        MouseOver = true;
        if(!soundPlayed && dice != null){
            StartCoroutine(PlaySound());
            soundPlayed = true;
        }        
    }

    void OnMouseExit(){
        if(!Selected){
            MouseOver = false;
            soundPlayed = false;
        }
    }

    //if dice added to display fix rotation
    public void DiceAdded(GameObject Dice){
        dice = Dice.transform;
        dice.rotation = new Quaternion(0,0,0,0);
        baseRot = dice.rotation;
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
                    GameObject Desc = GameObject.Instantiate(ItemDesc,(transform.position + new Vector3(-3,6f,0)),Quaternion.identity);
                    GameObject dice = transform.GetChild(0).gameObject;
                    DiceTemplate dt = dice.GetComponent<DiceRoll>().diceTemplate;
                    Desc.transform.Find("Desc").GetComponent<TMP_Text>().text = $"{dt.name}\n{dt.description}";
                    Desc.transform.Find("RightSide").Find("SideInfo").Find("High").GetComponent<TMP_Text>().text = $"High: {dt.hiVal}";
                    Desc.transform.Find("RightSide").Find("SideInfo").Find("Low").GetComponent<TMP_Text>().text = $"Low: {dt.loVal}";
                    Desc.transform.Find("Sell").GetChild(0).GetComponent<TMP_Text>().text = $"Sell: ${dt.baseSellValue}";
                    Desc.transform.SetParent(transform);
                    

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
