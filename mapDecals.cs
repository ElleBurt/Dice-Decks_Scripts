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
    GenMapV2 genMap;
    // Start is called before the first frame update
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
        genMap = FindObjectOfType<GenMapV2>();
    }


    //Basically just if clicked and camera at map then call the game controller to tell it
    void Update()
    {
        /* 
        if(Input.GetMouseButtonDown(0) && active && gameController.sceneStage == GameController.currentStage.MapView){
            if(Regex.Replace(transform.parent.name,@"\D","") == genMap.currentRow.ToString() && genMap.connectedIcons[gameController.lastIconTransform.gameObject].Contains(gameObject)){
                StartCoroutine(gameController.IconSelected(transform));
            }
        }
        */
    }

    void OnMouseDown(){
        StartCoroutine(genMap.IconSelected(transform));
    }

    void OnMouseOver(){
        transform.localScale = new Vector3(1,1,1)*1.25f;
        active = true;
    }

     void OnMouseExit(){
        transform.localScale = new Vector3(1,1,1);
        active = false;
    }
}
