﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;


namespace KerbalKonstructs
{
    public class AdvancedTextures : StaticModule
    {

        public string newShader = null;

        public string transforms = "Any";

        public string _MainTex = null;          // texture
        public string _BumpMap = null;          // normal map
        public string _ParallaxMap = null;      // height map
        public string _Emissive = null;         // legacy shader  U4 name for emissive map
        public string _EmissionMap = null;      // U5 std shader name for emissive map
        public string _MetallicGlossMap = null; // U5 metallic (standard shader)
        public string _OcclusionMap = null;     // ambient occlusion
        public string _SpecGlossMap = null;     // U5 metallic (standard shader - spec gloss setup)


        public void Start()
        {
            foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!transforms.Equals("Any", StringComparison.CurrentCultureIgnoreCase) && !transforms.Contains(renderer.transform.name))
                    continue;

                ReplaceShader(renderer,newShader);

                var myFields = this.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                foreach (var texturemap in myFields)
                {
                    if (texturemap.Name.Contains("_") && (texturemap.GetValue(this) != null))
                    {
                        Texture2D newTexture = GameDatabase.Instance.GetTexture(((string)texturemap.GetValue(this)), (texturemap.Name.Equals("_BumpMap", StringComparison.CurrentCultureIgnoreCase)));
                        renderer.material.SetTexture(texturemap.Name, newTexture);
                    }
                }
            }
        }


        private void ReplaceShader(MeshRenderer renderer, string newShaderName)
        {
            if (string.IsNullOrEmpty(newShaderName) || !KKShader.HasShader(newShaderName))
            {
                return;
            }

            Shader newShader = KKShader.GetShader(newShaderName);
            renderer.material.shader = newShader;
        }

    }
}
