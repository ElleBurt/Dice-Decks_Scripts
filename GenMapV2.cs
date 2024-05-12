using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

// controlls the frequency of encounters in the game and the R.A.B.
public enum GameDifficulty{
    Explorer,
    Traveler,
    Survivor,
    Scavanger,
}

// controlls the amount of rows and stages in the game
public enum GameLength{
    Short,
    Medium,
    Long,
    Marathon,
}

// controlls things like animation speeds
public enum GameSpeed{
    Slow = 3,
    Fast = 2,
    Rapid = 1,
}

public class GenMapV2 : MonoBehaviour
{

    private static readonly Dictionary<Rarity, int> roundWeights = new Dictionary<Rarity, int>(){
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

    //camera Properties
    private Transform DiceView;
    private Transform MapView;
    public Coroutine cameraMove;
    private Camera mainCamera;

    //game object references
    private GameObject PlayerToken;

    //enemy variables
    private int enemyRowGap;
    private bool miniBossesEnabled;

    //row variables
    private int totalRows;
    private int totalStages;
    private int rowsPerStage;

    //icon variables
    public Sprite[] generalIconArray;
    public Transform lastIcon;
    public Transform currentIcon;

    //game variables
    private float gameProgression;
    public int currentRow;
    private float randomAidBias;

    private List<GameObject> DiceHeld = new List<GameObject>();
    private List<GameObject> CardsHeld = new List<GameObject>();

    public GameDifficulty gameDiff;
    public GameLength gameLength;
    public GameSpeed gameSpeed;

    //game stats
    private int totalHitsTaken = 0;
    private int totalDamageTaken = 0;
    private int totalEnemiesKilled = 0;
    private int totalMoneyHeld = 0;

    private float MaxHealth;
    private float CurrentHealth;

    //scripts
    private GameController gameController;
    private MapEvents mapEvents;
    private DiceRoller diceRoller;
    private CardHolder cardHolder;
    
    //enviroment variables
    private TMP_Text HealthText;
    private TMP_Text moneyText;
    private GameObject HealthVile;   
    private GameObject Scroll;     
    private GameObject sceneLights;
    private GameObject coinPrefab;
    private Coroutine movingToken = null;
    private Coroutine droppingToken = null;



    void Start(){
        switch(gameLength){
            case GameLength.Short:
                totalStages = 3;
                rowsPerStage = 6;
            break;
            case GameLength.Medium:
                totalStages = 5;
                rowsPerStage = 7;
            break;
            case GameLength.Long:
                totalStages = 7;
                rowsPerStage = 8;
            break;
            case GameLength.Marathon:
                totalStages = 8;
                rowsPerStage = 9;
            break;
        }
        totalRows = totalStages*rowsPerStage;

        switch(gameDiff){
            case GameDifficulty.Explorer:
                miniBossesEnabled = false;
                enemyRowGap = 3;
            break;
            case GameDifficulty.Traveler:
                miniBossesEnabled = true;
                enemyRowGap = 3;
            break;
            case GameDifficulty.Survivor:
                miniBossesEnabled = true;
                enemyRowGap = 2;
            break;
            case GameDifficulty.Scavanger:
                miniBossesEnabled = true;
                enemyRowGap = 1;
            break;
        }
        generalIconArray = Resources.LoadAll<Sprite>("UI/MapIcons/InRotation");

        float totalHeight = (gameObject.GetComponent<GridLayoutGroup>().cellSize.y + gameObject.GetComponent<GridLayoutGroup>().spacing.y) * totalRows;

        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(gameObject.GetComponent<RectTransform>().sizeDelta.x , totalHeight);

        gameController = FindObjectOfType<GameController>();
        diceRoller = FindObjectOfType<DiceRoller>();
        mapEvents = FindObjectOfType<MapEvents>();
        cardHolder = FindObjectOfType<CardHolder>();

        HealthVile = GameObject.Find("HealthVile");
        HealthText = GameObject.Find("HealthCount").transform.GetChild(0).GetComponent<TMP_Text>();

        mainCamera = Camera.main;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K)){
            StartGame();
        }
    }


    private void StartGame(){
        CurrentHealth = 0;
        diceRoller.ActivateDice();
        UpdateHealth(MaxHealth,false);
        Scroll = GameObject.Instantiate(Resources.Load<GameObject>("Scene/Prefabs/Scroll"), new Vector3(4.9f, 1.3f, 113.2f), Quaternion.identity);
        Scroll.transform.rotation = Quaternion.Euler(-90f, -90f, 0f);
        GenMapV2Init();
        MoveCameraTo(lastIcon,true);
    }

    private void GameProgressed() {
        gameProgression = (currentRow / totalRows)*100;

        switch(gameDiff){
            case GameDifficulty.Explorer:
                randomAidBias = gameProgression / 1.125f; //100% gameProgress is 88.8% R.A.B
            break;
            case GameDifficulty.Traveler:
                randomAidBias = gameProgression / 1.5f; //100% gameProgress is 66.6% R.A.B
            break;
            case GameDifficulty.Survivor:
                randomAidBias = gameProgression / 2.5f; //100% gameProgress is 44.4% R.A.B
            break;
            case GameDifficulty.Scavanger:
                randomAidBias = gameProgression / 3f; //100% gameProgress is 33.3% R.A.B
            break;
        }
    }

    public void GenMapV2Init(){
        int rowsUntilEnemy = enemyRowGap;
        int Stage = totalRows / totalStages;
        for(int i = 0; i < totalRows; i++){

            GameObject MapRow = Instantiate(Resources.Load<GameObject>("UI/Prefabs/MapRow"),transform);

            GameObject row = Instantiate(Resources.Load<GameObject>("UI/Prefabs/Rowx"),MapRow.transform);
            row.transform.name = $"Row{i}";

            int iconsThisRow;
            if(i == 0 || Stage == 1 ){
                iconsThisRow = 1;
            }else if(i % 2 == 0){
                iconsThisRow = 2;
            }else{
                iconsThisRow = 3;
            }

            GameObject lines = null;
            Quaternion lineRot = Quaternion.Euler(0,0,0);
            float offset = 0;

            int iconsNextRow = Stage-1 == 1 ? 1 : ((i+1) % 2) == 0 ? 2 : 3;


            if(i == 0 || Stage == 1){
                lines = iconsNextRow == 3 ? Resources.Load<GameObject>("UI/Prefabs/1or3Lines") : Resources.Load<GameObject>("UI/Prefabs/1or2Lines");
            }else if(Stage -1 == 1){
                lines = iconsThisRow == 3 ? Resources.Load<GameObject>("UI/Prefabs/1or3Lines") : Resources.Load<GameObject>("UI/Prefabs/1or2Lines");
            }else{
                lines = Resources.Load<GameObject>("UI/Prefabs/2or3Lines");  
            }

            lineRot = iconsNextRow > iconsThisRow ? Quaternion.Euler(0,0,0) : Quaternion.Euler(0,0,180);
            offset = iconsNextRow > iconsThisRow ? 0 : 7f;

            GameObject rowLines = Instantiate(lines,MapRow.transform);
            rowLines.transform.localRotation = lineRot;
            rowLines.GetComponent<RectTransform>().offsetMin = new Vector2(0,offset);
            rowLines.GetComponent<RectTransform>().offsetMax = new Vector2(0,offset);


            row.GetComponent<GridLayoutGroup>().constraintCount = iconsThisRow;

            int spacing = iconsThisRow == 1 ? 0 : 11;

            row.GetComponent<GridLayoutGroup>().spacing = new Vector2(spacing,0);

            string name = i == 0 ? "Start" : Stage == 1 ? "Boss" : rowsUntilEnemy == 0 ? "Skull" : "";

            if(rowsUntilEnemy == 0){
                rowsUntilEnemy = enemyRowGap;
            }else{
                rowsUntilEnemy--;
            }

            if(name == ""){
                initIcon(name,iconsThisRow,true,true,row.transform);
            }else{
                initIcon(name,iconsThisRow,true,false,row.transform);
            }            

            if(Stage == 1){
                Stage = totalRows / totalStages;
            }else{
                Stage--;
            }
        }

        lastIcon = GameObject.FindGameObjectsWithTag("Start")[0].transform;
        lastIcon = lastIcon;
    }

    private void initIcon(string name, int iconsThisRow, bool iterate, bool randomise, Transform row ){
        int iterations = iterate ? iconsThisRow : 1;
        
        
        for(int j = 0; j < iterations; j++){

            Sprite img = randomise ? generalIconArray[Random.Range(0,generalIconArray.Length)] : Resources.Load<Sprite>($"UI/MapIcons/{name}");

            GameObject icon = Instantiate(Resources.Load<GameObject>("UI/Prefabs/Icon"),row);

            icon.GetComponent<Image>().sprite = img;
            icon.GetComponent<Image>().color = Color.black;
            icon.transform.tag = name == "Start" || name == "Boss" ? name : "MapIcon";

            icon.transform.name = icon.transform.name.Replace("(Clone)", "");

        }
    }


       //hides or shows the icons
    public void displayIcons(bool show){

        List<GameObject> icons = new List<GameObject>();
        icons.AddRange(GameObject.FindGameObjectsWithTag("MapIcon"));
        icons.AddRange(GameObject.FindGameObjectsWithTag("Boss"));
        icons.AddRange(GameObject.FindGameObjectsWithTag("Start"));

        float target = !show ? 0.0f : 255.0f;
        float fadeNum = show ? 0.0f : 255.0f;

        if(show){
            foreach(GameObject icon in icons){
                StartCoroutine(FadeFactor(icon.transform.GetChild(0).GetComponent<Image>(),show));
            }
        }else{
            foreach(GameObject icon in icons){
                StartCoroutine(FadeFactor(icon.transform.GetChild(0).GetComponent<Image>(),show));
            }
            
        }
        
    }

    public List<DiceTemplate> RandomDice(int amount){

        int baseRarityPerc = Mathf.CeilToInt(randomAidBias);
        int maxRarityPerc = Mathf.Clamp(Mathf.CeilToInt(gameProgression * 1.5f),1,101);

        //1-100 
        int rarityPercent = Random.Range(baseRarityPerc,maxRarityPerc);

        List<Rarity> rarities = new List<Rarity>();

        foreach(KeyValuePair<Rarity, int> kvp in roundWeights.OrderBy(x => x.Value)){
            if(rarityPercent < kvp.Value){
                break;
            }
            rarities.Add(kvp.Key);
        }

        rarities = rarities.OrderBy(x => rarityMap[x]).ToList();

        List<DiceTemplate> items = new List<DiceTemplate>();

        for(int i = 0; i < amount; i++){
            foreach(Rarity rarity in rarities){
                DiceTemplate[] diceTemplates = Resources.LoadAll<DiceTemplate>($"Dice/{rarity.ToString()}");
                List<DiceTemplate> templates = diceTemplates.ToList();

                if(templates.Count > 0){
                    DiceTemplate dice = templates[Random.Range(0, templates.Count)];
                    items.Add(dice);
                    templates.Remove(dice);
                    break;
                }else{
                    continue;
                }
            }
        }

        return items;
    }

    public List<CardTemplate> RandomCards(int amount){
        int baseRarityPerc = Mathf.CeilToInt(randomAidBias);
        int maxRarityPerc = Mathf.Clamp(Mathf.CeilToInt(gameProgression * 1.5f),1,101);

        //1-100 
        int rarityPercent = Random.Range(baseRarityPerc,maxRarityPerc);

        List<Rarity> rarities = new List<Rarity>();

        foreach(KeyValuePair<Rarity, int> kvp in roundWeights.OrderBy(x => x.Value)){
            if(rarityPercent < kvp.Value){
                break;
            }
            rarities.Add(kvp.Key);
        }

        rarities = rarities.OrderBy(x => rarityMap[x]).ToList();

        List<CardTemplate> items = new List<CardTemplate>();

        for(int i = 0; i < amount; i++){
            foreach(Rarity rarity in rarities){
                CardTemplate[] cardTemplates = Resources.LoadAll<CardTemplate>($"Cards/{rarity.ToString()}");
                List<CardTemplate> templates = cardTemplates.ToList();

                if(templates.Count > 0){
                    CardTemplate card = templates[Random.Range(0, templates.Count)];
                    items.Add(card);
                    templates.Remove(card);
                    break;
                }else{
                    continue;
                }
            }
        }

        return items;
    }

    public void RoundConclusion(){
        currentRow++;
        Scroll = GameObject.Instantiate(Resources.Load<GameObject>("Scene/Prefab/Scroll"), new Vector3(4.9f, 1.3f, 113.2f), Quaternion.identity);
        Scroll.transform.rotation = Quaternion.Euler(-90f, -90f, 0f);
        displayIcons(true);
        MoveCameraTo(lastIcon,true);
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

        StartCoroutine(AnimHealth(CurrentHealthVolume, NewHealthVolume));
    }

    public void IncreaseMaxHealth(float amount){
        MaxHealth += amount;
        UpdateHealth(amount,false);
    }

    private IEnumerator AnimHealth(float CurrentHealthVolume, float NewHealthVolume){

        float Elapsed = 0f;

        while(Elapsed < (int)gameSpeed){

            HealthVile.GetComponent<Liquid>().fillAmount = Mathf.Lerp(CurrentHealthVolume, NewHealthVolume, Elapsed / (int)gameSpeed);
            Elapsed += Time.deltaTime;

            yield return null;

        }
    }

    public IEnumerator IconSelected(Transform icon){

        currentIcon = icon;

        if(movingToken != null){
            StopCoroutine(movingToken);
            movingToken = null;
        }

        movingToken = StartCoroutine(MovePlayerToken());

        yield return new WaitForSeconds(0.5f);

        MoveCameraTo(GameObject.FindGameObjectsWithTag("DiceTrayView")[0].transform,false);

        yield return new WaitForSeconds(1f);

        //genMap.clearEnviro();
        displayIcons(false);

        yield return new WaitForSeconds(0.2f);

        Scroll.GetComponent<Animator>().SetBool("IconSelected", true);

        yield return new WaitForSeconds(2f);

        StartCoroutine(IconRoutine(icon.name));

    }

    public IEnumerator DropPlayerToken(){
        float Elapsed = 0f;
        //genMap.HighlightPaths(lastIcon);
        
        
        
        if(PlayerToken == null){
            PlayerToken = GameObject.Instantiate(Resources.Load<GameObject>("Scene/Prefabs/PlayerToken"),Vector3.zero,Quaternion.Euler(-90,0,0));
            PlayerToken.transform.SetParent(lastIcon);
            PlayerToken.transform.localPosition = new Vector3(0,0,-10);
        }

        while(Elapsed < (int)gameSpeed){
            PlayerToken.transform.localPosition = Vector3.Lerp(PlayerToken.transform.localPosition,Vector3.zero, Elapsed / (int)gameSpeed);
            Elapsed += Time.deltaTime;
            yield return null;
        }

        droppingToken = null;
    }

    public IEnumerator MovePlayerToken(){

        if(droppingToken != null){
            StopCoroutine(droppingToken);
            PlayerToken.transform.localPosition = Vector3.zero;
            droppingToken = null;
        }

        float Elapsed = 0f;
        PlayerToken.transform.SetParent(currentIcon);

        while(Elapsed < (int)gameSpeed){
            PlayerToken.transform.localPosition = Vector3.Lerp(PlayerToken.transform.localPosition, Vector3.zero, Elapsed / (int)gameSpeed);
            Elapsed += Time.deltaTime;
            yield return null;
        }

        lastIcon = currentIcon;

        Elapsed = 0f;
        float StartValue = 0f;
        float EndValue = 1f;

        while(Elapsed < (int)gameSpeed){
            float CurrentValue = Mathf.Lerp(StartValue, EndValue, Elapsed / (int)gameSpeed);
            PlayerToken.GetComponent<MeshRenderer>().material.SetFloat("_Step", CurrentValue);
            Elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(PlayerToken,0.1f);
    }

    public IEnumerator IconRoutine(string iconName){
        diceRoller.canRoll = false;
        switch(iconName){
            case "Encounter":
                StartCoroutine(mapEvents.SpawnEnemy(currentRow));
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

                foreach(MapEventTemplate eventTemplate in Resources.LoadAll<MapEventTemplate>("Scene/Template")){
                    if(iconName == eventTemplate.name){
                        StartCoroutine(mapEvents.SpawnEvent(eventTemplate));
                        break;
                    }
                }

            break;
        }
        yield return null;
    }
    

    //fades the icons in
    private IEnumerator FadeFactor(Image icon, bool show){
        float Elapsed = 0f;

        Color StartColor = show ? new Color(0,0,0,0) : new Color(0,0,0,1);
        Color EndColor = show ? new Color(0,0,0,1) : new Color(0,0,0,0);

        icon.color = StartColor;

        while(Elapsed < (int)gameSpeed){
    
            icon.color = Color.Lerp(icon.color, EndColor, Elapsed / (int)gameSpeed);
            Elapsed += Time.deltaTime;
            yield return null;
        }
    }



    private IEnumerator MoveCamera(Transform newView, bool toMap){

        float Elapsed = 0f;

        Transform target = toMap ? GameObject.FindGameObjectsWithTag("MapView")[0].transform : GameObject.FindGameObjectsWithTag("DiceTrayView")[0].transform;
        Vector3 targetPos = target.position;
        
        while(Elapsed < (int)gameSpeed){
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Elapsed / (int)gameSpeed);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation ,target.rotation, Elapsed / (int)gameSpeed);
            Elapsed += Time.deltaTime;
            yield return null;
        }

        if(!toMap){
            Destroy(Scroll, 1f);
        }else{
            if(droppingToken != null){
                StopCoroutine(droppingToken);
                droppingToken = null;
            }
            droppingToken = StartCoroutine(DropPlayerToken());
        }
    }

    public void MoveCameraTo(Transform newView, bool toMap){
        if(cameraMove != null){
            StopCoroutine(cameraMove);
            cameraMove = null;
        }
        cameraMove = StartCoroutine(MoveCamera(newView,toMap));
    }
}
