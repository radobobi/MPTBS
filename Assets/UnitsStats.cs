using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

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

    public static int[,] _allLvlUps;
    public int[,] AllLvlUps {
        get {
            return _allLvlUps;
        }
    }

    /* XP requirements for levels 1 through 15 */
    public static List<int> _xpTable = new List<int> {0,10,20,30,45,60,75,90,110,130,150,175,200,250,300};
    public List<int> XPTable {
        get {
            return _xpTable;
        }
    }

    public static void Start() {
    }

    public static UnitsStats CreateUnitsStats() {
        var thisObj = UnitsStatsObj.AddComponent<UnitsStats>();
        //calls Start() on the object and initializes it.
        readUnitStatsFile();
        readLvlUpsFile();
        return thisObj;
    }

    private static void readUnitStatsFile() {
        _allStats = new List<List<string>>();
        int counter = 0;
        using (var fs = File.OpenRead(@"Assets/Units/UnitsStats.csv"))
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

    private static void readLvlUpsFile() {
        List<List<string>>  temp_lists = new List<List<string>>();
        int counter = 0;
        using (var fs = File.OpenRead(@"Assets/Units/LevelUps.csv"))
        using (var reader = new StreamReader(fs)) {
            while (!reader.EndOfStream) {
                var line = reader.ReadLine();
                List<string> temp = new List<string>();
                temp = line.Split(',').ToList<string>();
                temp_lists.Add(temp);
                /*print(temp_lists[counter][0] + " " + temp_lists[counter][1] + " " + temp_lists[counter][2] + " " + temp_lists[counter][3]);*/
                counter++;
            }
        }

        int tempInt;
        _allLvlUps = new int[(int)LevelUpOptions.LENGTH*4,(int)UnitType.LENGTH];
        for (int i=1; i<=(int)LevelUpOptions.LENGTH*4; i++) {
            for (int j=1; j<=(int)UnitType.LENGTH; j++) {
                Int32.TryParse(temp_lists[i][j], out tempInt);
                /*print("i: " + i + " j: " + j);*/
                _allLvlUps[i-1, j-1] = tempInt;
            }
        }
    }
}