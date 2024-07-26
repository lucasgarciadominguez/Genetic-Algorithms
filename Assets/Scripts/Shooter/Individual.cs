using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
[Serializable]
public class Individual: IComparable<Individual>
{
    public float degree;
    public float strength;
    public float degreeY;
    public float fitness;

    public Individual(float d, float s, float degreeY)
    {
        fitness = +1000f;
        degree = d;
        strength = s;
        this.degreeY = degreeY;
    }

    public int CompareTo(Individual other)
    {
        return fitness.CompareTo(other.fitness);
    }
}
