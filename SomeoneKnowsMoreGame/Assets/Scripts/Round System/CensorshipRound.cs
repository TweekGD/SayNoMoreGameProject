using Mirror;
using UnityEngine;

public class Censorship : BaseMinigame
{
    [Server]
    public override void OnRoundStart() { }

    [Server]
    public override void OnRoundTick(float deltaTime) { }

    [Server]
    public override void OnRoundEnd() { }
}
