using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "MainMenu";
    [SerializeField] private List<GameObject> persistentPrefabs;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        CreatePersistentServices();

        SceneManager.LoadScene(firstSceneName);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void CreatePersistentServices()
    {
        foreach (var prefab in persistentPrefabs)
        {
            var instance = Instantiate(prefab);
            DontDestroyOnLoad(instance);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var entryPoints = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var obj in entryPoints)
        {
            if (obj is ISceneEntryPoint entry)
            {
                entry.Initialize();
            }
        }
    }
}