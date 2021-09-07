using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudController : MonoBehaviour
{   
    
    int currentHealth = 0;
    public int maxHealth = 100;
    public HealthBar healthBar;
    

    float currentO2 = 0;      // x sekundi kiseonika
    public float maxO2 = 5;
    public OxygenBar oxygenBar;
    bool isUnderwater = false;
    bool hasAir = true;
    int[] suffocationDmg = new int[5] {1,3,5,7,10};
    int suffocationStage = 0;
    float suffocationCooldown = 1;      // 1 sekunda
     

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        currentO2 = maxO2;
    }

    // Update is called once per frame
    void Update()
    {   

        // Health
        if(Input.GetKeyDown(KeyCode.T))
        {
            DamagePlayer(10);
        }

        if(Input.GetKeyDown(KeyCode.Y))
        {
            HealPlayer(10);
        }

        if(!hasAir)
        {
            if(suffocationCooldown > 0)
                suffocationCooldown -= Time.deltaTime;
            else
            {
                if(suffocationStage == suffocationDmg.Length - 1)
                {
                    DamagePlayer(suffocationDmg[suffocationStage]);
                    suffocationCooldown = 1;
                }
                else
                {
                    DamagePlayer(suffocationDmg[suffocationStage]);
                    suffocationCooldown = 1;
                    suffocationStage += 1;
                }
            }

        }
        else        // reset suffocation damage
        {
            suffocationCooldown = 1;
            suffocationStage = 0;
        }

        // Oxygen

        isUnderwater = CheckUnderwater();

        if(isUnderwater)
        {
            if(currentO2 > 0)
            {
                currentO2 -= Time.deltaTime;
                oxygenBar.SetOxygen(currentO2);
            }
            else
            {
                currentO2 = 0;
                oxygenBar.SetOxygen(currentO2);
                hasAir = false;
            }

        }
        else
        {
            float deltaO2 = 3 * Time.deltaTime;
            if(currentO2 + deltaO2 < maxO2)
            {
                currentO2 += deltaO2;
                oxygenBar.SetOxygen(currentO2);
            }
            else
            {
                currentO2 = maxO2;
                oxygenBar.SetOxygen(currentO2);
            }

            if(currentO2 > 0)
                hasAir = true;

            
        }
    }

    public void DamagePlayer(int amount)
    {
        currentHealth -= amount;
        healthBar.SetHealth(currentHealth);
    }

    public void HealPlayer(int amount)
    {
        if(currentHealth + amount < maxHealth)
            currentHealth += amount;
        else
            currentHealth = maxHealth;

        healthBar.SetHealth(currentHealth);
    }

    bool CheckUnderwater()
    {

        foreach(GameObject w in GameObject.FindGameObjectsWithTag("Ocean"))
        {
            float radius = w.GetComponent<CelestialObject>().ShapeSettings.radius;
            float dx = Camera.main.transform.position.x - w.transform.position.x;
            float dy = Camera.main.transform.position.y - w.transform.position.y;
            float dz = Camera.main.transform.position.z - w.transform.position.z;
            float temp = Mathf.Sqrt(dx*dx + dy*dy + dz*dz);

            if(temp < radius)
                return true;
        }

        return false;
    }
}
