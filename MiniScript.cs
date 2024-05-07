using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class MiniScript : MonoBehaviour
{
    public bool selected = false;

    public EnemyTemplate enemyTemplate;

    public float CurrentHealth;
    public float MaxHealth;

    private GameObject Health;

    MapEvents mapEvents;
    public int TickDamage;
    public string EffectInflicted;

    private GameObject UI;
    private GameObject healthBar;
    private Slider slider;
    private TMP_Text textName;
    private TMP_Text healthStats;
    private TMP_Text diffIndicator;
    private TMP_Text atkIndicator;


    public void SetupMini(){
        GameObject EnemyInfo = GameObject.Instantiate(Resources.Load<GameObject>("UI/Prefabs/EnemyInfo"));
        EnemyInfo.transform.SetParent(transform);
        EnemyInfo.GetComponent<RectTransform>().anchoredPosition = Vector3.zero + new Vector3(0,enemyTemplate.height,0);
        EnemyInfo.GetComponent<RectTransform>().localEulerAngles = new Vector3(0,90,0);
        EnemyInfo.transform.name = "EnemyInfo";

        mapEvents = FindObjectOfType<MapEvents>();
        UI = transform.Find("EnemyInfo").gameObject;
        textName = UI.transform.Find("Name").gameObject.GetComponent<TMP_Text>();
        healthBar = UI.transform.Find("HealthBar").gameObject;
        slider = healthBar.GetComponent<Slider>();
        healthStats = healthBar.transform.Find("healthStats").gameObject.GetComponent<TMP_Text>();
        atkIndicator = UI.transform.Find("DMG").gameObject.GetComponent<TMP_Text>();
        diffIndicator = UI.transform.Find("Diff").gameObject.GetComponent<TMP_Text>();

        CurrentHealth = enemyTemplate.MaxHealth;
        MaxHealth = enemyTemplate.MaxHealth;
        slider.maxValue = MaxHealth;
        slider.value = MaxHealth;
        textName.text = enemyTemplate.name;
        healthStats.text = $"{CurrentHealth} / {MaxHealth}";
        diffIndicator.text = $"{translateDiff()}";
        atkIndicator.text = $"{enemyTemplate.atkPower}";
    }

    Dictionary <string, int> romanNumbersDictionary = new Dictionary <string, int>(){
        {"I",1}, {"IV",4}, {"V",5}, {"IX",9}, {"X",10}, {"XL",40}, {"L",50}
    };

    public string translateDiff(){
        string romanResult = "";

        int num = (int)enemyTemplate.enemyDiff;

       foreach(KeyValuePair<string, int> item in romanNumbersDictionary.Reverse()) {
            if (num <= 0) break;
            while (num >= item.Value) {
                romanResult += item.Key;
                num -= item.Value;
            }
        }
        return romanResult;
    }


    public void TickDamageInflicted(){
        UpdateHealth(TickDamage,true);
        if(mapEvents.SelectedMiniDied){
            StartCoroutine(mapEvents.MiniDamaged(0));
        }
    }

    
    void OnMouseDown(){
        mapEvents.SelectMiniToAttack(gameObject);
    }

    public void UpdateHealth(float ChangeFactor, bool Damaged){

        //ternary opperator for knowing if to add or subtract health
        float NewHealth = Damaged ? CurrentHealth -= ChangeFactor : CurrentHealth += ChangeFactor;
        StartCoroutine(HealthAnim(NewHealth));
        if(NewHealth <= 0){
            mapEvents.SelectedMiniDied = true;
        }
        CurrentHealth = NewHealth;
        healthStats.text = $"{CurrentHealth} / {MaxHealth}";
        
    }

    private IEnumerator HealthAnim(float NewHealth){

        float timeElapsed = 0;
        float duration = 2f;

        while(timeElapsed < duration){
            slider.value = Mathf.Lerp(slider.value, NewHealth,timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    
}
