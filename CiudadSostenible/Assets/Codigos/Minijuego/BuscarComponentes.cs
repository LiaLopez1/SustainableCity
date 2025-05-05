using UnityEngine;
using UnityEditor;

public class BuscarComponentes : EditorWindow
{
    [MenuItem("Herramientas/Buscar Script en Escena")]
    public static void ShowWindow()
    {
        GetWindow<BuscarComponentes>("Buscar Script");
    }

    private string scriptName = "DragItem";

    void OnGUI()
    {
        GUILayout.Label("Buscar GameObjects con este script", EditorStyles.boldLabel);
        scriptName = EditorGUILayout.TextField("Nombre del Script:", scriptName);

        if (GUILayout.Button("Buscar en Escena"))
        {
            MonoBehaviour[] all = GameObject.FindObjectsOfType<MonoBehaviour>();
            foreach (var mb in all)
            {
                if (mb.GetType().Name == scriptName)
                {
                    Debug.Log($"Encontrado en: {mb.gameObject.name}", mb.gameObject);
                }
            }
        }
    }
}
