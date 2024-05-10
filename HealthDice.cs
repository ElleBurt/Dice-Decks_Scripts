using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDice : MonoBehaviour
{
    public DiceTemplate diceTemplate;
    public Transform emptyFacingup;

    public Transform DiceSlot;

    public bool hasBeenRolled = false;
    public bool accountedFor = false;

    public string faceName;
    public bool inScoringPhase = false;

   private float forceX, forceY,forceZ;

   private float skewTolerance = 0.01f;

   [Header("Audio")]
   private AudioSource audioSource;


    public bool DestroyMe = false;
   
   Rigidbody body;

   CampFireController CFcontroller;
   DiceRoller diceRoller;
   


   private void Awake(){
        audioSource = GetComponent<AudioSource>();
        CFcontroller = FindObjectOfType<CampFireController>();
        diceRoller = FindObjectOfType<DiceRoller>();
        body = GetComponent<Rigidbody>();
        transform.rotation = new Quaternion(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360),0);
   }

   private void Update(){
        if (body != null){
            if (body.velocity.magnitude > 0){
                hasBeenRolled = true;
            }
            
            if(body.velocity.magnitude == 0 && hasBeenRolled && !inScoringPhase){

                if(IsLevel()){
                    
                    if(!accountedFor){
                        body.isKinematic = true;
                        accountedFor = true;
                        CFcontroller.processRoll(gameObject);
                    }

                }else{

                    diceRoller.callReroll(gameObject);

                }
            }
        }

        if(DestroyMe){
            StartCoroutine(shrinkDice());
        }
   }

    private IEnumerator shrinkDice(){
        yield return new WaitForSeconds(0.5f);
        while(Vector3.Distance(transform.localScale, Vector3.zero) > 0.1f){
            transform.localScale *= 0.9f;
            yield return null;
        }
        Destroy(gameObject);
    }

   private bool IsLevel(){
        foreach(Transform child in transform ){
            
            if(child.GetComponent<ParticleSystem>() != null){
                continue;
            }

            if(Vector3.Dot(child.forward, Vector3.up)>(1f - skewTolerance)){

                string name = child.name.Contains(".") ? child.name.Substring(0,child.name.Length - 4) : child.name;

                faceName = name;
                emptyFacingup = child.transform;
                
                return true;
            }
        }
        return false;
   }

    private void OnCollisionEnter(Collision other){
        StartCoroutine(PlayHit());
    }

    IEnumerator PlayHit(){

        audioSource.pitch = Random.Range(0.9f,1.4f);
        audioSource.PlayOneShot(diceRoller.diceHit);
        
        yield return new WaitForSeconds(1f);
        
    }
    
}
