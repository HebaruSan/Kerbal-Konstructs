﻿using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;
using System.Reflection;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Modules;

namespace KerbalKonstructs.Core
{
	public class StaticInstance
	{

        // Position
        [CFGSetting] public CelestialBody CelestialBody = null;
        [CFGSetting] public Vector3d RadialPosition = Vector3.zero;
        [CFGSetting] public Vector3 Orientation;
        [CFGSetting] public double RadiusOffset;
        [CFGSetting] public float RotationAngle;
        [CFGSetting] public bool isScanable = false;
        [CFGSetting] public double ModelScale = 1f;

        // Legacy Faclility Setting
        [CFGSetting] public string FacilityType = "None";

        // Calculated References
        [CFGSetting] public double RefLatitude = 361f;
        [CFGSetting] public double RefLongitude = 361f;

        // Visibility and Grouping
        [CFGSetting] public double VisibilityRange = 25000f;
        [CFGSetting] public string Group  = "Ungrouped";
        [CFGSetting] public string GroupCenter = "false";
        [CFGSetting] public bool useRadiusOffset = true;

        public GameObject gameObject;
        public PQSCity pqsCity;
        //public PQSCity2 pqsCity2;
        internal StaticModel model;

        public UrlDir.UrlConfig configUrl;
        public String configPath;

        public bool hasFacilities = false;
        public bool hasLauchSites = false;
        public LaunchSite launchSite;

        public KKFacilityType facilityType = KKFacilityType.None;
        public List<KKFacility> myFacilities = new List<KKFacility>();


        // used for non KKFacility objects like AirRace
        public string legacyfacilityID;

		internal bool editing;
        internal bool preview;

        private Vector3d origScale;
        internal bool isActive;

        internal int indexInGroup = 0;

		private List<Renderer> _rendererComponents;


        /// <summary>
        /// Updates the static instance with new settings
        /// </summary>
		public void update()
		{
			if (pqsCity != null)
			{
                pqsCity.repositionRadial = RadialPosition;
                pqsCity.repositionRadiusOffset = RadiusOffset;
                pqsCity.reorientInitialUp = Orientation;
                pqsCity.reorientFinalAngle = RotationAngle;
                pqsCity.transform.localScale = origScale * ModelScale;
                pqsCity.Orientate();
            }
			// Notify modules about update
			foreach (StaticModule module in gameObject.GetComponents<StaticModule>())
			    module.StaticObjectUpdate();
		}

        internal void HighlightObject(Color highlightColor)
		{
			Renderer[] rendererList = gameObject.GetComponentsInChildren<Renderer>();
			_rendererComponents = new List<Renderer>(rendererList);

			foreach (Renderer renderer in _rendererComponents)
			{
				renderer.material.SetFloat("_RimFalloff", 1.8f);
				renderer.material.SetColor("_RimColor", highlightColor);
			}
		}

        internal void ToggleAllColliders(bool enable)
		{
			Transform[] gameObjectList = gameObject.GetComponentsInChildren<Transform>();

			List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.GetComponent<Collider>() != null select t.gameObject).ToList();

			foreach (GameObject gocollider in colliderList)
			{
				gocollider.GetComponent<Collider>().enabled = enable;
			}
		}

        internal double GetDistanceToObject(Vector3d vPosition)
		{
			double fDistance = 0;
			fDistance = Vector3d.Distance(gameObject.transform.position, vPosition);
			return fDistance;
		}


