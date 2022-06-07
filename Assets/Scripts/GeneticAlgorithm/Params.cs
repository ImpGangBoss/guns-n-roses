using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GA Params", menuName = "GA Data", order = 1)] 
public class Params : ScriptableObject
{
    [SerializeField] string strategyName;
    [SerializeField] int populationSize;
    [SerializeField] int dnaLength;
    [SerializeField] int elitism;
    [SerializeField] float mutationRate;
    [Range(1f, 2f)] [SerializeField] float exponentialBase;
    [Range(1f, 1.5f)] [SerializeField] float obstacleDetectionPenalty;
    [Range(1f, 1.5f)] [SerializeField] float friendDetectionBonus;
    [SerializeField] float generationLifeTime;
    [SerializeField] float searchRange;
    [SerializeField] float obstacleDetectionRange;

    public int PopulationSize
    {
        get => populationSize;
        set => populationSize = value;
    }

    public int Elitism
    {
        get => elitism;
        set => elitism = value;
    }

    public float MutationRate
    {
        get => mutationRate;
        set => mutationRate = value;
    }

    public float ObstacleDetectionPenalty
    {
        get => obstacleDetectionPenalty;
        set => obstacleDetectionPenalty = value;
    }

    public float FriendDetectionBonus
    {
        get => friendDetectionBonus;
        set => friendDetectionBonus = value;
    }

    public float GenerationLifeTime
    {
        get => generationLifeTime;
        set => generationLifeTime = value;
    }

    public int DnaLength
    {
        get => dnaLength;
        set => dnaLength = value;
    }

    public float ExponentialBase
    {
        get => exponentialBase;
        set => exponentialBase = value;
    }

    public float SearchRange
    {
        get => searchRange;
        set => searchRange = value;
    }

    public float ObstacleDetectionRange
    {
        get => obstacleDetectionRange;
        set => obstacleDetectionRange = value;
    }

    public string StrategyName => strategyName;
}
