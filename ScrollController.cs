using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollController : MonoBehaviour
{
    public bool isEventMap;
    private GenMapV2 genMapV2;
    private Animator animator;

    private void Awake() {
        genMapV2 = FindObjectOfType<GenMapV2>();
        animator = gameObject.GetComponent<Animator>();
    }

    private void OnDestroy() {
        if(isEventMap){
            genMapV2.RoundConclusion();
        }
    }

    private void DestroyMap(){
        Destroy(gameObject);
    }

    public void PlayAnimation(Texture2D texture,bool eventMap){
        Material newMat = new Material(gameObject.GetComponent<SkinnedMeshRenderer>().material);
        newMat.SetTexture("_BaseMap", texture);
        gameObject.GetComponent<SkinnedMeshRenderer>().material = newMat;

        isEventMap = eventMap;

        int speed = (int)genMapV2.gameSpeed;
        animator.SetFloat("SpeedMulti",(float)speed);
        animator.SetBool("MapSpawned",true);
    }

    public void IconSelected(){
        animator.SetBool("IconSelected",true);
    }

    private void DisplayIcons(){
        if(!isEventMap){
            genMapV2.displayIcons(true);
        }
    }

    private void HideIcons(){
        if(!isEventMap){
            genMapV2.displayIcons(false);
        }
    }
}
