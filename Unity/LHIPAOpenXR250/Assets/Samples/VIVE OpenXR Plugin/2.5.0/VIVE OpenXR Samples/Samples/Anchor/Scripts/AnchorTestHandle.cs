// Copyright HTC Corporation All Rights Reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using VIVE.OpenXR.Feature;
using VIVE.OpenXR.Toolkits;
using VIVE.OpenXR.Toolkits.Anchor;

namespace VIVE.OpenXR.Samples.Anchor
{
    /// <summary>
    /// This script is used to test AnchorManager.
    /// 
    /// Every frame, it will check if persisted anchors are existed.
    /// If existed, it will create anchors from them. If the anchor is existed, it will dispose it first.
    /// 
    /// Anchors created by AnchorManager.CreateAnchor() didn't have tracking ability.
    /// Only Anchors created by AnchorManager.CreateSpatialAnchorFromPersistedAnchor() have tracking ability.
    /// If Anchors has tracking ability, it will update pose over time.
    /// 
    /// The panchorObj1 and panchorObj2 are used to show the pose of the trackable anchors.
    /// 
    /// </summary>
    public class AnchorTestHandle : MonoBehaviour
    {
        public Transform rig;
        public Transform anchorPoseD;
        public Transform anchorPose1;
        public Transform anchorPose2;
        public Transform obj;
        public GameObject panchorObj1;
        public GameObject panchorObj2;
        public AnchorManager.Anchor anchor1;
        public AnchorManager.Anchor anchor2;
        string anchor1FromPA;
        string anchor2FromPA;
        public TextMeshProUGUI statusResponse;
        public TextMeshProUGUI statusOrigin;
        public TextMeshProUGUI statusAnchor;

        public Button btnCreateAnchor1;
        public Button btnCreateAnchor2;
        public Button btnFollowAnchor1;
        public Button btnFollowAnchor2;
        public Button btnResetObj;
        public Button btnClearSpatialAnchors;

        public Button btnPersistAnchor1;
        public Button btnPersistAnchor2;
        public Button btnUnpersistAnchor1;
        public Button btnUnpersistAnchor2;
        public Button btnClearPersistedAnchors;
        public Button btnAcquirePAC;
        public Button btnReleasePAC;

        public Button btnExportAll;
        public Button btnImportAll;
        public Button btnClearExportedAnchors;

        public Button btnFloor;
        public Button btnDevice;
        public Button btnReloadScene;
        public Button btnQuitApp;

        // For Editor test
        public bool testCreateAnchor1;
        public bool testCreateAnchor2;
        public bool testFollowAnchor1;
        public bool testFollowAnchor2;
        public bool testResetObj;
        public bool testClearSpatialAnchors;

        public bool testCreatePAC;
        public bool testDestroyPAC;
        public bool testPersistAnchor1;
        public bool testPersistAnchor2;
        public bool testUnpersistAnchor1;
        public bool testUnpersistAnchor2;
        public bool testClearPersistedAnchors;

        public bool testExportAll;
        public bool testImportAll;
        public bool testClearExportedAnchors;

        public bool testFloor;
        public bool testDevice;
        public bool testReloadScene;

        public XRInputSubsystem xrInputSubsystem;

        private int lastPACount = -1;
        private int lastFoundExportedFilesCount = -1;
        private FutureTask<(XrResult, IntPtr)> taskPAC = null;

        bool isAnchorSupported;
        bool isPAnchorSupported;
        bool isPACollectionAcquired;

        // 持 means "hold", "persisted".
        const string utf8Word = "持";

        // In Update, check enumerated spatial anchors to update these flags
        bool hasPersistedAnchor1;
        string persistedAnchor1Name;
        bool hasPersistedAnchor2;
        string persistedAnchor2Name;

        // Any anchor file in the app's local storage set flag to true
        bool hasPersistedAnchorFiles;

        bool doesUIInteractionsNeedUpdated = false;
        float lastUpdateTime = 0;

        readonly FutureTaskManager<XrSpace, XrResult> tmPA = new FutureTaskManager<XrSpace, XrResult>();
        readonly FutureTaskManager<string, (XrResult, AnchorManager.Anchor)> tmFPA = new FutureTaskManager<string, (XrResult, AnchorManager.Anchor)>();
        readonly List<Task> tasks = new List<Task>();

        void GetXRInputSubsystem()
        {
            List<XRInputSubsystem> xrSubsystemList = new List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(xrSubsystemList);
            foreach (var xrSubsystem in xrSubsystemList)
            {
                if (xrSubsystem.running)
                {
                    xrInputSubsystem = xrSubsystem;
                    break;
                }
            }
        }

        private void OnEnable()
        {
            btnCreateAnchor1.onClick.AddListener(OnCreateAnchor1);
            btnCreateAnchor2.onClick.AddListener(OnCreateAnchor2);
            btnFollowAnchor1.onClick.AddListener(OnFollowAnchor1);
            btnFollowAnchor2.onClick.AddListener(OnFollowAnchor2);
            btnResetObj.onClick.AddListener(OnResetObj);
            btnClearSpatialAnchors.onClick.AddListener(OnClearAllAnchors);

            btnAcquirePAC.onClick.AddListener(OnAcquirePersistedAnchorCollection);
            btnReleasePAC.onClick.AddListener(OnReleasePersistedAnchorCollection);
            btnPersistAnchor1.onClick.AddListener(OnPersistAnchor1);
            btnPersistAnchor2.onClick.AddListener(OnPersistAnchor2);
            btnUnpersistAnchor1.onClick.AddListener(OnUnpersistAnchor1);
            btnUnpersistAnchor2.onClick.AddListener(OnUnpersistAnchor2);

            btnClearPersistedAnchors.onClick.AddListener(OnClearPersistedAnchors);
            btnExportAll.onClick.AddListener(OnExportAll);
            btnImportAll.onClick.AddListener(OnImportAll);
            btnClearExportedAnchors.onClick.AddListener(OnClearExportedAnchors);

            btnFloor.onClick.AddListener(OnFloor);
            btnDevice.onClick.AddListener(OnDevice);
            btnReloadScene.onClick.AddListener(OnReloadScene);
            btnQuitApp.onClick.AddListener(Application.Quit);
        }

