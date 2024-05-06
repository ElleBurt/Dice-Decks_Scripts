using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Text.RegularExpressions;

public class mapDecals : MonoBehaviour
{
    private bool active = false;

    public int connections;

    GameController gameController;
    GenMap genMap;
    // Start is called before the first frame update
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
        genMap = FindObjectOfType<GenMap>();
    }


    //Basically just if clicked and camera at map then call the game controller to tell it
    void Update()
    {
        if(Input.GetMouseButtonDown(0) && active && gameController.sceneStage == GameController.currentStage.MapView){
            if(Regex.Replace(transform.parent.name,@"\D","") == gameController.currentRound.ToString() && genMap.connectedIcons[gameController.lastIconTransform.gameObject].Contains(gameObject)){
                StartCoroutine(gameController.IconSelected(transform));
            }
        }
        
    }

    //icon get bigger or icon get smaller me thinks this explains itself
    void OnMouseOver(){
        transform.GetChild(0).localScale = new Vector3(1,1,1)*1.25f;
        active = true;
    }

     void OnMouseExit(){
        transform.GetChild(0).localScale = new Vector3(1,1,1);
        active = false;
    }
}
