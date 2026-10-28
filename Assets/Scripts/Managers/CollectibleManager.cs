using System;
using System.Collections.Generic;
using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Coleccionables;

namespace ElMinero.Managers
{
    /// <summary>
    /// Gestiona los coleccionables presentes en el nivel actual.
    /// Cuenta las vetas recolectadas contra el total para el HUD y el bono de salida.
    /// </summary>
    public class CollectibleManager : MonoBehaviour, IResettable
    {
        private int totalSilverInLevel = 0;
        private int collectedSilver = 0;
        private int totalGemsInLevel = 0;
        private int collectedGems = 0;

        private readonly List<Collectible> registeredCollectibles = new List<Collectible>();

        // Eventos C#
        public event Action<int, int> OnSilverUpdated; // (recolectadas, total)
        public event Action<int, int> OnGemsUpdated;   // (recolectadas, total)

        public int TotalSilverInLevel => totalSilverInLevel;
        public int CollectedSilver => collectedSilver;
        public int TotalGemsInLevel => totalGemsInLevel;
        public int CollectedGems => collectedGems;
        public bool AllSilverCollected => totalSilverInLevel > 0 && collectedSilver >= totalSilverInLevel;

        private void Start()
        {
            EscanearColeccionables();
        }

        public void EscanearColeccionables()
        {
            registeredCollectibles.Clear();
            var all = FindObjectsByType<Collectible>(FindObjectsInactive.Include);
            registeredCollectibles.AddRange(all);

            totalSilverInLevel = 0;
            totalGemsInLevel = 0;
            collectedSilver = 0;
            collectedGems = 0;

            foreach (var col in registeredCollectibles)
            {
                if (col is SilverVein) totalSilverInLevel++;
                else if (col is AmethystGem) totalGemsInLevel++;
            }

            OnSilverUpdated?.Invoke(collectedSilver, totalSilverInLevel);
            OnGemsUpdated?.Invoke(collectedGems, totalGemsInLevel);
        }

        public void SetExpectedCounts(int silver, int gems)
        {
            if (silver > 0) totalSilverInLevel = silver;
            if (gems > 0) totalGemsInLevel = gems;
            OnSilverUpdated?.Invoke(collectedSilver, totalSilverInLevel);
            OnGemsUpdated?.Invoke(collectedGems, totalGemsInLevel);
        }

        public void RegistrarVetaRecolectada()
        {
            collectedSilver++;
            OnSilverUpdated?.Invoke(collectedSilver, totalSilverInLevel);
        }

        public void RegistrarGemaRecolectada()
        {
            collectedGems++;
            OnGemsUpdated?.Invoke(collectedGems, totalGemsInLevel);
        }

        public void ResetObject()
        {
            collectedSilver = 0;
            collectedGems = 0;

            foreach (var c in registeredCollectibles)
            {
                if (c != null)
                {
                    c.ResetObject();
                }
            }

            OnSilverUpdated?.Invoke(collectedSilver, totalSilverInLevel);
            OnGemsUpdated?.Invoke(collectedGems, totalGemsInLevel);
        }
    }
}
