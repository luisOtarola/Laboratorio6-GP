using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class PopulationDisplay : MonoBehaviour
{
    public GameObject rowPrefab;        // prefab de fila
    public Transform contentParent;     // ScrollView → Content

    public void ShowPopulation(List<int[,]> population, List<float> fitnesses)
    {
        // Limpiar viejas filas
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // Determinar mejor y peor fitness
        float maxFitness = fitnesses.Max();
        float minFitness = fitnesses.Min();

        for (int i = 0; i < population.Count; i++)
        {
            GameObject row = Instantiate(rowPrefab, contentParent);
            TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
            texts[0].text = $"Individuo {i + 1}";
            texts[1].text = $"Fitness: {fitnesses[i]:F2}";

            // Colorear según mejor/peor
            if (fitnesses[i] == maxFitness)
            {
                texts[0].color = Color.green;
                texts[1].color = Color.green;
            }
            else if (fitnesses[i] == minFitness)
            {
                texts[0].color = Color.red;
                texts[1].color = Color.red;
            }
            else
            {
                texts[0].color = Color.white;
                texts[1].color = Color.white;
            }
        }
    }
}
