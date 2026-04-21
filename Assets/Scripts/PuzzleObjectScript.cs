using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleObjectScript", menuName = "Scriptable Objects/PuzzleObjectScript")]
public class PuzzleObjectScript : ScriptableObject
{
    public Texture2D outlineTexture;
    [Min(0f)] public float timeBonusSeconds = 10f;
    public List<GameObject> puzzleObjects;
}
