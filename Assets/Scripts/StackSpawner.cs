using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class StackSpawner : MonoSingleton<StackSpawner>
{
    [Header("References")]
    [SerializeField]
    private PickableStack stackPrefab;
    [SerializeField] private HexagonController hexagonPrefab;

    [Header("References")]
    [SerializeField]
    private List<int> scoreTresholds;

    [Header("Debug")]
    [SerializeField]
    private int maxColorVarierty;
    [SerializeField] private int tresholdIndex;
    [Tooltip("Only for demonstration, do not modify this region")]
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField] private List<Transform> stacks;
    private const int _count = 3;

    protected override void Awake()
    {
        base.Awake();
        maxColorVarierty = 3;
        spawnPoints = GetComponentsInChildren<Transform>();
    }

    private void Start()
    {
        SpawnStacks();

        InputManager.instance.StackPlacedOnGridEvent += OnStackPlaced;
        CanvasManager.instance.ScoreUpdatedEvent += OnScoreUpdated;
    }

    private void OnScoreUpdated(int score)
    {
        if (score > scoreTresholds[tresholdIndex])
        {
            // color enum count, -1 because exlude NONE satus
            if (Enum.GetNames(typeof(ColorInfo.ColorEnum)).Length - 1 > maxColorVarierty)
            {
                if (tresholdIndex < scoreTresholds.Count - 1)
                    tresholdIndex++;

                maxColorVarierty++;
            }
        }

        if (maxColorVarierty == 5) CanvasManager.instance.ScoreUpdatedEvent -= OnScoreUpdated;
    }

    private void OnStackPlaced(PickableStack stackToRemove)
    {
        stacks.Remove(stackToRemove.transform);

        if (CheckCanSpawn()) SpawnStacks();
    }

    private bool CheckCanSpawn()
    {
        return stacks.Count == 0;
    }

    private void SpawnStacks()
    {
        for (var i = 0; i < _count; i++)
        {
            var spawnPosIndex = i + 1; // Because when use this extension "GetComponentsInChildren" it adds this transform itself to the array too
            var cloneStack = Instantiate(stackPrefab, spawnPoints[spawnPosIndex].position, Quaternion.identity);
            stacks.Add(cloneStack.transform);
        }

        SpawnHex();
    }

    private void SpawnHex()
    {
        for (var s = 0; s < stacks.Count; s++)
        {
            var randomHexCount = GetRandomAmount(1, 5);

            for (var i = 0; i < randomHexCount; i++)
            {
                var color = GetRandomColor();
                var mat = new Material(GridManager.instance.BlockMaterial);
                mat.color = GridManager.instance.colorPack.HexagonColorInfo[GridManager.instance.colorPack.GetColorEnumIndex(color)].HexColor;

                var hex = Instantiate(hexagonPrefab, Vector3.zero, Quaternion.identity, stacks[s]);

                var verticalPos = i * GridManager.instance.VERTICAL_PLACEMENT_OFFSET;
                var spawnPos = new Vector3(0, verticalPos, 0);
                hex.transform.localPosition = spawnPos;
                hex.Initialize(color, mat);
            }
        }
    }

    #region GETTERS

    private ColorInfo.ColorEnum GetRandomColor()
    {

        var randomIndex = Random.Range(1, maxColorVarierty + 1);

        return (ColorInfo.ColorEnum)randomIndex;
    }

    private int GetRandomAmount(int min, int max)
    {
        return Random.Range(min, max);
    }

    #endregion
}

[Serializable]
public class ContentInfo
{
    public ColorInfo.ColorEnum color;
    public int amount;
}
