using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHost
{
    ParasiteControl parasite { get; }
    void TakeControl(ParasiteControl parasite);
}
