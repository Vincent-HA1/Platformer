using System;

public class BigCoin : Collectible
{
    public Action<BigCoin> PickedUpBigCoin;

    protected override void Collect()
    {
        PickedUpBigCoin?.Invoke(this);
        base.Collect();
    }
}
