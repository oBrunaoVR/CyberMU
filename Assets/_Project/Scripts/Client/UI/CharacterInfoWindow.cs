using CyberMU.Shared.Characters;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CyberMU.Client.UI
{
    // JANELA DO PERSONAGEM TEMPORÁRIA (tecla C, como no original).
    // Somente leitura: mostra o que o SERVIDOR enviou (CharacterStatsMessage,
    // entregue só ao dono na entrada no mundo). O cliente não calcula nem
    // altera nada; sem botões de distribuir pontos por enquanto.
    [AddComponentMenu("CyberMU/Character Info Window (temporária)")]
    public class CharacterInfoWindow : MonoBehaviour
    {
        // Estado vindo do servidor.
        private CharacterStatsMessage stats;
        private bool statsReceived;

        // Estado da janela.
        private bool handlerRegistered;
        private bool open;

        private void Update()
        {
            bool clientActive = NetworkClient.active;

            if (clientActive && !handlerRegistered)
            {
                // requireAuthentication = false: o CLIENTE pode receber a
                // mensagem a qualquer momento; quem decide enviá-la (e para
                // quem) é sempre o servidor.
                NetworkClient.RegisterHandler<CharacterStatsMessage>(OnStats, false);
                handlerRegistered = true;
            }

            if (!clientActive && handlerRegistered)
            {
                NetworkClient.UnregisterHandler<CharacterStatsMessage>();
                handlerRegistered = false;
                ResetState(); // sair e entrar de novo recomeça do zero
            }

            // Tecla C abre/fecha (MU_099D_REFERENCE.md: FONTE DA ÉPOCA),
            // só com a cápsula no mundo. Keyboard.current é null sem teclado.
            if (InWorld() && Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
                open = !open;
        }

        private void OnGUI()
        {
            if (!open || !InWorld()) return;

            const float width = 260f;
            const float height = 280f;
            var area = new Rect(16f, (Screen.height - height) / 2f, width, height);

            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("Personagem (temporária) — C fecha");

            if (!statsReceived)
            {
                GUILayout.Label("Aguardando dados do servidor...");
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label($"Nome: {stats.name}");
            GUILayout.Label($"Classe: {ClassNames.GetDisplayName(stats.classId)}");
            GUILayout.Label($"Nível: {stats.level}");
            GUILayout.Space(6f);
            GUILayout.Label($"Força: {stats.strength}");
            GUILayout.Label($"Agilidade: {stats.agility}");
            GUILayout.Label($"Vitalidade: {stats.vitality}");
            GUILayout.Label($"Energia: {stats.energy}");
            if (stats.classId == ClassId.Overlord)
                GUILayout.Label($"Comando: {stats.command}");
            GUILayout.Space(6f);
            GUILayout.Label($"Pontos disponíveis: {stats.levelUpPoints}");

            GUILayout.EndArea();
        }

        private void OnStats(CharacterStatsMessage msg)
        {
            stats = msg;
            statsReceived = true;
        }

        private static bool InWorld()
        {
            return NetworkClient.active && NetworkClient.localPlayer != null;
        }

        private void ResetState()
        {
            stats = default;
            statsReceived = false;
            open = false;
        }
    }
}
