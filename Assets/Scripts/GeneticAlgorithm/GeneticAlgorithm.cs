using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm<T>
{
    public List<DNA<T>> Population { get; private set; }
    public int Generation { get; private set; }
    public float BestFitness { get; private set; }
    public T[] BestGenes { get; private set; }

    public int Elitism;
    public float MutationRate;

    private float _fitnessSum;
    private List<DNA<T>> _newPopulation;
    private int _dnaSize;
    private Func<T> _getRandomGene;
    private Func<int, float> _fitnessFunction;

    public GeneticAlgorithm(int popSize, int dnaSize, Func<T> getRandomGene,
     Func<int, float> fitnessFunction, int elitism,  float mutationRate = 0.05f)
    {
        Generation = 1;
        Elitism = elitism;
        MutationRate = mutationRate;
        Population = new List<DNA<T>>(popSize);
        _newPopulation = new List<DNA<T>>(popSize);
        _dnaSize = dnaSize;
        _getRandomGene = getRandomGene;
        _fitnessFunction = fitnessFunction;

        BestGenes = new T[dnaSize];
        
        for (int i = 0; i < popSize; ++i)
            Population.Add(new DNA<T>(dnaSize, getRandomGene, fitnessFunction, initGenes: true));
    }

    public int CompareDNA(DNA<T> a, DNA<T> b)
    {
        if (a.Fitness > b.Fitness) return -1;
        if (a.Fitness < b.Fitness) return 1;
        return 0;
    } 

    public void NewGeneration(int newIndividualsNum = 0, bool crossoverNewIndividuals = false)
    {
        int finalCount = Population.Count + newIndividualsNum;
        
        if (finalCount <= 0)
            return;

        if (Population.Count > 0)
            CalculateFitness();

        _newPopulation.Clear();

        for (int i = 0; i < finalCount; ++i)
            if (i < Elitism && i < Population.Count)
                _newPopulation.Add(Population[i]);
            else if (i < Population.Count || crossoverNewIndividuals)
            {
                DNA<T> parent1 = ChooseParent();
                DNA<T> parent2 = ChooseParent();

                parent1 ??= GetRandomParent();
                parent2 ??= GetRandomParent();

                if (parent1 == null || parent2 == null)
                    Debug.LogError("One of parents is equal to null");

                DNA<T> child = parent1.Crossover(parent2);

                child.Mutate(MutationRate);
                _newPopulation.Add(child);
            }
            else
                _newPopulation.Add(new DNA<T>(_dnaSize, _getRandomGene, _fitnessFunction));

        (Population, _newPopulation) = (_newPopulation, Population);
        Generation++;
    }

    public void CalculateFitness()
    {
        _fitnessSum = 0f;
        for (int i = 0; i < Population.Count; ++i)
            _fitnessSum += Population[i].CalculateFitness(i);

        Population.Sort(CompareDNA);
        
        var best = Population[0];
        for (int i = 0; i < best.Genes.Length; ++i)
            BestGenes[i] = best.Genes[i];
        BestFitness = best.Fitness;
    }

    private DNA<T> ChooseParent()
    {
        float randomNumber = UnityEngine.Random.value * _fitnessSum;

        foreach (var individual in Population)
        {
            if (randomNumber < individual.Fitness)
                return individual;

            randomNumber -= individual.Fitness;
        } 

        return null;
    }

    private DNA<T> GetRandomParent() => Population[UnityEngine.Random.Range(0, Population.Count)];

    public void ForceMutate()
    {
        foreach (var dna in Population)
            dna.Mutate(1f);

    }
}
