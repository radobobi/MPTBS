using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class CombatManager : MonoBehaviour {

    private static GameObject cmObj;
    public static GameObject CMObj {
        get {
            if (cmObj == null) {
                cmObj = new GameObject("Combat Manager Object");
            }
            return cmObj;
        }
    }

    private Army _attackers;
    private Army _defenders;

    private bool _battleOver;
    private int _round = 1;

    private string _log = "";
    public string Log {
        get { return _log; }
    }

    // Use this for initialization
    public void Start() {
        _battleOver = false;
    }

    public static CombatManager CreateMyCM(Army attackers, Army defenders) {
        var thisObj = CMObj.AddComponent<CombatManager>();
        //calls Start() on the object and initializes it.
        thisObj._attackers = attackers;
        thisObj._defenders = defenders;
        return thisObj;
    }

    public void ConductBattle() {
        print("Battle begins.");
        _battleOver = false;

        while (!_battleOver && _round < 100) {
            print("*** ROUND " + _round + " ***");
            _log += "\n" + "*** ROUND " + _round + " ***";

            ConductRange((int)UnitCombat.Magic);
            ConductRange((int)UnitCombat.Ranged);
            ConductRange((int)UnitCombat.Melee);

            RoundCleanup();
            ++_round;
        }
        print("Battle Is Over");
        /*print(_log);*/
    }

    private void RoundCleanup() {
        _attackers.RoundCleanup();
        _defenders.RoundCleanup();
    }

    private void ConductRange(int unitRange) {
        List<Unit> defenderUnits = new List<Unit>();
        List<Unit> attackerUnits = new List<Unit>();
        if (unitRange == (int)UnitCombat.Magic) {
            defenderUnits = _defenders.Magic;
            attackerUnits = _attackers.Magic;
        }
        else if (unitRange == (int)UnitCombat.Ranged) {
            defenderUnits = _defenders.Ranged;
            attackerUnits = _attackers.Ranged;
        }
        else if (unitRange == (int)UnitCombat.Melee) {
            defenderUnits = _defenders.Melee;
            attackerUnits = _attackers.Melee;
        }

        if (defenderUnits.Count == 0 && attackerUnits.Count == 0) {
            return;
        }

        for (int i = 20; i >= 0; i--) {
            for (int j = 0; j < defenderUnits.Count; j++) {
                Unit aUnit = defenderUnits[j];
                if (aUnit.Initiative == i) {
                    Unit targetUnit = aUnit.chooseTarget(_attackers);
                    /*print("Defender chose an attacker ");*/
                    _log += "\n Defender chose an attacker ";
                    if (targetUnit.applyDmgFrom(aUnit, _log)) {
                        /*print("Attacker Died ");*/
                        _attackers.removeUnitFromArmy(targetUnit);
                        _log += "\n Attacker Died ";
                        if (_attackers.isEmpty()) {
                            _battleOver = true;
                            return;
                        }
                    }
                }
            }
            for (int j = 0; j < attackerUnits.Count; j++) {
                Unit aUnit = attackerUnits[j];
                if (aUnit.Initiative == i) {
                    Unit targetUnit = aUnit.chooseTarget(_defenders);
                    /*print("Defender chose an attacker ");*/
                    _log += "\n Attacker chose a defender ";
                    if (targetUnit.applyDmgFrom(aUnit, _log)) {
                        /*print("Attacker Died ");*/
                        _defenders.removeUnitFromArmy(targetUnit);
                        _log += "\n Defender Died ";
                        if (_defenders.isEmpty()) {
                            _battleOver = true;
                            return;
                        }
                    }
                }
            }
        }
    }
}
