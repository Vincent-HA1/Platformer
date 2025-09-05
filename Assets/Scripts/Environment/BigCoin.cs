using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BigCoin : Collectible
{
    public Action<BigCoin> PickedUpBigCoin;

    protected override void Collect()
    {
        PickedUpBigCoin?.Invoke(this);
        base.Collect();
    }
}
