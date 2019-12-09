using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCard : GenericCard
{
    public bool aoe;
    public int range;
    public int damage;
    public int decreaseNextWeaponCostBy;
    public bool decreaseNextWeaponCost;
    public int energyGain;
    public int moveGain;
    public int armorRemove;
    public bool usesCharge;
    public int chargeGain;
}
