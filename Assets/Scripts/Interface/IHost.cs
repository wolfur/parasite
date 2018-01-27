using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHost
{
    ParasiteControl parasite { get; }
    float launchForce { get; }

    void TakeControl(ParasiteControl parasite);
}
