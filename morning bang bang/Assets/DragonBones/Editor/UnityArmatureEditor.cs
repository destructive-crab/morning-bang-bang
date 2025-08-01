using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using UnityEditor.SceneManagement;

namespace DragonBones
{
    [CustomEditor(typeof(UnityArmatureInstance))]
    public class UnityArmatureEditor : UnityEditor.Editor
    {
        private long _nowTime = 0;
        private float _frameRate = 1.0f / 24.0f;

        private int _armatureIndex = -1;
        private int _animationIndex = -1;

        private List<string> _armatureNames = null;
        private List<string> _animationNames = null;

        private UnityArmatureInstance unityArmatureInstance = null;

        private SerializedProperty _playTimesPro;
        private SerializedProperty _timeScalePro;
        private SerializedProperty _flipXPro;
        private SerializedProperty _flipYPro;
        private SerializedProperty _closeCombineMeshsPro;

        void ClearUp()
        {
            this._armatureIndex = -1;
            this._animationIndex = -1;

            this._armatureNames = null;
            this._animationNames = null;
        }

        void OnEnable()
        {
            this.unityArmatureInstance = target as UnityArmatureInstance;
            if (_IsPrefab())
            {
                return;
            }

            this._nowTime = System.DateTime.Now.Ticks;

            this._playTimesPro = serializedObject.FindProperty("_playTimes");
            this._timeScalePro = serializedObject.FindProperty("_timeScale");
            this._flipXPro = serializedObject.FindProperty("_flipX");
            this._flipYPro = serializedObject.FindProperty("_flipY");
            this._closeCombineMeshsPro = serializedObject.FindProperty("_closeCombineMeshs");

            // Update armature.
            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                unityArmatureInstance.Armature == null &&
                unityArmatureInstance.unityData != null &&
                !string.IsNullOrEmpty(unityArmatureInstance.armatureName))
            {
                // Clear cache
                DBUnityFactory.factory.Clear(true);

                // Unload
                EditorUtility.UnloadUnusedAssetsImmediate();
                System.GC.Collect();

                // Load data.
                var dragonBonesData = DBUnityFactory.factory.LoadData(unityArmatureInstance.unityData);

                // Refresh texture atlas.
                DBUnityFactory.factory.RefreshAllTextureAtlas(unityArmatureInstance);

                // Refresh armature.
                DBUnityEditor.ChangeArmatureData(unityArmatureInstance, unityArmatureInstance.armatureName, dragonBonesData.name);

                // Refresh texture.
                unityArmatureInstance.Armature.InvalidUpdate(null, true);

                // Play animation.
                if (!string.IsNullOrEmpty(unityArmatureInstance.animationName))
                {
                    unityArmatureInstance.GetComponent<Animation>().Play(unityArmatureInstance.animationName, _playTimesPro.intValue);
                }
            }

            // Update hideFlags.
            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                unityArmatureInstance.Armature != null &&
                unityArmatureInstance.Armature.parent != null)
            {
                unityArmatureInstance.gameObject.hideFlags = HideFlags.NotEditable;
            }
            else
            {
                unityArmatureInstance.gameObject.hideFlags = HideFlags.None;
            }

