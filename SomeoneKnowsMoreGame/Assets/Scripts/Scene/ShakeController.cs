using DG.Tweening;
using UnityEngine;

public class ShakeController : MonoBehaviour
{
    [SerializeField] private float _duration = 0.3f;
    [SerializeField] private float _strength = 0.01f;
    [SerializeField] private float _rotationStrength = 0.5f;
    [SerializeField] private int _vibrato = 8;
    [SerializeField] private float _randomness = 90f;
    [SerializeField] private bool _fadeOut = true;

    private Vector3 _originPosition;
    private Quaternion _originRotation;

    private Sequence shakeSequence;

    private void Awake()
    {
        _originPosition = transform.localPosition;
        _originRotation = transform.localRotation;
    }
    public void Shake()
    {
        shakeSequence = DOTween.Sequence();

        shakeSequence.Append(transform.DOShakePosition(_duration, _strength, _vibrato, _randomness, false, _fadeOut, ShakeRandomnessMode.Full));
        shakeSequence.Append(transform.DOShakeRotation(_duration, _rotationStrength, _vibrato, _randomness, _fadeOut, ShakeRandomnessMode.Full));
    }
    private void ResetTransform()
    {
        transform.localPosition = _originPosition;
        transform.localRotation = _originRotation;
    }
    private void OnDisable()
    {
        shakeSequence?.Kill();

        ResetTransform();
    }
    private void OnDestroy()
    {
        shakeSequence?.Kill();

        ResetTransform();
    }
}