using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniScript : MonoBehaviour
{
    private bool hovered = false;
    public bool selected = false;

    public int atkPow;

    public float CurrentHealth;
    public float MaxHealth;

    private GameObject Health;

    private delegate bool Comparison(float CurrentHealthVolume, float NewHealthVolume);

    MapEvents mapEvents;

    // Start is called before the first frame update
    void Awake()
    {
        mapEvents = FindObjectOfType<MapEvents>();
        Health = transform.Find("Vile").Find("Health").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if(hovered && Input.GetMouseButtonDown(0)){
            mapEvents.SelectMiniToAttack(gameObject);
        }
    }

    void OnMouseOver(){
        hovered = true;
    }
    void OnMouseExit(){
        hovered = false;
    }

    public void UpdateHealth(float ChangeFactor, bool Damaged){

        //ternary opperator for knowing if to add or subtract health
        float NewHealth = Damaged ? CurrentHealth -= ChangeFactor : CurrentHealth += ChangeFactor;

        float MinVileValue = 2.5f;
        float MaxVileValue = -1f;
        float HealthPercentile = NewHealth / MaxHealth;
        

        float CurrentHealthVolume = Health.GetComponent<Liquid>().fillAmount;
        float NewHealthVolume = MinVileValue + (MaxVileValue - MinVileValue) * HealthPercentile;
        
        //HealthText.text = MaxHealth.ToString() + "/" + NewHealth.ToString();

        if(CurrentHealth <= 0){
            mapEvents.SelectedMiniDied = true;
        }

        

        if(Damaged){
            StartCoroutine(AnimHealth((CurrentHealthVolume, NewHealthVolume) => CurrentHealthVolume < NewHealthVolume, CurrentHealthVolume, NewHealthVolume, true));
        }else{
            StartCoroutine(AnimHealth((CurrentHealthVolume, NewHealthVolume) => CurrentHealthVolume > NewHealthVolume, CurrentHealthVolume, NewHealthVolume, false));
        }

       
    }

    private IEnumerator AnimHealth(Comparison comp, float CurrentHealthVolume, float NewHealthVolume, bool Damaged){

        while(comp(CurrentHealthVolume,NewHealthVolume)){

            CurrentHealthVolume = Damaged ? CurrentHealthVolume + 0.1f : CurrentHealthVolume - 0.1f;

            Health.GetComponent<Liquid>().fillAmount = CurrentHealthVolume;

            yield return null;

        }
    }
}
