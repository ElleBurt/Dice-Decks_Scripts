using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Text.RegularExpressions;

public class mapDecals : MonoBehaviour
{
    private bool active = false;

    public Vector3 size;
    public int connections;

    GameController gameController;
    // Start is called before the first frame update
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
    }


    //Basically just if clicked and camera at map then call the game controller to tell it
    void Update()
    {
        if(Input.GetMouseButtonDown(0) && active && gameController.cameraAlignedToMap){
            if(Regex.Replace(transform.parent.name,@"\D","") == gameController.currentRound.ToString()){
                StartCoroutine(gameController.IconSelected(transform));
            }
        }
        
    }

    //icon get bigger or icon get smaller me thinks this explains itself
    void OnMouseOver(){
        gameObject.GetComponent<DecalProjector>().size = size*1.25f;
        active = true;
    }

     void OnMouseExit(){
        gameObject.GetComponent<DecalProjector>().size = size;
        active = false;
    }
}
