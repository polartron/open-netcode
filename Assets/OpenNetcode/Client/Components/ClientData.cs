using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ClientData : IComponentData
{
    public Entity LocalPlayer;
    public int LocalPlayerServerEntityId;
    public int LastReceivedSnapshotIndex;
    public int LastReceivedSnapshotTick;
    public int Version;
    public bool Resetting;
}
