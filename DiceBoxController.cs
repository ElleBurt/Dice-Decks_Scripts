using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class DiceBoxController : MonoBehaviour
{
    private DiceRoller diceRoller;
    private GenMapV2 genMapV2;

    public bool fromMarket = false;

    void Awake(){

        genMapV2 = FindObjectOfType<GenMapV2>();
        diceRoller = FindObjectOfType<DiceRoller>();
        genMapV2 = FindObjectOfType<GenMapV2>();
    }

    private void SpawnDice(){
        int spawnIndex = 1;
        foreach(DiceTemplate diceTemplate in genMapV2.RandomDice(3)){

            Transform spawn = transform.Find($"DS{spawnIndex}");

            GameObject Dice = GameObject.Instantiate(diceTemplate.dice,spawn.position ,Quaternion.identity);
            Dice.transform.SetParent(spawn);
            Dice.GetComponent<DiceRoll>().diceTemplate = diceTemplate;
            Rigidbody rb = Dice.GetComponent<Rigidbody>();
            Dice.transform.rotation = new Quaternion(0,0,0,0);
            rb.isKinematic = true;
            spawn.GetComponent<DiceDisplay>().DiceAdded(Dice, ObjectState.Booster);

            spawnIndex++;
        }
        genMapV2.MoveCameraTo(genMapV2.DiceView,false);
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

        if(fromMarket){
            genMapV2.MoveCameraTo(GameObject.Find("MarketTableView").transform,false);
        }else{
            genMapV2.retractScroll();
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


