using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseHero: BaseClass
{
    public string heroType;
    public int strength;
    public int intelligence;
    public int dexterity;
    public int agility;

    public List<BaseAttack> magicAttacks = new List<BaseAttack>();
}
