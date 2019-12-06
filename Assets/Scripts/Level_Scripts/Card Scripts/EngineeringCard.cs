using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineeringCard : GenericCard
{
    public int stunDuration;
    public bool persistent;
    public Player tauntedPlayer;
    public int damageIncrease;
    public int chargeDecrease;
    public int chargeGain;
    public Player targetPlayer;
    public int damageReduce;
    public IEnemy ignoreEnemy;
    public int drawAmount;
    public int discardAmount;
    public int drawMax;
    public int discardMax;
    public int discountAllWeaponsAmount;
    public bool grantAOE;
    public int increaseAOEbyAmount;
    public int discountWeapon;
    public bool onlyAfterEnemyDeath;
    public int increaseRange;
    public int armorCannotFallBelowAmount;
    public bool playWhenDrawn;
    public bool woundPlayer;
    public int gainPlating;
    public bool doubleDamage;
    public bool stopWhenRoomClear;
    public int setPlating;
    public bool choice;
    public int gainHealth;
    public bool exileNextCard;
    public GenericCard card;
    public bool makeEnemyTargetEnemies;
    public int spendExtraEnergy;
    public int extraCardsToPlay;
}
