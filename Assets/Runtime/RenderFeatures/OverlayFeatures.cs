using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace Circles3D.Runtime.RenderFeatures
{
    public class OverlayFeatures : ScriptableRendererFeature
    {
        public OverlayPass hurtPass;
        public OverlayPass blockPass;
        
        public override void Create()
        {
            if (hurtPass == null) hurtPass = new OverlayPass();
            if (blockPass == null) blockPass = new OverlayPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            hurtPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            renderer.EnqueuePass(hurtPass);
            blockPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            renderer.EnqueuePass(blockPass);
        }

        [System.Serializable]
        public class OverlayPass : ScriptableRenderPass
        {
            [Range(0f, 1f)]
            public float debugAmount;
            public Color color = Color.red;

            private static Material material;
            private static Mesh mesh;
            private static Shader shader;
            
            public float weight { get; set; }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                if (!shader)
                {
                    shader = Shader.Find("Hidden/HurtOverlay");
                }

                if (!material)
                {
                    material = new Material(shader);
                    material.hideFlags = HideFlags.HideAndDontSave;
                }

                if (!mesh)
                {
                    mesh = new Mesh();
                    mesh.hideFlags = HideFlags.HideAndDontSave;

                    mesh.SetVertices(new[]
                    {
                        new Vector3(-1f, -1f, 0f),
                        new Vector3(3f, -1f, 0f),
                        new Vector3(-1f, 3f, 0f),
                    });

                    mesh.SetUVs(0, new[]
                    {
                        new Vector2(0f, 1f),
                        new Vector2(2f, 1f),
                        new Vector2(0f, -1f),
                    });

                    mesh.SetIndices(new[] { 0, 1, 2 }, MeshTopology.Triangles, 0);
                }
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var weight = 0f;
                if (renderingData.cameraData.isSceneViewCamera) weight = debugAmount;
                else if (Application.isPlaying) weight = this.weight;

                var cmd = CommandBufferPool.Get("HurtPass");
                cmd.Clear();
                
                cmd.SetGlobalFloat("_HurtWeight", weight);
                cmd.SetGlobalColor("_HurtColor", color);
                cmd.DrawMesh(mesh, Matrix4x4.identity, material, 0, 0);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}