using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBoosterController : MonoBehaviour
{   
    GameController gameController;
    Camera mainCamera;
    float cameraMoveSpeed;
    Transform MapView;
    public Vector3 cardSelectionPos;
    public int cardsReady = 0;
    CardHolder cardHolder;

    void Awake(){
        gameController = FindObjectOfType<GameController>();
        cardHolder = FindObjectOfType<CardHolder>();
        mainCamera = gameController.mainCamera;
        cameraMoveSpeed = gameController.cameraMoveSpeed;
        MapView = gameController.MapView;
    } 

    void Update(){
        if(cardsReady == 3){
            foreach(Transform card in transform.Find("cardSpawnPoint")){
                card.gameObject.GetComponent<CardController>().canPickup = true;
            }
        }
    }

    public void cardSelected(GameObject selectedCard){

        cardHolder.CardAdded(selectedCard.GetComponent<CardController>().cardTemplate);

        foreach(Transform card in transform.Find("cardSpawnPoint")){
            card.gameObject.GetComponent<CardController>().canPickup = false;
            Destroy(card.gameObject, 0.3f); 
        }

        gameObject.GetComponent<Animator>().SetBool("ThrowPack",true);

        Destroy(gameObject, 3f);

        gameController.RoundConclusion();
    }

    public IEnumerator OpenSequence(){
        StartCoroutine(MapViewAnim());
        for(float i = 1f; i < 4f; i++){
            AddCard(i/4f);
        }
        yield return new WaitForSeconds(2);

        Vector3 moveTo = cardSelectionPos - new Vector3(15,0,0);
        foreach(Transform card in transform.Find("cardSpawnPoint")){
            StartCoroutine(MoveCard(card,moveTo));
            moveTo += new Vector3(15,0,0);
        }
    }

    private void AddCard(float offset){
        CardTemplate cardTemplate = gameController.CardTemplates[Random.Range(0,gameController.CardTemplates.Count)];
        GameObject card = GameObject.Instantiate(gameController.cardPrefab, transform.Find("cardSpawnPoint").position + new Vector3(0,0,-offset), Quaternion.identity * Quaternion.Euler(0,0,0));
        card.transform.SetParent(transform.Find("cardSpawnPoint"));
        card.transform.localScale *= 2;
        card.GetComponent<CardController>().cardTemplate = cardTemplate;
        card.GetComponent<CardController>().SetupCard();
    }
    
    public IEnumerator MapViewAnim(){
        yield return new WaitForSeconds(2f);
        while(Vector3.Distance(mainCamera.transform.position, MapView.position) > 0.1f){
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, MapView.position, cameraMoveSpeed * Time.deltaTime);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation ,Quaternion.Euler(46.2f,0f,0f), cameraMoveSpeed * Time.deltaTime);
            yield return new WaitForSeconds(0.01f);
        }
    }

    public IEnumerator MoveCard(Transform card, Vector3 moveTo){
        while(Vector3.Distance(card.position,moveTo) > 0.1f && Quaternion.Angle(card.rotation, Quaternion.Euler(-30,180,0)) > 0.1f){
            card.position = Vector3.Lerp(card.position, moveTo, cameraMoveSpeed * Time.deltaTime);
            card.rotation = Quaternion.Slerp(card.rotation , Quaternion.Euler(-30,180,0), cameraMoveSpeed * Time.deltaTime);
            yield return new WaitForSeconds(0.01f);
        }
        cardsReady++;
    }
}