        private void OnAcquirePersistedAnchorCollection()
        {
            if (AnchorManager.IsPersistedAnchorCollectionAcquired()) return;
            if (taskPAC != null) return;

            taskPAC = AnchorManager.AcquirePersistedAnchorCollection();
            // Acturally, it will auto complete by default.  Just make sure we can continue with AutoCompleteTask
            taskPAC.AutoComplete();
            taskPAC.AutoCompleteTask.ContinueWith((act) => {
                taskPAC = null;
                CheckSupported();
                UINeedUpdate();
            });
        }

        private void OnReleasePersistedAnchorCollection()
        {
            // the taskPAC will be canceled in ReleasePersistedAnchorCollection
            taskPAC = null;

            anchor1FromPA = null;
            anchor2FromPA = null;
            hasPersistedAnchor1 = false;
            hasPersistedAnchor2 = false;
            persistedAnchor1Name = null;
            persistedAnchor2Name = null;
            //hasPersistedAnchorFiles = false;

            AnchorManager.ReleasePersistedAnchorCollection();
            CheckSupported();
            UINeedUpdate();
        }


        private void OnDisable()
        {
            // Remove listeners
            btnCreateAnchor1.onClick.RemoveListener(OnCreateAnchor1);
            btnCreateAnchor2.onClick.RemoveListener(OnCreateAnchor2);
            btnFollowAnchor1.onClick.RemoveListener(OnFollowAnchor1);
            btnFollowAnchor2.onClick.RemoveListener(OnFollowAnchor2);
            btnResetObj.onClick.RemoveListener(OnResetObj);
            btnClearSpatialAnchors.onClick.RemoveListener(OnClearAllAnchors);

            btnAcquirePAC.onClick.RemoveListener(OnAcquirePersistedAnchorCollection);
            btnReleasePAC.onClick.RemoveListener(OnReleasePersistedAnchorCollection);
            btnPersistAnchor1.onClick.RemoveListener(OnPersistAnchor1);
            btnPersistAnchor2.onClick.RemoveListener(OnPersistAnchor2);
            btnUnpersistAnchor1.onClick.RemoveListener(OnUnpersistAnchor1);
            btnUnpersistAnchor2.onClick.RemoveListener(OnUnpersistAnchor2);

            btnClearPersistedAnchors.onClick.RemoveListener(OnClearPersistedAnchors);
            btnExportAll.onClick.RemoveListener(OnExportAll);
            btnImportAll.onClick.RemoveListener(OnImportAll);
            btnClearExportedAnchors.onClick.RemoveListener(OnClearExportedAnchors);
            btnFloor.onClick.RemoveListener(OnFloor);

            btnDevice.onClick.RemoveListener(OnDevice);
            btnReloadScene.onClick.RemoveListener(OnReloadScene);
            btnQuitApp.onClick.RemoveListener(Application.Quit);

            // No need to clear all persistance anchor.  Reload scene can test the persist.

            // Disable Persistance Anchor
            AnchorManager.ReleasePersistedAnchorCollection();

            // Dispose all anchors
            OnClearAllAnchors();

            // Dispose all tasks
            foreach (var task in tasks)
            {
                task.Dispose();
            }
            tasks.Clear();

            // Clear all task managers.  It's readonly, so no need to dispose.
            tmPA.Clear();
            tmFPA.Clear();
        }

        private void OnDestroy()
        {
            // Dispose all anchors
            OnClearAllAnchorsInner(true);

            // Dispose all tasks
            foreach (var task in tasks)
            {
                task.Dispose();
            }
            tasks.Clear();

            // Clear all task managers.  It's readonly, so no need to dispose.
            tmPA.Clear();
            tmFPA.Clear();
        }

        void UINeedUpdate()
        {
            doesUIInteractionsNeedUpdated = true;
        }

        void UpdateUIInteractions()
        {
            if ((Time.time - lastUpdateTime) > 2.0f)
            {
                lastUpdateTime = Time.time;
                doesUIInteractionsNeedUpdated = true;
            }

            if (!doesUIInteractionsNeedUpdated)
                return;
            doesUIInteractionsNeedUpdated = false;

            bool hasAnchor1 = anchor1 != null;
            bool hasAnchor2 = anchor2 != null;
            bool hasAnchor = hasAnchor1 || hasAnchor2;

            // Create / Follow / Reset / Dispose
            // If created, can created again.
            // If no Anchor, cannot follow.
            // however these interactions no need block
            btnCreateAnchor1.interactable = isAnchorSupported && !hasPersistedAnchor1;
            btnCreateAnchor2.interactable = isAnchorSupported && !hasPersistedAnchor2;
            btnFollowAnchor1.interactable = hasAnchor1 && !anchor1.IsPersisted;
            btnFollowAnchor2.interactable = hasAnchor2 && !anchor2.IsPersisted;
            btnResetObj.interactable = true;
            btnClearSpatialAnchors.interactable = hasAnchor;

            // Create Persisted Anchor Collection / Destroy Persisted Anchor Collection
            // If has created, can destroy
            // If has not created, can create
            btnAcquirePAC.interactable = isPAnchorSupported && !isPACollectionAcquired && taskPAC == null;
            btnReleasePAC.interactable = isPAnchorSupported && isPACollectionAcquired;

            // Persist / Unpersist / Clear
            // If has anchor, can persist
            // If has persisted anchor, can unpersist
            // If has persisted anchor, can clear
            btnPersistAnchor1.interactable = isPACollectionAcquired && hasAnchor1;
            btnPersistAnchor2.interactable = isPACollectionAcquired && hasAnchor2;
            btnUnpersistAnchor1.interactable = isPACollectionAcquired && hasPersistedAnchor1;
            btnUnpersistAnchor2.interactable = isPACollectionAcquired && hasPersistedAnchor2;
            btnClearPersistedAnchors.interactable = isPACollectionAcquired && (hasPersistedAnchor1 || hasPersistedAnchor2);

            // Export / Import / Clear
            // If has persisted anchor, can export
            // If has exported anchor, can import
            // If has exported anchor, can clear
            btnExportAll.interactable = isPACollectionAcquired && (hasPersistedAnchor1 || hasPersistedAnchor2) && tasks.Count == 0;
            btnImportAll.interactable = isPACollectionAcquired && hasPersistedAnchorFiles && tasks.Count == 0;
            // Not to clear when exporting or importing
            btnClearExportedAnchors.interactable = hasPersistedAnchorFiles && tasks.Count == 0;

            string sa1 = "", sa2 = "";
            if (hasAnchor1)
                sa1 = "A1: " + anchor1.Name + (anchor1.IsTrackable ? " T" : "") + (anchor1.IsPersisted ? " P" : "") + "\n";
            if (hasAnchor2)
                sa2 = "A2: " + anchor2.Name + (anchor2.IsTrackable ? " T" : "") + (anchor2.IsPersisted ? " P" : "") + "\n";
            statusAnchor.text = sa1 + sa2 +
                (hasPersistedAnchor1 ? "PA1: " + persistedAnchor1Name + "\n" : "") +
                (hasPersistedAnchor2 ? "PA2: " + persistedAnchor2Name + "\n" : "");
        }

