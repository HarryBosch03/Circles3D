using Runtime.Player;
using UnityEngine;

public class BodyAnimator : MonoBehaviour
{
    public float torsoTwist;
    
    [Space]
    public Transform head;
    public Transform torso;
    public Transform root;

    private PlayerAvatar player;
    
    private void Awake()
    {
        player = GetComponentInParent<PlayerAvatar>();
    }

    private void Update()
    {
        var twist = Quaternion.Euler(90f + torsoTwist, 90f, 90f);
        root.localRotation = twist;
        
        var view = player.view;

        var start = Quaternion.Euler(0f, view.eulerAngles.y, 0f);
        var end = view.rotation;
        
        torso.rotation = Quaternion.Slerp(start, end, 0.5f);
        head.rotation = end;
    }
}
