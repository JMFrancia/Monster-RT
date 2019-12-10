using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TGS;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    static Combatant[] combatants;
    TerrainGridSystem tgs;

    static int turnIndex = 0;
    static int round = 1;

    private void Start()
    {
        tgs = TerrainGridSystem.instance;

        combatants = GameObject.FindGameObjectsWithTag(Globals.TagNames.COMBATANT).Select(go => go.GetComponent<Combatant>()).ToArray();

        foreach (Combatant combatant in combatants) {
            Cell cell = tgs.CellGetAtPosition(combatant.transform.position, true);
            Debug.Log("Combatant " + combatant.name + " at cell " + cell.row + ", " + cell.column);
        }

        Debug.Log("Round 1");

        BeginTurn();
    }

    public static void EndTurn() {
        turnIndex = (turnIndex + 1) % combatants.Length;
        if(turnIndex == 0) {
            round++;
            Debug.Log("Round " + round);
        }
        BeginTurn();
    }

    static void BeginTurn() {
        combatants[turnIndex].BeginTurn();
    }
}