        /// <summary>
        /// Spawns a new Instance in the Gameworld and registers itself to the Static Database
        /// </summary>
        /// <param name="editing"></param>
        /// <param name="bPreview"></param>
        internal void spawnObject(Boolean editing, Boolean bPreview)
		{
            // mangle Squads statics
            if (model.isSquad)
            {
                InstanceUtil.MangleSquadStatic(gameObject);
            }

            // Objects spawned at runtime should be active, ones spawned at loading not
            InstanceUtil.SetActiveRecursively(this,editing);

			Transform[] gameObjectList = gameObject.GetComponentsInChildren<Transform>();
			List<GameObject> rendererList = (from t in gameObjectList where t.gameObject.GetComponent<Renderer>() != null select t.gameObject).ToList();

			setLayerRecursively(gameObject, 15);

			if (bPreview)
				this.ToggleAllColliders(false);

			this.preview = bPreview;

			if (editing)
				KerbalKonstructs.instance.selectObject(this, true, true, bPreview);

			double objvisibleRange = VisibilityRange;

			if (objvisibleRange < 1) objvisibleRange = 25000f;

            PQSCity.LODRange range = new PQSCity.LODRange
            {
                renderers = new GameObject[0],
                objects = new GameObject[0],
                visibleRange = (float) objvisibleRange
            };

            pqsCity = gameObject.AddComponent<PQSCity>();
            pqsCity.lod = new[] { range };
            pqsCity.frameDelta = 10000; //update interval for its own visiblility range checking. unused by KK, so set this to a high value
            pqsCity.repositionRadial = RadialPosition; //position
            pqsCity.repositionRadiusOffset = RadiusOffset; //height
            pqsCity.reorientInitialUp = Orientation; //orientation
            pqsCity.reorientFinalAngle = RotationAngle; //rotation x axis
            pqsCity.reorientToSphere = true; //adjust rotations to match the direction of gravity
            gameObject.transform.parent = CelestialBody.pqsController.transform;
            pqsCity.sphere = CelestialBody.pqsController;
            origScale = pqsCity.transform.localScale;             // save the original scale for later use
            pqsCity.transform.localScale *= (float) ModelScale;
            pqsCity.order = 100;
            pqsCity.modEnabled = true;
            pqsCity.repositionToSphere = true; //enable repositioning
            pqsCity.repositionToSphereSurface = false; //Snap to surface?

            CelestialBody.pqsController.GetSurfaceHeight(RadialPosition);

            pqsCity.OnSetup();
            pqsCity.Orientate();

            //PQSCity2.LodObject lodObject = new PQSCity2.LodObject();
            //lodObject.visibleRange = VisibilityRange;
            //lodObject.objects = new GameObject[] { };
            //pqsCity2 = gameObject.AddComponent<PQSCity2>();
            //pqsCity2.objects = new [] { lodObject } ;
            //pqsCity2.objectName = "";
            //pqsCity2.lat = RefLatitude;
            //pqsCity2.lon = RefLongitude;
            //pqsCity2.alt = RadiusOffset;
            //pqsCity2.up = Orientation;
            //pqsCity2.rotation = RotationAngle;
            //pqsCity2.sphere = CelestialBody.pqsController;


            //pqsCity2.OnSetup();
            //pqsCity2.Orientate();


            foreach (StaticModule module in model.modules)
			{
				Type moduleType = AssemblyLoader.loadedAssemblies.SelectMany(asm => asm.assembly.GetTypes()).FirstOrDefault(t => t.Namespace == module.moduleNamespace && t.Name == module.moduleClassname);
				MonoBehaviour mod = gameObject.AddComponent(moduleType) as MonoBehaviour;

				if (mod != null)
				{
					foreach (string fieldName in module.moduleFields.Keys)
					{
						FieldInfo field = mod.GetType().GetField(fieldName);
						if (field != null)
						{
							field.SetValue(mod, Convert.ChangeType(module.moduleFields[fieldName], field.FieldType));
						}
						else
						{
							Log.UserWarning("Field " + fieldName + " does not exist in " + module.moduleClassname);
						}
					}
				}
				else
				{
                    Log.UserError("Module " + module.moduleClassname + " could not be loaded in " + gameObject.name);
				}
			}

			foreach (GameObject gorenderer in rendererList)
			{
				gorenderer.GetComponent<Renderer>().enabled = true;
			}

            StaticDatabase.AddStatic(this);

            // Add them to the bodys objectlist, so they show up as anomalies
            // After we got a new Name from StaticDatabase.AddStatic()
            if (isScanable)
            {
                Log.Normal("Added " + gameObject.name + " to scanable Objects");
                var pqsObjectList = CelestialBody.pqsSurfaceObjects.ToList();
                pqsObjectList.Add(pqsCity as PQSSurfaceObject);
                CelestialBody.pqsSurfaceObjects = pqsObjectList.ToArray();
            }
        }


        /// <summary>
        /// Sets tje Layer of the Colliders
        /// </summary>
        /// <param name="sGameObject"></param>
        /// <param name="newLayerNumber"></param>
        internal void setLayerRecursively(GameObject sGameObject, int newLayerNumber)
		{

            var transforms = gameObject.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].gameObject.GetComponent<Collider>() == null)
					transforms[i].gameObject.layer = newLayerNumber;
                else if (!transforms[i].gameObject.GetComponent<Collider>().isTrigger) transforms[i].gameObject.layer = newLayerNumber;
            }
		}

        /// <summary>
        /// resets the object highlightColor to 0 and resets the editing flag.
        /// </summary>
        /// <param name="enableColliders"></param>
        internal void deselectObject(Boolean enableColliders)
		{
			this.editing = false;
			if (enableColliders)
				this.ToggleAllColliders(true);

			Color highlightColor = new Color(0, 0, 0, 0);
			this.HighlightObject(highlightColor);
		}
	}
}
