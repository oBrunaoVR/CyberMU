using Mirror;
using UnityEngine;
using UnityEngine.AI;

// Lado do SERVIDOR do movimento point-and-click.
// Recebe o destino pedido pelo dono da cápsula, valida e move com NavMeshAgent.
// O NavMeshAgent só fica ligado no servidor; os clientes recebem a posição
// pelo NetworkTransform.
[RequireComponent(typeof(NavMeshAgent))]
public class ServerPlayerMover : NetworkBehaviour
{
    // PENDENTE DE VERIFICAÇÃO: velocidade do MU 0.99D
    [SerializeField] float moveSpeed = 5f;

    // Distância máxima (em metros) entre o ponto clicado e o NavMesh para o
    // pedido ser aceito. Valor técnico de validação, não é regra do MU.
    [SerializeField] float maxDistanceFromNavMesh = 0.5f;

    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false; // desligado em todo lugar até o servidor ligar
    }

    public override void OnStartServer()
    {
        agent.speed = moveSpeed;
        agent.enabled = true;
    }

    // Chamado pelo cliente dono da cápsula; o corpo roda no servidor.
    // Por padrão o Mirror só aceita este Command vindo do dono do objeto.
    [Command]
    public void CmdRequestMove(Vector3 requestedPoint)
    {
        if (!TryGetValidDestination(requestedPoint, out Vector3 destination))
            return;

        agent.SetDestination(destination);
    }

    [Server]
    bool TryGetValidDestination(Vector3 requestedPoint, out Vector3 destination)
    {
        destination = Vector3.zero;

        if (float.IsNaN(requestedPoint.x) || float.IsNaN(requestedPoint.y) || float.IsNaN(requestedPoint.z))
            return false;

        if (!NavMesh.SamplePosition(requestedPoint, out NavMeshHit hit, maxDistanceFromNavMesh, NavMesh.AllAreas))
            return false;

        destination = hit.position;
        return true;
    }
}
