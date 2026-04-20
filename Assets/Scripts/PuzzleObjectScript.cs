using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleObjectScript", menuName = "Scriptable Objects/PuzzleObjectScript")]
public class PuzzleObjectScript : ScriptableObject
{
    public Texture2D outlineTexture;
    public List<GameObject> puzzleObjects;
}
