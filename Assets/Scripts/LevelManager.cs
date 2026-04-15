using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelManager : MonoBehaviour
{
    [Header("Puzzle Library")]
    [SerializeField] private string puzzlesFolderPath = "Assets/Puzzles";
    [SerializeField] private List<PuzzleObjectScript> availablePuzzles = new();
    
    private BoxCollider spawnArea;
    [SerializeField, Min(0.001f)] private float spawnPadding = 0.45f;

    [Header("Transitions")]
    [SerializeField] private float clearAnimationDuration = 0.25f;

    private readonly List<GameObject> spawnedObjects = new();
    private int currentPuzzleIndex = -1;
    private bool isTransitioning;
    private Coroutine transitionRoutine;

    private void Awake()
    {
#if UNITY_EDITOR
        if (availablePuzzles.Count == 0)
        {
            RefreshPuzzleLibrary();
        }
#endif
        spawnArea = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        // TODO: Load tutorial
        LoadPuzzle(0);
    }

    private void Update()
    {
        // TODO: Remove this
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
        {
            if (!isTransitioning)
            {
                // TODO: Split based off current score and easy, medium, hard puzzles
                int index = Random.Range(0, availablePuzzles.Count);
                LoadPuzzle(index);
            }
        }
        // TODO: Check if puzzle is solved and then load next puzzle
    }

    private void LoadPuzzle(int puzzleIndex)
    {
        if (availablePuzzles.Count == 0 || puzzleIndex < 0 || puzzleIndex >= availablePuzzles.Count || isTransitioning)
        {
            return;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        currentPuzzleIndex = puzzleIndex;
        transitionRoutine = StartCoroutine(TransitionToPuzzleRoutine(puzzleIndex));
    }

    [ContextMenu("Refresh Puzzle Library")]
    public void RefreshPuzzleLibrary()
    {
        availablePuzzles.Clear();

#if UNITY_EDITOR
        var puzzleGuids = AssetDatabase.FindAssets("t:PuzzleObjectScript", new[] { puzzlesFolderPath });

        System.Array.Sort(puzzleGuids, (left, right) => string.Compare(
            AssetDatabase.GUIDToAssetPath(left),
            AssetDatabase.GUIDToAssetPath(right),
            System.StringComparison.OrdinalIgnoreCase));

        foreach (var t in puzzleGuids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(t);
            var puzzle = AssetDatabase.LoadAssetAtPath<PuzzleObjectScript>(assetPath);
            if (puzzle == null) continue;
            if (puzzle.puzzleObjects.Count == 0)
            {
                Debug.LogWarning($"Puzzle '{puzzle.name}' has no puzzle objects and will be skipped.", this);
                continue;
            }
            availablePuzzles.Add(puzzle);
        }

        WarnAboutOversizedPuzzles();
#endif
    }

    private IEnumerator TransitionToPuzzleRoutine(int puzzleIndex)
    {
        isTransitioning = true;

        if (spawnedObjects.Count > 0)
        {
            yield return PuzzleTransitionAnimator.ShrinkAndDestroy(spawnedObjects, clearAnimationDuration);
        }

        SpawnPuzzle(availablePuzzles[puzzleIndex]);

        isTransitioning = false;
        transitionRoutine = null;
    }

    private void SpawnPuzzle(PuzzleObjectScript puzzle)
    {
        var spawnAreaTransform = spawnArea.transform;
        var origin = spawnAreaTransform.TransformPoint(spawnArea.center);
        var rotation = spawnAreaTransform.rotation;
        var spawnAreaSize = GetspawnAreaSize(spawnArea);
        float spacing = Mathf.Max(0.001f, spawnPadding);

        int objectCount = puzzle.puzzleObjects.Count;
        int maxColumns = Mathf.Max(1, Mathf.FloorToInt(spawnAreaSize.x / spacing));
        int maxRows = Mathf.Max(1, Mathf.FloorToInt(spawnAreaSize.z / spacing));

        int gridColumns = Mathf.Clamp(Mathf.CeilToInt(Mathf.Sqrt(objectCount)), 1, maxColumns);
        int gridRows = Mathf.CeilToInt(objectCount / (float)gridColumns);

        while (gridRows > maxRows && gridColumns < maxColumns)
        {
            gridColumns++;
            gridRows = Mathf.CeilToInt(objectCount / (float)gridColumns);
        }

        if (gridRows > maxRows)
        {
#if UNITY_EDITOR
            Debug.LogWarning(
                $"Puzzle '{puzzle.name}' exceeds the spawn spawnArea. It needs {gridColumns} x {gridRows} slots but the spawnArea can fit {maxColumns} x {maxRows} with padding {spacing}.",
                this);
#endif
        }

        for (int index = 0; index < objectCount; index++)
        {
            var puzzleObject = puzzle.puzzleObjects[index];

            var localOffset = GetGridOffset(index, gridColumns, gridRows, spacing);
            var worldOffset = spawnAreaTransform.right * localOffset.x + spawnAreaTransform.forward * localOffset.z;
            var spawnedObject = Instantiate(puzzleObject, origin + worldOffset, rotation, spawnAreaTransform);
            spawnedObjects.Add(spawnedObject);
        }
    }

    private static Vector3 GetGridOffset(int index, int gridColumns, int gridRows, float spacing)
    {
        int row = index / gridColumns;
        int column = index % gridColumns;

        float centeredColumn = column - (gridColumns - 1) * 0.5f;
        float centeredRow = row - (gridRows - 1) * 0.5f;

        return new Vector3(centeredColumn * spacing, 0f, centeredRow * spacing);
    }

    private static Vector3 GetspawnAreaSize(BoxCollider spawnArea)
    {
        var scaledSize = Vector3.Scale(spawnArea.size, spawnArea.transform.lossyScale);
        return new Vector3(Mathf.Abs(scaledSize.x), Mathf.Abs(scaledSize.y), Mathf.Abs(scaledSize.z));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            RefreshPuzzleLibrary();
        }
    }

    private void WarnAboutOversizedPuzzles()
    {
        if (availablePuzzles.Count == 0)
        {
            return;
        }
        
        if (spawnArea == null)
        {
            return;
        }

        var spawnAreaSize = GetspawnAreaSize(spawnArea);
        float spacing = Mathf.Max(0.001f, spawnPadding);
        int maxColumns = Mathf.Max(1, Mathf.FloorToInt(spawnAreaSize.x / spacing));
        int maxRows = Mathf.Max(1, Mathf.FloorToInt(spawnAreaSize.z / spacing));

        foreach (var puzzle in availablePuzzles)
        {
            if (puzzle == null || puzzle.puzzleObjects == null)
            {
                continue;
            }

            int objectCount = puzzle.puzzleObjects.Count;
            int gridColumns = Mathf.Clamp(Mathf.CeilToInt(Mathf.Sqrt(objectCount)), 1, maxColumns);
            int gridRows = Mathf.CeilToInt(objectCount / (float)gridColumns);

            while (gridRows > maxRows && gridColumns < maxColumns)
            {
                gridColumns++;
                gridRows = Mathf.CeilToInt(objectCount / (float)gridColumns);
            }

            if (gridRows > maxRows)
            {
                Debug.LogWarning(
                    $"Puzzle '{puzzle.name}' exceeds the spawn spawnArea. It needs {gridColumns} x {gridRows} slots but the spawnArea can fit {maxColumns} x {maxRows} with padding {spacing}.",
                    this);
            }
        }
    }
#endif
}
