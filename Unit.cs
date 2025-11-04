using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Script used to handle the stats of each unit (hp, attack, 
speed, etc.) and the actions that will affect a unit (taking damage,
getting healed, etc.)*/
public class Unit : MonoBehaviour
{
    public string name;
    public double maxHP;
    public double currentHP;
    public int attack;
    public int speed;
    public GameObject self;
    public string element;
    public int healingAmount;



    /* Function for the unit to take damage. Returns either
    true or false on whether or not the unit is dead (if their
    current HP is less than or equal to 0)*/
    public bool takeDamage(double dmg)
    {
        currentHP -= dmg;

        if (currentHP <= 0) return true;
        else return false;
    }

    /* Function for the unit to get healed. If the current HP
    goes over the max HP, the unit's HP is set to the max instead*/
    public void getHealed(double healAmount)
    {
        currentHP += healAmount;

        if (currentHP > maxHP) currentHP = maxHP;
    }
}
