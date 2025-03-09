using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable
{
    Transform Transform { get; }
    bool IsAlive { get; }
    void TakeDamage(float damage);
    Vector3 Position { get; }
}