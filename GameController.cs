using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Camera Properties")]
    public Transform DiceView;
    public Transform MapView;
    public int cameraMoveSpeed;
    public bool cameraAlignedToMap = false;
    public bool cameraAlignedToDice = false;
    public Camera mainCamera;

    [Header("Map Properties")]
    public float TokenMoveSpeed;
    public GameObject ScrollPre;
    public GameObject PlayerTokenPrefab;
    GenMap genMap;
    GameObject PlayerToken;
    MapEvents mapEvents;

    [Header("Event")]
    public List<MapEventTemplate> Events = new List<MapEventTemplate>();

    [Header("Cards")]
    public List<CardTemplate> CardTemplates = new List<CardTemplate>();
    public GameObject cardPrefab;
    

    [Header("Health Properties")]
    public float MaxHealth;
    public float CurrentHealth;
    public TMP_Text HealthText;
    public GameObject HealthVile;    


    [Header("DiceRoller properties")]
    DiceRoller diceRoller;

    [Header("atkCardHolder Properties")]
    AtkCardHolder atkCardHolder;


    [Header("Game Properties")]
    public int currentRound = 1;
    Transform currentIconTransform;
    public Transform lastIconTransform;
    private GameObject Scroll;
    private delegate bool Comparison(float CurrentHealthVolume, float NewHealthVolume);

    
    void Start()
    {
        //get all scripts
        mainCamera = FindObjectOfType<Camera>();
        genMap = FindObjectOfType<GenMap>();       
        diceRoller = FindObjectOfType<DiceRoller>();
        atkCardHolder = FindObjectOfType<AtkCardHolder>();
        mapEvents = FindObjectOfType<MapEvents>();
    }

    //spawns in the starter dice and scroll, Generates map and moves camera to map. sets last icon to start icon
    void GameStarted(){
        CurrentHealth = 0;
        diceRoller.ActivateDice();
        UpdateHealth(MaxHealth,false);
        Scroll = GameObject.Instantiate(ScrollPre, new Vector3(4.9f, 1.3f, 113.2f), Quaternion.identity);
        Scroll.transform.rotation = Quaternion.Euler(-90f, -90f, 0f);
        genMap.IconGeneration();
        StartCoroutine(MapViewAnim());

        
    }

    //temp key press to start game
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K)){
            GameStarted();
        }
    }

    public void RoundConclusion(){
        currentRound++;
        Scroll = GameObject.Instantiate(ScrollPre, new Vector3(4.9f, 1.3f, 113.2f), Quaternion.identity);
        Scroll.transform.rotation = Quaternion.Euler(-90f, -90f, 0f);
        genMap.displayIcons(true);
        StartCoroutine(MapViewAnim());
    }

    public void UpdateHealth(float ChangeFactor, bool Damaged){

        //ternary opperator for knowing if to add or subtract health
        float NewHealth = Damaged ? CurrentHealth -= ChangeFactor : CurrentHealth += ChangeFactor;

        float MinVileValue = 9;
        float MaxVileValue = -7;
        float HealthPercentile = NewHealth / MaxHealth;
        

        float CurrentHealthVolume = HealthVile.GetComponent<Liquid>().fillAmount;
        float NewHealthVolume = MinVileValue + (MaxVileValue - MinVileValue) * HealthPercentile;
        
        HealthText.text = MaxHealth.ToString() + "/" + NewHealth.ToString();

        

        if(Damaged){
            StartCoroutine(AnimHealth((CurrentHealthVolume, NewHealthVolume) => CurrentHealthVolume < NewHealthVolume, CurrentHealthVolume, NewHealthVolume, true));
        }else{
            StartCoroutine(AnimHealth((CurrentHealthVolume, NewHealthVolume) => CurrentHealthVolume > NewHealthVolume, CurrentHealthVolume, NewHealthVolume, false));
        }

       
    }

    public void IncreaseMaxHealth(float amount){
        MaxHealth += amount;
         UpdateHealth(amount,false);
    }

    private IEnumerator AnimHealth(Comparison comp, float CurrentHealthVolume, float NewHealthVolume, bool Damaged){

        while(comp(CurrentHealthVolume,NewHealthVolume)){

            CurrentHealthVolume = Damaged ? CurrentHealthVolume + 0.1f : CurrentHealthVolume - 0.1f;

            HealthVile.GetComponent<Liquid>().fillAmount = CurrentHealthVolume;

            yield return null;

        }
    }


    //called when an icon is clicked, moves player token to that icon, then disolves it and moves camera back to dice tray. clears environment and hides decals. scroll also rolls back up
    public IEnumerator IconSelected(Transform icon){

        currentIconTransform = icon;

        StartCoroutine(MovePlayerToken());

        yield return new WaitForSeconds(1.5f);

        StartCoroutine(DiceViewAnim());
        atkCardHolder.ReplenishCards();

        yield return new WaitForSeconds(1f);

        genMap.clearEnviro();
        genMap.displayIcons(false);

        yield return new WaitForSeconds(0.2f);

        Scroll.GetComponent<Animator>().SetBool("IconSelected", true);

        yield return new WaitForSeconds(2f);

        StartCoroutine(IconRoutine(icon.name));

    }

    public IEnumerator IconRoutine(string iconName){

        switch(iconName){
            case "Encounter":
                StartCoroutine(mapEvents.SpawnEnemy(currentRound));
            break;

            case "Card Booster":
                mapEvents.SpawnBooster();
            break;

            default:

                foreach(MapEventTemplate eventTemplate in Events){
                    if(iconName == eventTemplate.name){
                        StartCoroutine(mapEvents.SpawnEvent(eventTemplate));
                        break;
                    }
                }

            break;
        }
        yield return null;
    }

    //drops the player token at the current icon 
    public IEnumerator DropPlayerToken(){
        genMap.HighlightPaths(lastIconTransform);
        
        float tokenOffset = Mathf.Abs(Vector3.Distance(lastIconTransform.position, new Vector3(3,0,0)))/17.5f;
        float tokenYOffset = -4f;
        

        if(PlayerToken == null){
            PlayerToken = GameObject.Instantiate(PlayerTokenPrefab, lastIconTransform.position + new Vector3(0,40,0), Quaternion.Euler(-90,0,0));
        }
        while(Vector3.Distance(PlayerToken.transform.position, (lastIconTransform.position + new Vector3(0,tokenYOffset + tokenOffset,0))) > 0.1f){
            PlayerToken.transform.position = Vector3.Lerp(PlayerToken.transform.position, (lastIconTransform.position + new Vector3(0,tokenYOffset + tokenOffset,0)), TokenMoveSpeed * Time.deltaTime);
            yield return null;
        }
        cameraAlignedToMap = true;
    }

    //moves the player token to the selected icon
    public IEnumerator MovePlayerToken(){
        float tokenOffset = Mathf.Abs(Vector3.Distance(currentIconTransform.position, new Vector3(3,0,0)))/17.5f;
        float tokenYOffset = -4f;

        while(Vector3.Distance(PlayerToken.transform.position, (currentIconTransform.position + new Vector3(0,tokenYOffset + tokenOffset,0))) > 0.1f){
            PlayerToken.transform.position = Vector3.Lerp(PlayerToken.transform.position, (currentIconTransform.position + new Vector3(0,tokenYOffset + tokenOffset,0)), TokenMoveSpeed * Time.deltaTime);
            yield return new WaitForSeconds(0.01f);
        }

        lastIconTransform = currentIconTransform.transform;

        StartCoroutine(DissolveToken());
    }

    //dissolves the token
    IEnumerator DissolveToken(){
        float StartValue = 0f;
        float EndValue = 1f;
        float TimeValue = 1f;
        float Elapsed = 0f;

        while(Elapsed < TimeValue){
            float CurrentValue = Mathf.Lerp(StartValue, EndValue, Elapsed / TimeValue);

            Elapsed += Time.deltaTime;
            PlayerToken.GetComponent<MeshRenderer>().material.SetFloat("_Step", CurrentValue);

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        Destroy(PlayerToken);
    }
    
    //moves camera to the map
    public IEnumerator MapViewAnim(){
        cameraAlignedToDice = false;
        yield return new WaitForSeconds(2f);
        while(Vector3.Distance(mainCamera.transform.position, MapView.position) > 0.1f){
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, MapView.position, cameraMoveSpeed * Time.deltaTime);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation ,Quaternion.Euler(46.2f,0f,0f), cameraMoveSpeed * Time.deltaTime);
            yield return new WaitForSeconds(0.01f);
        }
        StartCoroutine(DropPlayerToken());
    }

    //moves camera to the dice tray
    public IEnumerator DiceViewAnim(){
        cameraAlignedToMap = false;
        yield return new WaitForSeconds(0.1f);
        while(Vector3.Distance(mainCamera.transform.position, DiceView.position) > 0.1f){
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, DiceView.position, cameraMoveSpeed * Time.deltaTime);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation ,Quaternion.Euler(24.4f,0f,0f), cameraMoveSpeed * Time.deltaTime);
            yield return new WaitForSeconds(0.01f);
        }
        cameraAlignedToDice = true;
        Destroy(Scroll, 1f);
    }

   

}
