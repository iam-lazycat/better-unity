using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public static class AlignToGroundModule
    {
        public static void AlignSelected()
        {
            var selection = Selection.gameObjects;
            if (selection == null || selection.Length == 0)
            {
                Debug.LogWarning("[Better Unity] Align To Terrain: nothing selected.");
                return;
            }

            Undo.SetCurrentGroupName("Align To Terrain");
            int group   = Undo.GetCurrentGroup();
            int success = 0;

            foreach (var go in selection)
                if (AlignObject(go)) success++;

            Undo.CollapseUndoOperations(group);
            Debug.Log($"[Better Unity] Aligned {success}/{selection.Length} object(s).");
        }

        static bool AlignObject(GameObject go)
        {
            int samples = BetterUnityPrefs.AlignSamplesPerAxis;
            bool alignRotation = BetterUnityPrefs.AlignRotationToNormal;
            float minNormalDot = BetterUnityPrefs.AlignMinNormalDot;
            LayerMask groundLayers = BetterUnityPrefs.AlignGroundLayers;
            const float startOffset = 500f;

            // collect all colliders that belong to this object so we can ignore them
            var selfColliders = new HashSet<Collider>(go.GetComponentsInChildren<Collider>(true));

            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return AlignByPoint(go, go.transform.position, alignRotation, groundLayers, startOffset, selfColliders);

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            float minX = bounds.min.x, maxX = bounds.max.x;
            float minZ = bounds.min.z, maxZ = bounds.max.z;
            float bottomY = bounds.min.y;
            float startY = bounds.max.y + startOffset;

            var hitPoints = new List<Vector3>();
            var hitNormals = new List<Vector3>();

            for (int xi = 0; xi < samples; xi++)
            {
                float tx = samples == 1 ? 0.5f : (float)xi / (samples - 1);
                float x = Mathf.Lerp(minX, maxX, tx);

                for (int zi = 0; zi < samples; zi++)
                {
                    float tz = samples == 1 ? 0.5f : (float)zi / (samples - 1);
                    float z = Mathf.Lerp(minZ, maxZ, tz);

                    var ray = new Ray(new Vector3(x, startY, z), Vector3.down);

                    // use RaycastAll and pick the first hit that isn't self
                    var allHits = Physics.RaycastAll(ray, Mathf.Infinity, groundLayers);
                    System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

                    foreach (var hit in allHits)
                    {
                        if (selfColliders.Contains(hit.collider)) continue;
                        if (Vector3.Dot(hit.normal, Vector3.up) < minNormalDot) continue;

                        hitPoints.Add(hit.point);
                        hitNormals.Add(hit.normal);
                        break; // take the closest valid hit only
                    }
                }
            }

            if (hitPoints.Count == 0)
            {
                Debug.LogWarning($"[Better Unity] Align To Ground: no ground found under \"{go.name}\".");
                return false;
            }

            var avgNormal = Vector3.zero;
            float lowestY = float.MaxValue;
            foreach (var p in hitPoints) if (p.y < lowestY) lowestY = p.y;
            foreach (var n in hitNormals) avgNormal += n;
            avgNormal = avgNormal.normalized;

            float pivotToBottom = go.transform.position.y - bottomY;

            Undo.RecordObject(go.transform, "Align To Ground");

            var pos = go.transform.position;
            pos.y = lowestY + pivotToBottom;
            go.transform.position = pos;

            if (alignRotation)
            {
                var fwd = Vector3.ProjectOnPlane(go.transform.forward, avgNormal).normalized;
                if (fwd == Vector3.zero)
                    fwd = Vector3.ProjectOnPlane(go.transform.right, avgNormal).normalized;
                go.transform.rotation = Quaternion.LookRotation(fwd, avgNormal);
            }

            return true;
        }

        static bool AlignByPoint(GameObject go, Vector3 pivot, bool alignRotation, LayerMask layers, float offset, HashSet<Collider> selfColliders)
        {
            var ray = new Ray(new Vector3(pivot.x, pivot.y + offset, pivot.z), Vector3.down);

            var allHits = Physics.RaycastAll(ray, Mathf.Infinity, layers);
            System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

            RaycastHit? validHit = null;
            foreach (var hit in allHits)
            {
                if (selfColliders.Contains(hit.collider)) continue;
                validHit = hit;
                break;
            }

            if (!validHit.HasValue)
            {
                Debug.LogWarning($"[Better Unity] Align To Ground: no ground found under \"{go.name}\".");
                return false;
            }

            Undo.RecordObject(go.transform, "Align To Ground");
            var p = go.transform.position;
            p.y = validHit.Value.point.y;
            go.transform.position = p;

            if (alignRotation)
            {
                var fwd = Vector3.ProjectOnPlane(go.transform.forward, validHit.Value.normal).normalized;
                if (fwd != Vector3.zero)
                    go.transform.rotation = Quaternion.LookRotation(fwd, validHit.Value.normal);
            }

            return true;
        }

        static bool AlignByPoint(GameObject go, Vector3 pivot, bool alignRotation, LayerMask layers, float offset)
        {
            var ray = new Ray(new Vector3(pivot.x, pivot.y + offset, pivot.z), Vector3.down);
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layers))
            {
                Debug.LogWarning($"[Better Unity] Align To Terrain: no ground found under \"{go.name}\".");
                return false;
            }

            Undo.RecordObject(go.transform, "Align To Terrain");
            var p = go.transform.position;
            p.y = hit.point.y;
            go.transform.position = p;

            if (alignRotation)
            {
                var fwd = Vector3.ProjectOnPlane(go.transform.forward, hit.normal).normalized;
                if (fwd != Vector3.zero)
                    go.transform.rotation = Quaternion.LookRotation(fwd, hit.normal);
            }

            return true;
        }
    }
}
