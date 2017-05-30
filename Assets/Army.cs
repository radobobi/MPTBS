using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class Army : MonoBehaviour {

    private static GameObject armyObj;
    public static GameObject ArmyObj {
        get {
            if (armyObj == null) {
                armyObj = new GameObject("Army Object");
            }
            return armyObj;
        }
    }

    protected List<Unit> _melee;
    public List<Unit> Melee {
        get {
            return _melee;
        }
    }

    protected List<Unit> _ranged;
    public List<Unit> Ranged {
        get {
            return _ranged;
        }
    }

    protected List<Unit> _magic;
    public List<Unit> Magic {
        get {
            return _magic;
        }
    }

    public void Start() {
        /*print("starting up an army");*/
        _melee = new List<Unit>();
        _ranged = new List<Unit>();
        _magic = new List<Unit>();
    }

    public void addUnitToArmy(Unit aUnit) {
        if (aUnit.UnitRange == (int)UnitCombat.Melee) {
            _melee.Add(aUnit);
        }
        else if (aUnit.UnitRange == (int)UnitCombat.Ranged) {
            _ranged.Add(aUnit);
        }
        else if (aUnit.UnitRange == (int)UnitCombat.Magic) {
            _magic.Add(aUnit);
        }
    }

    public void removeUnitFromArmy(Unit aUnit) {
        List<Unit> temp = new List<Unit>();
        if (aUnit.UnitRange == (int)UnitCombat.Melee) {
            temp = _melee;
        }
        else if (aUnit.UnitRange == (int)UnitCombat.Ranged) {
            temp = _ranged;
        }
        else if (aUnit.UnitRange == (int)UnitCombat.Magic) {
            temp = _magic;
        }
        for (int i = 0; i < temp.Count; i++) {
            if (aUnit == temp[i]) {
                print("Removing Unit#" + aUnit.MyID + " from army");
                temp.RemoveAt(i);
            }
        }
    }

    public void RoundCleanup() {
        RoundCleanup((int)UnitCombat.Melee);
        RoundCleanup((int)UnitCombat.Ranged);
        RoundCleanup((int)UnitCombat.Magic);
    }

    private void RoundCleanup(int type) {
        List<Unit> temp = new List<Unit>();
        if (type== (int)UnitCombat.Melee) {
            temp = _melee;
        }
        else if (type == (int)UnitCombat.Ranged) {
            temp = _ranged;
        }
        else if (type == (int)UnitCombat.Magic) {
            temp = _magic;
        }

        for (int i=0; i<temp.Count; i++) {
            Unit aUnit = temp[i];
            aUnit.RoundCleanup();
        }
    }

    public static Army CreateMyArmy() {
        var thisObj = ArmyObj.AddComponent<Army>();
        //calls Start() on the object and initializes it.
        return thisObj;
    }

    public bool isEmpty() {
        if (_melee.Count+_ranged.Count+_magic.Count == 0) {
            return true;
        }
        return false;
    }
}