using UnityEngine;
using System.Collections;

public class Combatant : MonoBehaviour
{
    public void BeginTurn() {
        Debug.Log(gameObject.name + " beginning turn!");
        StartCoroutine(EndTurnInOneSecond());
    }

    IEnumerator EndTurnInOneSecond() {
        yield return new WaitForSeconds(1);
        BattleManager.EndTurn();
    }
}
