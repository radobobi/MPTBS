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
    Mage = 2,
    LENGTH = 3,
}

public enum StatsOrder {
    UnitName = 0,
    MaxHP = 1,
    MinDmg = 2,
    MaxDmg = 3,
    HitsPerTurn = 4,
    Initiative = 5,
    Block = 6,
    Accuracy = 7,
    UnitCombat = 8,
    Regen = 9,
    LENGTH = 10,
}

public class CONSTANTS : MonoBehaviour {

}
