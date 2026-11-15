using CyberMU.Shared.Characters;
using Mirror;
using UnityEngine;

namespace CyberMU.Client.UI
{
    // TELA DE SELEÇÃO DE PERSONAGEM TEMPORÁRIA (mesmo estilo da LoginScreen).
    // Aparece depois que o login é aceito e some quando a cápsula nasce.
    // - Mostra os 5 slots: vazio, ou nome + nome exibido da classe + nível.
    // - "Criar" abre um formulário com nome + classe. Só aparecem as classes
    //   que o SERVIDOR mandou como desbloqueadas (classe bloqueada fica
    //   escondida, como no MU original). Nome exibido vem da tabela de nomes.
    // - Mostra as mensagens de recusa do servidor.
    // Não decide nada: quem valida (nome, classe, dono do personagem) é o servidor.
    [AddComponentMenu("CyberMU/Character Select Screen (temporária)")]
    public class CharacterSelectScreen : MonoBehaviour
    {
        private const int SlotCount = 5; // mesmo limite do servidor (5 por conta)

        // Estado vindo do servidor.
        private CharacterSummary[] characters = new CharacterSummary[0];
        private ClassId[] unlockedClasses = new ClassId[0];
        private bool listReceived;

        // Estado da tela.
        private bool handlersRegistered;
        private bool listRequested;
        private int selectedSlot = -1;
        private bool creating;
        private string newName = string.Empty;
        private int newClassIndex;
        private string statusMessage;

        private void Update()
        {
            bool clientActive = NetworkClient.active;

            if (clientActive && !handlersRegistered)
            {
                // requireAuthentication = false: o CLIENTE pode receber essas
                // respostas a qualquer momento; quem exige login é o servidor.
                NetworkClient.RegisterHandler<CharacterListResponseMessage>(OnListResponse, false);
                NetworkClient.RegisterHandler<CharacterCreateResponseMessage>(OnCreateResponse, false);
                NetworkClient.RegisterHandler<EnterWorldResponseMessage>(OnEnterWorldResponse, false);
                handlersRegistered = true;
            }

            if (!clientActive && handlersRegistered)
            {
                NetworkClient.UnregisterHandler<CharacterListResponseMessage>();
                NetworkClient.UnregisterHandler<CharacterCreateResponseMessage>();
                NetworkClient.UnregisterHandler<EnterWorldResponseMessage>();
                handlersRegistered = false;
                ResetState(); // sair e entrar de novo recomeça do zero
            }

            // Assim que o login é aceito, pede a lista UMA vez.
            if (clientActive && IsAuthenticated() && !listRequested)
            {
                listRequested = true;
                NetworkClient.Send(new CharacterListRequestMessage());
            }
        }

        private void OnGUI()
        {
            // Só aparece: conectado + login aceito + ainda sem cápsula.
            // (Em Server Only o NetworkClient nunca fica ativo: não aparece.)
            if (!NetworkClient.active || !IsAuthenticated()) return;
            if (NetworkClient.localPlayer != null) return;

            const float width = 360f;
            const float height = 400f;
            var area = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);

            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("Seleção de personagem (temporária)");

            if (!listReceived)
            {
                GUILayout.Label("Carregando personagens...");
                GUILayout.EndArea();
                return;
            }

            DrawSlots();
            GUILayout.Space(8f);

            if (creating) DrawCreateForm();
            else DrawActions();

            if (!string.IsNullOrEmpty(statusMessage))
                GUILayout.Label(statusMessage);

            GUILayout.EndArea();
        }

        private void DrawSlots()
        {
            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (TryGetBySlot(slot, out CharacterSummary c))
                {
                    string marker = selectedSlot == slot ? "> " : "   ";
                    string label = $"{marker}{c.name} — {ClassNames.GetDisplayName(c.classId)} — nível {c.level}";
                    if (GUILayout.Button(label))
                    {
                        selectedSlot = slot;
                        statusMessage = null;
                    }
                }
                else
                {
                    GUILayout.Label($"   Slot {slot + 1}: (vazio)");
                }
            }
        }

        private void DrawActions()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Criar"))
            {
                creating = true;
                newName = string.Empty;
                newClassIndex = 0;
                statusMessage = null;
            }

            bool hasSelection = TryGetBySlot(selectedSlot, out CharacterSummary selected);
            GUI.enabled = hasSelection;
            if (GUILayout.Button("Entrar") && hasSelection)
            {
                statusMessage = "Entrando...";
                NetworkClient.Send(new EnterWorldRequestMessage { characterId = selected.id });
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        private void DrawCreateForm()
        {
            GUILayout.Label("Novo personagem");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Nome", GUILayout.Width(50f));
            newName = GUILayout.TextField(newName, CharacterNameRules.Default.MaxLength);
            GUILayout.EndHorizontal();

            // Só as classes desbloqueadas enviadas pelo servidor.
            for (int i = 0; i < unlockedClasses.Length; i++)
            {
                bool on = newClassIndex == i;
                bool clicked = GUILayout.Toggle(on, ClassNames.GetDisplayName(unlockedClasses[i]));
                if (clicked && !on) newClassIndex = i;
            }

            GUILayout.BeginHorizontal();
            GUI.enabled = unlockedClasses.Length > 0;
            if (GUILayout.Button("Confirmar") && unlockedClasses.Length > 0)
            {
                statusMessage = "Criando...";
                NetworkClient.Send(new CharacterCreateRequestMessage
                {
                    name = newName,
                    classId = unlockedClasses[newClassIndex],
                });
            }
            GUI.enabled = true;
            if (GUILayout.Button("Cancelar"))
            {
                creating = false;
                statusMessage = null;
            }
            GUILayout.EndHorizontal();
        }

        private void OnListResponse(CharacterListResponseMessage msg)
        {
            characters = msg.characters ?? new CharacterSummary[0];
            unlockedClasses = msg.unlockedClasses ?? new ClassId[0];
            listReceived = true;
            if (newClassIndex >= unlockedClasses.Length) newClassIndex = 0;
            if (!TryGetBySlot(selectedSlot, out _)) selectedSlot = -1;
        }

        private void OnCreateResponse(CharacterCreateResponseMessage msg)
        {
            statusMessage = msg.message;
            if (msg.code == CharacterCreateCode.Accepted)
            {
                creating = false;
                selectedSlot = msg.character.slot;
                // Pede a lista de novo: o servidor é a fonte da verdade.
                NetworkClient.Send(new CharacterListRequestMessage());
            }
        }

        private void OnEnterWorldResponse(EnterWorldResponseMessage msg)
        {
            // Quando aceito, a tela some sozinha ao nascer a cápsula;
            // só a recusa precisa aparecer.
            statusMessage = msg.code == EnterWorldCode.Accepted ? null : msg.message;
        }

        private bool TryGetBySlot(int slot, out CharacterSummary result)
        {
            foreach (CharacterSummary c in characters)
            {
                if (c.slot == slot)
                {
                    result = c;
                    return true;
                }
            }
            result = default;
            return false;
        }

        private static bool IsAuthenticated()
        {
            return NetworkClient.connection != null && NetworkClient.connection.isAuthenticated;
        }

        private void ResetState()
        {
            characters = new CharacterSummary[0];
            unlockedClasses = new ClassId[0];
            listReceived = false;
            listRequested = false;
            selectedSlot = -1;
            creating = false;
            newName = string.Empty;
            newClassIndex = 0;
            statusMessage = null;
        }
    }
}
