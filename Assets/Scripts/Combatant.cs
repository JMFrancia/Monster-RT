using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TGS;
using Sirenix.OdinInspector;

public class Combatant : BaseBattlefieldObject
{
    [SerializeField] Color moveRangeColor = Color.cyan;
    [SerializeField] Color blockedMoveRangeColor = Color.red;
    [SerializeField] float movementTime = .5f;
    [SerializeField] int speed = 1; //Speed as max move Dist for now
    [SerializeField] int maxEnergy = 3;
    [SerializeField] int maxHealth = 8;
    [SerializeField] int stamina = 2;
    [SerializeField] int attackEnergy = 3;
    [SerializeField] int attackRange = 1;

    Color energyBarColor = Color.green;
    Color healthbarColor = Color.red;

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
    Animator animator;
    Queue<int> moveQueue;
    bool inTransit = false;
    Color[] originalCellColors;
    List<int> moveRange;
    int currentCellIndex;
    int currentMove;
    int totalMoves = 0;

    private void Awake()
    {
        movable = true;
        destructable = true;
        tgs = TerrainGridSystem.instance;
        animator = GetComponent<Animator>();
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
        if (!inTransit)
        {
            if (moveQueue.Count > 0)
            {
                Vector3 dest = tgs.CellGetPosition(moveQueue.Dequeue());
                gameObject.transform.LookAt(new Vector3(dest.x, transform.position.y, dest.z));
                LeanTween.move(gameObject, dest, movementTime).setOnComplete(() => inTransit = false);
                inTransit = true;
                totalMoves++;
            } else {
                state = State.IDLE;
                ShowMoveRange(true);
            }
        }
    }

    void OnIdleUpdate()
    {
        if (Input.GetMouseButtonUp(0))
        {
            int t_cell = tgs.cellHighlightedIndex;
            //tgs.CellFadeOut(t_cell, Color.red, 50);
            if (t_cell != -1 && t_cell != currentCellIndex) {
                BaseBattlefieldObject obj = BattleManager.GetBattlefieldObjectAtCell(tgs.cells[t_cell]);
                int dist = tgs.CellGetHexagonDistance(currentCellIndex, t_cell);
                 if (obj != null) {
                    Debug.Log("Target: " + obj.gameObject.name + ", dist: " + dist + ", energy req: " + attackEnergy + " out of " + energy);
                    if (dist <= attackRange && attackEnergy <= energy) {
                        Debug.Log("attacking!");
                        Attack(obj);
                    }
                } else {
                    MoveTo(t_cell);
                }
            }
        }
    }

    void MoveTo(int cellIndex) {
        int startCell = tgs.CellGetIndex(tgs.CellGetAtPosition(transform.position, true));
        List<int> moveList = tgs.FindPath(startCell, cellIndex, maxSteps: GetMoveRange());
        if (moveList == null)
            return;
        energy -= moveList.Count;
        //tgs.CellFadeOut(moveList, Color.green, 5f);
        moveQueue = new Queue<int>(moveList);
        state = State.MOVING;
        ShowMoveRange(false);
    }

    public void BeginTurn()
    {
        Debug.Log(gameObject.name + " beginning turn!");
        turn = true;
        state = State.IDLE;
        energy = Mathf.Min(maxEnergy, energy + stamina);
        totalMoves = 0;
        ShowMoveRange(true);

    }

    public void EndTurn()
    {
        turn = false;
        ShowMoveRange(false);
        energy = Mathf.Min(energy * 2, maxEnergy);
        BattleManager.EndTurn();
    }

    void Attack(BaseBattlefieldObject target) {
        energy -= attackEnergy;
        Vector3 rotateTowards = Vector3.Scale(target.transform.position, new Vector3(1f, 0, 1f)) + new Vector3(0f, transform.position.y, 0f);
        transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));
        animator.SetTrigger("Attack");
//        LeanTween.rotate(gameObject, rotateTowards, .2f).setOnComplete(() => animator.SetTrigger("Attack"));
    }

    int GetMoveRange() {
        return Mathf.Min(speed - totalMoves, energy);
    }

    void ShowMoveRange(bool show) {
        BattleManager.ClearDebugSpheres();
        if (show) {
            //tgs.highlightEffect = HIGHLIGHT_EFFECT.DualColors;
            currentCellIndex = tgs.CellGetAtPosition(transform.position, true).index;
            moveRange = tgs.CellGetNeighboursWithinRange(currentCellIndex, 0, GetMoveRange());
            originalCellColors = new Color[moveRange.Count];
            for (int n = 0; n < moveRange.Count; n++)
            {
                originalCellColors[n] = tgs.CellGetColor(moveRange[n]);
                Color setColor = BattleManager.GetBattlefieldObjectAtCell(tgs.cells[moveRange[n]]) == null ? moveRangeColor : blockedMoveRangeColor;
                tgs.CellSetColor(moveRange[n], setColor);
            }
        } else {
            for (int n = 0; n < moveRange.Count; n++)
            {
                tgs.CellSetColor(moveRange[n], originalCellColors[n]);
            }
        }
    }
}