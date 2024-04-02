using Runtime.Player;
using UnityEngine;

public class BodyAnimator : MonoBehaviour
{
    public int chainLength;
    
    [Space]
    public Transform head;

    private PlayerAvatar player;


    private void Awake()
    {
        player = GetComponentInParent<PlayerAvatar>();
    }

    private void Update()
    {
        var view = player.orientation;

        var t = head;
        var weight = 1f / chainLength;
        for (var i = 0; i < chainLength; i++)
        {
            t.localRotation = Quaternion.Euler(-view.y * weight, 0f, 0f);
            t = t.parent;
        }
    }
}
