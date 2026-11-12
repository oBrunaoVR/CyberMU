using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

// Lado do CLIENTE do movimento point-and-click.
// Só no jogador local: ao clicar com o botão esquerdo no chão, envia o ponto
// ao servidor. Este script NUNCA move a cápsula diretamente.
[RequireComponent(typeof(ServerPlayerMover))]
public class ClientClickToMove : NetworkBehaviour
{
    // Camadas que o clique pode atingir. O padrão ignora a camada
    // "Ignore Raycast", onde ficam as cápsulas dos jogadores.
    [SerializeField] LayerMask clickableLayers = Physics.DefaultRaycastLayers;
    [SerializeField] float maxRayDistance = 500f;

    ServerPlayerMover mover;

    void Awake()
    {
        mover = GetComponent<ServerPlayerMover>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, clickableLayers))
        {
            mover.CmdRequestMove(hit.point);
        }
    }
}
