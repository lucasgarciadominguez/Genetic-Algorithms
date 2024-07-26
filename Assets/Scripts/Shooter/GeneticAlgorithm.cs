
/**********************************************************************
*Project           : Genetic Algorithms
*
*Author : Lucas García
*
*
*Purpose : Testing Genetic Algorithms shooting at target
*
**********************************************************************/
using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[Serializable]
public class GeneticAlgorithm
{

    public List<Individual> population;
    private int _currentIndex;

    public int CurrentGeneration;
    public int MaxGenerations;

    public string Summary;

    int maskDegree = 0b111111110000000000000000; // Mask for degree
    int maskStrength = 0b000000001111111100000000; // Mask for strength
    int maskDegreeY = 0b000000000000000011111111;  // Mask for degreeY
    public GeneticAlgorithm(int numberOfGenerations, int populationSize)
    {
        CurrentGeneration = 0;
        MaxGenerations = numberOfGenerations;
        GenerateRandomPopulation(populationSize);
        Summary = "";
    }
    public void GenerateRandomPopulation(int size)
    {
        population = new List<Individual>();
        for (int i = 0; i < size; i++)
        {
            population.Add(new Individual(Random.Range(0f, 90f), Random.Range(0f, 12f), Random.Range(-35f, 35f)));
        }
        StartGeneration();
    }

    public Individual GetFittest()
    {
        population.Sort();  // Sorts the population by the fitness of individuals first
        return population[0];
    }


    public void StartGeneration()
    {
        _currentIndex = 0;
        CurrentGeneration++;
    }
    public Individual GetNext()
    {
        if (_currentIndex == population.Count)  // When the current index of individual equals the total size of the population
        {
            EndGeneration();
            if (CurrentGeneration >= MaxGenerations)
            {
                Debug.Log(Summary);
                return null;
            }
            StartGeneration();
        }

        return population[_currentIndex++];
    }

