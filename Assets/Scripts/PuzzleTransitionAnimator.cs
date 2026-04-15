using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PuzzleTransitionAnimator
{
    public static IEnumerator ShrinkAndDestroy(List<GameObject> spawnedObjects, float duration)
    {
        var objectsToClear = new List<GameObject>(spawnedObjects);
        if (objectsToClear.Count == 0)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            foreach (var t in objectsToClear)
            {
                Object.Destroy(t);
            }

            spawnedObjects.Clear();
            yield break;
        }

        var transforms = new List<Transform>(objectsToClear.Count);
        var originalScales = new List<Vector3>(objectsToClear.Count);

        foreach (var spawnedObject in objectsToClear)
        {
            transforms.Add(spawnedObject.transform);
            originalScales.Add(spawnedObject.transform.localScale);
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float progress = Mathf.Clamp01(elapsedTime / duration);
            float scaleMultiplier = Mathf.Lerp(1f, 0f, progress);

            for (int index = 0; index < transforms.Count; index++)
            {
                transforms[index].localScale = originalScales[index] * scaleMultiplier;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (var t in objectsToClear)
        {
            Object.Destroy(t);
        }

        spawnedObjects.Clear();
    }
}
