using System;
using System.Collections;
using UnityEngine;

public class RouletteController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform wheelRoot;

    [Header("Spin Settings")]
    [SerializeField] private int sliceCount = 8;
    [SerializeField] private int minimumFullRotations = 4;
    [SerializeField] private int maximumFullRotations = 6;
    [SerializeField] private float spinDuration = 3f;
    [SerializeField] private AnimationCurve spinEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float pointerAngleOffset;

    private Coroutine spinCoroutine;
    private bool isSpinning;

    public bool IsSpinning => isSpinning;

    public void SetSliceCount(int count)
    {
        sliceCount = Mathf.Max(1, count);
    }

    private void Awake()
    {
        if (wheelRoot == null)
        {
            Debug.LogWarning($"RouletteController.Awake: wheelRoot is not assigned on '{name}'.", this);
        }

        if (sliceCount <= 0)
        {
            Debug.LogWarning($"RouletteController.Awake: sliceCount must be greater than 0 on '{name}'.", this);
        }
    }

    public void Play(int targetIndex, Action onComplete = null)
    {
        if (isSpinning)
        {
            Debug.LogWarning($"RouletteController.Play: spin already in progress on '{name}'.", this);
            return;
        }

        if (wheelRoot == null)
        {
            Debug.LogWarning($"RouletteController.Play: wheelRoot is not assigned on '{name}'.", this);
            return;
        }

        if (sliceCount <= 0)
        {
            Debug.LogWarning($"RouletteController.Play: invalid sliceCount {sliceCount} on '{name}'.", this);
            return;
        }

        Debug.Log($"RouletteController.Play: spinning to targetIndex={targetIndex}, sliceCount={sliceCount}.", this);
        spinCoroutine = StartCoroutine(SpinRoutine(targetIndex, onComplete));
    }

    public void StopImmediately()
    {
        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
            spinCoroutine = null;
        }

        isSpinning = false;
    }

    private IEnumerator SpinRoutine(int targetIndex, Action onComplete)
    {
        isSpinning = true;

        float startZ = NormalizeAngle(wheelRoot.localEulerAngles.z);
        float sliceAngle = 360f / sliceCount;
        float targetSliceAngle = -(targetIndex * sliceAngle) + pointerAngleOffset;
        int fullRotations = UnityEngine.Random.Range(minimumFullRotations, maximumFullRotations + 1);
        float totalRotation = (fullRotations * 360f) + Mathf.Abs(DeltaAngle(startZ, targetSliceAngle));
        float endZ = targetSliceAngle;
        float previousAppliedRotation = 0f;

        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);
            float easedT = spinEase.Evaluate(t);
            float currentAppliedRotation = Mathf.LerpUnclamped(0f, totalRotation, easedT);
            float deltaRotation = currentAppliedRotation - previousAppliedRotation;
            previousAppliedRotation = currentAppliedRotation;

            wheelRoot.Rotate(0f, 0f, -deltaRotation);
            yield return null;
        }

        wheelRoot.localRotation = Quaternion.Euler(0f, 0f, endZ);

        isSpinning = false;
        spinCoroutine = null;
        Debug.Log($"RouletteController.SpinRoutine: completed at targetIndex={targetIndex}.", this);
        onComplete?.Invoke();
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f)
        {
            angle -= 360f;
        }

        while (angle < -180f)
        {
            angle += 360f;
        }

        return angle;
    }

    private float DeltaAngle(float from, float to)
    {
        return Mathf.DeltaAngle(from, to);
    }
}