            _UpdateParameters();
        }

        public override void OnInspectorGUI()
        {
            if (_IsPrefab())
            {
                return;
            }

            serializedObject.Update();

            if (_armatureIndex == -1)
            {
                _UpdateParameters();
            }

            // DragonBones Data
            EditorGUILayout.BeginHorizontal();

            unityArmatureInstance.unityData = EditorGUILayout.ObjectField("DragonBones Data", unityArmatureInstance.unityData, typeof(UnityDragonBonesData), false) as UnityDragonBonesData;

            var created = false;
            if (unityArmatureInstance.unityData != null)
            {
                if (unityArmatureInstance.Armature == null)
                {
                    if (GUILayout.Button("Create"))
                    {
                        created = true;
                    }
                }
                else
                {
                    if (GUILayout.Button("Reload"))
                    {
                        if (EditorUtility.DisplayDialog("DragonBones Alert", "Are you sure you want to reload data", "Yes", "No"))
                        {
                            created = true;
                        }
                    }
                }
            }
            else
            {
                //create UnityDragonBonesData by a json data
                if (GUILayout.Button("JSON"))
                {
                    PickJsonDataWindow.OpenWindow(unityArmatureInstance);
                }
            }

            if (created)
            {
                //clear cache
                DBUnityFactory.factory.Clear(true);
                ClearUp();
                unityArmatureInstance.animationName = null;

                if (DBUnityEditor.ChangeDragonBonesData(unityArmatureInstance, unityArmatureInstance.unityData.dragonBonesJSON))
                {
                    _UpdateParameters();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (unityArmatureInstance.Armature != null)
            {
                var dragonBonesData = unityArmatureInstance.Armature.armatureData.parent;

                // Armature
                if (DBUnityFactory.factory.GetAllDragonBonesData().ContainsValue(dragonBonesData) && _armatureNames != null)
                {
                    var armatureIndex = EditorGUILayout.Popup("Armature", _armatureIndex, _armatureNames.ToArray());
                    if (_armatureIndex != armatureIndex)
                    {
                        _armatureIndex = armatureIndex;

                        var armatureName = _armatureNames[_armatureIndex];
                        DBUnityEditor.ChangeArmatureData(unityArmatureInstance, armatureName, dragonBonesData.name);
                        _UpdateParameters();

                        unityArmatureInstance.gameObject.name = armatureName;

                        MarkSceneDirty();
                    }
                }

                // Animation
                if (_animationNames != null && _animationNames.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    List<string> anims = new List<string>(_animationNames);
                    anims.Insert(0, "<None>");
                    var animationIndex = EditorGUILayout.Popup("Animation", _animationIndex + 1, anims.ToArray()) - 1;
                    if (animationIndex != _animationIndex)
                    {
                        _animationIndex = animationIndex;
                        if (animationIndex >= 0)
                        {
                            unityArmatureInstance.animationName = _animationNames[animationIndex];
                            var animationData = unityArmatureInstance.GetComponent<Animation>().animations[unityArmatureInstance.animationName];
                            unityArmatureInstance.GetComponent<Animation>().Play(unityArmatureInstance.animationName, _playTimesPro.intValue);
                            _UpdateParameters();
                        }
                        else
                        {
                            unityArmatureInstance.animationName = null;
                            _playTimesPro.intValue = 0;
                            unityArmatureInstance.GetComponent<Animation>().Stop();
                        }

                        MarkSceneDirty();
                    }

                    if (_animationIndex >= 0)
                    {
                        if (unityArmatureInstance.GetComponent<Animation>().isPlaying)
                        {
                            if (GUILayout.Button("Stop"))
                            {
                                unityArmatureInstance.GetComponent<Animation>().Stop();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Play"))
                            {
                                unityArmatureInstance.GetComponent<Animation>().Play(null, _playTimesPro.intValue);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    //playTimes
                    EditorGUILayout.BeginHorizontal();
                    var playTimes = _playTimesPro.intValue;
                    EditorGUILayout.PropertyField(_playTimesPro, false);
                    if (playTimes != _playTimesPro.intValue)
                    {
                        if (!string.IsNullOrEmpty(unityArmatureInstance.animationName))
                        {
                            unityArmatureInstance.GetComponent<Animation>().Reset();
                            unityArmatureInstance.GetComponent<Animation>().Play(unityArmatureInstance.animationName, _playTimesPro.intValue);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    // TimeScale
                    var timeScale = _timeScalePro.floatValue;
                    EditorGUILayout.PropertyField(_timeScalePro, false);
                    if (timeScale != _timeScalePro.floatValue)
                    {
                        unityArmatureInstance.GetComponent<Animation>().timeScale = _timeScalePro.floatValue;
                    }
                }

                //
                EditorGUILayout.Space();

                if (!unityArmatureInstance.isUGUI)
                {
                   
                }

                EditorGUILayout.Space();

                // Flip
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Flip");
                var flipX = _flipXPro.boolValue;
                var flipY = _flipYPro.boolValue;
                _flipXPro.boolValue = GUILayout.Toggle(_flipXPro.boolValue, "X", GUILayout.Width(30));
                _flipYPro.boolValue = GUILayout.Toggle(_flipYPro.boolValue, "Y", GUILayout.Width(30));
                if (flipX != _flipXPro.boolValue || flipY != _flipYPro.boolValue)
                {
                    unityArmatureInstance.Armature.flipX = _flipXPro.boolValue;
                    unityArmatureInstance.Armature.flipY = _flipYPro.boolValue;

                    MarkSceneDirty();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }

            if (unityArmatureInstance.Armature != null && unityArmatureInstance.Armature.parent == null)
            {
                if (!Application.isPlaying && !this.unityArmatureInstance.isUGUI)
                {
                    //
                    var oldValue = this._closeCombineMeshsPro.boolValue;
                    if (!this._closeCombineMeshsPro.boolValue)
                    {
                        this._closeCombineMeshsPro.boolValue = EditorGUILayout.Toggle("CloseCombineMeshs", this._closeCombineMeshsPro.boolValue);

                        if (GUILayout.Button("Show Slots"))
                        {
                            ShowSlotsWindow.OpenWindow(this.unityArmatureInstance);
                        }
                    }

                    if(oldValue != this._closeCombineMeshsPro.boolValue)
                    {
                        if(this._closeCombineMeshsPro.boolValue)
                        {
                            this.unityArmatureInstance.CloseCombineMeshs();
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (!EditorApplication.isPlayingOrWillChangePlaymode && GUI.changed && Selection.activeGameObject == unityArmatureInstance.gameObject)
            {
                EditorUtility.SetDirty(unityArmatureInstance);
                HandleUtility.Repaint();
            }
        }

        void OnSceneGUI()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && unityArmatureInstance.Armature != null)
            {
                var dt = (System.DateTime.Now.Ticks - _nowTime) * 0.0000001f;
                if (dt >= _frameRate)
                {
                    unityArmatureInstance.Armature.AdvanceTime(dt);

                    foreach (var slot in unityArmatureInstance.Armature.GetSlots())
                    {
                        if (slot.childArmature != null)
                        {
                            slot.childArmature.AdvanceTime(dt);
                        }
                    }

                    //
                    _nowTime = System.DateTime.Now.Ticks;
                }
            }
        }

        private void _UpdateParameters()
        {
            if (unityArmatureInstance.Armature != null)
            {
                _frameRate = 1.0f / (float)unityArmatureInstance.Armature.armatureData.frameRate;

                if (unityArmatureInstance.Armature.armatureData.parent != null)
                {
                    _armatureNames = unityArmatureInstance.Armature.armatureData.parent.armatureNames;
                    _animationNames = unityArmatureInstance.GetComponent<Animation>().animationNames;
                    _armatureIndex = _armatureNames.IndexOf(unityArmatureInstance.Armature.name);
                    //
                    if (!string.IsNullOrEmpty(unityArmatureInstance.animationName))
                    {
                        _animationIndex = _animationNames.IndexOf(unityArmatureInstance.animationName);
                    }
                }
                else
                {
                    _armatureNames = null;
                    _animationNames = null;
                    _armatureIndex = -1;
                    _animationIndex = -1;
                }
            }
            else
            {
                _armatureNames = null;
                _animationNames = null;
                _armatureIndex = -1;
                _animationIndex = -1;
            }
        }

        private bool _IsPrefab()
        {
            return PrefabUtility.GetCorrespondingObjectFromSource(unityArmatureInstance.gameObject) == null
                && PrefabUtility.GetPrefabInstanceHandle(unityArmatureInstance.gameObject) != null;
        }

        private List<string> _GetSortingLayerNames()
        {
            var internalEditorUtilityType = typeof(InternalEditorUtility);
            var sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);

            return new List<string>(sortingLayersProperty.GetValue(null, new object[0]) as string[]);
        }

        private void MarkSceneDirty()
        {
            EditorUtility.SetDirty(unityArmatureInstance);
            //
            if (!Application.isPlaying && !_IsPrefab())
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
}