        void CheckSupported()
        {
            isAnchorSupported = AnchorManager.IsSupported();
            Debug.Log("AnchorTestHandle: Is Anchor supported: " + isAnchorSupported);
            isPAnchorSupported = AnchorManager.IsPersistedAnchorSupported();
            Debug.Log("AnchorTestHandle: Is Persisted Anchor supported: " + isPAnchorSupported);
            isPACollectionAcquired = AnchorManager.IsPersistedAnchorCollectionAcquired();
            Debug.Log("AnchorTestHandle: Is Persisted Anchor Collection acquired: " + isPACollectionAcquired);
        }

        IEnumerator Start()
        {
            CheckSupported();
            UINeedUpdate();
            UpdateUIInteractions();

            yield return null;  // yield and let Time.unscaledTime to be updated
            float t = Time.unscaledTime;
            while (xrInputSubsystem == null)
            {
                yield return null;
                GetXRInputSubsystem();
                if (Time.unscaledTime - t > 5)
                {
#if UNITY_EDITOR
                    Debug.LogError("Get XRInputSubsystem timeout.  Check if you acturally enable the OpenXR's Mock Runtime.");
#else
                    Debug.LogError("Get XRInputSubsystem timeout.  Check if you acturally enable any VR runtime."); 
#endif
                    statusResponse.text = "No VR runtime";
                    yield break;
                }
            }

            // Check again
            CheckSupported();
            UINeedUpdate();

            if (!isPAnchorSupported) yield break;

            Debug.Log("AnchorTestHandle: AcquirePersistedAnchorCollection");

            taskPAC = AnchorManager.AcquirePersistedAnchorCollection();
            taskPAC.Debug = true;
            while (!AnchorManager.IsPersistedAnchorCollectionAcquired())
            {
                Debug.Log("AnchorTestHandle: Wait PersistedAnchorCollection acquired");
                yield return null;
            }
            taskPAC = null;

            AnchorManager.GetPersistedAnchorProperties(out ViveAnchor.XrPersistedAnchorPropertiesGetInfoHTC properties);
            Debug.Log("AnchorTestHandle: PersistAnchorProperties.maxPersistedAnchorCount=" + properties.maxPersistedAnchorCount);

            CheckSupported();

            Debug.Log("AnchorTestHandle: Start() finished");

            UINeedUpdate();
            UpdateUIInteractions();
        }

        public Pose GetRelatedPoseToRig(Transform t)
        {
            return new Pose(rig.InverseTransformPoint(t.position), Quaternion.Inverse(rig.rotation) * t.rotation);
        }

        string MakeAnchorName(string userName)
        {
            // userName means user defined name.  Add frame count to make it unique.
            return userName + " (" + Time.frameCount + ")";
        }

        string MakePersistedAnchorName(string anchorName)
        {
            // the anchor name support UTF-8 encoding.
            return anchorName + utf8Word;
        }

        string MakeSpatialAnchorName(string persistedName)
        {
            // Remove the utf8Word from the anchor name
            return persistedName.Substring(0, persistedName.Length - utf8Word.Length);
        }

        /// <summary>
        /// Help create anchor by anchor manager
        /// </summary>
        /// <param name="relatedPose">pose related to camera rig</param>
        /// <param name="name">the anchor's name</param>
        /// <returns></returns>
        AnchorManager.Anchor CreateAnchor(Pose relatedPose, string name)
        {
            if (!AnchorManager.IsSupported())
            {
                Debug.LogError("AnchorManager: Anchor is not supported.");
                statusResponse.text = "Anchor is not supported.";
                return null;
            }
            var anchor = AnchorManager.CreateAnchor(relatedPose, MakeAnchorName(name));
            if (anchor == null)
            {
                statusResponse.text = "Create " + name + " failed";
                Debug.LogError("Create " + name + " failed");
                // Even error, still got.  Use fake data.
                return anchor;
            }
            else
            {
                string msg = "Create Anchor n=" + anchor.Name + " space=" + anchor.GetXrSpace() + " at p=" + relatedPose.position + " & r=" + relatedPose.rotation.eulerAngles;
                statusResponse.text = msg;
                Debug.Log(msg);
                return  anchor;
            }
        }

        public void OnCreateAnchor1()
        {
            if (anchor1 != null)
            {
                anchor1.Dispose();
                anchor1 = null;
            }
            anchor1 = CreateAnchor(GetRelatedPoseToRig(anchorPose1), "anchor1");
            UINeedUpdate();
        }

        public void OnCreateAnchor2()
        {
            if (anchor2 != null)
            {
                anchor2.Dispose();
                anchor2 = null;
            }
            anchor2 = CreateAnchor(GetRelatedPoseToRig(anchorPose2), "anchor2");
            UINeedUpdate();
        }

