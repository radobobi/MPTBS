using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class Unit : MonoBehaviour {

    public static int UnitCount = 0;

    protected int _myID;
    public int MyID {
        get { return _myID; }
        set { _myID = value; }
    }

    protected string _myName;
    public string MyName {
        get { return _myName; }
        set { _myName = value; }
    }

    protected int _myType;
    public int MyType {
        get { return _myType; }
        set { _myType = value; }
    }

    protected int _XP;
    public int XP {
        get { return _XP; }
        set { _XP = value; }
    }

    protected int _maxHP;
    public int MaxHP {
        get { return _maxHP; }
    }

    protected int _currentHP = 0;
    public int CurrentHP {
        get { return _currentHP; }
    }

    protected int _initiative = 0;
    public int Initiative {
        get { return _initiative; }
    }

    protected int _minDmg = 0;
    public int MinDmg {
        get { return _minDmg; }
    }

    protected int _maxDmg = 0;
    public int MaxDmg {
        get { return _maxDmg; }
    }

    protected int _hitsPerTurn = 0;
    public int HitsPerTurn {
        get { return _hitsPerTurn; }
    }

    protected int _block = 0;
    public int Block {
        get { return _block; }
    }

    protected int _currentBlock = 0;
    public int CurrentBlock {
        get { return _currentBlock; }
    }

    protected float _accuracy = 0;
    public float Accuracy {
        get { return _accuracy; }
    }

    protected int _unitRange;
    public int UnitRange {
        get { return _unitRange; }
    }

    private static GameObject unitObj;
    public static GameObject UnitObj {
        get {
            if (unitObj == null) {
                unitObj = new GameObject("Unit Object");
            }
            return unitObj;
        }
    }

    public static Unit CreateMyUnit() {
        var thisObj = UnitObj.AddComponent<Unit>();
        //calls Start() on the object and initializes it.
        UnitCount++;
        return thisObj;
    }

    public Unit SetParams(int unitType, string name) {
        _myType = unitType;
        _myID = UnitCount;
        print("Unit #" + _myID);
        if (_myType == (int)UnitType.Swordsman) {
            _myName = name;
            if (_myName != "") {
                _myName = "Swordsman";
            }
            _maxHP = (int)BaseSwordsman.maxHP;
            _currentHP = _maxHP;
            _XP = 0;
            _minDmg = (int)BaseSwordsman.minDmg;
            _maxDmg = (int)BaseSwordsman.maxDmg;
            _hitsPerTurn = (int)BaseSwordsman.hitsPerTurn;
            _initiative = (int)BaseSwordsman.initiative;
            _block = (int)BaseSwordsman.block;
            _currentBlock = _block;
            _accuracy = (int)BaseSwordsman.accuracy;
            _unitRange = (int)BaseSwordsman.unitCombat;
        }
        return this;
    }

    /* select a random target from the possible group of targets. Melee units can only target the closest row of units (so they target melee first then ranged then magic). */
    public Unit chooseTarget(Army anArmy) {
        Unit target = null;

        List<Unit> meleeUnits = anArmy.Melee;
        List<Unit> rangeUnits = anArmy.Ranged;
        List<Unit> magicUnits = anArmy.Magic;

        int randFrom = 0;

        randFrom += meleeUnits.Count;

        if (_unitRange != (int)UnitCombat.Melee || randFrom == 0) {
            randFrom += rangeUnits.Count;
        }
        if (_unitRange != (int)UnitCombat.Melee || randFrom == 0) {
            randFrom += magicUnits.Count;
        }
        
        int targetIndex = UnityEngine.Random.Range(1, randFrom + 1) - 1;

        if (targetIndex < meleeUnits.Count) {
            target = meleeUnits[targetIndex];
        }
        else if (targetIndex < meleeUnits.Count + rangeUnits.Count) {
            target = rangeUnits[targetIndex - meleeUnits.Count];
        }
        else if (targetIndex < meleeUnits.Count + rangeUnits.Count + magicUnits.Count) {
            target = magicUnits[targetIndex - meleeUnits.Count - rangeUnits.Count];
        }

        return target;
    }

    public bool applyDmgFrom(Unit aUnit, String _log) {
        _log += " and dealt ";
        int damageToTake;
        int reducedDamage;
        for (int i = 1; i <= aUnit.HitsPerTurn; i++) {
            damageToTake = UnityEngine.Random.Range(aUnit.MinDmg, aUnit.MaxDmg + 1);
            reducedDamage = Math.Max(damageToTake - _currentBlock, 0);
            if (_currentBlock > 0) { _currentBlock--; } /* each hit reduces block by 1 until the round is over */
            _currentHP = Math.Max(0, _currentHP - reducedDamage);
            _log += damageToTake;
            if (i != _hitsPerTurn) {
                _log += ", ";
            }
            print("dealt " + reducedDamage + " damage");
            print("left enemy (Unit#" + _myID + ") with " + _currentHP + " hp");
        }
        _log += " damage, leaving them with " + _currentHP + " HP ";
        if (_currentHP <= 0) {
            return true;
        }
        else {
            return false;
        }
    }
}