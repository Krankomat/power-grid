using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractionStateHandleable 
{
    PlayerManager PlayerManager { get; set; }
    bool IsActive { get; set; }
    void HandleControls();
    void Enter();
    void Process(); 
    void Exit(); 
}
