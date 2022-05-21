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

    private float fitnessSum;
    private List<DNA<T>> newPopulation;

    public GeneticAlgorithm(int popSize, int dnaSize, Func<T> getRandomGene,
     Func<int, float> fitnessFunction, int elitism,  float mutationRate = 0.05f)
    {
        Generation = 1;
        Elitism = elitism;
        MutationRate = mutationRate;
        Population = new List<DNA<T>>(popSize);
        newPopulation = new List<DNA<T>>(popSize);

        BestGenes = new T[dnaSize];
        
        for (int i = 0; i < popSize; ++i)
            Population.Add(new DNA<T>(dnaSize, getRandomGene, fitnessFunction, initGenes: true));
    }

    public int CompareDNA(DNA<T> a, DNA<T> b)
    {
        if (a.Fitness > b.Fitness)
            return -1;
        else if (a.Fitness < b.Fitness)
            return 1;
        else
            return 0;
    } 

    public void NewGeneration()
    {
        if (Population.Count <= 0)
            return;

        CalculateFitness();
        newPopulation.Clear();

        for(int i = 0; i < Population.Count; ++i)
            if (i < Elitism)
                newPopulation.Add(Population[i]);
            else
            {
                DNA<T> parent1 = ChooseParent();
                DNA<T> parent2 = ChooseParent();

                if (parent1 == null || parent2 == null)
                    Debug.LogError("One of parents is equal to null");

                DNA<T> child = parent1.Crossover(parent2);

                child.Mutate(MutationRate);
                newPopulation.Add(child);
            }

        var tmpList = Population;
        Population = newPopulation;
        newPopulation = tmpList;

        Generation++;
    }

    public void CalculateFitness()
    {
        fitnessSum = 0f;
        for (int i = 0; i < Population.Count; ++i)
            fitnessSum += Population[i].CalculateFitness(i);

        Population.Sort(CompareDNA);
        
        var best = Population[0];
        for (int i = 0; i < best.Genes.Length; ++i)
            BestGenes[i] = best.Genes[i];
        BestFitness = best.Fitness;
    }

    private DNA<T> ChooseParent()
    {
        float randomNumber = UnityEngine.Random.value * fitnessSum;

        for (int i = 0; i < Population.Count; ++i)
        {
            if (randomNumber < Population[i].Fitness)
                return Population[i];

            randomNumber -= Population[i].Fitness;
        } 

        return null;
    }
}
