using System.Collections.Generic;
using UnityEngine;

public class ColonyOverlayManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject territoryPrefab;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomInThreshold = 10f;
    [SerializeField] private float zoomOutThreshold = 50f;
    
    private Dictionary<I_Colony, TerritoryVisual> activeVisuals = new Dictionary<I_Colony, TerritoryVisual>();
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        
        ColonieSystem.OnColonyCreatedEvent += HandleColonyCreated;
        ColonieSystem.OnColonyDissolvedEvent += HandleColonyDissolved;
        
        StartCoroutine(UpdateVisualsRoutine());
    }

    void OnDestroy()
    {
        ColonieSystem.OnColonyCreatedEvent -= HandleColonyCreated;
        ColonieSystem.OnColonyDissolvedEvent -= HandleColonyDissolved;
    }

    void HandleColonyCreated(I_Colony _colony)
    {
        if (_colony == null || activeVisuals.ContainsKey(_colony)) return;
        
        Random.InitState(_colony.GetId() * 555); 
        Color colColor = Color.HSVToRGB(Random.Range(0f, 1f), 0.7f, 0.9f);
        
        GameObject visObj = Instantiate(territoryPrefab, Vector3.zero, Quaternion.identity, transform);
        visObj.name = $"Territory_Colony_{_colony.GetId()}";
        
        TerritoryVisual visual = visObj.GetComponent<TerritoryVisual>();
        if (visual != null)
        {
            visObj.transform.position = _colony.GetColonyCenter(); 
            visual.Initialize(_colony.GetInfluenceRadius(), colColor);
            
            activeVisuals.Add(_colony, visual);
        }
    }

    void HandleColonyDissolved(I_Colony _colony)
    {
        if (activeVisuals.TryGetValue(_colony, out TerritoryVisual visual))
        {
            if (visual != null) Destroy(visual.gameObject);
            activeVisuals.Remove(_colony);
        }
    }

    System.Collections.IEnumerator UpdateVisualsRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            if (mainCam == null)
            {
                yield return wait;
                continue;
            }
            
            float currentSize = mainCam.orthographicSize;
            float zoomFactor = Mathf.InverseLerp(zoomInThreshold, zoomOutThreshold, currentSize);
            
            foreach (KeyValuePair<I_Colony, TerritoryVisual> kvp in activeVisuals)
            {
                I_Colony col = kvp.Key;
                TerritoryVisual vis = kvp.Value;

                if (col == null || vis == null) continue;
                
                vis.transform.position = col.GetColonyCenter(); 
                vis.UpdateRadius(col.GetInfluenceRadius());
                
                vis.SetFillOpacity(zoomFactor);
                
                string speciesName = "Unknown";
                
                Colony c = col as Colony;
                if (c != null)
                {
                    int speciesRank = 1;
                    
                    foreach (KeyValuePair<I_Colony, TerritoryVisual> otherKvp in activeVisuals)
                    {
                        Colony other = otherKvp.Key as Colony;
                        
                        if (other != null && other != c && other.ColonySpecies == c.ColonySpecies)
                        {
                            if (other.GetId() < c.GetId())
                            {
                                speciesRank++;
                            }
                        }
                    }
                    
                    speciesName = $"{c.ColonySpecies} {speciesRank}"; 
                }
                
                int population = col.GetInhabitants(); 

                vis.UpdateLabel(speciesName, population);
            }

            yield return wait;
        }
    }
}