using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceDisplay : MonoBehaviour
{

    private Vector3 basePos;
    private Quaternion baseRot;
    private Vector3 hoverOffset = new Vector3(0f, 2f, 0f);
    private bool MouseOver = false;

    public Transform dice;

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
            }else{
                dice.position = basePos+hoverOffset;
            }
        }
        
        
    }

    //play "tunked" sound
    IEnumerator PlaySound(){
        transform.GetComponentInParent<AudioSource>().Play();
        yield return null;
    }
}