    public void EndGeneration()
    {
        population.Sort(); // Sorts the population by the fitness of individuals first
        Summary += $"{GetFittest().fitness};";  // Makes a string with the highest value of fitness for each generation
        if (CurrentGeneration < MaxGenerations)
        {   // If the generation is not the maximum set in ShotgunConfiguration.cs
            CrossoverUniformBinary();
            MutationByExchangeBinary();
        }
    }
    #region Real Crossovers
    public void CrossoverMonoPoint()
    {
        //SELECTION
        var ind1 = population[0];   // The two most developed individuals
        var ind2 = population[1];
        //

        //Single Point Crossover//
        var new1 = new Individual(ind1.degree, ind2.strength, ind2.degreeY);
        var new2 = new Individual(ind2.degree, ind1.strength, ind1.degreeY);

        //REPLACEMENT
        population.RemoveAt(population.Count - 1);
        population.RemoveAt(population.Count - 1);
        population.Add(new1);
        population.Add(new2);
    }
    public void CrossoverMonoPoint2()
    {
        //SELECTION
        var ind1 = population[0];   // The two most developed individuals
        var ind2 = population[1];
        //

        //Single Point Crossover//
        var new1 = new Individual(ind2.degree, ind1.strength, ind2.degreeY);
        var new2 = new Individual(ind1.degree, ind2.strength, ind1.degreeY);

        //REPLACEMENT
        population.RemoveAt(population.Count - 1);
        population.RemoveAt(population.Count - 1);
        population.Add(new1);
        population.Add(new2);
    }
    public void CrossoverCombined()
    {
        // SELECTION
        var ind1 = population[0];   // The first selected individual
        var ind2 = population[1];   // The second selected individual


        float alpha = 0.4f; //for me this a correct value

        int valueDegree1 = Convert.ToInt32(ind1.degree);
        int valueDegree2 = Convert.ToInt32(ind2.degree);
        int h = Mathf.Abs(valueDegree1 - valueDegree2);

        float minDegree = Random.Range((valueDegree1 - alpha * h), (valueDegree2 - alpha * h));

        int valueDegreeY1 = Convert.ToInt32(ind1.degreeY);
        int valueDegreeY2 = Convert.ToInt32(ind2.degreeY);
        h = Mathf.Abs(valueDegreeY1 - valueDegreeY2);

        float minDegreeY = Random.Range((valueDegreeY1 - alpha * h), (valueDegreeY2 - alpha * h));

        int valueStrength1 = Convert.ToInt32(ind1.strength);
        int valueStrength2 = Convert.ToInt32(ind2.strength);
        h = Mathf.Abs(valueStrength1 - valueStrength2);

        float minStrength = Random.Range((valueStrength1 - alpha * h), (valueStrength2 - alpha * h));

        var new1 = new Individual(minDegree, minStrength, minDegreeY);


        // REPLACEMENT
        population.RemoveAt(population.Count - 1);
        population.Add(new1);
    }
    #endregion
    #region Binary Crossovers
    void CrossoverBinaryMonoPoint()
    {
        //SELECTION
        var ind1 = population[0];   // The first selected individual
        var ind2 = population[1];   // The second selected individual

        int binaryNumberInd1 = ConvertIndividualToBinary(ind1);
        int binaryNumberInd2 = ConvertIndividualToBinary(ind2);
        // Variable to store bits to exchange
        int numbersToChange1 = 0;
        int numbersToChange2 = 0;

        // Loop through the bits from position 5 to position 8
        for (int i = 9; i >= 6; i--)
        {
            int mask = 1 << i; // Mask for the bit at position i
            int bit1 = (binaryNumberInd1 & mask) >> i; // Get the bit at position i of the first individual
            int bit2 = (binaryNumberInd2 & mask) >> i;

            // Shift the bits to the corresponding position in the numbers to exchange
            numbersToChange1 |= bit2 << (9 - i); // Add the bit from the second individual to the first
            numbersToChange2 |= bit1 << (9 - i);
        }

        // Embed the exchanged bits in positions 9 to 6 of the original number
        for (int i = 9; i >= 6; i--)
        {
            int mask = ~(1 << i); // Mask to clear the bit at position i
            int bitToInsert1 = (numbersToChange1 >> (9 - i)) & 1; // Get the exchanged bit for the first individual
            int bitToInsert2 = (numbersToChange2 >> (9 - i)) & 1;

            // Clear the bit at position i of the original number and then insert the exchanged bit
            binaryNumberInd1 &= mask;
            binaryNumberInd1 |= bitToInsert2 << i;

            binaryNumberInd2 &= mask;
            binaryNumberInd2 |= bitToInsert1 << i;
        }
        int newIndividual1Degree = (binaryNumberInd1 & maskDegree) >> 16;
        int newIndividual1Strength = (binaryNumberInd1 & maskStrength) >> 8; // Shift right by 6 bits to align with the strength bits
        int newIndividual1DegreeY = binaryNumberInd1 & maskDegreeY;

        int newIndividual2Degree = (binaryNumberInd2 & maskDegree) >> 16;
        int newIndividual2Strength = (binaryNumberInd2 & maskStrength) >> 8; // Shift right by 6 bits to align with the strength bits
        int newIndividual2DegreeY = binaryNumberInd2 & maskDegreeY;

        // Create new individuals from the unpacked values
        var new1 = new Individual(newIndividual1Degree, newIndividual1Strength, newIndividual1DegreeY);
        var new2 = new Individual(newIndividual2Degree, newIndividual2Strength, newIndividual2DegreeY);

        // Replace the less fit individuals with the newly generated individuals
        population.RemoveAt(population.Count - 1);
        population.RemoveAt(population.Count - 1);
        population.Add(new1);
        population.Add(new2);
    }
    public void CrossoverMultiPointBinary()
    {
        //SELECTION
        var ind1 = population[0];
        var ind2 = population[1];

        int binaryNumberInd1 = ConvertIndividualToBinary(ind1);
        int binaryNumberInd2 = ConvertIndividualToBinary(ind2);
        // Variable to store bits to exchange
        int numbersToChange1 = 0;
        int numbersToChange2 = 0;

        // Loop through the bits from position 4 to position 5
        for (int i = 5; i >= 4; i--)
        {
            int mask = 1 << i; // Mask for the bit at position i
            int bit1 = (binaryNumberInd1 & mask) >> i; // Get the bit at position i of the first individual
            int bit2 = (binaryNumberInd2 & mask) >> i;

            // Shift the bits to the corresponding position in the numbers to exchange
            numbersToChange1 |= bit2 << (5 - i); // Add the bit from the second individual to the first
            numbersToChange2 |= bit1 << (5 - i);
        }

        // Embed the exchanged bits in positions 4 to 5 of the original number
        for (int i = 5; i >= 4; i--)
        {
            int mask = ~(1 << i); // Mask to clear the bit at position i
            int bitToInsert1 = (numbersToChange1 >> (5 - i)) & 1; // Get the exchanged bit for the first individual
            int bitToInsert2 = (numbersToChange2 >> (5 - i)) & 1;

            // Clear the bit at position i of the original number and then insert the exchanged bit
            binaryNumberInd1 &= mask;
            binaryNumberInd1 |= bitToInsert2 << i;

            binaryNumberInd2 &= mask;
            binaryNumberInd2 |= bitToInsert1 << i;
        }
        // Unpack the values
        int newIndividual1Degree = (binaryNumberInd1 & maskDegree) >> 16;
        int newIndividual1Strength = (binaryNumberInd1 & maskStrength) >> 8; // Shift right by 6 bits to align with the strength bits
        int newIndividual1DegreeY = binaryNumberInd1 & maskDegreeY;

        int newIndividual2Degree = (binaryNumberInd2 & maskDegree) >> 16;
        int newIndividual2Strength = (binaryNumberInd2 & maskStrength) >> 8; // Shift right by 6 bits to align with the strength bits
        int newIndividual2DegreeY = binaryNumberInd2 & maskDegreeY;

        // Create new individuals from the unpacked values
        var new1 = new Individual(newIndividual1Degree, newIndividual1Strength, newIndividual1DegreeY);
        var new2 = new Individual(newIndividual2Degree, newIndividual2Strength, newIndividual2DegreeY);

        // Replace the less fit individuals with the newly generated individuals
        population.RemoveAt(population.Count - 1);
        population.RemoveAt(population.Count - 1);
        population.Add(new1);
        population.Add(new2);

    }
    void CrossoverUniformBinary()
    {
        // Selection of individuals
        var ind1 = population[0];
        var ind2 = population[1];

        int binaryNumberInd1 = ConvertIndividualToBinary(ind1);
        int binaryNumberInd2 = ConvertIndividualToBinary(ind2);
        int mask = 0b101111010010101010111101;
        ////here
        binaryNumberInd1 &= mask;
        binaryNumberInd2 &= mask;
        // Unpack the values
        int newIndividual1Degree = (binaryNumberInd1 & maskDegree) >> 16;
        int newIndividual1Strength = (binaryNumberInd1 & maskStrength) >> 8; // Shift right by 6 bits to align with the strength bits
        int newIndividual1DegreeY = binaryNumberInd1 & maskDegreeY;

        int newIndividual2Degree = (binaryNumberInd2 & maskDegree) >> 16;
        int newIndividual2Strength = (binaryNumberInd2 & maskStrength) >> 8; // Shift right by 6 bits to align with the strength bits
        int newIndividual2DegreeY = binaryNumberInd2 & maskDegreeY;

        // Create new individuals from the unpacked values

        if (newIndividual1Degree > 90 || newIndividual1Degree < 0)
        {
            newIndividual1Degree = Convert.ToInt32(ind1.degree);
        }
        if (newIndividual2Degree > 90 || newIndividual2Degree < 0)
        {
            newIndividual2Degree = Convert.ToInt32(ind2.degree);
        }
        if (newIndividual1DegreeY > 35 || newIndividual1DegreeY < -35)
        {
            newIndividual1DegreeY = Convert.ToInt32(ind1.degreeY);
        }
        if (newIndividual2DegreeY > 35 || newIndividual2DegreeY < -35)
        {
            newIndividual2DegreeY = Convert.ToInt32(ind2.degreeY);
        }
        var new1 = new Individual(newIndividual1Degree, newIndividual1Strength, newIndividual1DegreeY);
        var new2 = new Individual(newIndividual2Degree, newIndividual2Strength, newIndividual2DegreeY);

        // Replace the old individuals with the new ones
        population.RemoveAt(population.Count - 1);
        population.RemoveAt(population.Count - 1);
        population.Add(new1);
        population.Add(new2);
    }
    #endregion
    void SelectionByTournament()
    {

    }
    private int ConvertIndividualToBinary(Individual individual)
    {
        return ((Convert.ToInt32(individual.degree) << 16) | (Convert.ToInt32(individual.strength) << 8)) | Convert.ToInt32(individual.degreeY);
    }
    #region Mutation
    public void MutationUniform()
    {
        foreach (var individual in population)
        {
            if (Random.Range(0f, 1f) < 0.02f)


            {
                individual.degree = Random.Range(0f, 90f);
            }
            if (Random.Range(0f, 1f) < 0.02f)
            {
                individual.degreeY = Random.Range(-35f, 35f);
            }
            if (Random.Range(0f, 1f) < 0.02f)
            {
                individual.strength = Random.Range(0f, 12f);
            }
        }
    }
    public void MutationUniformBinary()
    {
        foreach (var individual in population)
        {
            if (Random.Range(0f, 1f) < 0.02f)
            {
                int binaryNumberInd1 = ((Convert.ToInt32(individual.degree) << 16) | (Convert.ToInt32(individual.strength) << 8)) | Convert.ToInt32(individual.degreeY);
                // Convert degree and strength values to a combined binary number

                // Mask to change the bit at position 5
                int mask = 1 << 5;

                // Apply XOR mask to change the bit at position 5
                binaryNumberInd1 ^= mask;
                // Unpack the values
                individual.degree = (binaryNumberInd1 & maskDegree) >> 16;
                individual.strength = (binaryNumberInd1 & maskStrength) >> 8; // Shift right by 6 bits to align with the strength bits
                individual.degreeY = binaryNumberInd1 & maskDegreeY;
            }
            if (Random.Range(0f, 1f) < 0.02f)
            {
                int binaryNumberInd1 = ((Convert.ToInt32(individual.degree) << 16) | (Convert.ToInt32(individual.strength) << 8)) | Convert.ToInt32(individual.degreeY);
                // Convert degree and strength values to a combined binary number

                // Mask to change the bit at position 5
                int mask = 1 << 12;

                // Apply XOR mask to change the bit at position 5
                binaryNumberInd1 ^= mask;
                // Unpack the values
                individual.degree = (binaryNumberInd1 & maskDegree) >> 16;
                individual.strength = (binaryNumberInd1 & maskStrength) >> 8; // Shift right by 6 bits to align with the strength bits
                individual.degreeY = binaryNumberInd1 & maskDegreeY;

            }
            if (Random.Range(0f, 1f) < 0.02f)
            {
                int binaryNumberInd1 = ((Convert.ToInt32(individual.degree) << 16) | (Convert.ToInt32(individual.strength) << 8)) | Convert.ToInt32(individual.degreeY);
                // Convert degree and strength values to a combined binary number

                // Mask to change the bit at position 5
                int mask = 1 << 20;

                // Apply XOR mask to change the bit at position 5
                binaryNumberInd1 ^= mask;
                // Unpack the values
                individual.degree = (binaryNumberInd1 & maskDegree) >> 16;
                individual.strength = (binaryNumberInd1 & maskStrength) >> 8; // Shift right by 6 bits to align with the strength bits
                individual.degreeY = binaryNumberInd1 & maskDegreeY;

            }
        }
    }

