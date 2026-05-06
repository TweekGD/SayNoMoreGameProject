using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-9999)]
public class GameEntryPoint : MonoBehaviour, ISceneEntryPoint
{
    [SerializeField] private List<GameObject> gamePrefabs;
    public void Initialize()
    {
        Debug.Log("Initialize Game Scene Components...");

        CreateGameServices();
    }
    private void CreateGameServices()
    {
        foreach (var prefab in gamePrefabs)
        {
            var instance = Instantiate(prefab);
        }
    }
}