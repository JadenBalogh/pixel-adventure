using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static InventorySystem InventorySystem { get; private set; }

    [SerializeField] private GameObject pixelPrefab;
    public static GameObject PixelPrefab { get => instance.pixelPrefab; }

    private void Awake()
    {
        instance = this;

        InventorySystem = GetComponent<InventorySystem>();
    }
}
