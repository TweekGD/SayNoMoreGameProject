using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-9999)]
public class MenuEntryPoint : MonoBehaviour, ISceneEntryPoint
{
    [SerializeField] private List<GameObject> menuPrefabs;
    public void Initialize()
    {
        Debug.Log("Initialize Menu Scene Components...");

        CreateMenuServices();
    }
    private void CreateMenuServices()
    {
        foreach (var prefab in menuPrefabs)
        {
            var instance = Instantiate(prefab);
            DontDestroyOnLoad(instance);
        }
    }
}