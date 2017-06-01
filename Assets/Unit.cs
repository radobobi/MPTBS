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

    protected int _level;
    public int Level {
        get { return _level; }
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

    protected int _regen;
    public int Regen {
        get { return _regen; }
    }

    protected int[] _lvlUps;
    public int[] LvlUps {
        get { return _lvlUps; }
    }

    protected int[] _lvlUpChoice;
    public int[] LvlUpChoice {
        get { return _lvlUpChoice; }
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
        UnitsStats theStats = UnitsStats.CreateUnitsStats();
        _myName = name;
        if (_myName == "") {
            _myName = theStats.AllStats[_myType + 1][(int)StatsOrder.UnitName];
        }
        Int32.TryParse(theStats.AllStats[_myType + 1][(int)StatsOrder.MaxHP], out _maxHP);
        _currentHP = _maxHP;
        _XP = 0;
        Int32.TryParse(theStats.AllStats[_myType + 1][(int)StatsOrder.MinDmg], out _minDmg);
        Int32.TryParse(theStats.AllStats[_myType + 1][(int)StatsOrder.MaxDmg], out _maxDmg);
        Int32.TryParse(theStats.AllStats[_myType + 1][(int)StatsOrder.HitsPerTurn], out _hitsPerTurn);
        Int32.TryParse(theStats.AllStats[_myType + 1][(int)StatsOrder.Initiative], out _initiative);
        Int32.TryParse(theStats.AllStats[_myType + 1][(int)StatsOrder.Block], out _block);
        _currentBlock = _block;
        float.TryParse(theStats.AllStats[_myType + 1][(int)StatsOrder.Accuracy], out _accuracy);
        Int32.TryParse(theStats.AllStats[_myType + 1][(int)StatsOrder.UnitCombat], out _unitRange);
        Int32.TryParse(theStats.AllStats[_myType + 1][(int)StatsOrder.Regen], out _regen);
        Int32.TryParse(theStats.AllStats[_myType + 1][(int)StatsOrder.Level], out _level);

        _lvlUps = new int[(int)LevelUpOptions.LENGTH];
        _lvlUpChoice = new int[2] { -1, -1};

        return this;
    }

    public void RoundCleanup() {
        _currentHP = _currentHP + _regen;
        _currentBlock = _block;
        LevelUpCheck();
    }

    /* Let's randomly select 2 options for level up for the player to choose from */
    public void LevelUpCheck() {
        UnitsStats theStats = UnitsStats.CreateUnitsStats();
        if (_level == 15 || (_lvlUpChoice[0] != -1 || _lvlUpChoice[1] != -1)) { return; }
        if (_XP >= theStats.XPTable[_level]) {
            int[] lvlOptions = new int[(int)LevelUpOptions.LENGTH];
            int totalOptions = 0;
            int i;
            for (i=0; i < (int)LevelUpOptions.LENGTH; i++) {
                if (theStats.AllLvlUps[i * 4 + 2, _myType] <= _level && (theStats.AllLvlUps[i * 4 + 3, _myType] > _level || theStats.AllLvlUps[i * 4 + 3, _myType] == 0)) {
                    lvlOptions[i] = Math.Max(theStats.AllLvlUps[i * 4 + 1, _myType] - _lvlUps[i],0);
                    totalOptions = totalOptions + lvlOptions[i];
                }
            }
            int randomLvlOption = UnityEngine.Random.Range(1, totalOptions + 1);
            int runningTotal = 0;
            for (i = 0; i < (int)LevelUpOptions.LENGTH; i++) {
                runningTotal = runningTotal + lvlOptions[i];
                if (randomLvlOption <= runningTotal) { break; }
            }
            totalOptions = totalOptions - lvlOptions[i];
            lvlOptions[i] = 0;
            int j = i;
            int counter = 0;
            while (j == i && counter < 100) {
                randomLvlOption = UnityEngine.Random.Range(1, totalOptions + 1);
                runningTotal = 0;
                for (j = 0; j < (int)LevelUpOptions.LENGTH; j++) {
                    runningTotal = runningTotal + lvlOptions[j];
                    if (randomLvlOption <= runningTotal) { break; }
                }
            }
            if (counter == 100) {
                j = -1;
            }
            /*print("Level Up Randomly Chosen between: " + i + " and " + j);*/
            int randomLvl = UnityEngine.Random.Range(1, 3);
            if (randomLvl == 1) {
                LevelUpUnit(i);
            }
            else {
                LevelUpUnit(j);
            }
        }
    }

    private void LevelUpUnit(int whichSkill) {
        UnitsStats theStats = UnitsStats.CreateUnitsStats();
        switch (whichSkill) {
            case 0:
            case 1:
            case 2:
                _maxHP = _maxHP + theStats.AllLvlUps[whichSkill * 4 + 0, _myType];
                _currentHP = _currentHP + theStats.AllLvlUps[whichSkill * 4 + 0, _myType];
                print("LEVEL UP - new max hp: " + _maxHP);
                break;
            case 3:
                _minDmg = _minDmg + theStats.AllLvlUps[whichSkill * 4 + 0, _myType];
                _maxDmg = _maxDmg + theStats.AllLvlUps[whichSkill * 4 + 0, _myType];
                print("LEVEL UP - new min-max dmg: " + _minDmg + "-" + _maxDmg);
                break;
            case 4:
            case 5:
                _initiative = _initiative + theStats.AllLvlUps[whichSkill * 4 + 0, _myType];
                print("LEVEL UP - new initiative: " + _initiative);
                break;
            case 6:
                _block = _block + theStats.AllLvlUps[whichSkill * 4 + 0, _myType];
                print("LEVEL UP - new block: " + _block);
                break;
            case 7:
                _regen = _regen + theStats.AllLvlUps[whichSkill * 4 + 0, _myType];
                print("LEVEL UP - new regen: " + _regen);
                break;
            case 8:
                _hitsPerTurn = _hitsPerTurn + theStats.AllLvlUps[whichSkill * 4 + 0, _myType];
                print("LEVEL UP - new APT: " + _hitsPerTurn);
                break;
            default:
                break;
        }
        _lvlUps[whichSkill]++;
        _level++;
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
        int damageToTake;
        int reducedDamage;
        aUnit.XP = aUnit.XP + 5;
        /*print("XP: " + aUnit.XP);*/
        for (int i = 1; i <= aUnit.HitsPerTurn; i++) {
            damageToTake = UnityEngine.Random.Range(aUnit.MinDmg, aUnit.MaxDmg + 1);
            reducedDamage = Math.Max(damageToTake - _currentBlock, 1);
            _currentHP = Math.Max(0, _currentHP - reducedDamage);
            _log += damageToTake;
            print("(Unit#" + aUnit.MyID + "-" + aUnit.MyName + ") dealt " + reducedDamage + "(" + damageToTake + "-" + _currentBlock + ") damage and left enemy (Unit#" + _myID + "-" + _myName + ") with " + _currentHP + " hp");
            if (_currentBlock > 0) { _currentBlock--; } /* each hit reduces block by 1 until the round is over */
        }
        if (_currentHP <= 0) {
            return true;
        }
        else {
            return false;
        }
    }
}