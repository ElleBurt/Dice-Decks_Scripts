using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

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

    [Header("Items")]
    
    public GameObject cardPrefab;
    CardHolder cardHolder;

    public List<DiceTemplate> DiceTemplates = new List<DiceTemplate>();

    public List<CardTemplate> CardTemplates = new List<CardTemplate>();

    public List<BoosterTemplate> boosters = new List<BoosterTemplate>();

    public List<GameObject> DiceHeld = new List<GameObject>();
    public List<GameObject> CardsHeld = new List<GameObject>();

    

    [Header("Health Properties")]
    public float MaxHealth;
    public float CurrentHealth;
    public TMP_Text HealthText;
    public GameObject HealthVile;    


    DiceRoller diceRoller;

    AtkCardHolder atkCardHolder;


    [Header("Game Properties")]
    public int currentRound = 1;
    Transform currentIconTransform;
    public Transform lastIconTransform;
    private GameObject Scroll;
    private delegate bool Comparison(float CurrentHealthVolume, float NewHealthVolume);
    public TMP_Text moneyText;



    [Header("Game Stats")]
    public int HitsTaken = 0;
    public int DamageTaken = 0;
    public int EnemiesKilled = 0;
    public int MoneyHeld = 0;
    public int TickDamage = 0;


    [Header("Booster properties")]
    public Dictionary<Rarity, int> roundWeights = new Dictionary<Rarity, int>(){
        {Rarity.CurrentlyImpossible,100},
        {Rarity.Legendary,95},
        {Rarity.Epic,80},
        {Rarity.Rare,40},
        {Rarity.Uncommon,15},
        {Rarity.Common,0},
    };

    private static readonly Dictionary<Rarity, int> rarityMap = new Dictionary<Rarity, int>{
        {Rarity.CurrentlyImpossible,1},
        {Rarity.Legendary,2},
        {Rarity.Epic,3},
        {Rarity.Rare,4},
        {Rarity.Uncommon,5},
        {Rarity.Common,6},
    };
    
    //public Dictionary<Rarity,List<CardTemplate>> cardWeights = new Dictionary<Rarity,List<CardTemplate>>();

    public GameObject sceneLights;

    public GameObject coinPrefab;


    public List<int> diceResults = new List<int>();

    ScoreCards scoreCards;


    public Dictionary<Rarity, (List<DiceTemplate>, List<CardTemplate>) > ItemWeights = new Dictionary<Rarity, (List<DiceTemplate>, List<CardTemplate>) >();

    public void SetItemWeights(){

        ItemWeights = new Dictionary<Rarity, (List<DiceTemplate>, List<CardTemplate>)>(){
            {Rarity.CurrentlyImpossible,(new List<DiceTemplate>{}, new List<CardTemplate>{})},
            {Rarity.Legendary,(new List<DiceTemplate>{}, new List<CardTemplate>{})},
            {Rarity.Epic,(new List<DiceTemplate>{}, new List<CardTemplate>{})},
            {Rarity.Rare,(new List<DiceTemplate>{}, new List<CardTemplate>{})},
            {Rarity.Uncommon,(new List<DiceTemplate>{}, new List<CardTemplate>{})},
            {Rarity.Common,(new List<DiceTemplate>{}, new List<CardTemplate>{})},
        };

        foreach(DiceTemplate template in DiceTemplates){
            ItemWeights[template.itemRarity].Item1.Add(template);
        }

        foreach(CardTemplate template in CardTemplates){
            ItemWeights[template.itemRarity].Item2.Add(template);
        }

        foreach (KeyValuePair<Rarity, (List<DiceTemplate>, List<CardTemplate>) > kvp in ItemWeights){
             Debug.Log($"Key = {kvp.Key}: ");
             foreach(DiceTemplate template in kvp.Value.Item1){
                Debug.Log($"\n {template.name}");
             }
             foreach(CardTemplate template in kvp.Value.Item2){
                Debug.Log($"\n {template.name}");
             }
        }
           

    }

    public (Rarity, int) RandomItem(string type){

        int baseRarityPerc = Mathf.Clamp(Mathf.CeilToInt(Mathf.Pow(currentRound,2) / Random.Range(1.2f,1.5f)),1,101);
        int maxRarityPerc = Mathf.Clamp(Mathf.CeilToInt(Mathf.Pow(currentRound,2)),1,101);

        //1-100 
        int rarityPercent = Random.Range(baseRarityPerc,maxRarityPerc);

        List<Rarity> rarities = new List<Rarity>();

        foreach(KeyValuePair<Rarity, int> kvp in roundWeights.OrderBy(x => x.Value)){
            if(rarityPercent < kvp.Value){
                break;
            }
            rarities.Add(kvp.Key);
        }

        (Rarity, int) item = (Rarity.Common,0);

        rarities = rarities.OrderBy(x => rarityMap[x]).ToList();


        Debug.Log(string.Join(", ",rarities));



        if(type == "Dice"){
            foreach(Rarity rarity in rarities){
                if(ItemWeights[rarity].Item1.Count > 0){
                    item = (rarity,Random.Range(0,ItemWeights[rarity].Item1.Count));
                    break;
                }else{
                    continue;
                }
            }
        }else{
            foreach(Rarity rarity in rarities){
                if(ItemWeights[rarity].Item2.Count > 0){
                    item = (rarity,Random.Range(0,ItemWeights[rarity].Item2.Count));
                    break;
                }else{
                    continue;
                }
            }
        }

        return item;
    }


    public void UpdateMoney(int ammount, bool isBuying){

        if(!isBuying){
            bool corrupt = false;
            GameObject cardTriggered = null;

            foreach(GameObject card in CardsHeld){
                if(card.GetComponent<CardController>().cardType == CardType.CorruptCoins){
                    corrupt = true;
                    cardTriggered = card;
                }
            }

            for(int i = 0; i < ammount; i++){
                int toAdd = corrupt ? Random.Range(1,4) == 3 ? 2 : 1 : 1;

                if(toAdd == 2){
                    CardController cardController = cardTriggered.GetComponent<CardController>();
                    scoreCards.ScoreAnim(CardType.CorruptCoins);
                }
                
                foreach(GameObject MoneyText in GameObject.FindGameObjectsWithTag("MoneyText")){
                    TMP_Text textElem = MoneyText.GetComponent<TMP_Text>();
                    textElem.text = $"${int.Parse(textElem.text.Substring(1)) + toAdd}";
                    MoneyHeld = int.Parse(textElem.text.Substring(1));
                }
                
            }

            foreach(GameObject coinSpawn in GameObject.FindGameObjectsWithTag("CoinSpawn")){
                if(coinSpawn.transform.parent.name == "MarketStand(Clone)"){
                    StartCoroutine(dropCoins(ammount, coinSpawn, 0.5f));
                }else{
                    StartCoroutine(dropCoins(ammount, coinSpawn, 1f));
                }
                
            }
        }else{
            foreach(GameObject MoneyText in GameObject.FindGameObjectsWithTag("MoneyText")){
                TMP_Text textElem = MoneyText.GetComponent<TMP_Text>();
                textElem.text = $"${int.Parse(textElem.text.Substring(1)) - ammount}";
                MoneyHeld = int.Parse(textElem.text.Substring(1));
            }
        }
        

        
    }

    private IEnumerator dropCoins(int ammount, GameObject coinSpawn, float scaleFactor){
        for(int i = 0; i < ammount; i++){
            GameObject coin = GameObject.Instantiate(coinPrefab, coinSpawn.transform.position, Quaternion.identity);
            coin.transform.localScale *= scaleFactor;
            coin.transform.parent = coinSpawn.transform;
            yield return new WaitForSeconds(0.2f);
            Destroy(coin, 0.5f);
        }
    }

    public void changeLights(Color newCol){
         
        foreach(Transform light in sceneLights.transform){
            
            Color Starter = light.gameObject.GetComponent<Light>().color;
            Color AimCol = newCol;

            StartCoroutine(lerpLights(light,Starter,AimCol));
        }
    }

    private IEnumerator lerpLights(Transform light, Color Start, Color newCol){

         float timeElapsed = 0f;
         float duration = 2f;

        while(timeElapsed < duration){

            light.gameObject.GetComponent<Light>().color = Color.Lerp(Start,newCol, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;

        }

        light.gameObject.GetComponent<Light>().color = newCol;
        
    }
    
    void Start()
    {
        //get all scripts
        mainCamera = FindObjectOfType<Camera>();
        genMap = FindObjectOfType<GenMap>();       
        diceRoller = FindObjectOfType<DiceRoller>();
        atkCardHolder = FindObjectOfType<AtkCardHolder>();
        mapEvents = FindObjectOfType<MapEvents>();
        cardHolder = FindObjectOfType<CardHolder>();
        scoreCards = FindObjectOfType<ScoreCards>(); 

        
    }
 
    //spawns in the starter dice and scroll, Generates map and moves camera to map. sets last icon to start icon
    void GameStarted(){
        CurrentHealth = 0;
        diceRoller.ActivateDice();
        UpdateHealth(MaxHealth,false);
        Scroll = GameObject.Instantiate(ScrollPre, new Vector3(4.9f, 1.3f, 113.2f), Quaternion.identity);
        Scroll.transform.rotation = Quaternion.Euler(-90f, -90f, 0f);
        genMap.IconGeneration();
        MoveCameraTo(lastIconTransform,new Vector3(0,13,-5));

        
    }

    float cameraZmin = -22;
    float cameraZmax = 45;
    Coroutine cameraMovingFromScroll;
    //temp key press to start game
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K)){
            GameStarted();
        }
        if(cameraAlignedToMap){
            Vector3 pos = mainCamera.transform.position;
            pos.z += Input.mouseScrollDelta.y*1.25f;


            if(pos.z > cameraZmin && pos.z < cameraZmax){
                if(Input.mouseScrollDelta.y < 0 && pos.z < 0){

                    if(cameraMovingFromScroll != null){
                        StopCoroutine(cameraMovingFromScroll);
                        cameraMovingFromScroll = null;
                    }

                    cameraMovingFromScroll = StartCoroutine(movingFromMap(new Vector3(107.68f,35.79f,-20.92f), Quaternion.Euler(18.7f,0,0)));

                }else if(Input.mouseScrollDelta.y > 0 && pos.z < 0){

                    if(cameraMovingFromScroll != null){
                        StopCoroutine(cameraMovingFromScroll);
                        cameraMovingFromScroll = null;
                    }

                    cameraMovingFromScroll = StartCoroutine(movingFromMap(lastIconTransform.position + new Vector3(0,13,-5), Quaternion.Euler(48f,0,0)));
                }else{
                    mainCamera.transform.position = pos;
                }

                
            }
        }
    }

    private IEnumerator movingFromMap(Vector3 TargetPos, Quaternion TargetRot){
        float timeElapsed = 0f;
        float duration = 2f;

        while(timeElapsed < duration){
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, TargetRot, timeElapsed / duration);
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, TargetPos, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
    }

    public void RoundConclusion(){
        currentRound++;
        Scroll = GameObject.Instantiate(ScrollPre, new Vector3(4.9f, 1.3f, 113.2f), Quaternion.identity);
        Scroll.transform.rotation = Quaternion.Euler(-90f, -90f, 0f);
        genMap.displayIcons(true);
        MoveCameraTo(lastIconTransform,new Vector3(0,13,-5));
    }

    public void UpdateHealth(float ChangeFactor, bool Damaged){

        //ternary opperator for knowing if to add or subtract health
        float NewHealth = Damaged ? CurrentHealth -= ChangeFactor : CurrentHealth += ChangeFactor;

        float MinVileValue = 4.89f;
        float MaxVileValue = -3.8f;
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

        MoveCameraTo(GameObject.FindGameObjectsWithTag("DiceTrayView")[0].transform,Vector3.zero);
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
        diceRoller.canRoll = false;
        switch(iconName){
            case "Encounter":
                StartCoroutine(mapEvents.SpawnEnemy(currentRound));
            break;

            case "Card Booster":
                mapEvents.SpawnBooster(false);
            break;

            case "Dice Booster":
                mapEvents.SpawnDiceBox(false);
            break;

            case "Boss":
                //mapEvents.SpawnDiceBox(false);
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
        
        
        Vector3 tokenOffset = lastIconTransform.position;
        

        if(PlayerToken == null){
            PlayerToken = GameObject.Instantiate(PlayerTokenPrefab, tokenOffset + new Vector3(0,30,0), Quaternion.Euler(-90,0,0));
        }
        while(Vector3.Distance(PlayerToken.transform.position,tokenOffset) > 0.1f){
            PlayerToken.transform.position = Vector3.Lerp(PlayerToken.transform.position,tokenOffset, TokenMoveSpeed * Time.deltaTime);
            yield return null;
        }
        cameraAlignedToMap = true;
    }

    //moves the player token to the selected icon
    public IEnumerator MovePlayerToken(){
        Vector3 tokenOffset = currentIconTransform.position;

        while(Vector3.Distance(PlayerToken.transform.position,tokenOffset) > 0.1f){
            PlayerToken.transform.position = Vector3.Lerp(PlayerToken.transform.position, tokenOffset, TokenMoveSpeed * Time.deltaTime);
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
    





    // Camera Functions
    public Coroutine cameraMove;


    private IEnumerator MoveCamera(Transform newView, Vector3 offset){

        cameraAlignedToMap = false;
        cameraAlignedToDice = false;
        
        Vector3 targetPos = newView.position + offset;
        Quaternion rotation = Quaternion.Euler(0f,0f,0f);


        if(newView.CompareTag("MapIcon") || newView.CompareTag("Start")){
            yield return new WaitForSeconds(2f);
            rotation = Quaternion.Euler(48f,0,0);
        }else{
            rotation = newView.rotation;
        }

        
        yield return new WaitForSeconds(0.1f);
        while(Vector3.Distance(mainCamera.transform.position, targetPos) > 0.1f){
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, cameraMoveSpeed * Time.deltaTime);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation ,rotation, cameraMoveSpeed * Time.deltaTime);
            yield return null;
        }

        if(newView.CompareTag("DiceTrayView")){
            cameraAlignedToDice = true;
            Destroy(Scroll, 1f);
        }else if(newView.CompareTag("MapIcon") || newView.CompareTag("Start")){
            StartCoroutine(DropPlayerToken());
        }
    }

    public void MoveCameraTo(Transform newView,Vector3 offset){
        if(cameraMove != null){
            StopCoroutine(cameraMove);
            cameraMove = null;
        }
        cameraMove = StartCoroutine(MoveCamera(newView,offset));
    }
   

}
