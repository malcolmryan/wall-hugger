using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class MouseProcessor : InputProcessor<Vector2>
{
    #if UNITY_EDITOR
    static MouseProcessor()
    {
        Initialize();
    }
    #endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<MouseProcessor>();
    }

    public override Vector2 Process(Vector2 pos, InputControl control)
    {
        Vector2 dir = Camera.main.ScreenToViewportPoint(pos);
        dir = 2*dir - Vector2.one;
        
        if (Camera.main.aspect > 1)
        {
            dir.x /= Camera.main.aspect;
        }
        else 
        {
            dir.y *= Camera.main.aspect;
        }

        return dir;
    }
}

