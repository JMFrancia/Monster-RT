using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TGS;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    [SerializeField] bool showDebugSpheres = false;
    [SerializeField] Button endTurnButton;

    static Combatant[] combatants;
    static TerrainGridSystem tgs;

    static int turnIndex = 0;
    static int round = 1;

    struct DebugSphere {
        public float radius;
        public Vector3 position;
        public Color color;

        public DebugSphere(Vector3 position, float radius, Color color)
        {
            this.position = position;
            this.radius = radius;
            this.color = color;
        }
    }
    static List<DebugSphere> debugSpheres = new List<DebugSphere>();

    private void Awake()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
        EditorApplication.playModeStateChanged += OnPlayModeStateChange;
    }

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

    public void EndCurrentTurn() {
        combatants[turnIndex].EndTurn();
    }

    public static void EndTurn() {
        turnIndex = (turnIndex + 1) % combatants.Length;
        if(turnIndex == 0) {
            round++;
            Debug.Log("Round " + round);
        }
        BeginTurn();
    }

    public static BaseBattlefieldObject GetBattlefieldObjectAtCell(Cell cell) {
        float radius = tgs.cellSize.y * .4f;
        Vector3 spherePos = tgs.CellGetPosition(cell) + new Vector3(0f, 1f, 0f) * radius;
        Collider[] colliders = Physics.OverlapSphere(spherePos, radius, LayerMask.GetMask(Globals.LayerNames.BATTLEFIELD_OBJECT));
        BaseBattlefieldObject[] bfObjs = colliders.Select(col => col.GetComponent<BaseBattlefieldObject>()).ToArray();
        if (bfObjs.Length > 1) {
            string errorMsg = string.Format("Overlapping battlfieldObjects detected at cell ({0}, {1}):", cell.row, cell.column);
            for(int n = 0; n < bfObjs.Length; n++) {
                errorMsg += string.Format("\n{0}. {1}", n.ToString(), bfObjs[n].gameObject.name);
            }
            Debug.LogError(errorMsg);
        }
        if(bfObjs.Length == 0) {
            AddDebugSphere(spherePos, radius, Color.black);
            return null;
        }
        AddDebugSphere(spherePos, radius, Color.green);
        return bfObjs[0];
    }

    static void BeginTurn() {
        combatants[turnIndex].BeginTurn();
    }

    public static void AddDebugSphere(Vector3 position, float radius, Color color) {
        debugSpheres.Add(new DebugSphere(position, radius, color));
    }

    public static void ClearDebugSpheres() {
        debugSpheres.Clear();
    }

    private void OnDrawGizmos()
    {
        if (showDebugSpheres)
        {
            foreach (DebugSphere sphere in debugSpheres)
            {
                Gizmos.color = sphere.color;
                Gizmos.DrawWireSphere(sphere.position, sphere.radius);
            }
        }
    }

    void OnPlayModeStateChange(PlayModeStateChange state) {
        if(state == PlayModeStateChange.ExitingPlayMode)
            debugSpheres.Clear();
    }

}
