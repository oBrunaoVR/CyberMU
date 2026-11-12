using Mirror;
using UnityEngine;

// Câmera do CLIENTE: segue o jogador local (o personagem desta janela)
// com distância e ângulo fixos. É só visual; não mexe em nada do jogo.
//
// Zoom e rotação: PENDENTE DE VERIFICAÇÃO se existiam na 0.99D
// (por isso esta câmera não gira e não tem zoom).
public class ClientCameraFollow : MonoBehaviour
{
    // PENDENTE DE VERIFICAÇÃO: enquadramento exato do MU 0.99D
    [SerializeField] float height = 12f;     // altura acima do personagem
    [SerializeField] float distance = 12f;   // distância para trás do personagem
    [SerializeField] float pitchAngle = 45f; // inclinação para baixo, em graus

    // Tempo aproximado (segundos) para a câmera alcançar o personagem.
    // Valor técnico de suavização, não é regra do MU.
    [SerializeField] float smoothTime = 0.08f;

    Vector3 velocity;

    void LateUpdate()
    {
        NetworkIdentity localPlayer = NetworkClient.localPlayer;
        if (localPlayer == null) return;

        Vector3 targetPosition = localPlayer.transform.position + new Vector3(0f, height, -distance);

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }
}
