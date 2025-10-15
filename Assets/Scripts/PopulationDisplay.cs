using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopulationDisplay : MonoBehaviour
{
    public GameObject rowPrefab;        // prefab de fila
    public Transform contentParent;     // ScrollView → Content

    public void ShowPopulation(List<int[,]> population, List<float> fitnesses)
    {
        // Limpiar viejas filas
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        for (int i = 0; i < population.Count; i++)
        {
            GameObject row = Instantiate(rowPrefab, contentParent);
            TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
            texts[0].text = $"Individuo {i + 1}";
            texts[1].text = $"Fitness: {fitnesses[i]:F2}";
        }
    }
}
