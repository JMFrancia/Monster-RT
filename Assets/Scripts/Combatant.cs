using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TGS;
using Sirenix.OdinInspector;

public class Combatant : MonoBehaviour
{
    [SerializeField] Color moveRangeColor = Color.cyan;
    [SerializeField] float movementTime = .5f;
    [SerializeField] float speed = 1f;
    [SerializeField] int maxEnergy = 3;
    [SerializeField] int maxHealth = 8;
    [SerializeField] int stamina = 2;

    Color energyBarColor = Color.green;
    Color healthbarColor = Color.red;

    Color[] originalCellColors;

    [ProgressBar(0, 0, MaxMember = "maxEnergy", Segmented = true, ColorMember = "energyBarColor")]
    [SerializeField]
    [ReadOnly]
    int energy;

    [ProgressBar(0,0, MaxMember = "maxHealth", Segmented = true, ColorMember = "healthbarColor")]
    [SerializeField]
    [ReadOnly]
    int health;

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

    List<int> moveRange;

    int currentCellIndex;

    private void Awake()
    {
        tgs = TerrainGridSystem.instance;
        health = maxHealth;
        energy = maxEnergy;
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
                List<int> moveList = tgs.FindPath(startCell, t_cell, maxSteps: Mathf.RoundToInt(energy * speed));
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
        ShowMoveRange(true);
    }

    public void ShowMoveRange(bool show) { 
        if (show) {
            Debug.Log("Showing move range");
            tgs.highlightEffect = HIGHLIGHT_EFFECT.DualColors;
            currentCellIndex = tgs.CellGetAtPosition(transform.position, true).index;
            Debug.Log("Cell index: " + currentCellIndex);
            Debug.Log("Energy: " + energy);
            Debug.Log("Speed: " + speed);
            moveRange = tgs.CellGetNeighboursWithinRange(currentCellIndex, 0, Mathf.RoundToInt(energy * speed));
            Debug.Log("Move range size: " + moveRange.Count);
            originalCellColors = new Color[moveRange.Count];
            for (int n = 0; n < moveRange.Count; n++)
            {
                originalCellColors[n] = tgs.CellGetColor(moveRange[n]);
                tgs.CellSetColor(moveRange[n], moveRangeColor);
//                tgs.CellSetTerritory(moveRange[n], 1);               
            }
        } else {
            for (int n = 0; n < moveRange.Count; n++)
            {
                tgs.CellSetColor(moveRange[n], originalCellColors[n]);
//                tgs.CellSetTerritory(moveRange[n], 0);
            }
        }
    }

    public void EndTurn()
    {
        turn = false;
        ShowMoveRange(false);
        BattleManager.EndTurn();
    }
}