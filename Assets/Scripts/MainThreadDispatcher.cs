using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> ExecuteOnMainThread = new Queue<Action>();
    private static MainThreadDispatcher instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void RunOnMainThread(Action action)
    {
        if(action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (ExecuteOnMainThread)
        {
            ExecuteOnMainThread.Enqueue(action);
        }
    }

    public void Update()
    {
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }
}
