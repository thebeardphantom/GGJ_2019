using DG.Tweening;
using RSG;
using UnityEngine;

public static class PromiseUtil
{
    #region Methods

    public static IPromise<AsyncOperation> ToPromise(this AsyncOperation op)
    {
        var promise = new Promise<AsyncOperation>();
        op.completed += operation =>
        {
            promise.Resolve(operation);
        };
        return promise;
    }

    public static IPromise<Tween> ToPromise(this Tween tween)
    {
        var promise = new Promise<Tween>();
        tween.onComplete += () =>
        {
            promise.Resolve(tween);
        };
        return promise;
    }

    public static IPromise ToUntypedPromise(this AsyncOperation op)
    {
        var promise = new Promise();
        op.completed += operation =>
        {
            promise.Resolve();
        };
        return promise;
    }

    public static IPromise ToUntypedPromise(this Tween tween)
    {
        var promise = new Promise();
        tween.onComplete += () =>
        {
            promise.Resolve();
        };
        return promise;
    }

    #endregion
}