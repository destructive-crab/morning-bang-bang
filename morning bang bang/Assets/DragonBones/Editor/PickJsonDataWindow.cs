using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// JSON数据拾取，为UnityArmatureComponent创建UnityDragonBonesData
    /// </summary>
    public class PickJsonDataWindow : EditorWindow
    {
        private const string ObjectSelectorUpdated = "ObjectSelectorUpdated";
        private const string ObjectSelectorClosed = "ObjectSelectorClosed";
        //private const string PickFileFileter = "_ske t:TextAsset";
        private const string PickFileFileter = "t:TextAsset";

        private UnityArmatureInstance unityArmatureComp;
        private TextAsset _dragonBoneJSONData;

        private bool _isOpenPickWindow = false;
        private int _controlID;

        //
        public static void OpenWindow(UnityArmatureInstance unityArmatureComp)
        {
            if (unityArmatureComp == null)
            {
                return;
            }

            //
            var win = GetWindow<PickJsonDataWindow>();
            win.unityArmatureComp = unityArmatureComp;
        }

        private void OnDestroy()
        {
            unityArmatureComp = null;
            _dragonBoneJSONData = null;

            _isOpenPickWindow = false;
            _controlID = 0;
        }

        private void Awake()
        {
            _dragonBoneJSONData = null;

            _isOpenPickWindow = false;
            _controlID = 0;

            this.maxSize = Vector2.one;
            this.minSize = Vector2.one;
        }

        private void OnGUI()
        {
            ShowPickJsonWindow();

            string commandName = UnityEngine.Event.current.commandName;
            if (commandName == ObjectSelectorUpdated)
            {
                //更新JSON数据
                _dragonBoneJSONData = EditorGUIUtility.GetObjectPickerObject() as TextAsset;
            }
            else if (commandName == ObjectSelectorClosed)
            {
                //根据选择的JSON数据设置DragonBonesData

                //这里不仅创建了DragonBonesData,并且更新了场景中的显示对象
                //UnityEditor.ChangeDragonBonesData(_armatureComp, _dragonBoneJSONData);

                if (_dragonBoneJSONData != null)
                {
                    SetUnityDragonBonesData();
                }

                Repaint();

                this.Close();
            }
        }

        private void ShowPickJsonWindow()
        {
            if (_isOpenPickWindow)
            {
                return;
            }

            _controlID = EditorGUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker<TextAsset>(null, false, PickFileFileter, _controlID);

            _isOpenPickWindow = true;
        }

        private void SetUnityDragonBonesData()
        {
            List<string> textureAtlasJSONs = new List<string>();
            DBUnityEditor.GetTextureAtlasConfigs(textureAtlasJSONs, AssetDatabase.GetAssetPath(_dragonBoneJSONData.GetInstanceID()));
            UnityDragonBonesData.TextureAtlas[] textureAtlas = DBUnityEditor.GetTextureAtlasByJSONs(textureAtlasJSONs);

            UnityDragonBonesData data = DBUnityEditor.CreateUnityDragonBonesData(_dragonBoneJSONData, textureAtlas);
            unityArmatureComp.unityData = data;
        }
    }
}