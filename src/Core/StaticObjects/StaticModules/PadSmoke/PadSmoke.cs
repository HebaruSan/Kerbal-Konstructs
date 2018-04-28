﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalKonstructs.Core;

namespace KerbalKonstructs
{
    public class PadSmoke : StaticModule
    {

        public string smokeReceiverName = "";
        public string smokeEmittersNames = "";


        private List<string> emitterTransforms = new List<string>();

        private string[] seperators = new string[] { " ", ",", ";" };


        public void Start()
        {

            emitterTransforms = smokeEmittersNames.Split(seperators, StringSplitOptions.RemoveEmptyEntries).ToList();

            Transform receiverTransform = gameObject.transform.FindRecursive(smokeReceiverName);
            Collider receiverCollider = receiverTransform.gameObject.GetComponent<Collider>();

            if (receiverCollider != null)
            {
                receiverCollider.tag = "LaunchpadFX";
                //receiverCollider.gameObject.layer = (1 << 30);

                KKPadFX padfx =  receiverCollider.gameObject.AddComponent<KKPadFX>();
                padfx.Setup(emitterTransforms);
            }
        }
    }

    public class KKPadFX : LaunchPadFX
    {

        internal static bool isInitialized = false;

        internal static ParticleSystem pSystem = null;
        internal static ParticleSystemRenderer pSystemRenderer = null;



        public void Setup(List<string> emitterTransformNames)
        {
            GetSquadPsystem();

            List<ParticleSystem> emitters = new List<ParticleSystem>();

            foreach (string emName in emitterTransformNames)
            {
                foreach (Transform emTransform in gameObject.transform.FindAllRecursive(emName))
                {
                    //ParticleSystem emPsystem  = emTransform.gameObject.AddComponent<ParticleSystem>();
                    //ParticleSystemRenderer emPsystemRenderer = emTransform.gameObject.AddComponent<ParticleSystemRenderer>();
                    ParticleSystem emPsystem = Instantiate(pSystem, emTransform.position, emTransform.rotation, emTransform);
                    ParticleSystemRenderer emPsystemRenderer = Instantiate(pSystemRenderer, emTransform.position, emTransform.rotation, emTransform);

                    emitters.Add(emPsystem);
                    FloatingOrigin.RegisterParticleSystem(emPsystem);
                }

            }

            ps = emitters.ToArray();
            
        }

        internal static void GetSquadPsystem()
        {
            if (!isInitialized)
            {
                pSystem = Resources.FindObjectsOfTypeAll<ParticleSystem>().Where(ps => ps.name == "PadSmokeLvl2").First();
                pSystemRenderer = Resources.FindObjectsOfTypeAll<ParticleSystemRenderer>().Where(ps => ps.name == "PadSmokeLvl2").First();

                if (pSystem == null || pSystemRenderer == null)
                {
                    Log.UserError("Failed to Setup Particle Systems");
                    return;
                }
                else
                {
                    isInitialized = true;
                }
            }

        }


    }


}
