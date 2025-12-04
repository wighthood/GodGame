using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Agents
{
    // Composant léger à ajouter à n'importe quelle créature/agent qui doit pouvoir participer
    // à la création de colonies.
    [DisallowMultipleComponent]
    public class ColonyAgent : MonoBehaviour, IColonyAgent
    {
        [Header("Colony Agent settings")]
        public bool autoRegister = true;

        [Tooltip("Si false, cet agent ne participera pas à la création de colonies")]
        public bool canFormColony = true;
        //Seules les agents de la même espèce peuvent créer une colonie ensemble.
        [Tooltip("Espèce de l'agent")]
        public string species = "Pimu";

        [Header("Movement tracking")]
        public float positionUpdateInterval = 0.5f;
        public float movementThreshold = 0.25f;

        private Vector3 _lastPosition;
        private float _timer = 0f;
        private bool _registered = false;

        void Start()
        {
            _lastPosition = transform.position;
            if (autoRegister && ColonieSystem.Instance != null)
            {
                ColonieSystem.Instance.RegisterAgent(this);
                _registered = true;
            }
        }

        void Update()
        {
            if (!_registered && autoRegister && ColonieSystem.Instance != null)
            {
                ColonieSystem.Instance.RegisterAgent(this);
                _registered = true;
            }

            _timer += Time.deltaTime;
            if (_timer >= positionUpdateInterval)
            {
                _timer = 0f;
                Vector3 current = transform.position;
                float dist = Vector3.Distance(current, _lastPosition);
                if (dist >= movementThreshold)
                {
                    if (_registered && ColonieSystem.Instance != null)
                    {
                        ColonieSystem.Instance.UpdateAgentCell(this, _lastPosition);
                    }
                    _lastPosition = current;
                }
            }
        }

        void OnDestroy()
        {
            if (_registered && ColonieSystem.Instance != null)
            {
                ColonieSystem.Instance.UnregisterAgent(this);
                _registered = false;
            }
        }

        // Utilitaire pour forcer l'enregistrement depuis un système externe (pouvoir de spawn)
        public void ForceRegister()
        {
            if (ColonieSystem.Instance != null)
            {
                ColonieSystem.Instance.RegisterAgent(this);
                _registered = true;
                _lastPosition = transform.position;
            }
        }

        // Accesseur utile pour éviter accès direct au champ depuis d'autres assemblies
        public string GetSpecies()
        {
            return species != null ? species : string.Empty;
        }

        // Implémentation de l'interface
        public bool CanFormColony => canFormColony;

        // Référence à la colonie courante (si assignée)
        private IColony _currentColony;

        public void SetCurrentColony(IColony colony)
        {
            _currentColony = colony;
        }

        public IColony GetCurrentColony()
        {
            return _currentColony;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            // Draw small marker and label in editor to show colony membership
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.15f);

            var col = GetCurrentColony();
            string label = "No colony";
            if (col != null)
            {
                label = $"Colony {col.Id} ({col.Species}) - {col.Inhabitants}/{col.MaxInhabitants}";
            }
            Handles.Label(transform.position + Vector3.up * 1.2f, label);
        }
#endif
    }
}
