using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private CellType _type;

    public CellType Type { get => _type; }
}
