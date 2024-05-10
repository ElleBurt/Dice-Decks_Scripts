using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameDifficulty{
    Explorer,
    Traveler,
    Survivor,
    Scavanger,
}

public enum GameLength{
    Short,
    Medium,
    Long,
    Marathon,
}

public class GenMapV2 : MonoBehaviour
{
    //enemy variables
    private int enemyRowGap;
    private bool miniBossesEnabled;

    //row variables
    private int totalRows;
    private int totalStages;
    private int rowsPerStage;

    //game variables
    private float GameProgression;
    private int currentRow;
    private float randomAidBias;
    
    public GameDifficulty gameDiff;
    public GameLength gameLength;

    void Start(){
        switch(gameLength){
            case GameLength.Short:
                totalStages = 3;
                rowsPerStage = 6;
            break;
            case GameLength.Medium:
                totalStages = 5;
                rowsPerStage = 5;
            break;
            case GameLength.Long:
                totalStages = 5;
                rowsPerStage = 8;
            break;
            case GameLength.Marathon:
                totalStages = 6;
                rowsPerStage = 10;
            break;
        }
        totalRows = totalStages*rowsPerStage;

        switch(gameDifficulty){
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

    }

    private void GameProgressed() {
        GameProgression = (currentRow / totalRows)*100;

        switch(gameDifficulty){
            case GameDifficulty.Explorer:
                randomAidBias = GameProgression / 1.5; //100% gameProgress is 66.6% R.A.B
            break;
            case GameDifficulty.Traveler:
                randomAidBias = GameProgression / 2.25; //100% gameProgress is 44.4% R.A.B
            break;
            case GameDifficulty.Survivor:
                randomAidBias = GameProgression / 4; //100% gameProgress is 25% R.A.B
            break;
            case GameDifficulty.Scavanger:
                randomAidBias = GameProgression / 8; //100% gameProgress is 12.5% R.A.B
            break;
        }
    }

    void GenMapV2Init(){
        for(int i = 0; i < totalRows; i++){
            GameObject row = GameObject.Instantiate(Resources.Load<GameObject>("UI/Prefabs/Rowx"));
            row.transform.parent = transform;
            row.transform.name = $"Row{i}";

        }
    }
}