    public void MutationByExchange()
    {
        foreach (var individual in population)
        {
            if (Random.Range(0f, 1f) < 0.02f)
            {
                individual.degree = individual.strength * 7.5f;
            }
            if (Random.Range(0f, 1f) < 0.02f)
            {
                individual.degreeY = individual.degree / 3f;
            }
            if (Random.Range(0f, 1f) < 0.02f)
            {
                individual.degreeY = -(individual.degree / 3f);
            }
            if (Random.Range(0f, 1f) < 0.02f)
            {
                individual.strength = individual.degree / 7.5f;
            }
        }
    }
    public void MutationByExchangeBinary()
    {
        foreach (var individual in population)
        {
            if (Random.Range(0f, 1f) < 0.02f)
            {
                int binaryNumberInd = ConvertIndividualToBinary(individual);

                // Get the bits at positions 13 and 19
                int bit13 = (binaryNumberInd >> 15) & 1;
                int bit19 = (binaryNumberInd >> 19) & 1;

                // Exchange the bits
                binaryNumberInd ^= (-bit13 ^ binaryNumberInd) & (1 << 19);
                binaryNumberInd ^= (-bit19 ^ binaryNumberInd) & (1 << 13);

                // Create new individuals from the unpacked values

                int newIndividual1Degree = (binaryNumberInd & maskDegree) >> 16;
                int newIndividual1Strength = (binaryNumberInd & maskStrength) >> 8; // Shift right by 6 bits to align with the strength bits
                int newIndividual1DegreeY = binaryNumberInd & maskDegreeY;

                if (newIndividual1Degree > 90 || newIndividual1Degree < 0)
                {
                    newIndividual1Degree = Convert.ToInt32(individual.degree);
                }
                if (newIndividual1DegreeY > 35 || newIndividual1DegreeY < -35)
                {
                    newIndividual1DegreeY = Convert.ToInt32(individual.degreeY);
                }

                // Unpack the values
                individual.degree = newIndividual1Degree;
                individual.strength = newIndividual1Strength; // Shift right by 6 bits to align with the strength bits
                individual.degreeY = newIndividual1DegreeY;
            }
        }
    }
    #endregion
}