using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GA Params", menuName = "GA Data", order = 1)] 
public class Params : ScriptableObject
{
    [SerializeField] int _populationSize;
    [SerializeField] int _dnaLength;
    [SerializeField] int _elitism;
    [SerializeField] float _mutationRate;
    [SerializeField] int _exponentialBase;
    [Range(1f, 1.5f)] [SerializeField] float _obstacleDetectionPenalty;
    [Range(1f, 1.5f)] [SerializeField] float _friendDetectionBonus;
    [SerializeField] float _generationLifeTime;
    [SerializeField] float _searchRange;
    [SerializeField] float _obstacleDetectionRange;

    public int PopulationSize
    {
        get => _populationSize;
        set => _populationSize = value;
    }

    public int Elitism
    {
        get => _elitism;
        set => _elitism = value;
    }

    public float MutationRate
    {
        get => _mutationRate;
        set => _mutationRate = value;
    }

    public float ObstacleDetectionPenalty
    {
        get => _obstacleDetectionPenalty;
        set => _obstacleDetectionPenalty = value;
    }

    public float FriendDetectionBonus
    {
        get => _friendDetectionBonus;
        set => _friendDetectionBonus = value;
    }

    public float GenerationLifeTime
    {
        get => _generationLifeTime;
        set => _generationLifeTime = value;
    }

    public int DnaLength
    {
        get => _dnaLength;
        set => _dnaLength = value;
    }

    public int ExponentialBase
    {
        get => _exponentialBase;
        set => _exponentialBase = value;
    }

    public float SearchRange
    {
        get => _searchRange;
        set => _searchRange = value;
    }

    public float ObstacleDetectionRange
    {
        get => _obstacleDetectionRange;
        set => _obstacleDetectionRange = value;
    }
}
