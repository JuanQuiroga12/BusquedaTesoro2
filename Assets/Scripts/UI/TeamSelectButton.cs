using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BusquedaTesoro.Core;

namespace BusquedaTesoro.UI
{
    /// <summary>
    /// Se coloca en cada botón "ELEGIR" de la pantalla de selección de grupos.
    /// Al presionar, guarda el equipo seleccionado y carga la escena de Gameplay.
    /// </summary>
    public class TeamSelectButton : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private TeamData.TeamID teamID;

        [Header("Escena destino")]
        [SerializeField] private string gameplaySceneName = "Gameplay";

        private void Start()
        {
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnSelectTeam);
            }
            else
            {
                Debug.LogError($"[TeamSelectButton] No se encontró componente Button en {gameObject.name}");
            }
        }

        private void OnSelectTeam()
        {
            TeamData.SelectedTeam = teamID;
            Debug.Log($"[TeamSelectButton] Equipo seleccionado: {TeamData.TeamDisplayName} (#{TeamData.TeamNumber})");
            SceneManager.LoadScene(gameplaySceneName);
        }

        private void OnDestroy()
        {
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveListener(OnSelectTeam);
            }
        }
    }
}