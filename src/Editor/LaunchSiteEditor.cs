﻿using System;
using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Globalization;
using Upgradeables;
using UpgradeLevel = Upgradeables.UpgradeableObject.UpgradeLevel;

namespace KerbalKonstructs.UI
{
    class LaunchSiteEditor : KKWindow
    {

        private static LaunchSiteEditor _instance = null;

        internal static LaunchSiteEditor instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LaunchSiteEditor();
				}
				return _instance;
			}
		}

		#region Variable Declarations

        #region GUI Windows
        // GUI Windows
        Rect siteEditorRect = new Rect(400, 45, 360, 625);

        #endregion

        #region GUI elements
        // GUI elements
        Vector2 descScroll;
        GUIStyle listStyle = new GUIStyle();
        GUIStyle navStyle = new GUIStyle();

        GUIStyle DeadButton;
        GUIStyle DeadButtonRed;
        GUIStyle KKWindows;
        GUIStyle BoxNoBorder;

        SiteType siteType;
        // ComboBox siteTypeMenu;
        #endregion

        #region Holders
        // Holders

        public static StaticInstance selectedObject = null;
        public StaticInstance selectedObjectPrevious = null;

        internal static String facType = "None";
        internal static String sGroup = "Ungrouped";
        internal String increment = "0.1";
        internal String siteName, siteTrans, siteDesc, siteAuthor, siteCategory, siteHidden, ILSActive;
        float flOpenCost, flCloseValue, flRecoveryFactor, flRecoveryRange, flLaunchRefund, flLength, flWidth;

        internal StaticInstance snapTargetInstance = null;

        private bool guiInitialized = false;

        #endregion

        #endregion

        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                return;
            }
            if (KerbalKonstructs.instance.selectedObject == null)
            {
                this.Close();
            }

            if ((KerbalKonstructs.instance.selectedObject != null) && (!KerbalKonstructs.instance.selectedObject.preview))
            {
                drawEditor(KerbalKonstructs.instance.selectedObject);
            }
        }

        public override void Open()
        {
            selectedObject = KerbalKonstructs.instance.selectedObject;
            ReadSettings();
            base.Open();
        }



        /// <summary>
        /// Constructor
        /// </summary>
        public LaunchSiteEditor()
        {
            listStyle.normal.textColor = Color.white;
            listStyle.onHover.background =
            listStyle.hover.background = new Texture2D(2, 2);
            listStyle.padding.left =
            listStyle.padding.right =
            listStyle.padding.top =
            listStyle.padding.bottom = 4;

            navStyle.padding.left = 0;
            navStyle.padding.right = 0;
            navStyle.padding.top = 1;
            navStyle.padding.bottom = 3;

            // siteTypeMenu = new ComboBox(siteTypeOptions[0], siteTypeOptions, "button", "box", null, listStyle);
        }

        #region draw Methods

        /// <summary>
        /// Wrapper to draw editors
        /// </summary>
        /// <param name="obj"></param>
        public void drawEditor(StaticInstance obj)
        {
            if (!guiInitialized)
            {
                InitializeLayout();
                guiInitialized = true;
            }
            if (obj != null)
            {


                siteEditorRect = GUI.Window(0xB00B1E4, siteEditorRect, drawLaunchSiteEditorWindow, "", KKWindows);


            }
        }

        #endregion

        private void InitializeLayout()
        {
            KKWindows = new GUIStyle(GUI.skin.window);
            KKWindows.padding = new RectOffset(8, 8, 3, 3);

            BoxNoBorder = new GUIStyle(GUI.skin.box);
            BoxNoBorder.normal.background = null;
            BoxNoBorder.normal.textColor = Color.white;

            DeadButton = new GUIStyle(GUI.skin.button);
            DeadButton.normal.background = null;
            DeadButton.hover.background = null;
            DeadButton.active.background = null;
            DeadButton.focused.background = null;
            DeadButton.normal.textColor = Color.yellow;
            DeadButton.hover.textColor = Color.white;
            DeadButton.active.textColor = Color.yellow;
            DeadButton.focused.textColor = Color.yellow;
            DeadButton.fontSize = 14;
            DeadButton.fontStyle = FontStyle.Normal;

            DeadButtonRed = new GUIStyle(GUI.skin.button);
            DeadButtonRed.normal.background = null;
            DeadButtonRed.hover.background = null;
            DeadButtonRed.active.background = null;
            DeadButtonRed.focused.background = null;
            DeadButtonRed.normal.textColor = Color.red;
            DeadButtonRed.hover.textColor = Color.yellow;
            DeadButtonRed.active.textColor = Color.red;
            DeadButtonRed.focused.textColor = Color.red;
            DeadButtonRed.fontSize = 12;
            DeadButtonRed.fontStyle = FontStyle.Bold;
        }


        #region Editors


        #region Launchsite Editor
        // Launchsite Editor handlers
        string stOpenCost;
        string stCloseValue;
        string stRecoveryFactor;
        string stRecoveryRange;
        string stLaunchRefund;
        string stLength;
        string stWidth;

        /// <summary>
        /// Launchsite Editor
        /// </summary>
        /// <param name="id"></param>
        void drawLaunchSiteEditorWindow(int id)
        {
            BoxNoBorder = new GUIStyle(GUI.skin.box);
            BoxNoBorder.normal.background = null;
            BoxNoBorder.normal.textColor = Color.white;

            DeadButton = new GUIStyle(GUI.skin.button);
            DeadButton.normal.background = null;
            DeadButton.hover.background = null;
            DeadButton.active.background = null;
            DeadButton.focused.background = null;
            DeadButton.normal.textColor = Color.yellow;
            DeadButton.hover.textColor = Color.white;
            DeadButton.active.textColor = Color.yellow;
            DeadButton.focused.textColor = Color.yellow;
            DeadButton.fontSize = 14;
            DeadButton.fontStyle = FontStyle.Normal;

            DeadButtonRed = new GUIStyle(GUI.skin.button);
            DeadButtonRed.normal.background = null;
            DeadButtonRed.hover.background = null;
            DeadButtonRed.active.background = null;
            DeadButtonRed.focused.background = null;
            DeadButtonRed.normal.textColor = Color.red;
            DeadButtonRed.hover.textColor = Color.yellow;
            DeadButtonRed.active.textColor = Color.red;
            DeadButtonRed.focused.textColor = Color.red;
            DeadButtonRed.fontSize = 12;
            DeadButtonRed.fontStyle = FontStyle.Bold;


            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Launchsite Editor", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
                {
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(UIMain.tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.Box(selectedObject.model.title);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Site Name: ", GUILayout.Width(120));
            siteName = GUILayout.TextField(siteName, GUILayout.Height(19));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Transform: ", GUILayout.Width(120));
            GUILayout.Box("" + siteTrans);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Length: ", GUILayout.Width(120));
            stLength = GUILayout.TextField(stLength, GUILayout.Height(19));
            GUILayout.Label(" m");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Width: ", GUILayout.Width(120));
            stWidth = GUILayout.TextField(stWidth, GUILayout.Height(19));
            GUILayout.Label(" m");
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("Site Category: ", GUILayout.Width(115));
            GUILayout.Label(siteCategory, GUILayout.Width(85));
            GUILayout.FlexibleSpace();
            GUI.enabled = !(siteCategory == "RocketPad");
            if (GUILayout.Button("RP", GUILayout.Width(25), GUILayout.Height(23)))
                siteCategory = "RocketPad";
            GUI.enabled = !(siteCategory == "Runway");
            if (GUILayout.Button("RW", GUILayout.Width(25), GUILayout.Height(23)))
                siteCategory = "Runway";
            GUI.enabled = !(siteCategory == "Helipad");
            if (GUILayout.Button("HP", GUILayout.Width(25), GUILayout.Height(23)))
                siteCategory = "Helipad";
            GUI.enabled = !(siteCategory == "Waterlaunch");
            if (GUILayout.Button("WA", GUILayout.Width(25), GUILayout.Height(23)))
                siteCategory = "Waterlaunch";
            GUI.enabled = !(siteCategory == "Other");
            if (GUILayout.Button("OT", GUILayout.Width(25), GUILayout.Height(23)))
                siteCategory = "Other";
            GUILayout.EndHorizontal();

            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Site Type: ", GUILayout.Width(120));
            if (siteType == SiteType.VAB)
                GUILayout.Label("VAB", GUILayout.Width(40));
            if (siteType == SiteType.SPH)
                GUILayout.Label("SPH", GUILayout.Width(40));
            if (siteType == SiteType.Any)
                GUILayout.Label("Any", GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            GUI.enabled = !(siteType == (SiteType)0);
            if (GUILayout.Button("VAB", GUILayout.Height(23)))
                siteType = SiteType.VAB;
            GUI.enabled = !(siteType == (SiteType)1);
            if (GUILayout.Button("SPH", GUILayout.Height(23)))
                siteType = SiteType.SPH;
            GUI.enabled = !(siteType == (SiteType)2);
            if (GUILayout.Button("Any", GUILayout.Height(23)))
                siteType = SiteType.Any;
            GUILayout.EndHorizontal();

            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Author: ", GUILayout.Width(120));
            siteAuthor = GUILayout.TextField(siteAuthor, GUILayout.Height(19));
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("Open Cost: ", GUILayout.Width(120));
            stOpenCost = GUILayout.TextField(stOpenCost, GUILayout.Height(19));
            GUILayout.Label(" \\F");
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("Close Value: ", GUILayout.Width(120));
            stCloseValue = GUILayout.TextField(stCloseValue, GUILayout.Height(19));
            GUILayout.Label(" \\F");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Recovery Factor: ", GUILayout.Width(120));
            stRecoveryFactor = GUILayout.TextField(stRecoveryFactor, GUILayout.Height(19));
            GUILayout.Label(" %");
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("Effective Range: ", GUILayout.Width(120));
            stRecoveryRange = GUILayout.TextField(stRecoveryRange, GUILayout.Height(19));
            GUILayout.Label(" m");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Launch Refund: ", GUILayout.Width(120));
            stLaunchRefund = GUILayout.TextField(stLaunchRefund, GUILayout.Height(19));
            GUILayout.Label(" %");
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("Site is hidden: ", GUILayout.Width(115));
            GUILayout.Label(siteHidden, GUILayout.Width(85));
            GUILayout.FlexibleSpace();
            GUI.enabled = !(siteHidden == "false");
            if (GUILayout.Button("No", GUILayout.Width(40), GUILayout.Height(23)))
                siteHidden = "false";
            GUI.enabled = !(siteHidden == "true");
            if (GUILayout.Button("Yes", GUILayout.Width(40), GUILayout.Height(23)))
                siteHidden = "true";
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            //if (ILSConfig.DetectNavUtils ()) {
                // NavUtilities config generator
                GUILayout.BeginHorizontal ();
                GUILayout.Label ("ILS/HSI on (NavUtilities)", GUILayout.Width (115));
                GUILayout.Label (ILSActive, GUILayout.Width (85));
                GUILayout.FlexibleSpace ();
                GUI.enabled = !(ILSActive == "false");
                if (GUILayout.Button ("No", GUILayout.Width (40), GUILayout.Height (23)))
                    ILSActive = "false";
                GUI.enabled = !(ILSActive == "true");
                if (GUILayout.Button ("Yes", GUILayout.Width (40), GUILayout.Height (23)))
                    ILSActive = "true";
                GUILayout.EndHorizontal ();
            //}
            GUI.enabled = true;
            GUILayout.Label("Description: ");
            descScroll = GUILayout.BeginScrollView(descScroll);
            siteDesc = GUILayout.TextArea(siteDesc, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();

            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            GUI.enabled = siteName.Length > 0;
            if (GUILayout.Button("Save", GUILayout.Width(115), GUILayout.Height(23)))
            {
                bool addToDB = false;
                if (!selectedObject.hasLauchSites)
                {
                    Log.Normal("Creating LaunchSite");
                    LaunchSite lsite = new LaunchSite();
                    selectedObject.launchSite = lsite;
                    Log.Normal ("created; lsite = " + lsite + "; launch site = " + selectedObject.launchSite);
                    selectedObject.hasLauchSites = true;
                    lsite.parentInstance = selectedObject;
                    selectedObject.launchSite.body = selectedObject.CelestialBody;
                    addToDB = true;
                }

                string oldName = selectedObject.launchSite.LaunchSiteName;
                string oldCategory = selectedObject.launchSite.Category;
                bool oldState = selectedObject.launchSite.ILSIsActive;

                selectedObject.launchSite.LaunchSiteName = siteName;
                selectedObject.launchSite.LaunchSiteLength = float.Parse(stLength);
                selectedObject.launchSite.LaunchSiteWidth = float.Parse(stWidth);
                selectedObject.launchSite.LaunchSiteType = siteType;
                selectedObject.launchSite.LaunchPadTransform = siteTrans;
                selectedObject.launchSite.LaunchSiteDescription = siteDesc;
                selectedObject.launchSite.OpenCost = float.Parse(stOpenCost);
                selectedObject.launchSite.CloseValue = float.Parse(stCloseValue);
                selectedObject.launchSite.RecoveryFactor = float.Parse(stRecoveryFactor);
                selectedObject.launchSite.RecoveryRange = float.Parse(stRecoveryRange);
                selectedObject.launchSite.LaunchRefund = float.Parse(stLaunchRefund);
                selectedObject.launchSite.OpenCloseState = "Open";
                selectedObject.launchSite.Category = siteCategory;
                selectedObject.launchSite.LaunchSiteIsHidden = bool.Parse(siteHidden);
                selectedObject.launchSite.ILSIsActive = bool.Parse(ILSActive);
                selectedObject.launchSite.LaunchSiteAuthor = siteAuthor;
                selectedObject.launchSite.refLat = (float)selectedObject.RefLatitude;
                selectedObject.launchSite.refLon = (float)selectedObject.RefLongitude;
                selectedObject.launchSite.refAlt = selectedObject.RadiusOffset;

                if (ILSConfig.DetectNavUtils ()) {
                    Log.Normal ("NavUtils detected");
                    Log.Debug ("object: " + selectedObject);
                    Log.Debug ("launchsite: " + selectedObject.launchSite);
                    Log.Debug("body: " + selectedObject.launchSite.body);

					bool regenerateILSConfig = false;
                    Log.Debug ("old name: " + oldName);
                    Log.Debug("new name: " + selectedObject.launchSite.LaunchSiteName);
					if (oldName != null && !oldName.Equals (siteName)) {
						ILSConfig.RenameSite (selectedObject.launchSite.LaunchSiteName, siteName);
						regenerateILSConfig = true;
					}

                    Log.Debug ("old category: " + oldCategory);
                    if (oldCategory != null && !oldCategory.Equals (siteCategory)) {
                        ILSConfig.HandleCategoryChange (selectedObject.launchSite.Category,
                            siteCategory, selectedObject);
                        regenerateILSConfig = true;
                    }

                    bool state = bool.Parse (ILSActive);
                    Log.Normal ("new state: " + state + "; old state: " + oldState);
                    if (oldState != state || regenerateILSConfig) {
                        if (state)
                            ILSConfig.GenerateFullILSConfig (selectedObject);
                        else
                            ILSConfig.DropILSConfig (selectedObject.launchSite.name, true);
                    }
                }


                if (addToDB)
                {
                    KerbalKonstructs.instance.SaveInstanceByCfg(selectedObject.configPath);
                    LaunchSiteManager.RegisterLaunchSite(selectedObject.launchSite);
                }
                KerbalKonstructs.instance.SaveInstanceByCfg(selectedObject.configPath);
                this.Close();
            }
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", GUILayout.Width(115), GUILayout.Height(23)))
            {
                this.Close();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("NOTE: If a newly created launchsite object does not display when launched from, a restart of KSP will be required for the site to be correctly rendered.");

            GUILayout.Space(1);
            GUILayout.Box(UIMain.tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
        #endregion

        #endregion

        #region Utility Functions


        /// <summary>
        /// Updates the Window Strings to the new settings
        /// </summary>
        /// <param name="instance"></param>
        public static void updateSelection(StaticInstance instance)
        {
            selectedObject = instance;

            facType = instance.FacilityType;

            if (facType == null || facType == "")
            {
                string DefaultFacType = instance.model.DefaultFacilityType;

                if (DefaultFacType == null || DefaultFacType == "None" || DefaultFacType == "")
                    facType = "None";
                else
                    facType = DefaultFacType;
            }

            sGroup = instance.Group;
            selectedObject.update();
        }

        internal void ReadSettings()
        {
            if (selectedObject.hasLauchSites)
            {
                //LaunchSite myLaunchSite = new LaunchSite();

                string sLaunchsiteDesc = selectedObject.launchSite.LaunchSiteDescription;
                string sModelDesc = selectedObject.model.description;

                // Edit or make a launchsite
                siteName = selectedObject.launchSite.LaunchSiteName;
                siteTrans = selectedObject.launchSite.LaunchPadTransform;

                if (sLaunchsiteDesc != "")
                    siteDesc = sLaunchsiteDesc;
                else
                    siteDesc = sModelDesc;


                siteCategory = selectedObject.launchSite.Category;
                siteHidden = selectedObject.launchSite.LaunchSiteIsHidden.ToString();
                ILSActive = selectedObject.launchSite.ILSIsActive.ToString();
                siteType = selectedObject.launchSite.LaunchSiteType;
                flOpenCost = selectedObject.launchSite.OpenCost;
                flCloseValue = selectedObject.launchSite.CloseValue;
                stOpenCost = string.Format("{0}", flOpenCost);
                stCloseValue = string.Format("{0}", flCloseValue);



                flRecoveryFactor = selectedObject.launchSite.RecoveryFactor;
                flRecoveryRange = selectedObject.launchSite.RecoveryRange;
                flLaunchRefund = selectedObject.launchSite.LaunchRefund;

                flLength = selectedObject.launchSite.LaunchSiteLength;

                if (flLength < 1)
                    flLength = selectedObject.model.DefaultLaunchSiteLength;

                flWidth = selectedObject.launchSite.LaunchSiteWidth;

                if (flWidth < 1)
                    flWidth = selectedObject.model.DefaultLaunchSiteWidth;

            }
            else
            {
                // Edit or make a launchsite
                siteName = selectedObject.gameObject.name;
                siteTrans = selectedObject.model.DefaultLaunchPadTransform;
                siteDesc = selectedObject.model.description;

                siteCategory = "Other";
                siteHidden = "false";
                ILSActive = "false";
                siteType = SiteType.Any;
                flOpenCost = 0f;
                flCloseValue = 0f;
                stOpenCost = string.Format("{0}", flOpenCost);
                stCloseValue = string.Format("{0}", flCloseValue);

                flRecoveryFactor = 0f;
                flRecoveryRange = 0f;
                flLaunchRefund = 0f;

                flLength = selectedObject.model.DefaultLaunchSiteLength;
                flWidth = selectedObject.model.DefaultLaunchSiteWidth;
            }

            stRecoveryFactor = string.Format("{0}", flRecoveryFactor);
            stRecoveryRange = string.Format("{0}", flRecoveryRange);
            stLaunchRefund = string.Format("{0}", flLaunchRefund);

            stLength = string.Format("{0}", flLength);
            stWidth = string.Format("{0}", flWidth);
            siteAuthor = selectedObject.model.author;
            // Debug.Log("KK: Making or editing a launchsite");
        }



        #endregion
    }
}
