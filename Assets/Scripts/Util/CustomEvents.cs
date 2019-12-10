using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CustomEvents
{

    [System.Serializable]
    public class GameObjectEvent : UnityEvent<GameObject>
    {
    }

    [System.Serializable]
    public class InteractionStateEvent : UnityEvent<InteractionState>
    {
    }

    [System.Serializable]
    public class ElectricNetworkEvent : UnityEvent<ElectricNetwork>
    {
    }

}
