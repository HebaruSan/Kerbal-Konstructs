﻿using KerbalKonstructs.Core;
using System.Reflection;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    public class CustomSpaceCenter
    {
        public string SpaceCenterName;

        private SpaceCenter _spaceCenter;
        internal StaticInstance staticInstance;
        internal GameObject gameObject;

        public static void CreateFromLaunchsite(LaunchSite site)
        {
            StaticInstance parentinstance = site.parentInstance;
            if (parentinstance != null)
            {
                var csc = new CustomSpaceCenter();
                csc.SpaceCenterName = site.LaunchSiteName;
                csc.staticInstance = parentinstance;
                csc.gameObject = site.parentInstance.gameObject;
                SpaceCenterManager.addSpaceCenter(csc);
            }
            else
            {
                Log.Normal("CreateFromLaunchsite failed because staticObject is null.");
            }
        }

        public SpaceCenter spaceCenter
        {
            get
            {
                return getSpaceCenter();
            }
        }


        public SpaceCenter getSpaceCenter()
        {
            if (_spaceCenter == null)
            {
                _spaceCenter = staticInstance.gameObject.AddComponent<SpaceCenter>();
                _spaceCenter.cb = staticInstance.CelestialBody;
                _spaceCenter.name = SpaceCenterName;
                _spaceCenter.AreaRadius = 3000;
                //_spaceCenter.spaceCenterAreaTrigger = new Collider();

                _spaceCenter.SpaceCenterTransform = staticInstance.gameObject.transform;

                Log.Normal("SpaceCenter Position: " + _spaceCenter.SpaceCenterTransform);


                FieldInfo Latitude = _spaceCenter.GetType().GetField("latitude", BindingFlags.NonPublic | BindingFlags.Instance);
                Latitude.SetValue(_spaceCenter, staticInstance.RefLatitude);
                FieldInfo Longitude = _spaceCenter.GetType().GetField("longitude", BindingFlags.NonPublic | BindingFlags.Instance);
                Longitude.SetValue(_spaceCenter, staticInstance.RefLongitude);


                FieldInfo SrfNVector = _spaceCenter.GetType().GetField("srfNVector", BindingFlags.NonPublic | BindingFlags.Instance);
                SrfNVector.SetValue(_spaceCenter, _spaceCenter.cb.GetRelSurfaceNVector(_spaceCenter.Latitude, _spaceCenter.Longitude));

                FieldInfo altitudeField = _spaceCenter.GetType().GetField("altitude", BindingFlags.NonPublic | BindingFlags.Instance);
                altitudeField.SetValue(_spaceCenter, staticInstance.RadiusOffset);
            }
            else
            {
                // Debug.Log("KK: getSpaceCenter was not null.");
            }

            return _spaceCenter;
        }

    }
}
