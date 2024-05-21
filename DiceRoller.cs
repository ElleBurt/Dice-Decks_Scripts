using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class DiceSlot{
    public Transform Slot { get; set; }
    public Vector3 SlotPos { get; set; }
    public bool SlotTaken {get; set; }
    public int slotNum { get; set; }
}


public class DiceRoller : MonoBehaviour
{
   [SerializeField] private float maxRandomForceValue;
   [SerializeField] TMP_Text scoreText;

   private float forceX, forceY,forceZ;

   private Score score;

   public GameObject RollButton;

   //Spawn point and forces to apply and direction to apply them
   public Vector3 ForceDirection;
   public float forceMagnitude;
   public GameObject DiceSpawnPosition;

   public bool canRoll = false;

   //This is the info for the display dice 
   public List<DiceSlot> DiceSlots = new List<DiceSlot>();

   public AudioClip diceHit;

   private GenMapV2 genMapV2;

   //Finds score script and also adds all the dice slots to a list 
   void Awake(){
        score = FindObjectOfType<Score>();
        genMapV2 = FindObjectOfType<GenMapV2>();

        Transform DiceDisplay = GameObject.FindWithTag("DiceDisplay").transform;

        int iter = 1;
        foreach(Transform child in DiceDisplay ){
            DiceSlots.Add(new DiceSlot{Slot = child, SlotPos = child.transform.position, SlotTaken = false, slotNum = iter});
            iter++;
        }
   }
   
   public void ActivateDice(){
        //gives you the starter 2x d6s 
        for(int i = 0; i < 2; i++){

            AddDice(Resources.Load<DiceTemplate>("Dice/Common/D4"));
            
        }
    
   }

   //adds the dice to the dice holder
   public void AddDice(DiceTemplate baseDice){

        bool slotFound = false;

        foreach (DiceSlot slot in DiceSlots){

            if(!slot.SlotTaken && !slotFound){

                GameObject Dice = GameObject.Instantiate(baseDice.dice, slot.SlotPos, Quaternion.identity);

                Dice.GetComponent<DiceRoll>().diceTemplate = baseDice;
                
                Dice.GetComponent<Rigidbody>().isKinematic = true;

                Dice.name = "dice-" + slot.slotNum.ToString();
                Dice.transform.SetParent(slot.Slot);

                slot.SlotTaken = true;

                DiceDisplay diceDisplay = slot.Slot.GetComponent<DiceDisplay>();

                diceDisplay.DiceAdded(Dice,ObjectState.Sell);

                genMapV2.DiceHeld.Add(Dice);

                slotFound = true;
            }
        }


   }

    //looks out for mouseDown to spawn in dice
    private bool hovered = false;
    private void Update(){

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit)){
                if(hit.transform.gameObject == RollButton){
                    hovered = true;
                    RollButton.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetFloat("_Hover", 1f);
                }else{
                    hovered = false;
                    RollButton.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetFloat("_Hover", 0f);
                }
            }

            if (Input.GetMouseButtonDown(0) && hovered && score.ScoringDice == false && canRoll){
                StartCoroutine(RollDice());
                score.canStartScoring = true;
                StartCoroutine(ButtonPress());
                canRoll = false;
            }
            
    }


    


    //rerolls any dice that arent level
   public void callReroll(GameObject dice){

        Rigidbody rb = dice.GetComponent<Rigidbody>();

        rb.isKinematic = true;
        
        Vector3 defPos = DiceSpawnPosition.transform.position;

        Vector3 spawnPoint = new Vector3(defPos.x + Random.Range(-3f,3f),defPos.y + Random.Range(-3f,3f),defPos.z + Random.Range(-3f,3f));

        dice.transform.position = DiceSpawnPosition.transform.position;

        

        rb.isKinematic = false;

        rb.AddForce(ForceDirection.normalized * forceMagnitude);

        forceX = Random.Range(0,maxRandomForceValue);
        forceY = Random.Range(0,maxRandomForceValue);
        forceZ = Random.Range(0,maxRandomForceValue);

        rb.AddTorque(forceX, forceY, forceZ);
   }

   
    IEnumerator ButtonPress(){
        RollButton.GetComponent<Animator>().SetBool("Pressed", true);
        yield return new WaitForSeconds(2f);
        RollButton.GetComponent<Animator>().SetBool("Pressed", false);
    }


    //Coroutine to roll all dice types held
    IEnumerator RollDice(){

        foreach (GameObject dice in genMapV2.DiceHeld){

            string slot = dice.transform.parent.name;

            DiceDisplay diceDisplay = GameObject.Find("diceDisplay").transform.Find(slot).GetComponent<DiceDisplay>();

            diceDisplay.DiceRemoved();

            Vector3 defPos = DiceSpawnPosition.transform.position;

            Vector3 spawnPoint = new Vector3(defPos.x + Random.Range(-3f,3f),defPos.y + Random.Range(-3f,3f),defPos.z + Random.Range(-3f,3f));


            dice.transform.position = spawnPoint;

            Rigidbody rb = dice.GetComponent<Rigidbody>();

            rb.isKinematic = false;

            rb.AddForce(ForceDirection.normalized * forceMagnitude);

            forceX = Random.Range(0,maxRandomForceValue);
            forceY = Random.Range(0,maxRandomForceValue);
            forceZ = Random.Range(0,maxRandomForceValue);

            rb.AddTorque(forceX, forceY, forceZ);

            yield return new WaitForSeconds(Random.Range(0.1f,0.3f));
        }
    }
}
