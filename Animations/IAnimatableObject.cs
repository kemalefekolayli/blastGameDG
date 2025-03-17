using UnityEngine;
public interface IAnimatableObject
{
    void AnimateMove(Vector3 startPosition, Vector3 endPosition, float duration);
    bool IsAnimating { get; }
}