using UnityEngine;

namespace CyberMU.Shared.Network
{
    // Remove o HUD do Mirror (botões Host / Client / Server Only) em builds.
    // No editor nada muda: o HUD continua lá para os testes com Server Only
    // e Multiplayer Play Mode. No build do jogador não pode haver "Host":
    // o código de servidor não existe nesse build, então um Host partiria
    // sem banco e quebraria.
    [AddComponentMenu("CyberMU/HUD do Mirror só no editor")]
    public class NetworkHudEditorOnly : MonoBehaviour
    {
#if !UNITY_EDITOR
        void Awake()
        {
            Mirror.NetworkManagerHUD hud = GetComponent<Mirror.NetworkManagerHUD>();
            if (hud != null) Destroy(hud);
        }
#endif
    }
}
