using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Scriptable Unit")]
public class ScriptableUnit : ScriptableObject
{
    public Faction faction;
    public ulong hp;
    public byte moveBlock;
}

public enum Faction
{
    ATTACKER = 0,
    DEFENDER
}
