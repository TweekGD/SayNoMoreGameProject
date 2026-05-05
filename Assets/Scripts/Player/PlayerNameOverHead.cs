using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;

public class PlayerNameOverHead : NetworkBehaviour
{
    private TextMeshPro nameText;
    private Transform cameraTransform;

    private void Awake()
    {
        nameText = GetComponent<TextMeshPro>();
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            gameObject.SetActive(false);
            return;
        }

        StartCoroutine(FindLocalCamera());
    }

    public void SetName(string value)
    {
        if (nameText != null)
            nameText.text = value;
    }

    private IEnumerator FindLocalCamera()
    {
        while (PlayerCameraRotation.PlayerCamera == null)
            yield return null;

        cameraTransform = PlayerCameraRotation.PlayerCamera.transform;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }
}