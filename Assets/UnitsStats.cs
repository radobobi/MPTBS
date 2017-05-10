using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class UnitsStats : MonoBehaviour {

    private static GameObject unitsStatsObj;
    public static GameObject UnitsStatsObj {
        get {
            if (unitsStatsObj == null) {
                unitsStatsObj = new GameObject("Stats Object");
            }
            return unitsStatsObj;
        }
    }

    public static List<List<string>> _allStats;
    public List<List<string>> AllStats {
        get {
            return _allStats;
        }
    }

    public static void Start() {
    }

    public static UnitsStats CreateUnitsStats() {
        var thisObj = UnitsStatsObj.AddComponent<UnitsStats>();
        //calls Start() on the object and initializes it.
        readUnitStatsFile();
        return thisObj;
    }

    private static void readUnitStatsFile() {
        _allStats = new List<List<string>>();
        int counter = 0;
        using (var fs = File.OpenRead(@"Assets/Units/UnitStats.txt"))
        using (var reader = new StreamReader(fs)) {
            while (!reader.EndOfStream) {
                var line = reader.ReadLine();
                List<string> temp = new List<string>();
                temp = line.Split(',').ToList<string>();
                _allStats.Add(temp);
                /*print(_allStats[counter][0] + " " + _allStats[counter][1] + " " + _allStats[counter][2] + " " + _allStats[counter][3] + " " + _allStats[counter][4] + " " + _allStats[counter][5] + " " + _allStats[counter][6]);*/
                counter++;
            }
        }
    }
}