using System.Text.RegularExpressions;
using Fusion;
using Runtime.Networking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Player
{
    [RequireComponent(typeof(PlayerAvatar))]
    public class AdminMenu : NetworkBehaviour
    {
        private bool open;
        private PlayerAvatar player;
        private string search;

        private void Awake() { player = GetComponent<PlayerAvatar>(); }

        private void Update()
        {
            if (HasInputAuthority)
            {
                if (!open && Keyboard.current.backquoteKey.wasPressedThisFrame) SetOpen(true);
                if (open && Keyboard.current.escapeKey.wasPressedThisFrame) SetOpen(false);
                if (open && InputManager.isControllingPlayer) SetOpen(false);
            }
        }

        private void SetOpen(bool open)
        {
            this.open = open;
            InputManager.SetIsControllingPlayer(!open);
        }

        private void OnGUI()
        {
            var str = "";
            for (var i = 0; i < player.statboard.mods.Count; i++)
            {
                str += player.statboard.mods[i].name;
                if (i != player.statboard.mods.Count - 1) str += ", ";
            }
            
            GUI.Label(new Rect(0f, 0f, Screen.width, 50f), str, new GUIStyle("label")
            {
                alignment = TextAnchor.MiddleCenter,
            });

            if (!open) return;

            var rect = new Rect();
            rect.size = new Vector2(Screen.height * 0.75f * (4f / 3f), Screen.height * 0.75f);
            rect.center = new Vector2(Screen.width, Screen.height) / 2f;

            GUI.Box(rect, "");
            var box = new GUIStyle("box");
            using (new GUILayout.AreaScope(rect))
            {
                GUILayout.Label("Admin Menu");
                search = GUILayout.TextField(search);
                
                using (new GUILayout.HorizontalScope(GUILayout.ExpandHeight(true)))
                {
                    using (new GUILayout.VerticalScope(box, GUILayout.ExpandWidth(true)))
                    {
                        foreach (var mod in player.statboard.modList.mods)
                        {
                            if (doesMatchSearch(mod.displayName) && GUILayout.Button($"Give {mod.displayName}")) player.statboard.AddModRpc(mod.identifier);
                        }
                    }

                    using (new GUILayout.VerticalScope(box, GUILayout.ExpandWidth(true)))
                    {
                        if (player.statboard.mods.Count > 0)
                        {
                            foreach (var mod in player.statboard.mods)
                            {
                                if (doesMatchSearch(mod.displayName) && GUILayout.Button($"Remove {mod.displayName}"))
                                {
                                    player.statboard.RemoveModRpc(mod.identifier);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            GUILayout.Button($"You have no Mods");
                        }
                    }
                }
            }

            bool doesMatchSearch(string text) => string.IsNullOrWhiteSpace(search) || new Regex(search, RegexOptions.IgnoreCase).IsMatch(text);
        }
    }
}