using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public string name;
    public int maxHP;
    public int currentHP;
    public int attack;
    public int speed;
    public GameObject self;

    public bool takeDamage(int dmg)
    {
        currentHP -= dmg;

        if (currentHP <= 0) return true;
        else return false;
    }

    public void getHealed(int healAmount)
    {
        currentHP += healAmount;

        if (currentHP > maxHP) currentHP = maxHP;
    }
}
