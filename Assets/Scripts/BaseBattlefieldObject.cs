using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public abstract class BaseBattlefieldObject : MonoBehaviour
{
    protected bool destructable;
    protected bool movable;
}
