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

    public GeneticAlgorithm(int popSize, int dnaSize, Func<T> getRandomGene,
     Func<int, float> fitnessFunction, int elitism,  float mutationRate = 0.05f)
    {
        Generation = 1;
        Elitism = elitism;
        MutationRate = mutationRate;
        Population = new List<DNA<T>>(popSize);
        _newPopulation = new List<DNA<T>>(popSize);

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

    public void NewGeneration()
    {
        if (Population.Count <= 0)
            return;

        CalculateFitness();
        _newPopulation.Clear();

        for (int i = 0; i < Population.Count; ++i)
            if (i < Elitism)
                _newPopulation.Add(Population[i]);
            else
            {
                DNA<T> parent1 = ChooseParent();
                DNA<T> parent2 = ChooseParent();

                if (parent1 == null || parent2 == null)
                    Debug.LogError("One of parents is equal to null");

                parent1 ??= GetRandomParent();

                DNA<T> child = parent1.Crossover(parent2);

                child.Mutate(MutationRate);
                _newPopulation.Add(child);
            }

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
}
