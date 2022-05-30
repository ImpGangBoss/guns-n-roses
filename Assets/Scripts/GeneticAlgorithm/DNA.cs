using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNA<T>
{
    public T[] Genes { get; private set;}
    public float Fitness { get; private set;}
    private readonly Func<T> _getRandomGene;
    private readonly Func<int, float> _fitnessFunction;

    public DNA(int size, Func<T> getRandomGene, Func<int, float> fitnessFunction, bool initGenes = true)
    {
        Genes = new T[size];
        this._getRandomGene = getRandomGene;
        this._fitnessFunction = fitnessFunction;

        if (initGenes)
            for (int i = 0; i < Genes.Length; i++)
                    Genes[i] = getRandomGene();

    }

    public float CalculateFitness(int index)
    {
        Fitness = _fitnessFunction(index);
        return Fitness;
    }

    public DNA<T> Crossover(DNA<T> otherParent)
    {
        DNA<T> child = new DNA<T>(Genes.Length, _getRandomGene, _fitnessFunction, false);

        for (int i = 0; i < Genes.Length; ++i)
            child.Genes[i] = UnityEngine.Random.value < 0.5f ? Genes[i] : otherParent.Genes[i];

        return child;
    }

    public void Mutate(float mutationRate)
    {
        for (int i = 0; i < Genes.Length; i++)
            if (UnityEngine.Random.value < mutationRate)
                 Genes[i] = _getRandomGene();
    }
}
