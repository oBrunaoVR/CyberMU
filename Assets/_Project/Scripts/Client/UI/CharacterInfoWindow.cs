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
            GUILayout.Label($"{Term(CharacterTermId.Experience)}: {stats.experience:N0}");
            GUILayout.Label(stats.experienceForNextLevel > 0
                ? $"Próximo nível: {stats.experienceForNextLevel:N0}"
                : "Próximo nível: —");
            // Valores calculados no servidor; hoje atual = máxima (sem dano
            // nem gasto de mana ainda). Classe com Vida/Mana PENDENTES mostra
            // "[A DEFINIR]" em vez de um 0 que pareceria valor real.
            if (stats.vitalsPending)
            {
                GUILayout.Label($"{Term(CharacterTermId.Health)}: [A DEFINIR]");
                GUILayout.Label($"{Term(CharacterTermId.Mana)}: [A DEFINIR]");
            }
            else
            {
                GUILayout.Label($"{Term(CharacterTermId.Health)}: {stats.currentHealth} / {stats.maxHealth}");
                GUILayout.Label($"{Term(CharacterTermId.Mana)}: {stats.currentMana} / {stats.maxMana}");
            }
            GUILayout.Space(6f);
            StatRow(stats.strength, StatId.Strength);
            StatRow(stats.agility, StatId.Agility);
            StatRow(stats.vitality, StatId.Vitality);
            StatRow(stats.energy, StatId.Energy);
            // Mesma regra do servidor (StatPointService): só a classe com
            // HasCommand no ClassCatalog mostra Comando.
            if (ClassCatalog.TryGet(stats.classId, out ClassDefinition classDef) && classDef.HasCommand)
                StatRow(stats.command, StatId.Command);
            GUILayout.Space(6f);
            GUILayout.Label($"Pontos disponíveis: {stats.levelUpPoints}");

            GUILayout.EndArea();
        }

        // Uma linha de atributo. O botão + só APARECE quando há pontos
        // disponíveis; o clique envia só o atributo e espera a ficha nova.
        // O nome do atributo vem da tabela de nomes (StatNames, NAMING.md).
        private void StatRow(int value, StatId stat)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{StatNames.GetDisplayName(stat)}: {value}");
            if (stats.levelUpPoints > 0 && GUILayout.Button("+", GUILayout.Width(24f)))
                NetworkClient.Send(new IncreaseStatRequestMessage { stat = stat });
            GUILayout.EndHorizontal();
        }

        // Nome exibido de Vida/Mana/EXP (CharacterTermNames, NAMING.md).
        private static string Term(CharacterTermId id)
        {
            return CharacterTermNames.GetDisplayName(id);
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
