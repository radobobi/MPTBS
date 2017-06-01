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
    Level = 10,
    LENGTH = 11,
}

public enum LevelUpOptions {
    HPGain1 = 0,
    HPGain2 = 1,
    HPGain3 = 2,
    DmgGain1 = 3,
    InGain1 = 4,
    InGain2 = 5,
    BlockGain1 = 6,
    RegenGain1 = 7,
    APTGain1 = 8,
    LENGTH = 9,
}

public class CONSTANTS : MonoBehaviour {

}