        public void MoveObjToAnchor(AnchorManager.Anchor anchor)
        {
            if (!AnchorManager.IsSupported())
                return;

            if (anchor == null)
            {
                statusResponse.text = "anchor is null";
                return;
            }

            if (AnchorManager.GetTrackingSpacePose(anchor, out Pose pose))
            {
                // Convert tracking space pose to world space pose
                obj.position = rig.TransformPoint(pose.position);
                obj.rotation = rig.rotation * pose.rotation;

                statusResponse.text = "Obj move to " + anchor.GetSpatialAnchorName();
            }
            else
            {
                statusResponse.text = "Fail to get anchor's pose";
            }
        }

        public void OnFollowAnchor1()
        {
            Debug.Log("AnchorTestHandle: OnFollowAnchor1()");
            MoveObjToAnchor(anchor1);
        }

        public void OnFollowAnchor2()
        {
            Debug.Log("AnchorTestHandle: OnFollowAnchor2()");
            MoveObjToAnchor(anchor2);
        }

        public void OnResetObj()
        {
            Debug.Log("AnchorTestHandle: OnResetObj()");
            if (obj != null && anchorPoseD != null)
            {
                obj.position = anchorPoseD.position;
                obj.rotation = anchorPoseD.rotation;
            }

            if (statusResponse != null)
                statusResponse.text = "Obj move to default pose";
        }

