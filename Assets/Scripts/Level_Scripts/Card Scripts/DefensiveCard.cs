using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefensiveCard : GenericCard
{
    public int decreaseDamage;
    public bool stacks;
    public bool ignoreAttack;
    public bool requiresSpace;
    public int range;
    public bool redirectToSelf;
    public int multiplyEnergyGainByAmount;
    public bool ignoreMaxEnergy;
    public int numberOfAttacksTaken;
    public Player targetPlayer;
    public int gainArmor;
    public bool spendCharge;
    public int amountOfCharge;
    public bool redirectToTarget;
    public bool inRange;
    public bool chooseLocation;
    public bool gainedEnergy;
    public bool reflectDamage;
    public bool ignoreArmor;
    public int weaponsPlayed;
    public bool drawNextCardAndPlayImmediately;
    public bool selfElusiveFromEnemies;
    public bool activateAfterAttacked;
}
