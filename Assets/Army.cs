using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

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
        if (aUnit.UnitRange == (int)UnitCombat.Melee) {
            for (int i=0; i<_melee.Count; i++) {
                if (aUnit == _melee[i]) {
                    print("removing unit " + i + " from army");
                    _melee.RemoveAt(i);
                }
            }
        }
        else if (aUnit.UnitRange == (int)UnitCombat.Ranged) {
            for (int i = 0; i < _ranged.Count; i++) {
                if (aUnit == _ranged[i]) {
                    print("removing unit " + i + " from army");
                    _ranged.RemoveAt(i);
                }
            }
        }
        else if (aUnit.UnitRange == (int)UnitCombat.Magic) {
            for (int i = 0; i < _magic.Count; i++) {
                if (aUnit == _magic[i]) {
                    print("removing unit " + i + " from army");
                    _magic.RemoveAt(i);
                }
            }
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