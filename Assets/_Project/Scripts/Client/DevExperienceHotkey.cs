using Mirror;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using CyberMU.Shared.Characters;
using UnityEngine.InputSystem;
#endif

namespace CyberMU.Client
{
    // COMANDO DE DESENVOLVEDOR (lado do cliente): F9 pede EXP de teste ao
    // servidor, com o personagem no mundo.
    // A classe existe em todas as builds (a cena referencia o componente e
    // uma classe ausente viraria aviso de "script faltando"), mas o CONTEÚDO
    // só é compilado no editor e em build de desenvolvimento: numa build
    // normal de jogador o componente fica completamente vazio — não há nem o
    // código da tecla nem a mensagem para enviar.
    [AddComponentMenu("CyberMU/Dev Experience Hotkey (só desenvolvimento)")]
    public class DevExperienceHotkey : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update()
        {
            // Só com cliente conectado e cápsula no mundo.
            if (!NetworkClient.active || NetworkClient.localPlayer == null) return;

            // Keyboard.current é null sem teclado.
            if (Keyboard.current == null || !Keyboard.current.f9Key.wasPressedThisFrame) return;

            // Pedido VAZIO: a quantidade de EXP é decidida só pelo servidor
            // (DevExperienceCommand). O cliente nunca envia valores.
            NetworkClient.Send(new DevGrantExperienceRequestMessage());
        }
#endif
    }
}
