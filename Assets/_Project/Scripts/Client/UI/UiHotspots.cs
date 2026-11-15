using System.Collections.Generic;
using UnityEngine;

namespace CyberMU.Client.UI
{
    // PONTO ÚNICO da pergunta "o mouse está sobre a interface?".
    // Cada janela visível registra sua área a cada quadro (Report, chamado
    // dentro do OnGUI); quem lê cliques no mundo (ClientClickToMove) chama
    // IsMouseOver antes de agir.
    //
    // Tempo do registro: o Update (onde o clique é lido) roda ANTES do OnGUI
    // no mesmo quadro. Por isso há DUAS listas: a do quadro atual sendo
    // preenchida e a do quadro ANTERIOR sendo consultada; elas trocam de
    // papel a cada quadro. Janela que fechou libera a área um quadro depois.
    //
    // Interface definitiva: quando a UI de verdade existir, basta IsMouseOver
    // também consultá-la aqui (ex.: EventSystem.IsPointerOverGameObject) —
    // quem lê cliques não precisa mudar.
    public static class UiHotspots
    {
        // Lista em preenchimento (quadro atual) e lista consultável
        // (quadro anterior), com o número do quadro a que cada uma pertence.
        private static List<Rect> filling = new List<Rect>();
        private static List<Rect> ready = new List<Rect>();
        private static int fillingFrame = -1;
        private static int readyFrame = -1;

        // Registra a área de uma janela visível. Chamar a cada OnGUI,
        // com o mesmo Rect usado no BeginArea (coordenadas do IMGUI:
        // origem no topo-esquerdo, y para baixo).
        public static void Report(Rect guiArea)
        {
            // O OnGUI roda vários passes por quadro (Layout, Repaint,
            // eventos); só o Repaint conta, para registrar uma vez só.
            if (Event.current != null && Event.current.type != EventType.Repaint)
                return;

            AdvanceFrame();
            filling.Add(TransformByGuiMatrix(guiArea));
        }

        // Diz se o mouse está sobre alguma janela registrada. Recebe a
        // posição do Input System (origem embaixo-esquerda, y para cima).
        public static bool IsMouseOver(Vector2 screenPosition)
        {
            AdvanceFrame();

            // Só vale a lista do quadro imediatamente anterior; mais velha
            // que isso significa que nenhuma janela estava visível.
            if (readyFrame != Time.frameCount - 1)
                return false;

            // Converte para as coordenadas do IMGUI (y invertido).
            var guiPoint = new Vector2(screenPosition.x, Screen.height - screenPosition.y);

            foreach (Rect area in ready)
                if (area.Contains(guiPoint))
                    return true;

            return false;
        }

        // Troca as listas quando o quadro muda (chamada barata nos demais).
        private static void AdvanceFrame()
        {
            int frame = Time.frameCount;
            if (frame == fillingFrame) return;

            (ready, filling) = (filling, ready);
            readyFrame = fillingFrame;
            filling.Clear();
            fillingFrame = frame;
        }

        // GUI.matrix pode escalar/deslocar a janela; aplica a matriz aos
        // quatro cantos e devolve o retângulo que os envolve, em pixels
        // reais da tela. Com a matriz identidade (o comum), devolve igual.
        private static Rect TransformByGuiMatrix(Rect r)
        {
            Matrix4x4 m = GUI.matrix;
            if (m.isIdentity) return r;

            Vector3 a = m.MultiplyPoint3x4(new Vector3(r.xMin, r.yMin, 0f));
            Vector3 b = m.MultiplyPoint3x4(new Vector3(r.xMax, r.yMin, 0f));
            Vector3 c = m.MultiplyPoint3x4(new Vector3(r.xMin, r.yMax, 0f));
            Vector3 d = m.MultiplyPoint3x4(new Vector3(r.xMax, r.yMax, 0f));

            return Rect.MinMaxRect(
                Mathf.Min(a.x, b.x, c.x, d.x),
                Mathf.Min(a.y, b.y, c.y, d.y),
                Mathf.Max(a.x, b.x, c.x, d.x),
                Mathf.Max(a.y, b.y, c.y, d.y));
        }
    }
}
