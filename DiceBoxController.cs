using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class DiceBoxController : MonoBehaviour
{
    private GameController gameController;
    private DiceRoller diceRoller;

    
    public bool wasBrought = false;

    void Awake(){

        gameController = FindObjectOfType<GameController>();
        diceRoller = FindObjectOfType<DiceRoller>();

        gameController.SetItemWeights();
        
    }

    private void SpawnDice(){

        for(int i = 0; i < 3; i++){

            //gets the relative spawnPos for the dice
            Transform spawn = transform.Find($"DSP{i+1}");

            (Rarity, int) values = gameController.RandomItem("Dice");

            Rarity rarity = values.Item1;
            int index = values.Item2;

            //gets a random dice template from the dict of rarities and templates
            DiceTemplate diceTemp = gameController.ItemWeights[rarity].Item1[index];

            gameController.ItemWeights[rarity].Item1.RemoveAt(index);

            GameObject Dice = GameObject.Instantiate(diceTemp.dice,spawn.position ,Quaternion.identity);
            Dice.transform.SetParent(spawn);
            Dice.GetComponent<DiceRoll>().diceTemplate = diceTemp;
            Rigidbody rb = Dice.GetComponent<Rigidbody>();
            Dice.transform.rotation = new Quaternion(0,0,0,0);
            rb.isKinematic = true;
            
            
            foreach(Transform col in transform){
                if(col.CompareTag("DiceBoxSpawn")){
                    col.gameObject.GetComponent<DiceBoxHover>().animFin = true;
                }
            }
        }
        gameController.MoveCameraTo(GameObject.FindGameObjectsWithTag("DiceTrayView")[0].transform,Vector3.zero);
    }

    public void closeBox(Transform child){
        foreach(Transform col in transform){
            if(col.CompareTag("DiceBoxSpawn")){
                col.gameObject.GetComponent<DiceBoxHover>().animFin = false;
                col.gameObject.GetComponent<DiceBoxHover>().hovered = false;
                col.GetComponent<CapsuleCollider>().enabled = false;
                
            }
        }

        Destroy(child.gameObject,0.1f);
        StartCoroutine(ThrowBox());
    }

    public IEnumerator ThrowBox(){
        yield return new WaitForSeconds(0.2f);
        foreach(Transform col in transform){
            if(col.CompareTag("DiceBoxSpawn") && col.childCount > 0){
                StartCoroutine(shrinkDice(col.GetChild(0)));
                
            }
        }
        yield return new WaitForSeconds(0.5f);
        gameObject.GetComponent<Animator>().SetBool("ThrowBag", true);
        Destroy(gameObject,3f);
        yield return new WaitForSeconds(1);

        if(wasBrought){
            MarketEventController MEC = FindObjectOfType<MarketEventController>();
            MEC.ProcessBoosters(); 
        }else{
            gameController.RoundConclusion();
        }
        
    }

    private IEnumerator shrinkDice(Transform die){
        while(Vector3.Distance(die.localScale, Vector3.zero) > 0.1f){
            die.localScale *= 0.9f;
            yield return null;
        }
        Destroy(die.gameObject);
    }

    public IEnumerator OpenSequence(){
        SpawnDice();
        yield return null;
    }

}


