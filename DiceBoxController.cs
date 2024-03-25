using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class DiceBoxController : MonoBehaviour
{
    private Dictionary<int,List<(Rarity, int)>> roundWeights = new Dictionary<int,List<(Rarity, int)>>();
    private Dictionary<Rarity,List<DiceTemplate>> diceWeights = new Dictionary<Rarity,List<DiceTemplate>>();

    public Transform DecalSpawns;

    private GameController gameController;
    private DiceRoller diceRoller;

    public Transform boxView;


    void Awake(){

        gameController = FindObjectOfType<GameController>();
        diceRoller = FindObjectOfType<DiceRoller>();
        DecalSpawns = GameObject.FindGameObjectsWithTag("DecalSpawns")[0].transform;
        boxView = GameObject.FindGameObjectsWithTag("DiceBox")[0].transform;

        int round = 1;
        int common = 70;
        int Uncommon = 25;
        int rare = 4;
        int epic = 1;

        foreach (Transform row in DecalSpawns)
        {   
            roundWeights.Add(round, new List<(Rarity, int)>{
                (Rarity.Common,common),
                (Rarity.Uncommon,Uncommon),
                (Rarity.Rare,rare),
                (Rarity.Epic,epic)
            });
            common -= 5;
            Uncommon -= -1;
            rare += 4;
            epic += 2;
            round++;
        }

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
            Transform spawn = transform.Find($"DSpawn{i+1}");

            //1-100 
            int rarityPercent = Random.Range(0,101);

            //gets the rarity by comparing the rarityPercent to the list of weights for the current round
            Rarity rarity = roundWeights[gameController.currentRound].FirstOrDefault(rar => rar.Item2 > rarityPercent).Item1;

            //gets a random dice template from the dict of rarities and templates
            DiceTemplate diceTemp = diceWeights[rarity][Random.Range(0,diceWeights[rarity].Count)];

            GameObject Dice = GameObject.Instantiate(diceTemp.dice,spawn.position - new Vector3(0,0,1) ,Quaternion.identity);
            Dice.GetComponent<DiceRoll>().diceTemplate = diceTemp;
            Dice.transform.localScale /= 1.25f;
            Dice.GetComponent<Rigidbody>().isKinematic = true;
            Dice.transform.rotation = new Quaternion(0,0,0,0);
            Dice.transform.SetParent(spawn);
            StartCoroutine(liftDice(Dice));
        }
        StartCoroutine(diceView());
    }

    public void closeBox(Transform child){
        for(int i = 0; i < 3; i++){
            if(transform.GetChild(i) == child){
                Destroy(transform.GetChild(i).GetChild(0).gameObject);
            }else{
                transform.GetChild(i).GetChild(0).position -= new Vector3(0,2.5f,0);
                Destroy(transform.GetChild(i).GetChild(0).gameObject,3);
            }
        }
        StartCoroutine(ThrowBox());
    }

    public IEnumerator ThrowBox(){
        yield return new WaitForSeconds(0.2f);
        gameObject.GetComponent<Animator>().SetBool("ThrowBox", true);
        Destroy(gameObject,3f);
        yield return new WaitForSeconds(2);
        gameController.RoundConclusion();
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

    private IEnumerator liftDice(GameObject dice){
        yield return new WaitForSeconds(3f);
        Vector3 startPos = dice.transform.position;
        while(Vector3.Distance(dice.transform.position, startPos + new Vector3(0,3,0)) > 0.1f){
            dice.transform.position = Vector3.Lerp(dice.transform.position, startPos + new Vector3(0,3,0), 5f * Time.deltaTime);
            yield return new WaitForSeconds(0.01f);
        }
        foreach(Transform col in transform){
            col.gameObject.GetComponent<DiceBoxHover>().animFin = true;
        }
    }
}

