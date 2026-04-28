using Mirror;
using UnityEngine;

public class HideLocalPlayerModel : NetworkBehaviour
{
    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshes;
    private void Start()
    {
        if (isLocalPlayer)
        {
            foreach (var skinnedMesh in skinnedMeshes)
            {
                skinnedMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }
    }
}
