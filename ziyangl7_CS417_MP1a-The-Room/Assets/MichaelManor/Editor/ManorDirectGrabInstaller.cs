using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MichaelManorEditor
{
    public static class ManorDirectGrabInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";

        [MenuItem("Tools/Michael Manor/Install Explicit Direct Grab Interactors")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform rig = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .First(transform => transform.name == "XR Origin (XR Rig)");
            NearFarInteractor[] nearFar = rig.GetComponentsInChildren<NearFarInteractor>(true)
                .OrderBy(interactor => interactor.handedness).ToArray();
            if (nearFar.Length != 2)
            {
                Debug.LogError($"Direct Grab install requires exactly two hand NearFarInteractors; found {nearFar.Length}.");
                return;
            }

            foreach (NearFarInteractor source in nearFar)
            {
                string hand = source.handedness.ToString();
                Transform parent = source.transform.parent;
                string objectName = hand + " Direct Grab Interactor";
                Transform old = parent.Find(objectName);
                if (old != null) Object.DestroyImmediate(old.gameObject);

                GameObject target = new GameObject(objectName);
                target.transform.SetParent(parent, false);
                target.transform.localPosition = Vector3.zero;
                target.transform.localRotation = Quaternion.identity;
                SphereCollider collider = target.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = 0.11f;

                XRDirectInteractor direct = target.AddComponent<XRDirectInteractor>();
                EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(source), direct);
                direct.handedness = source.handedness;
                direct.attachTransform = source.attachTransform;
                direct.keepSelectedTargetValid = true;
                direct.improveAccuracyWithSphereCollider = true;

                // The controller objects are intentionally inactive when the desktop XR
                // simulator is driving the rig, so include inactive parents here.
                XRInteractionGroup group = source.GetComponentInParent<XRInteractionGroup>(true);
                if (group != null)
                {
                    SerializeStartingGroupMember(group, direct);
                }

                EditorUtility.SetDirty(direct);
                EditorUtility.SetDirty(collider);
                if (group != null) EditorUtility.SetDirty(group);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Installed explicit Left and Right XR Direct Interactors with the same hand inputs and attach transforms as the Near-Far interactors.");
        }

        private static void SerializeStartingGroupMember(XRInteractionGroup group, XRDirectInteractor direct)
        {
            SerializedObject serializedGroup = new SerializedObject(group);
            SerializedProperty members = serializedGroup.FindProperty("m_StartingGroupMembers");
            List<Object> retained = new List<Object>();
            for (int i = 0; i < members.arraySize; i++)
            {
                Object member = members.GetArrayElementAtIndex(i).objectReferenceValue;
                if (member != null && !(member is XRDirectInteractor)) retained.Add(member);
            }

            // Priority order is poke, direct touch, then the combined near/far ray.
            int directIndex = Mathf.Min(1, retained.Count);
            retained.Insert(directIndex, direct);
            members.arraySize = retained.Count;
            for (int i = 0; i < retained.Count; i++)
            {
                members.GetArrayElementAtIndex(i).objectReferenceValue = retained[i];
            }
            serializedGroup.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("Tools/Michael Manor/Test Explicit Direct Grab Interactors (Play Mode)")]
        public static void TestInPlayMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Enter Play Mode before testing explicit Direct Grab interactors.");
                return;
            }

            XRDirectInteractor[] directs = Object.FindObjectsByType<XRDirectInteractor>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool valid = directs.Length == 2;
            foreach (XRDirectInteractor direct in directs)
            {
                SphereCollider sphere = direct.GetComponent<SphereCollider>();
                XRInteractionGroup group = direct.GetComponentInParent<XRInteractionGroup>(true);
                valid &= direct.enabled && sphere != null && sphere.isTrigger && sphere.radius >= 0.1f &&
                         direct.attachTransform != null && group != null && group.ContainsGroupMember(direct);
            }

            if (!valid)
            {
                Debug.LogError($"Explicit Direct Grab test failed; found {directs.Length} valid candidates.");
                return;
            }

            Debug.Log("EXPLICIT DIRECT GRAB PASS: two enabled hand XRDirectInteractors, trigger volumes, attach transforms, and interaction-group registration.");
        }
    }
}
