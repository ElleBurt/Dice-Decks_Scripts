using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class MiniScript : MonoBehaviour
{
    private EnemyController eCtrl;
    public bool selected = false;

    

    //stats
    public float CurrentHealth;
    public float MaxHealth;
    public string diffTier;
    public int AttackPower;
    public int MoneyGain;
    public string eName;
    public int TickDamage;
    public string EffectInflicted;
    public float height;
    public float width;

    // UI elements
    private GameObject Health;
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
        EnemyInfo.GetComponent<RectTransform>().anchoredPosition = Vector3.zero + new Vector3(0,height,0);
        EnemyInfo.GetComponent<RectTransform>().localEulerAngles = new Vector3(0,90,0);
        EnemyInfo.transform.name = "EnemyInfo";

        eCtrl = FindObjectOfType<EnemyController>();
        UI = transform.Find("EnemyInfo").gameObject;
        textName = UI.transform.Find("Name").gameObject.GetComponent<TMP_Text>();
        healthBar = UI.transform.Find("HealthBar").gameObject;
        slider = healthBar.GetComponent<Slider>();
        healthStats = healthBar.transform.Find("healthStats").gameObject.GetComponent<TMP_Text>();
        atkIndicator = UI.transform.Find("DMG").gameObject.GetComponent<TMP_Text>();
        diffIndicator = UI.transform.Find("Diff").gameObject.GetComponent<TMP_Text>();

        CurrentHealth = MaxHealth;
        MaxHealth = MaxHealth;
        slider.maxValue = MaxHealth;
        slider.value = MaxHealth;
        textName.text = eName;
        healthStats.text = $"{CurrentHealth} / {MaxHealth}";
        diffIndicator.text = diffTier;
        atkIndicator.text = $"{AttackPower}";
    }

    
    void OnMouseDown(){
        eCtrl.SelectEnemy(gameObject);
    }

    public void UpdateHealth(float ChangeFactor, bool Damaged){

        //ternary opperator for knowing if to add or subtract health
        float NewHealth = Damaged ? CurrentHealth -= ChangeFactor : CurrentHealth += ChangeFactor;
        CurrentHealth = NewHealth < 0 ? 0 : NewHealth;
        StartCoroutine(HealthAnim(NewHealth));
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
