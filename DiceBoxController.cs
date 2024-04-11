using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class DiceBoxController : MonoBehaviour
{
    private Dictionary<Rarity, int> roundWeights = new Dictionary<Rarity, int>(){
        {Rarity.CurrentlyImpossible,100},
        {Rarity.Legendary,95},
        {Rarity.Epic,80},
        {Rarity.Rare,40},
        {Rarity.Uncommon,15},
        {Rarity.Common,0},
    };
    private Dictionary<Rarity,List<DiceTemplate>> diceWeights = new Dictionary<Rarity,List<DiceTemplate>>();


    private GameController gameController;
    private DiceRoller diceRoller;

    public Transform boxView;


    void Awake(){

        gameController = FindObjectOfType<GameController>();
        diceRoller = FindObjectOfType<DiceRoller>();
        boxView = GameObject.FindGameObjectsWithTag("DiceBox")[0].transform;

        foreach(DiceTemplate template in diceRoller.DiceBlueprints){
            if(diceWeights.ContainsKey(template.itemRarity)){
                diceWeights[template.itemRarity].Add(template);
            }else{
                diceWeights.Add(template.itemRarity, new List<DiceTemplate>{template});
            }
        }
        
    }

    private void SpawnDice(){

        for(int i = 0; i < 3; i++){

            //gets the relative spawnPos for the dice
            Transform spawn = transform.Find($"DSP{i+1}");

            int baseRarityPerc = Mathf.Clamp(Mathf.CeilToInt(Mathf.Pow(gameController.currentRound,2) / Random.Range(1.2f,1.5f)),1,101);
            int maxRarityPerc = Mathf.Clamp(Mathf.CeilToInt(Mathf.Pow(gameController.currentRound,2)),1,101);

            //1-100 
            int rarityPercent = Random.Range(baseRarityPerc,maxRarityPerc);

            //gets the rarity by comparing the rarityPercent to the list of weights for the current round
            Rarity rarity = Rarity.Common;

            foreach(KeyValuePair<Rarity, int> kvp in roundWeights){
                if(rarityPercent > kvp.Value){
                    rarity = kvp.Key;
                    break;
                }
            }

            int diceIndex = Random.Range(0,diceWeights[rarity].Count);
            //gets a random dice template from the dict of rarities and templates
            DiceTemplate diceTemp = diceWeights[rarity][diceIndex];

            diceWeights[rarity].RemoveAt(diceIndex);

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
        StartCoroutine(diceView());
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
        gameController.RoundConclusion();
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

    private IEnumerator diceView(){
        Camera mainCamera = gameController.mainCamera;
        float cameraMoveSpeed = gameController.cameraMoveSpeed;
        yield return new WaitForSeconds(1.5f);
        while(Vector3.Distance(mainCamera.transform.position, boxView.position) > 0.1f){
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, boxView.position, cameraMoveSpeed * Time.deltaTime);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation ,Quaternion.Euler(30f,0f,0f), cameraMoveSpeed * Time.deltaTime);
            yield return new WaitForSeconds(0.01f);
        }
        
        
    }

}


