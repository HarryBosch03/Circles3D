using System;
using Runtime.Player;
using UnityEngine;

public class BodyAnimator : MonoBehaviour
{
    public float torsoTwist;
    
    [Space]
    public Transform head;
    public Transform torso;
    public Transform root;
    public Renderer renderer;

    private PlayerAvatar player;

    private void OnValidate()
    {
        if (!renderer) renderer = GetComponentInChildren<Renderer>();
    }

    private void Awake()
    {
        player = GetComponentInParent<PlayerAvatar>();
    }

    private void Update()
    {
        renderer.enabled = !player.HasInputAuthority;
        
        var twist = Quaternion.Euler(90f + torsoTwist, 90f, 90f);
        root.localRotation = twist;
        
        var view = player.movement.orientation;

        torso.localRotation = Quaternion.Euler(view.x * 0.5f, 0f, 0f);
        head.rotation = Quaternion.Euler(view.x, view.y, 0f);
    }
}
