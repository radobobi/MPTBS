using UnityEngine;
using UnityEditor;
using System.Collections;

public enum UnitCombat {
    Melee = 0,
    Ranged = 1,
    Magic = 2,
    LENGTH = 3,
}

public enum UnitType {
    Swordsman = 0,
    Archer = 1,
    Sorceress = 2,
    LENGTH = 3,
}

public enum BaseSwordsman {
    maxHP = 25,
    minDmg = 5,
    maxDmg = 7,
    hitsPerTurn = 1,
    initiative = 2,
    block = 1,
    accuracy = 100,
    unitCombat = UnitCombat.Melee,
}

public class CONSTANTS : MonoBehaviour {

}
