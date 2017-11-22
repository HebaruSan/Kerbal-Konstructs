using System;
using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

namespace KerbalKonstructs.UI
{
    class EditorGUI : KKWindow
    {

        private static EditorGUI _instance = null;
        internal static EditorGUI instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EditorGUI();
                }
                return _instance;
            }
        }

        #region Variable Declarations
        private CelestialBody body;

        internal bool foldedIn = false;
        internal bool doneFold = false;

        #region Texture Definitions
        // Texture definitions
        internal Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);
        internal Texture tCopyPos = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/copypos", false);
        internal Texture tPastePos = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/pastepos", false);
        internal Texture tSnap = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/snapto", false);
        internal Texture tFoldOut = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldin", false);
        internal Texture tFoldIn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
        internal Texture tFolded = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
        internal Texture textureWorld = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/world", false);
        internal Texture textureCubes = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/cubes", false);

        #endregion

        #region Switches
        // Switches
        internal bool enableColliders = false;
        internal static bool isScanable = false;

        //public static bool editingLaunchSite = false;

        //   public static bool editingFacility = false;

        internal bool SnapRotateMode = false;

        #endregion

        #region GUI Windows
        // GUI Windows
        internal Rect toolRect = new Rect(300, 35, 330, 695);

        #endregion

        #region GUI elements
        // GUI elements
        internal GUIStyle listStyle = new GUIStyle();
        internal GUIStyle navStyle = new GUIStyle();

        internal GUIStyle DeadButton;
        internal GUIStyle DeadButtonRed;
        internal GUIStyle KKWindows;
        internal GUIStyle BoxNoBorder;

        internal GUIContent[] siteTypeOptions = {
                                            new GUIContent("VAB"),
                                            new GUIContent("SPH"),
                                            new GUIContent("ANY")
                                        };
        // ComboBox siteTypeMenu;
        #endregion

        #region Holders
        // Holders

        internal static StaticInstance selectedObject = null;
        internal StaticInstance selectedObjectPrevious = null;
        internal static LaunchSite lTargetSite = null;

        internal static String facType = "None";
        internal static String sGroup = "Ungrouped";
        private double increment = 0.1;

        internal Vector3 vbsnapangle1 = new Vector3(0, 0, 0);
        internal Vector3 vbsnapangle2 = new Vector3(0, 0, 0);

        internal Vector3 snapSourceWorldPos = new Vector3(0, 0, 0);
        internal Vector3 snapTargetWorldPos = new Vector3(0, 0, 0);

        internal String sSTROT = "";

        internal GameObject selectedSnapPoint = null;
        internal GameObject selectedSnapPoint2 = null;
        internal StaticInstance snapTargetInstance = null;
        internal StaticInstance snapTargetInstancePrevious = null;

        private Vector3 snpspos = new Vector3(0, 0, 0);
        private Vector3 snptpos = new Vector3(0, 0, 0);
        private Vector3 vDrift = new Vector3(0, 0, 0);
        private Vector3 vCurrpos = new Vector3(0, 0, 0);

        private VectorRenderer upVR = new VectorRenderer();
        private VectorRenderer fwdVR = new VectorRenderer();
        private VectorRenderer rightVR = new VectorRenderer();

        private VectorRenderer northVR = new VectorRenderer();
        private VectorRenderer eastVR = new VectorRenderer();

        private Vector3d savedposition;
        private double savedalt;
        private double savedrot;
        private bool savedpos = false;

        private static Space referenceSystem = Space.Self;

        private static Vector3d position = Vector3d.zero;
        private Vector3d referenceVector = Vector3d.zero;
        private Vector3 orientation = Vector3.zero;

        private static double altitude;
        private static double latitude, longitude;

        private double rotation = 0d;

        private static double vis = 0;

        private static double modelScale = 1f;

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
                CloseEditors();
                CloseVectors();
            }

            if ((KerbalKonstructs.instance.selectedObject != null) && (!KerbalKonstructs.instance.selectedObject.preview))
            {
                drawEditor(KerbalKonstructs.instance.selectedObject);

                DrawObject.DrawObjects(KerbalKonstructs.instance.selectedObject.gameObject);
            }
        }


        public override void Close()
        {
            CloseVectors();
            CloseEditors();
            base.Close();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public EditorGUI()
        {
            listStyle.normal.textColor = Color.white;
            listStyle.onHover.background = listStyle.hover.background = new Texture2D(2, 2);
            listStyle.padding.left = listStyle.padding.right =
                listStyle.padding.top = listStyle.padding.bottom = 4;

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
                if (selectedObject != obj)
                {
                    updateSelection(obj);
                    position = selectedObject.gameObject.transform.position;
                    Planetarium.fetch.CurrentMainBody.GetLatLonAlt(position, out latitude, out longitude, out altitude);
                    SetupVectors();
                }


                if (foldedIn)
                {
                    if (!doneFold)
                        toolRect = new Rect(toolRect.xMin, toolRect.yMin, toolRect.width - 45, toolRect.height - 250);

                    doneFold = true;
                }
                else
                {
                    if (doneFold)
                        toolRect = new Rect(toolRect.xMin, toolRect.yMin, toolRect.width + 45, toolRect.height + 250);

                    doneFold = false;
                }

                toolRect = GUI.Window(0xB00B1E3, toolRect, InstanceEditorWindow, "", KKWindows);

                //if (editingLaunchSite)
                //{
                //    siteEditorRect = GUI.Window(0xB00B1E4, siteEditorRect, drawLaunchSiteEditorWindow, "", KKWindows);
                //}
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

        #region Instance Editor

        /// <summary>
        /// Instance Editor window
        /// </summary>
        /// <param name="windowID"></param>
        void InstanceEditorWindow(int windowID)
        {
            //initialize values
            rotation = selectedObject.RotationAngle;
            referenceVector = (Vector3d)selectedObject.RadialPosition;
            orientation = selectedObject.Orientation;
            modelScale = selectedObject.ModelScale;
            //isScanable = bool.Parse((string)selectedObject.getSetting("isScanable"));

            // make this new when converted to PQSCity2
            // fill the variables here for later use
            if (position == Vector3d.zero)
            {
                position = selectedObject.gameObject.transform.position;
                Planetarium.fetch.CurrentMainBody.GetLatLonAlt(position, out latitude, out longitude, out altitude);
            }

            UpdateVectors();

            string smessage = "";


            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Instance Editor", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
                {
                    //KerbalKonstructs.instance.saveObjects();
                    KerbalKonstructs.instance.deselectObject(true, true);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();

            tFolded = foldedIn ? tFoldOut : tFoldIn;

            if (GUILayout.Button(tFolded, GUILayout.Height(23), GUILayout.Width(23)))
            {
                foldedIn = !foldedIn;
            }

            GUILayout.Button(selectedObject.model.title + " ("+ selectedObject.indexInGroup.ToString() + ")", GUILayout.Height(23));

            GUILayout.EndHorizontal();

            GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Position");
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent(tCopyPos, "Copy Position"), GUILayout.Width(23), GUILayout.Height(23)))
                {
                    savedpos = true;
                    savedposition = position;

                    savedalt = altitude;
                    savedrot = rotation;
                    // Debug.Log("KK: Instance position copied");
                }
                if (GUILayout.Button(new GUIContent(tPastePos, "Paste Position"), GUILayout.Width(23), GUILayout.Height(23)))
                {
                    if (savedpos)
                    {
                        position = savedposition;
                        altitude = savedalt;
                        rotation = savedrot;
                        saveSettings();
                        // Debug.Log("KK: Instance position pasted");
                    }
                }

                if (!foldedIn)
                {
                    if (GUILayout.Button(new GUIContent(tSnap, "Snap to Target"), GUILayout.Width(23), GUILayout.Height(23)))
                    {
                        if (snapTargetInstance != null)
                        {
                            Vector3 snapTargetPos = (Vector3)snapTargetInstance.RadialPosition;
                            double snapTargetAlt = snapTargetInstance.RadiusOffset;
                            selectedObject.RadialPosition = snapTargetPos;
                            selectedObject.RadiusOffset = snapTargetAlt;
                        }
                        updateSelection(selectedObject);
                    }
                }

                GUILayout.FlexibleSpace();
                if (!foldedIn)
                {
                    GUILayout.Label("Increment");
                    increment = double.Parse(GUILayout.TextField(increment.ToString(), 5, GUILayout.Width(48)));

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    var h = GUILayout.Height(18);
                    if (GUILayout.Button("0.001", h))
                    {
                        increment = 0.001;
                    }
                    if (GUILayout.Button("0.01", h))
                    {
                        increment = 0.01;
                    }
                    if (GUILayout.Button("0.1", h))
                    {
                        increment = 0.1;
                    }
                    if (GUILayout.Button("1", h))
                    {
                        increment = 1;
                    }
                    if (GUILayout.Button("10", h))
                    {
                        increment = 10;
                    }
                    if (GUILayout.Button("25", h))
                    {
                        increment = 25;
                    }
                    if (GUILayout.Button("100", h))
                    {
                        increment = 100;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                else
                {
                    GUILayout.Label("i");
                    increment = double.Parse(GUILayout.TextField(increment.ToString(), 3, GUILayout.Width(25)));
                    var h = GUILayout.Height(23);

                    if (GUILayout.Button("0.1", h))
                    {
                        increment = 0.1;
                    }
                    if (GUILayout.Button("1", h))
                    {
                        increment = 1;
                    }
                    if (GUILayout.Button("10", h))
                    {
                        increment = 10;
                    }
                    if (GUILayout.Button("100", h))
                    {
                        increment = 100;
                    }
                }
            }
            GUILayout.EndHorizontal();

            //
            // Set reference butons
            //
            GUILayout.BeginHorizontal();
            GUILayout.Label("Reference System: ");
            GUILayout.FlexibleSpace();
            GUI.enabled = (referenceSystem == Space.World);

            if (GUILayout.Button(new GUIContent(textureCubes, "Model"), GUILayout.Height(23), GUILayout.Width(23)))
            {
                referenceSystem = Space.Self;
                UpdateVectors();
            }

            GUI.enabled = (referenceSystem == Space.Self);
            if (GUILayout.Button(new GUIContent(textureWorld, "World"), GUILayout.Height(23), GUILayout.Width(23)))
            {
                referenceSystem = Space.World;
                UpdateVectors();
            }
            GUI.enabled = true;

            GUILayout.Label(referenceSystem.ToString());

            GUILayout.EndHorizontal();
            float fTempWidth = 80f;
            //
            // Position editing
            //
            GUILayout.BeginHorizontal();

            bool holdLeft, tickLeft;

            if (referenceSystem == Space.Self)
            {
                GUILayout.Label("Back / Forward:");
                GUILayout.FlexibleSpace();

                if (foldedIn) fTempWidth = 40f;

                holdLeft = GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21));
                tickLeft = GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21));
                if (holdLeft || tickLeft)
                {
                    SetTransform(Vector3d.back * modelScale * increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3d.forward * modelScale * increment);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Left / Right:");
                GUILayout.FlexibleSpace();
                holdLeft = GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21));
                tickLeft = GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21));
                if (holdLeft || tickLeft)
                {
                    SetTransform(Vector3d.left * modelScale * increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3d.right * modelScale * increment);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Down / Up:");
                GUILayout.FlexibleSpace();
                holdLeft = GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21));
                tickLeft = GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21));
                if (holdLeft || tickLeft)
                {
                    SetTransform(Vector3d.down * modelScale * increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3d.up * modelScale * increment);
                }

            }
            else
            {
                GUILayout.Label("West / East :");
                GUILayout.FlexibleSpace();

                if (foldedIn) fTempWidth = 40f;

                holdLeft = GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21));
                tickLeft = GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21));
                if (holdLeft || tickLeft)
                {
                    Setlatlng(0d, modelScale * -increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(0d, modelScale * increment);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("South / North:");
                GUILayout.FlexibleSpace();
                holdLeft = GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21));
                tickLeft = GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21));
                if (holdLeft || tickLeft)
                {
                    Setlatlng(modelScale * -increment, 0d);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(modelScale * increment, 0d);
                }

            }

            GUILayout.EndHorizontal();

            GUI.enabled = true;

            if (!foldedIn)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Box("Latitude");
                    GUILayout.Box(latitude.ToString("#0.0000000"));
                    GUILayout.Box("Longitude");
                    GUILayout.Box(longitude.ToString("#0.0000000"));
                }
                GUILayout.EndHorizontal();
            }

            GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

            //
            // Altitude editing
            //
            GUILayout.BeginHorizontal();
            GUILayout.Label("Alt.");
            GUILayout.FlexibleSpace();
            altitude = double.Parse(GUILayout.TextField(altitude.ToString(), 25, GUILayout.Width(fTempWidth)));
            holdLeft = GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21));
            tickLeft = GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21));
            if (holdLeft || tickLeft)
            {
                altitude -= modelScale * increment;
                saveSettings();
            }
            if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
            {
                altitude += modelScale * increment;
                saveSettings();
            }
            GUILayout.EndHorizontal();

            var pqsc = selectedObject.CelestialBody.pqsController;

            if (!foldedIn)
            {
                if (GUILayout.Button("Snap to Terrain", GUILayout.Height(21)))
                {
                    altitude = 1.0d + (pqsc.GetSurfaceHeight(selectedObject.RadialPosition) - pqsc.radius);
                    saveSettings();
                }
            }

            GUI.enabled = true;

            if (!foldedIn)
                GUILayout.Space(5);

            GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

            fTempWidth = 80f;

            GUI.enabled = true;

            if (!foldedIn)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Vis.");
                    GUILayout.FlexibleSpace();
                    vis = double.Parse(GUILayout.TextField(vis.ToString(), 15, GUILayout.Width(120)));
                    var w = GUILayout.Width(30);
                    var h = GUILayout.Height(23);

                    if (GUILayout.Button("Min", w, h))
                    {
                        vis = -1;
                        // saveSettings will clamp a negative value to the minimum
                        saveSettings();
                    }
                    if (GUILayout.Button("-", w, h))
                    {
                        vis -= modelScale * increment * 2500;
                        saveSettings();
                    }
                    if (GUILayout.Button("+", w, h))
                    {
                        vis += modelScale * increment * 2500;
                        saveSettings();
                    }
                    if (GUILayout.Button("Max", w, h))
                    {
                        try
                        {
                            vis = modelScale * KerbalKonstructs.instance.maxEditorVisRange;
                        }
                        catch
                        {
                            vis = Double.MaxValue;
                        }
                        saveSettings();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

            if (!foldedIn)
            {
                //
                // Orientation quick preset
                //
                GUILayout.Space(1);
                GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                GUILayout.Space(2);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Orientation");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("U", "Top Up"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        orientation = new Vector3(0, 1, 0); saveSettings();
                    }
                    if (GUILayout.Button(new GUIContent("D", "Bottom Up"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        orientation = new Vector3(0, -1, 0); saveSettings();
                    }
                    if (GUILayout.Button(new GUIContent("L", "On Left"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        orientation = new Vector3(1, 0, 0); saveSettings();
                    }
                    if (GUILayout.Button(new GUIContent("R", "On Right"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        orientation = new Vector3(-1, 0, 0); saveSettings();
                    }
                    if (GUILayout.Button(new GUIContent("F", "On Front"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        orientation = new Vector3(0, 0, 1); saveSettings();
                    }
                    if (GUILayout.Button(new GUIContent("B", "On Back"), GUILayout.Height(21), GUILayout.Width(18)))
                    {
                        orientation = new Vector3(0, 0, -1); saveSettings();
                    }
                }
                GUILayout.EndHorizontal();

                //
                // Orientation adjustment
                //
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Pitch:");
                    GUILayout.FlexibleSpace();

                    fTempWidth = foldedIn ? 40f : 80f;

                    holdLeft = GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21));
                    tickLeft = GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21));
                    if (holdLeft || tickLeft)
                    {
                        SetPitch(increment);
                    }
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                    {
                        SetPitch(-increment);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Roll:");
                    GUILayout.FlexibleSpace();
                    holdLeft = GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21));
                    tickLeft = GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21));
                    if (holdLeft || tickLeft)
                    {
                        SetRoll(increment);
                    }
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                    {
                        SetRoll(-increment);
                    }

                }
                GUILayout.EndHorizontal();


                //
                // Rotation
                //
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Heading:");
                    GUILayout.FlexibleSpace();
                    GUILayout.TextField(heading.ToString(), 9, GUILayout.Width(fTempWidth));
                    var w = GUILayout.Width(30);
                    var h = GUILayout.Height(23);

                    if (GUILayout.RepeatButton("<<", w, h))
                    {
                        SetRotation(-increment);
                    }
                    if (GUILayout.Button("<", w, h))
                    {
                        SetRotation(-increment);
                    }
                    if (GUILayout.Button(">", w, h))
                    {
                        SetRotation(increment);
                    }
                    if (GUILayout.RepeatButton(">>", w, h))
                    {
                        SetRotation(increment);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(1);
                GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                GUILayout.Space(2);

                //
                // Scale
                //
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Model Scale: ");
                    GUILayout.FlexibleSpace();
                    modelScale = double.Parse(GUILayout.TextField(modelScale.ToString(), 4, GUILayout.Width(fTempWidth)));
                    var w = GUILayout.Width(30);
                    var h = GUILayout.Height(23);

                    if (GUILayout.RepeatButton("<<", w, h))
                    {
                        modelScale -= increment;
                        vis        *= modelScale / (modelScale + increment);
                        saveSettings();
                    }
                    if (GUILayout.Button("<", w, h))
                    {
                        modelScale -= increment;
                        vis        *= modelScale / (modelScale + increment);
                        saveSettings();
                    }
                    if (GUILayout.Button(">", w, h))
                    {
                        modelScale += increment;
                        vis        *= modelScale / (modelScale - increment);
                        saveSettings();
                    }
                    if (GUILayout.RepeatButton(">>", w, h))
                    {
                        modelScale += increment;
                        vis        *= modelScale / (modelScale - increment);
                        saveSettings();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

            }

            GUI.enabled = true;

            if (!foldedIn)
            {
                if (GUILayout.Button("Facility Type: " + facType, GUILayout.Height(23)))
                {
                    if (!FacilityEditor.instance.IsOpen())
                        FacilityEditor.instance.Open();
                }
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Group: ", GUILayout.Height(23));
                GUILayout.FlexibleSpace();

                if (!foldedIn)
                    sGroup = GUILayout.TextField(sGroup, 30, GUILayout.Width(185), GUILayout.Height(23));
                else
                    sGroup = GUILayout.TextField(sGroup, 30, GUILayout.Width(135), GUILayout.Height(23));
            }
            GUILayout.EndHorizontal();

            GUI.enabled = !KerbalKonstructs.instance.bDisablePositionEditing;

            if (!foldedIn)
            {
                GUILayout.Space(3);

                GUILayout.BeginHorizontal();
                {
                    enableColliders = GUILayout.Toggle(enableColliders, "Enable Colliders", GUILayout.Width(140), GUILayout.Height(23));

                    Transform[] gameObjectList = selectedObject.gameObject.GetComponentsInChildren<Transform>();
                    List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.GetComponent<Collider>() != null select t.gameObject).ToList();

                    if (enableColliders)
                    {
                        foreach (GameObject collider in colliderList)
                        {
                            collider.GetComponent<Collider>().enabled = true;
                        }
                    }
                    else
                    {
                        foreach (GameObject collider in colliderList)
                        {
                            collider.GetComponent<Collider>().enabled = false;
                        }
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Duplicate", GUILayout.Width(130), GUILayout.Height(23)))
                    {
                        KerbalKonstructs.instance.saveObjects();
                        StaticModel oModel = selectedObject.model;
                        double fOffset = selectedObject.RadiusOffset;
                        Vector3 vPosition = selectedObject.RadialPosition;
                        float fAngle = selectedObject.RotationAngle;
                        smessage = "Spawned duplicate " + selectedObject.model.title;
                        KerbalKonstructs.instance.deselectObject(true, true);
                        spawnInstance(oModel, fOffset, vPosition, fAngle);
                        MiscUtils.HUDMessage(smessage, 10, 2);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                {
                    bool isScanable2 = GUILayout.Toggle(isScanable, "Static will show up on anomaly scanners", GUILayout.Width(250), GUILayout.Height(23));
                    if (isScanable2 != isScanable)
                        isScanable = isScanable2;
                    saveSettings();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(6);

            }

            if (foldedIn)
            {
                if (GUILayout.Button("Duplicate", GUILayout.Height(23)))
                {
                    KerbalKonstructs.instance.SaveInstanceByCfg(selectedObject.configPath);
                    StaticModel oModel = selectedObject.model;
                    double fOffset = selectedObject.RadiusOffset;
                    Vector3 vPosition = selectedObject.RadialPosition;
                    float fAngle = selectedObject.RotationAngle;
                    smessage = "Spawned duplicate " + selectedObject.model.title;
                    KerbalKonstructs.instance.deselectObject(true, true);
                    spawnInstance(oModel, fOffset, vPosition, fAngle);
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
            }

            GUI.enabled = true;

            GUI.enabled = !LaunchSiteEditor.instance.IsOpen();
            // Make a new LaunchSite here:
            if (!foldedIn)
            {

                if (!selectedObject.hasLauchSites && string.IsNullOrEmpty(selectedObject.model.DefaultLaunchPadTransform))
                    GUI.enabled = false;

                if (GUILayout.Button((selectedObject.hasLauchSites ? "Edit" : "Make") + " Launchsite", GUILayout.Height(23)))
                {

                    LaunchSiteEditor.instance.Open();
                }
            }

            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    KerbalKonstructs.instance.SaveInstanceByCfg(selectedObject.configPath);
                    smessage = "Saved changes to this object.";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Deselect", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    KerbalKonstructs.instance.SaveInstanceByCfg(selectedObject.configPath);
                    smessage = "Saved changes to this object.";
                    KerbalKonstructs.instance.deselectObject(true, true);
                }
            }
            GUILayout.EndHorizontal();

            if (!foldedIn)
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Delete Instance", GUILayout.Height(21)))
                {
                    DeleteInstance();
                }

                GUILayout.Space(15);
            }


            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            if (GUI.tooltip != "")
            {
                var labelSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(GUI.tooltip));
                GUI.Box(new Rect(Event.current.mousePosition.x - (25 + (labelSize.x / 2)), Event.current.mousePosition.y - 40, labelSize.x + 10, labelSize.y + 5), GUI.tooltip);
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }


        #endregion

        /// <summary>
        /// closes the sub editor windows
        /// </summary>
        public static void CloseEditors()
        {
            FacilityEditor.instance.Close();
            LaunchSiteEditor.instance.Close();
        }


        #endregion

        #region Utility Functions


        internal void DeleteInstance()
        {
            if (snapTargetInstance == selectedObject)
                snapTargetInstance = null;
            if (snapTargetInstancePrevious == selectedObject)
                snapTargetInstancePrevious = null;
            if (selectedObjectPrevious == selectedObject)
                selectedObjectPrevious = null;

            if (selectedObject.hasLauchSites)
            {
                LaunchSiteManager.DeleteLaunchSite(selectedObject.launchSite);
            }

            KerbalKonstructs.instance.deleteObject(selectedObject);
            selectedObject = null;
        }


        /// <summary>
        /// Spawns an Instance of an defined StaticModel
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fOffset"></param>
        /// <param name="vPosition"></param>
        /// <param name="fAngle"></param>
        /// <returns></returns>
        public void spawnInstance(StaticModel model, double fOffset, Vector3d vPosition, float fAngle)
        {
            StaticInstance instance = new StaticInstance();
            instance.gameObject = UnityEngine.Object.Instantiate(model.prefab);
            instance.RadiusOffset = fOffset;
            instance.CelestialBody = KerbalKonstructs.instance.getCurrentBody();
            string newGroup = selectedObject?.Group ?? "Ungrouped";
            instance.Group = newGroup;
            instance.RadialPosition = vPosition;
            rotation = instance.RotationAngle = fAngle;
            instance.Orientation = Vector3.up;
            instance.VisibilityRange = 25000f;

            instance.model = model;
            Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/" + KerbalKonstructs.newInstancePath);
            instance.configPath = KerbalKonstructs.newInstancePath + "/" + model.name + "-instances.cfg";
            instance.configUrl = null;

            enableColliders = false;
            instance.spawnObject(true, false);
        }

        public static void setTargetSite(LaunchSite lsTarget, string sName = "")
        {
            lTargetSite = lsTarget;
        }

        /// <summary>
        /// the starting position of direction vectors (a bit right and up from the Objects position)
        /// </summary>
        private Vector3 vectorDrawPosition
        {
            get
            {
                return selectedObject.pqsCity.transform.position
                    + 4 * selectedObject.pqsCity.transform.up
                    + 4 * selectedObject.pqsCity.transform.right;
            }
        }


        /// <summary>
        /// returns the heading the selected object
        /// </summary>
        /// <returns></returns>
        public float heading
        {
            get
            {
                Vector3 myForward = Vector3.ProjectOnPlane(selectedObject.gameObject.transform.forward, upVector);
                float myHeading;

                if (Vector3.Dot(myForward, eastVector) > 0)
                {
                    myHeading = Vector3.Angle(myForward, northVector);
                }
                else
                {
                    myHeading = 360 - Vector3.Angle(myForward, northVector);
                }
                return myHeading;
            }
        }

        /// <summary>
        /// gives a vector to the east
        /// </summary>
        private Vector3 eastVector
        {
            get
            {
                return Vector3.Cross(upVector, northVector).normalized;
            }
        }

        /// <summary>
        /// vector to north
        /// </summary>
        private Vector3 northVector
        {
            get
            {
                body = FlightGlobals.ActiveVessel.mainBody;
                return Vector3.ProjectOnPlane(body.transform.up, upVector).normalized;
            }
        }

        private Vector3 upVector
        {
            get
            {
                body = FlightGlobals.ActiveVessel.mainBody;
                return (Vector3)body.GetSurfaceNVector(latitude, longitude).normalized;
            }
        }

        /// <summary>
        /// Sets the vectors active and updates thier position and directions
        /// </summary>
        private void UpdateVectors()
        {
            if (selectedObject == null)
                return;

            if (referenceSystem == Space.Self)
            {
                fwdVR.SetShow(true);
                upVR.SetShow(true);
                rightVR.SetShow(true);

                northVR.SetShow(false);
                eastVR.SetShow(false);

                fwdVR.Vector = selectedObject.pqsCity.transform.forward;
                fwdVR.Start = vectorDrawPosition;
                fwdVR.draw();

                upVR.Vector = selectedObject.pqsCity.transform.up;
                upVR.Start = vectorDrawPosition;
                upVR.draw();

                rightVR.Vector = selectedObject.pqsCity.transform.right;
                rightVR.Start = vectorDrawPosition;
                rightVR.draw();
            }
            else if (referenceSystem == Space.World)
            {
                northVR.SetShow(true);
                eastVR.SetShow(true);

                fwdVR.SetShow(false);
                upVR.SetShow(false);
                rightVR.SetShow(false);

                northVR.Vector = northVector;
                northVR.Start = vectorDrawPosition;
                northVR.draw();

                eastVR.Vector = eastVector;
                eastVR.Start = vectorDrawPosition;
                eastVR.draw();
            }
        }

        /// <summary>
        /// creates the Vectors for later display
        /// </summary>
        private void SetupVectors()
        {
            // draw vectors
            fwdVR.Color = new Color(0, 0, 1);
            fwdVR.Vector = selectedObject.pqsCity.transform.forward;
            fwdVR.Scale = 30d;
            fwdVR.Start = vectorDrawPosition;
            fwdVR.SetLabel("forward");
            fwdVR.Width = 0.01d;
            fwdVR.SetLayer(5);

            upVR.Color = new Color(0, 1, 0);
            upVR.Vector = selectedObject.pqsCity.transform.up;
            upVR.Scale = 30d;
            upVR.Start = vectorDrawPosition;
            upVR.SetLabel("up");
            upVR.Width = 0.01d;

            rightVR.Color = new Color(1, 0, 0);
            rightVR.Vector = selectedObject.pqsCity.transform.right;
            rightVR.Scale = 30d;
            rightVR.Start = vectorDrawPosition;
            rightVR.SetLabel("right");
            rightVR.Width = 0.01d;

            northVR.Color = new Color(0.9f, 0.3f, 0.3f);
            northVR.Vector = northVector;
            northVR.Scale = 30d;
            northVR.Start = vectorDrawPosition;
            northVR.SetLabel("north");
            northVR.Width = 0.01d;

            eastVR.Color = new Color(0.3f, 0.3f, 0.9f);
            eastVR.Vector = eastVector;
            eastVR.Scale = 30d;
            eastVR.Start = vectorDrawPosition;
            eastVR.SetLabel("east");
            eastVR.Width = 0.01d;
        }

        /// <summary>
        /// stops the drawing of the vectors
        /// </summary>
        private void CloseVectors()
        {
            northVR.SetShow(false);
            eastVR.SetShow(false);
            fwdVR.SetShow(false);
            upVR.SetShow(false);
            rightVR.SetShow(false);
        }

        /// <summary>
        /// sets the latitude and lognitude from the deltas of north and east and creates a new reference vector
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        internal void Setlatlng(double north, double east)
        {
            body = Planetarium.fetch.CurrentMainBody;
            double latOffset = north / (body.Radius * KKMath.deg2rad);
            latitude += latOffset;
            double lonOffset = east / (body.Radius * KKMath.deg2rad);
            longitude += lonOffset * Math.Cos(Mathf.Deg2Rad * latitude);

            referenceVector = body.GetRelSurfaceNVector(latitude, longitude).normalized * body.Radius;
            saveSettings();
        }


        /// <summary>
        /// rotates a object around an right axis by an amount
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="amount"></param>
        internal void SetPitch(double amount)
        {
            Vector3 upProjeced = Vector3.ProjectOnPlane(orientation, Vector3.forward);
            double compensation = Vector3.Dot(Vector3.right, upProjeced);
            double internalRotation = rotation - compensation;
            Vector3 realRight = KKMath.RotateVector(Vector3.right, Vector3.back, internalRotation);

            Quaternion rotate = Quaternion.AngleAxis((float)amount, realRight);
            orientation = rotate * orientation;

            Vector3 oldfwd = selectedObject.gameObject.transform.forward;
            Vector3 oldright = selectedObject.gameObject.transform.right;
            saveSettings();
            Vector3 newfwd = selectedObject.gameObject.transform.forward;

            // compensate for unwanted rotation
            float deltaAngle = Vector3.Angle(Vector3.ProjectOnPlane(oldfwd, upVector), Vector3.ProjectOnPlane(newfwd, upVector));
            if (Vector3.Dot(oldright, newfwd) > 0)
            {
                deltaAngle *= -1f;
            }
            rotation += deltaAngle;

            saveSettings();
        }

        /// <summary>
        /// rotates a object around forward axis by an amount
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="amount"></param>
        internal void SetRoll(double amount)
        {
            Vector3 upProjeced = Vector3.ProjectOnPlane(orientation, Vector3.forward);
            double compensation = Vector3.Dot(Vector3.right, upProjeced);
            double internalRotation = rotation - compensation;
            Vector3 realFwd = KKMath.RotateVector(Vector3.forward, Vector3.right, internalRotation);

            Quaternion rotate = Quaternion.AngleAxis((float)amount, realFwd);
            orientation = rotate * orientation;


            Vector3 oldfwd = selectedObject.gameObject.transform.forward;
            Vector3 oldright = selectedObject.gameObject.transform.right;
            Vector3 oldup = selectedObject.gameObject.transform.up;
            saveSettings();
            Vector3 newfwd = selectedObject.gameObject.transform.forward;

            Vector3 deltaVector = oldfwd - newfwd;

            // try to compensate some of the pitch
            float deltaUpAngle = Vector3.Dot(oldup, deltaVector);
            if (Math.Abs(deltaUpAngle) > 0.0001)
            {
                SetPitch(-1 * deltaUpAngle);
            }

            // compensate for unwanted rotation
            float deltaAngle = Vector3.Angle(Vector3.ProjectOnPlane(oldfwd, upVector), Vector3.ProjectOnPlane(newfwd, upVector));
            if (Vector3.Dot(oldright, newfwd) > 0)
            {
                deltaAngle *= -1f;
            }
            rotation += deltaAngle;
            saveSettings();
        }


        /// <summary>
        /// changes the rotation by a defined amount
        /// </summary>
        /// <param name="increment"></param>
        internal void SetRotation(double increment)
        {
            rotation += increment;
            rotation = (360d + rotation) % 360d;
            saveSettings();
        }


        /// <summary>
        /// Updates the StaticObject position with a new transform
        /// </summary>
        /// <param name="direction"></param>
        internal void SetTransform(Vector3 direction)
        {
            // adjust transform for scaled models
            direction = direction / (float) modelScale;
            direction = selectedObject.gameObject.transform.TransformVector(direction);
            double northInc = Vector3d.Dot(northVector, direction);
            double eastInc = Vector3d.Dot(eastVector, direction);
            double upInc = Vector3d.Dot(upVector, direction);

            altitude += upInc;
            Setlatlng(northInc, eastInc);
        }


        /// <summary>
        /// Saves the current instance settings to the object.
        /// </summary>
        internal void saveSettings()
        {
            selectedObject.Orientation = orientation;

            if (modelScale < 0.01f)
                modelScale = 0.01f;

            rotation = (360d + rotation) % 360d;

            try
            {
                if (vis > modelScale * KerbalKonstructs.instance.maxEditorVisRange)
                {
                    vis = modelScale * KerbalKonstructs.instance.maxEditorVisRange;
                }
            }
            catch
            {
                // Just in case of overflow
                vis = Double.MaxValue;
            }
            if (vis < 1000)
            {
                vis = 1000;
            }

            selectedObject.RadialPosition = referenceVector;

            selectedObject.RadiusOffset = altitude;
            selectedObject.RotationAngle = (float)rotation;
            selectedObject.VisibilityRange = vis;
            selectedObject.RefLatitude = latitude;
            selectedObject.RefLongitude = longitude;

            selectedObject.FacilityType = facType;
            selectedObject.Group = sGroup;

            selectedObject.ModelScale = modelScale;

            selectedObject.isScanable = isScanable;

            updateSelection(selectedObject);

        }

        /// <summary>
        /// Updates the Window Strings to the new settings
        /// </summary>
        /// <param name="instance"></param>
        public static void updateSelection(StaticInstance instance)
        {
            selectedObject = instance;

            isScanable = selectedObject.isScanable;

            vis = instance.VisibilityRange;
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

        private void FixDrift(bool bRotate = false)
        {
            if (selectedSnapPoint == null || selectedSnapPoint2 == null
                || selectedObject == null || snapTargetInstance == null)
                return;

            Vector3d snapSourceLocalPos = selectedSnapPoint.transform.localPosition;
            Vector3d snapSourceWorldPos = selectedSnapPoint.transform.position;
            Vector3d selSourceWorldPos = selectedObject.gameObject.transform.position;
            float selSourceRot = selectedObject.pqsCity.reorientFinalAngle;
            Vector3d snapTargetLocalPos = selectedSnapPoint2.transform.localPosition;
            Vector3d snapTargetWorldPos = selectedSnapPoint2.transform.position;
            Vector3d selTargetWorldPos = snapTargetInstance.gameObject.transform.position;
            float selTargetRot = snapTargetInstance.pqsCity.reorientFinalAngle;
            double spdist = 0;

            if (bRotate)
            {
                spdist = Vector3.Distance(selSourceRot * snapSourceWorldPos, selTargetRot * snapTargetWorldPos);
            }
            else
            {
                spdist = Vector3.Distance(snapSourceWorldPos, snapTargetWorldPos);

                for (int iGiveUp = 0; spdist > 0.01 && iGiveUp < 100; ++iGiveUp)
                {
                    snpspos = selectedSnapPoint.transform.position;
                    snptpos = selectedSnapPoint2.transform.position;

                    vDrift = snpspos - snptpos;
                    vCurrpos = selectedObject.pqsCity.repositionRadial;
                    selectedObject.RadialPosition = vCurrpos + vDrift;
                    updateSelection(selectedObject);

                    spdist = Vector3.Distance(selectedSnapPoint.transform.position, selectedSnapPoint2.transform.position);
                }
            }
        }

        internal void CheckEditorKeys()
        {
            if (selectedObject != null)
            {
                if (IsOpen())
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        SetTransform(Vector3d.forward * increment);
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        SetTransform(Vector3d.back * increment);
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        SetTransform(Vector3d.right * increment);
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        SetTransform(Vector3d.left * increment);
                    }
                    if (Input.GetKey(KeyCode.E))
                    {
                        SetRotation(-increment);
                    }
                    if (Input.GetKey(KeyCode.Q))
                    {
                        SetRotation(increment);
                    }
                    if (Input.GetKey(KeyCode.PageUp))
                    {
                        altitude += increment;
                        saveSettings();
                    }
                    if (Input.GetKey(KeyCode.PageDown))
                    {
                        altitude -= increment;
                        saveSettings();
                    }
                    if (Event.current.keyCode == KeyCode.Return)
                    {
                        saveSettings();
                    }
                }
            }
        }

        void SnapToTarget(bool bRotate = false)
        {
            if (selectedSnapPoint == null || selectedSnapPoint2 == null
                || selectedObject == null || snapTargetInstance == null)
                return;

            Vector3 snapPointRelation = new Vector3(0, 0, 0);
            Vector3 snapPoint2Relation = new Vector3(0, 0, 0);
            Vector3 snapVector = new Vector3(0, 0, 0);
            Vector3 snapVectorNoRot = new Vector3(0, 0, 0);
            Vector3 vFinalPos = new Vector3(0, 0, 0);

            Vector3 snapSourcePos = selectedSnapPoint.transform.localPosition;
            snapSourceWorldPos = selectedSnapPoint.transform.position;
            Vector3 selSourcePos = selectedObject.gameObject.transform.position;
            Vector3 snapTargetPos = selectedSnapPoint2.transform.localPosition;
            snapTargetWorldPos = selectedSnapPoint2.transform.position;

            vbsnapangle1 = selectedSnapPoint.transform.position;
            vbsnapangle2 = selectedSnapPoint2.transform.position;

            if (!bRotate)
            {
                // Quaternion quatSelObj = Quaternion.AngleAxis(selSourceRot, selSourcePos);
                snapPointRelation = snapSourcePos;
                //quatSelObj * snapSourcePos;

                //Quaternion quatSelTar = Quaternion.AngleAxis(selTargetRot, selTargetPos);
                snapPoint2Relation = snapTargetPos;
                //quatSelTar * snapTargetPos;

                snapVector = (snapPoint2Relation - snapPointRelation);
                vFinalPos = snapTargetInstance.RadialPosition + snapVector;
            }
            else
            {
                // THIS SHIT DO NOT WORK
                //MiscUtils.HUDMessage("Snapping with rotation.", 60, 2);
                // Stick the origins on each other
                vFinalPos = snapTargetInstance.RadialPosition;
                selectedObject.RadialPosition = vFinalPos;
                updateSelection(selectedObject);

                // Get the offset of the source and move by that
                snapPointRelation = selectedObject.gameObject.transform.position -
                    selectedSnapPoint.transform.TransformPoint(selectedSnapPoint.transform.localPosition);
                MiscUtils.HUDMessage("" + snapPointRelation.ToString(), 60, 2);
                vFinalPos = snapTargetInstance.pqsCity.repositionRadial + snapPointRelation;
                selectedObject.RadialPosition = vFinalPos;
                updateSelection(selectedObject);

                // Get the offset of the target and move by that
                snapPoint2Relation = snapTargetInstance.gameObject.transform.position -
                    selectedSnapPoint2.transform.TransformPoint(selectedSnapPoint2.transform.localPosition);
                MiscUtils.HUDMessage("" + snapPoint2Relation.ToString(), 60, 2);
                vFinalPos = snapTargetInstance.pqsCity.repositionRadial + snapPoint2Relation;
            }

            snapSourcePos = selectedSnapPoint.transform.localPosition;
            snapTargetPos = selectedSnapPoint2.transform.localPosition;
            snapVectorNoRot = (snapSourcePos - snapTargetPos);

            selectedObject.RadialPosition = vFinalPos;
            selectedObject.RadiusOffset = snapTargetInstance.RadiusOffset + snapVectorNoRot.y;

            updateSelection(selectedObject);
            if (!bRotate)
                FixDrift();
        }

        #endregion
    }
}
