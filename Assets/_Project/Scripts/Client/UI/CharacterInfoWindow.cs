using CyberMU.Shared.Characters;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CyberMU.Client.UI
{
    // JANELA DO PERSONAGEM TEMPORÁRIA (tecla C, como no original).
    // Mostra o que o SERVIDOR enviou (CharacterStatsMessage, entregue só ao
    // dono). O cliente não calcula nem altera nada: o botão + só envia QUAL
    // atributo aumentar; validação, quantidade e o novo valor vêm do servidor.
    // A janela registra sua área em UiHotspots: clique sobre ela não vira
    // movimento (ClientClickToMove consulta antes de enviar o destino).
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
            const float height = 360f;
            var area = new Rect(16f, (Screen.height - height) / 2f, width, height);
            UiHotspots.Report(area); // clique aqui não deve virar movimento

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
            // Os dois valores vêm prontos do servidor; 0 em
            // experienceForNextLevel significa nível máximo.
            GUILayout.Label($"EXP: {stats.experience:N0}");
            GUILayout.Label(stats.experienceForNextLevel > 0
                ? $"Próximo nível: {stats.experienceForNextLevel:N0}"
                : "Próximo nível: —");
            // Valores calculados no servidor; hoje atual = máxima (sem dano
            // nem gasto de mana ainda).
            GUILayout.Label($"Vida: {stats.currentHealth} / {stats.maxHealth}");
            GUILayout.Label($"Mana: {stats.currentMana} / {stats.maxMana}");
            GUILayout.Space(6f);
            StatRow("Força", stats.strength, StatId.Strength);
            StatRow("Agilidade", stats.agility, StatId.Agility);
            StatRow("Vitalidade", stats.vitality, StatId.Vitality);
            StatRow("Energia", stats.energy, StatId.Energy);
            if (stats.classId == ClassId.Overlord)
                StatRow("Comando", stats.command, StatId.Command);
            GUILayout.Space(6f);
            GUILayout.Label($"Pontos disponíveis: {stats.levelUpPoints}");

            GUILayout.EndArea();
        }

        // Uma linha de atributo. O botão + só APARECE quando há pontos
        // disponíveis; o clique envia só o atributo e espera a ficha nova.
        private void StatRow(string label, int value, StatId stat)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {value}");
            if (stats.levelUpPoints > 0 && GUILayout.Button("+", GUILayout.Width(24f)))
                NetworkClient.Send(new IncreaseStatRequestMessage { stat = stat });
            GUILayout.EndHorizontal();
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