        private void OnClearAllAnchorsInner(bool isDestroy = false)
        {
            Debug.Log("AnchorTestHandle: OnClearAllAnchors()");
            try
            {
                if (!isDestroy)
                {
                    // Not to touch object when destroy
                    if (obj != null && anchorPoseD != null)
                    {
                        obj.position = anchorPoseD.position;
                        obj.rotation = anchorPoseD.rotation;
                    }

                    if (statusResponse != null)
                    {
                        if (hasPersistedAnchor1 || hasPersistedAnchor2)
                            statusResponse.text = "Dispose spatial anchors but will create again by persisted";
                        else
                            statusResponse.text = "Dispose spatial anchors";
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("AnchorTestHandle: OnClearAllAnchorsInner: " + e.Message);
            }

            if (anchor1 != null)
            {
                anchor1.Dispose();
                anchor1 = null;
            }
            if (anchor2 != null)
            {
                anchor2.Dispose();
                anchor2 = null;
            }
        }

        public void OnClearAllAnchors()
        {
            OnClearAllAnchorsInner();
        }

        public void OnPersistAnchor1()
        {
            Debug.Log("AnchorTestHandle: OnPersistAnchor1()");
            if (anchor1 == null)
            {
                statusResponse.text = "anchor1 is null";
                return;
            }

            // Create new persisted anchor name by frame count.  Not to follow the original anchor name.
            var newName = MakePersistedAnchorName(MakeAnchorName("anchor1"));
            Debug.Log($"AnchorTestHandle: Persist {anchor1.Name} to {newName}");
            var task = tmPA.GetTask(anchor1.GetXrSpace());
            if (task != null)
            {
                statusResponse.text = "Persist " + newName + " is running, please wait";
                return;
            }

            task = AnchorManager.PersistAnchor(anchor1, newName);
            tmPA.AddTask(anchor1.GetXrSpace(), task);
            statusResponse.text = "Persist " + newName + " task is running, please wait";
        }

        public void OnPersistAnchor2()
        {
            Debug.Log("AnchorTestHandle: OnPersistAnchor2()");
            if (anchor2 == null)
            {
                statusResponse.text = "anchor2 is null";
                return;
            }

            // Create new persisted anchor name by frame count.  Not to follow the original anchor name.
            var newName = MakePersistedAnchorName(MakeAnchorName("anchor2"));
            Debug.Log($"AnchorTestHandle: Persist {anchor2.Name} to {newName}");
            var task = tmPA.GetTask(anchor2.GetXrSpace());
            if (task != null)
            {
                statusResponse.text = "Persist " + newName + " is running, please wait";
                return;
            }

            task = AnchorManager.PersistAnchor(anchor2, newName);
            tmPA.AddTask(anchor2.GetXrSpace(), task);
            statusResponse.text = "Persist " + newName + " task is running, please wait";
        }

        public void OnUnpersistAnchor1()
        {
            Debug.Log("AnchorTestHandle: OnUnpersistAnchor1()");
            if (!hasPersistedAnchor1 || string.IsNullOrEmpty(persistedAnchor1Name))
            {
                statusResponse.text = "persist anchor1 not exist";
                return;
            }

            var ret = AnchorManager.UnpersistAnchor(persistedAnchor1Name) == XrResult.XR_SUCCESS;
            if (ret)
                statusResponse.text = "Unpersist " + persistedAnchor1Name + " success";
            else
                statusResponse.text = "Unpersist " + persistedAnchor1Name + " failed: " + ret;

            hasPersistedAnchor1 = false;
            persistedAnchor1Name = null;
            UINeedUpdate();
        }

        public void OnUnpersistAnchor2()
        {
            Debug.Log("AnchorTestHandle: OnUnpersistAnchor2()");
            if (!hasPersistedAnchor2 || string.IsNullOrEmpty(persistedAnchor2Name))
            {
                statusResponse.text = "persist anchor2 not exist";
                return;
            }

            var ret = AnchorManager.UnpersistAnchor(persistedAnchor2Name) == XrResult.XR_SUCCESS;
            if (ret)
                statusResponse.text = "Unpersist " + persistedAnchor2Name + " success";
            else
                statusResponse.text = "Unpersist " + persistedAnchor2Name + " failed: " + ret;

            hasPersistedAnchor2 = false;
            persistedAnchor2Name = null;
            UINeedUpdate();
        }

        public void OnClearPersistedAnchors()
        {
            Debug.Log("AnchorTestHandle: OnClearPersistedAnchors()");
            var ret = AnchorManager.ClearPersistedAnchors() == XrResult.XR_SUCCESS;
            if (ret)
            {
                hasPersistedAnchor1 = false;
                hasPersistedAnchor2 = false;
                persistedAnchor1Name = null;
                persistedAnchor2Name = null;

                statusResponse.text = "Clear persisted anchors success";
            }
            else
                statusResponse.text = "Clear persisted anchors failed: " + ret;
        }

        public IEnumerator WaitExportFinish()
        {
            while (tasks.Count > 0)
            {
                Debug.Log("AnchorTestHandle: WaitExportFinish: " + tasks.Count);
                foreach (var task in tasks)
                {
                    if (task.IsCompleted)
                    {
                        Task<(XrResult, string, byte[])> t = task as Task<(XrResult, string, byte[])>;
                        if (t.Result.Item1 == XrResult.XR_SUCCESS)
                        {
                            // write to file
                            string path = GetExportPath();
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            string paname = t.Result.Item2;
                            if (!paname.EndsWith(utf8Word))
                                throw new System.Exception("Anchor name is not ended with " + utf8Word);
                            paname = paname.Substring(0, paname.Length - 1);
                            string file = Path.Combine(path, paname + ".pa");
                            Debug.Log("AnchorTestHandle: Data length: " + t.Result.Item3.Length);
                            File.WriteAllBytes(file, t.Result.Item3);
                            Debug.Log("AnchorTestHandle: Exported anchor to " + file);
                            statusResponse.text = "Exported anchor to " + file;
                        }
                        else
                        {
                            Debug.LogError("AnchorTestHandle: Export persisted anchor failed: " + t.Result.Item1);
                            statusResponse.text = "Export persisted anchor failed: " + t.Result.Item1;
                        }
                        tasks.Remove(task);
                        break;
                    }
                }
                yield return null;
            }

            UINeedUpdate();

            statusResponse.text = "All persisted anchors exported";
        }

        public void OnExportAll()
        {
            Debug.Log("AnchorTestHandle: OnExportAll()");
            // Create export folder
            string path = GetExportPath();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            OnClearExportedAnchors();

            if (tasks.Count > 0)
            {
                statusResponse.text = "Other task is running, please wait";
                return;
            }

            statusResponse.text = "Exporting anchors";

            if (hasPersistedAnchor1)
            {
                Task t = AnchorManager.ExportPersistedAnchor(persistedAnchor1Name);
                tasks.Add(t);
            }

            if (hasPersistedAnchor2)
            {
                Task t = AnchorManager.ExportPersistedAnchor(persistedAnchor2Name);
                tasks.Add(t);
            }

            // Has tasks, export/import/clear buttons should be disabled
            UINeedUpdate();

            StartCoroutine(WaitExportFinish());
        }

        public IEnumerator WaitImportFinish()
        {
            while (tasks.Count > 0)
            {
                foreach (var task in tasks)
                {
                    if (task.IsCompleted)
                    {
                        tasks.Remove(task);
                        break;
                    }
                }
                yield return null;
            }

            // No tasks, export/import/clear buttons should be enabled
            UINeedUpdate();
            statusResponse.text = "All persisted anchors imported";
        }

        public void OnImportAll()
        {
            Debug.Log("AnchorTestHandle: OnImportAll()");
            if (tasks.Count > 0)
            {
                statusResponse.text = "Other task is running, please wait";
                return;
            }

            var list = GetExportedFilesList();
            if (list == null)
            {
                statusResponse.text = "No exported anchor files";
                return;
            }

            string names = "";
            try
            {
                foreach (var file in list)
                {
                    Task tread = new Task(async () =>
                    {
                        try
                        {
                            Debug.Log("AnchorTestHandle: Read file: " + file);
                            var data = File.ReadAllBytes(file);
                            Debug.Log("AnchorTestHandle: data length: " + data.Length);

                            AnchorManager.GetPersistedAnchorNameFromBuffer(data, out string name);
                            Debug.Log("AnchorTestHandle: data is from " + name);
                            names += name + "\n";

                            Task<XrResult> importTask = AnchorManager.ImportPersistedAnchor(data);
                            await importTask;

                            if (importTask.Result == XrResult.XR_SUCCESS)
                                Debug.Log("AnchorTestHandle: Import persisted anchor success");
                            else
                                Debug.LogError("AnchorTestHandle: Import persisted anchor failed: " + importTask.Result);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("AnchorTestHandle: Read file " + file + " failed: " + e.Message);
                            statusResponse.text = "Read file " + file + " failed: " + e.Message;
                        }
                    });
                    tread.Start();
                    tasks.Add(tread);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("AnchorTestHandle: Import persisted anchor failed: " + e.Message);
                statusResponse.text = "Import persisted anchor failed: " + e.Message;
            }

            // Has tasks, export/import/clear buttons should be disabled
            UINeedUpdate();
            statusResponse.text = "Importing " + list.Count + "persist anchor(s):\n" + names;


            StartCoroutine(WaitImportFinish());
        }

        public void OnClearExportedAnchors()
        {
            Debug.Log("AnchorTestHandle: OnClearExportedAnchors()");
            var list = GetExportedFilesList();
            if (list != null)
            {
                foreach (var file in list)
                {
                    Debug.Log("AnchorTestHandle: Delete file " + file);
                    File.Delete(file);
                }
                hasPersistedAnchorFiles = false;
                statusResponse.text = "Del " + list.Count + " files";
            }
            UINeedUpdate();
        }

        string GetExportPath()
        {
            return Path.Combine(Application.persistentDataPath, "Anchors");
        }

        List<string> GetExportedFilesList()
        {
            // Get persistance storage path
            // Check if any anchor file in the app's local storage
            string path = GetExportPath();
            if (!Directory.Exists(path)) return null;

            string[] files = Directory.GetFiles(path);
            List<string> anchorFiles = new List<string>();
            if (files.Length == 0) return null;
            foreach (string file in files ) {
                if (file.EndsWith(".pa"))
                    anchorFiles.Add(file);
            }
            // Print log if files count changed
            if (anchorFiles.Count != lastFoundExportedFilesCount)
            {
                Debug.Log("AnchorTestHandle: Found " + anchorFiles.Count + " anchor files");
                foreach (var file in anchorFiles)
                    Debug.Log("AnchorTestHandle:     " + file);
                lastFoundExportedFilesCount = anchorFiles.Count;
            }

            hasPersistedAnchorFiles = anchorFiles.Count > 0;
            return anchorFiles;
        }

        public void CheckExportedFiles()
        {
            if (hasPersistedAnchorFiles) return;

            GetExportedFilesList();
        }

        public void EnumeratePersistedAnchors()
        {
            if (!isAnchorSupported || !isPAnchorSupported || !isPACollectionAcquired)
                return;

            if (AnchorManager.GetNumberOfPersistedAnchors(out int count) != XrResult.XR_SUCCESS) {
                Debug.LogError("AnchorTestHandle: GetNumberOfPersistedAnchors failed");
                return;
            }

            var tmpHasPA1 = hasPersistedAnchor1;
            var tmpHasPA2 = hasPersistedAnchor2;
            hasPersistedAnchor1 = false;
            hasPersistedAnchor2 = false;
            persistedAnchor1Name = null;
            persistedAnchor2Name = null;

            if (count != lastPACount)
            {
                Debug.Log("AnchorTestHandle: GetNumberOfPersistedAnchors=" + count);
                lastPACount = count;
            }

            if (count == 0)
                return;

            // Enumerate persisted anchors
            if (AnchorManager.EnumeratePersistedAnchorNames(out string[] names) != XrResult.XR_SUCCESS)
            {
                Debug.LogError("AnchorTestHandle: EnumeratePersistedAnchorNames failed");
                return;
            }

            foreach (var name in names)
            {
                if (name.EndsWith(utf8Word))
                {
                    if (name.StartsWith("anchor1"))
                    {
                        if (!tmpHasPA1)
                            Debug.Log("AnchorTestHandle: Found persisted anchor1: " + name);
                        hasPersistedAnchor1 = true;
                        persistedAnchor1Name = name;
                    }
                    else if (name.StartsWith("anchor2"))
                    {
                        if (!tmpHasPA2)
                            Debug.Log("AnchorTestHandle: Found persisted anchor2: " + name);
                        hasPersistedAnchor2 = true;
                        persistedAnchor2Name = name;
                    }
                }

                // Others don't care
            }

            if (hasPersistedAnchor1 != tmpHasPA1 && hasPersistedAnchor1 == false)
                Debug.Log("AnchorTestHandle: Lost persisted anchor1");

            if (hasPersistedAnchor2 != tmpHasPA2 && hasPersistedAnchor2 == false)
                Debug.Log("AnchorTestHandle: Lost persisted anchor2");

            if (hasPersistedAnchor1 != tmpHasPA1 || hasPersistedAnchor2 != tmpHasPA2)
                UINeedUpdate();
        }

        /// <summary>
        /// If both anchor and persited anchor are existed, check if the anchor is created from the persisted anchor.  If not, destroy the anchor.
        /// </summary>
        /// <param name="hasPA">Persisted anchor exist</param>
        /// <param name="paName">Persisted anchor's name</param>
        /// <param name="anchor">if anchor is not null, anchor exist</param>
        /// <param name="anchorFromPA">If the anchor is from a persisted anchor, the persisted anchor's name</param>
        /// <returns>If the anchor need be destroyed</returns>
        bool NeedDestroy(bool hasPA, string paName, AnchorManager.Anchor anchor, string anchorFromPA) {
            // If no persisted anchor, no need to destroy the anchor.
            if (!hasPA)
                return false;
            // If no anchor, no need to destroy the anchor.
            if (anchor == null)
                return false;
            // If the anchor is created from the same persisted anchor, no need to destroy it.
            if (anchorFromPA == paName)
                return false;
            // If the anchor is created from other persisted anchor, need to destroy it.
            return true;
        }

        bool NeedCancelCFPA(bool hasPA, string paName, FutureTask<(XrResult, AnchorManager.Anchor)> task, string anchorFromPA) {
            // If task is already created from the same persisted anchor, no need to cancel it.
            if (anchorFromPA == paName)
                return false;
            // If task is already created from the same persisted anchor, no need to cancel it.
            // If no persisted anchor, keep the task.
            return hasPA && task != null;
        }

        // If persisted anchors are existed, create anchor from them.
        void UpdateAnchorsIfPersistExist()
        {
            // If both anchor and persited anchor are existed, check if the anchor is created from the persisted anchor.  If not, dispose the anchor.

            // Check if the anchor is realted to the persisted anchor.
            bool a1NeedDispose = NeedDestroy(hasPersistedAnchor1, persistedAnchor1Name, anchor1, anchor1FromPA);
            bool a2NeedDispose = NeedDestroy(hasPersistedAnchor2, persistedAnchor2Name, anchor2, anchor2FromPA);

            if (a1NeedDispose)
            {
                Debug.Log("AnchorTestHandle: Dispose existed anchor1 because we want to create it from persisted anchor");
                anchor1.Dispose();
                anchor1 = null;
                anchor1FromPA = "";
            }

            if (a2NeedDispose)
            {
                Debug.Log("AnchorTestHandle: Dispose existed anchor2 because we want to create it from persisted anchor");
                anchor2.Dispose();
                anchor2 = null;
                anchor2FromPA = "";
            }

            // Use TaskManager to keep the tasks
            var task1 = tmFPA.GetTask(anchor1FromPA);
            var task2 = tmFPA.GetTask(anchor2FromPA);

            // Check if the task is related to the persisted anchor.
            bool cfpa1NeedCancel = NeedCancelCFPA(hasPersistedAnchor1, persistedAnchor1Name, task1, anchor1FromPA);
            bool cfpa2NeedCancel = NeedCancelCFPA(hasPersistedAnchor2, persistedAnchor2Name, task2, anchor2FromPA);

            if (cfpa1NeedCancel)
            {
                tmFPA.RemoveTask(task1);
                task1 = null;
                anchor1FromPA = "";
            }

            if (cfpa2NeedCancel)
            {
                tmFPA.RemoveTask(task2);
                task2 = null;
                anchor2FromPA = "";
            }

            bool needCreateAnchor1 = hasPersistedAnchor1 && anchor1 == null && task1 == null && !string.IsNullOrEmpty(persistedAnchor1Name);
            if (needCreateAnchor1)
            {
                task1 = AnchorManager.CreateSpatialAnchorFromPersistedAnchor(persistedAnchor1Name, MakeSpatialAnchorName(persistedAnchor1Name));
                tmFPA.AddTask(persistedAnchor1Name, task1);
                anchor1FromPA = persistedAnchor1Name;
            }

            bool needCreateAnchor2 = hasPersistedAnchor2 && anchor2 == null && task2 == null && !string.IsNullOrEmpty(persistedAnchor2Name);
            if (needCreateAnchor2)
            {
                task2 = AnchorManager.CreateSpatialAnchorFromPersistedAnchor(persistedAnchor2Name, MakeSpatialAnchorName(persistedAnchor2Name));
                tmFPA.AddTask(persistedAnchor2Name, task2);
                anchor2FromPA = persistedAnchor2Name;
            }

            if (task1 != null && task1.IsPollCompleted)
            {
                var result = task1.Complete();
                tmFPA.RemoveTask(task1);
                if (result.Item1 != XrResult.XR_SUCCESS)
                {
                    Debug.LogError("AnchorTestHandle: Create anchor1 from persisted anchor failed");
                    statusResponse.text = "Create anchor1 from persisted anchor failed";
                    anchor1FromPA = null;
                }
                else
                {
                    anchor1 = result.Item2;
                }
            }

            if (task2 != null && task2.IsPollCompleted)
            {
                var result = task2.Complete();
                tmFPA.RemoveTask(task2);
                if (result.Item1 != XrResult.XR_SUCCESS)
                {
                    Debug.LogError("AnchorTestHandle: Create anchor2 from persisted anchor failed");
                    statusResponse.text = "Create anchor2 from persisted anchor failed";
                    anchor2FromPA = null;
                }
                else
                {
                    anchor2 = result.Item2;
                }
            }
        }

        float lastUpdateTimeTAP = -1;

        void UpdateTrackableAnchorsPose()
        {
            bool panchorObj1ShouldBeSeen = false;
            bool panchorObj2ShouldBeSeen = false;

            if (anchor1 == null || !anchor1.IsTrackable)
                panchorObj1ShouldBeSeen = false;

            if (anchor2 == null || !anchor2.IsTrackable)
                panchorObj2ShouldBeSeen = false;

            if (anchor1 != null && anchor1.IsTrackable)
            {
                if (AnchorManager.GetTrackingSpacePose(anchor1, out Pose pose))
                {
                    panchorObj1ShouldBeSeen = true;
                    panchorObj1.transform.position = rig.TransformPoint(pose.position);
                    panchorObj1.transform.rotation = rig.rotation * pose.rotation;
                }
                else
                {
                    panchorObj1ShouldBeSeen = false;
                    Debug.LogError("AnchorTestHandle: GetTrackingSpacePose for anchor1 is failed");
                }
            }

            if (anchor2 != null && anchor2.IsTrackable)
            {
                if (AnchorManager.GetTrackingSpacePose(anchor2, out Pose pose))
                {
                    panchorObj2ShouldBeSeen = true;
                    panchorObj2.transform.position = rig.TransformPoint(pose.position);
                    panchorObj2.transform.rotation = rig.rotation * pose.rotation;
                }
                else
                {
                    panchorObj2ShouldBeSeen = false;
                    Debug.LogError("AnchorTestHandle: GetTrackingSpacePose for anchor2 is failed");
                }
            }

            var switchObj1 = panchorObj1ShouldBeSeen != panchorObj1.activeInHierarchy;
            if (switchObj1)
                panchorObj1.SetActive(panchorObj1ShouldBeSeen);

            var switchObj2 = panchorObj2ShouldBeSeen != panchorObj2.activeInHierarchy;
            if (switchObj2)
                panchorObj2.SetActive(panchorObj2ShouldBeSeen);

            // force update log once if switchObj1 or switchObj2
            if (switchObj1 || switchObj2)
                lastUpdateTimeTAP = -1;

            if (panchorObj1ShouldBeSeen || panchorObj2ShouldBeSeen)
            {
                if (Time.time - lastUpdateTimeTAP > 0.25f)
                {
                    StackTraceLogType stackTraceLogTypeOrigin = Application.GetStackTraceLogType(LogType.Log);
                    Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                    if (panchorObj1ShouldBeSeen)
                    {
                        var pos = panchorObj1.transform.localPosition;
                        var rot = panchorObj1.transform.localRotation.eulerAngles;
                        Debug.Log("AnchorTestHandle: panchorObj1 pos=" + pos.x + "," + pos.y + "," + pos.z);
                        Debug.Log("AnchorTestHandle: panchorObj1 rot=" + rot.x + "," + rot.y + "," + rot.z);
                    }

                    if (panchorObj2ShouldBeSeen)
                    {
                        var pos = panchorObj2.transform.localPosition;
                        var rot = panchorObj2.transform.localRotation.eulerAngles;
                        Debug.Log("AnchorTestHandle: panchorObj2 pos=" + pos.x + "," + pos.y + "," + pos.z);
                        Debug.Log("AnchorTestHandle: panchorObj2 rot=" + rot.x + "," + rot.y + "," + rot.z);
                    }
                    Application.SetStackTraceLogType(LogType.Log, stackTraceLogTypeOrigin);
                    lastUpdateTimeTAP = Time.time;
                }
            }
        }

        void PerformEditorTest()
        {
            if (testCreateAnchor1)
            {
                testCreateAnchor1 = false;
                OnCreateAnchor1();
            }

            if (testCreateAnchor2)
            {
                testCreateAnchor2 = false;
                OnCreateAnchor2();
            }

            if (testFollowAnchor1)
            {
                testFollowAnchor1 = false;
                OnFollowAnchor1();
            }

            if (testFollowAnchor2)
            {
                testFollowAnchor2 = false;
                OnFollowAnchor2();
            }

            if (testResetObj)
            {
                testResetObj = false;
                OnResetObj();
            }

            if (testClearSpatialAnchors)
            {
                testClearSpatialAnchors = false;
                OnClearAllAnchors();
            }

            if (testCreatePAC)
            {
                testCreatePAC = false;
                OnAcquirePersistedAnchorCollection();
            }

            if (testDestroyPAC)
            {
                testDestroyPAC = false;
                OnReleasePersistedAnchorCollection();
            }

            if (testPersistAnchor1)
            {
                testPersistAnchor1 = false;
                OnPersistAnchor1();
            }

            if (testPersistAnchor2)
            {
                testPersistAnchor2 = false;
                OnPersistAnchor2();
            }

            if (testExportAll)
            {
                testExportAll = false;
                OnExportAll();
            }

            if (testImportAll)
            {
                testImportAll = false;
                OnImportAll();
            }

            if (testClearExportedAnchors)
            {
                testClearExportedAnchors = false;
                OnClearExportedAnchors();
            }

            if (testUnpersistAnchor1)
            {
                testUnpersistAnchor1 = false;
                OnUnpersistAnchor1();
            }

            if (testUnpersistAnchor2)
            {
                testUnpersistAnchor2 = false;
                OnUnpersistAnchor2();
            }

            if (testClearPersistedAnchors)
            {
                testClearPersistedAnchors = false;
                OnClearPersistedAnchors();
            }

            if (testFloor)
            {
                testFloor = false;
                OnFloor();
            }

            if (testDevice)
            {
                testDevice = false;
                OnDevice();
            }

            if (testReloadScene)
            {
                testReloadScene = false;
                OnReloadScene();
            }
        }

        List<FutureTask<XrResult>> toRemovePA = new List<FutureTask<XrResult>>();
        List<FutureTask<(XrResult, AnchorManager.Anchor)>> toRemoveFPA = new List<FutureTask<(XrResult, AnchorManager.Anchor)>>();

        void UpdateTasks()
        {
            toRemovePA.Clear();
            // Check persist anchor tasks
            foreach (var taskTuple in tmPA.GetTasks())
            {
                var anchor = taskTuple.Item1;
                var task = taskTuple.Item2;
                if (!AnchorManager.GetSpatialAnchorName(anchor, out string name))
                    Debug.LogError("Faild to get anchor name: " + anchor);

                if (task.IsPollCompleted)
                {
                    toRemovePA.Add(task);
                    if (task.PollResult == XrResult.XR_SUCCESS)
                    {
                        var result = task.Complete();
                        if (result == XrResult.XR_SUCCESS)
                        {
                            Debug.Log("AnchorTestHandle: Persist anchor " + anchor + "=" + name + " success");
                        }
                        else
                        {
                            Debug.LogError("AnchorTestHandle: Persist anchor " + anchor + "=" + name + " failed: " + result);
                        }
                    }
                    else
                    {
                        Debug.LogError("AnchorTestHandle: Persist anchor " + anchor + "=" + name + " failed: " + task.PollResult);
                    }
                }
            }
            foreach (var task in toRemovePA)
            {
                tmPA.RemoveTask(task);
            }
            toRemovePA.Clear();

            // Check create from persisted anchor tasks
            toRemoveFPA.Clear();
            foreach (var taskTuple in tmFPA.GetTasks())
            {
                var paName = taskTuple.Item1;
                var task = taskTuple.Item2;
                if (task.IsPollCompleted)
                {
                    toRemoveFPA.Add(task);
                    if (task.PollResult == XrResult.XR_SUCCESS)
                    {
                        var result = task.Complete();
                        if (result.Item1 == XrResult.XR_SUCCESS)
                        {
                            Debug.Log("AnchorTestHandle: Create anchor from persisted anchor " + paName + " success");
                            if (paName == persistedAnchor1Name)
                            {
                                anchor1 = result.Item2;
                            }
                            else if (paName == persistedAnchor2Name)
                            {
                                anchor2 = result.Item2;
                            }
                        }
                        else
                        {
                            Debug.LogError("AnchorTestHandle: Create anchor from persisted anchor " + paName + " failed: " + result.Item1);
                            if (paName == persistedAnchor1Name)
                            {
                                anchor1FromPA = null;
                            }
                            else if (paName == persistedAnchor2Name)
                            {
                                anchor2FromPA = null;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("AnchorTestHandle: Create anchor from persisted anchor " + paName + " failed: " + task.PollResult);
                        if (paName == persistedAnchor1Name)
                        {
                            anchor1FromPA = null;
                        }
                        else if (paName == persistedAnchor2Name)
                        {
                            anchor2FromPA = null;
                        }
                    }
                }
            }
            foreach (var task in toRemoveFPA)
            {
                tmFPA.RemoveTask(task);
            }
            toRemoveFPA.Clear();
        }

        void Update()
        {
            PerformEditorTest();

            if (!isAnchorSupported)
            {
                UpdateUIInteractions();
                return;
            }

            UpdateTasks();

            // Check persisted files
            CheckExportedFiles();

            // Enumerate persisted anchors
            EnumeratePersistedAnchors();

            // Update anchors if persisted anchors are existed
            UpdateAnchorsIfPersistExist();

            // Update trackable anchors' pose
            UpdateTrackableAnchorsPose();

            // Update UI interactions
            UpdateUIInteractions();
        }

        public void OnFloor()
        {
            Debug.Log("AnchorTestHandle: OnFloor()");
            if (xrInputSubsystem == null)
            {
                Debug.LogError("xrInputSubsystem is null");
                statusOrigin.text = "xrInputSubsystem is null";
                return;
            }


            OnResetObj();
            OnClearAllAnchors();
            if (xrInputSubsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor))
            {
                statusOrigin.text = "Set tracking origin to floor. Reset Obj and anchors";
            }
            else
            {
                statusOrigin.text = "Fail to set tracking origin to floor";
            }
        }

        public void OnDevice()
        {
            Debug.Log("AnchorTestHandle: OnDevice()");
            if (xrInputSubsystem == null)
            {
                Debug.LogError("AnchorTestHandle: xrInputSubsystem is null");
                statusOrigin.text = "xrInputSubsystem is null";
                return;
            }


            OnResetObj();
            OnClearAllAnchors();
            if (xrInputSubsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device))
            {
                statusOrigin.text = "Set tracking origin to device. Reset Obj and anchors";
            }
            else
            {
                statusOrigin.text = "Fail to set tracking origin to device";
            }
        }

        public void OnReloadScene()
        {
            Debug.Log("AnchorTestHandle: OnReloadScene()");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
