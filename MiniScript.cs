using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    // Start is called before the first frame update
    void Awake()
    {
        mapEvents = FindObjectOfType<MapEvents>();
        UI = transform.Find("EnemyInfo").gameObject;
        textName = UI.transform.Find("Name").gameObject.GetComponent<TMP_Text>();
        healthBar = UI.transform.Find("HealthBar").gameObject;
        slider = healthBar.GetComponent<Slider>();
        healthStats = healthBar.transform.Find("healthStats").gameObject.GetComponent<TMP_Text>();

    }

    public void SetupMini(){
        CurrentHealth = enemyTemplate.MaxHealth;
        MaxHealth = enemyTemplate.MaxHealth;
        slider.maxValue = MaxHealth;
        slider.value = MaxHealth;
        textName.text = enemyTemplate.name;
        healthStats.text = $"{CurrentHealth} / {MaxHealth}";
    }

    // Update is called once per frame
    void Update()
    {
        

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
