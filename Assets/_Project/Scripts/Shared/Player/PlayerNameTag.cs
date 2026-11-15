using Mirror;
using UnityEngine;

namespace CyberMU.Shared.Player
{
    // Nome do personagem acima da cápsula, visível para TODOS os jogadores.
    // O SERVIDOR preenche characterName ao criar o jogador; o SyncVar do
    // Mirror entrega o valor a todos os clientes junto com o nascimento.
    // VISUAL TEMPORÁRIO: texto desenhado com OnGUI (mesmo estilo das telas
    // provisórias); será trocado por UI de verdade depois.
    [AddComponentMenu("CyberMU/Player Name Tag (temporário)")]
    public class PlayerNameTag : NetworkBehaviour
    {
        // Altura do texto acima do centro da cápsula, em metros.
        // Valor só visual, não é regra do MU.
        [SerializeField] private float heightAboveHead = 1.4f;

        [SyncVar] public string characterName = string.Empty;

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(characterName)) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 world = transform.position + Vector3.up * heightAboveHead;
            Vector3 screen = cam.WorldToScreenPoint(world);
            if (screen.z <= 0f) return; // atrás da câmera: não desenha

            const float width = 200f;
            const float height = 22f;
            // OnGUI conta o Y de cima para baixo; WorldToScreenPoint, de baixo
            // para cima. Por isso a conversão com Screen.height.
            var rect = new Rect(screen.x - width / 2f, Screen.height - screen.y - height, width, height);
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(rect, characterName, style);
        }
    }
}
