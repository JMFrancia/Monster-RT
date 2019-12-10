using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TGS;

public class Combatant : MonoBehaviour
{
    [SerializeField] float movementTime = .5f;

    enum State
    {
        IDLE,
        MOVING
    }

    bool turn = false;
    TerrainGridSystem tgs;
    State state = State.IDLE;

    Queue<int> moveQueue;
    int currentMove;
    bool moving = false;

    private void Awake()
    {
        tgs = TerrainGridSystem.instance;
    }

    private void Update()
    {
        if (turn)
        {
            switch(state) {
                case State.IDLE:
                    OnIdleUpdate();
                    break;
                case State.MOVING:
                    OnMovingUpdate();
                    break;
                default:
                    break;
            }
        }
    }

    void OnMovingUpdate() {
        if (!moving)
        {
            if (moveQueue.Count > 0)
            {
                Vector3 dest = tgs.CellGetPosition(moveQueue.Dequeue());
                gameObject.transform.LookAt(new Vector3(dest.x, transform.position.y, dest.z));
                LeanTween.move(gameObject, dest, movementTime).setOnComplete(() => moving = false);
                moving = true;
            } else {
                EndTurn();
            }
        }
    }

    void OnIdleUpdate()
    {
        if (Input.GetMouseButtonUp(0))
        {
            int t_cell = tgs.cellHighlightedIndex;
            tgs.CellFadeOut(t_cell, Color.red, 50);
            if (t_cell != -1)
            {
                int startCell = tgs.CellGetIndex(tgs.CellGetAtPosition(transform.position, true));
                List<int> moveList = tgs.FindPath(startCell, t_cell);
                if (moveList == null)
                    return;
                tgs.CellFadeOut(moveList, Color.green, 5f);
                moveQueue = new Queue<int>(moveList);
                state = State.MOVING;
            }
        }
    }

    public void BeginTurn()
    {
        Debug.Log(gameObject.name + " beginning turn!");
        turn = true;
        state = State.IDLE;
        //StartCoroutine(EndTurnInSeconds(1));
    }

    IEnumerator EndTurnInSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        EndTurn();
    }

    public void EndTurn()
    {
        turn = false;
        BattleManager.EndTurn();
    }
}