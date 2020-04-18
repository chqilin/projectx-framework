using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace ProjectX
{
    public class XUtility
    {
        #region Unity Generic Utilities
		public static string PlatformString
		{
			get
			{
				switch (Application.platform) 
				{
				case RuntimePlatform.Android: 
					return "Android";
				case RuntimePlatform.IPhonePlayer: 
					return "iOS";
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.OSXPlayer:
					return "macOS";
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.WindowsPlayer:
					return "Windows";
				default:
					return "Unknown";
				}
			}
		}

        public static string ApplicationDataPath(string path)
        {
            return Path.Combine(Application.dataPath, path);
        }

        public static string StreamingAssetsPath(string path)
        {
            return Path.Combine(Application.streamingAssetsPath, path);
        }

        public static string PersistentDataPath(string path)
        {
            return Path.Combine(Application.persistentDataPath, path);
        }

        public static string TemporaryPath(string path)
        {
            return Path.Combine(Application.temporaryCachePath, path);
        }

        [System.Obsolete("This method has problem.")]
        public static string ReadTextFromStreamingAssets(string location)
        {
            Debug.Log("Read text from StreamingAssets : " + location);
            location = string.Format("{0}/{1}", Application.streamingAssetsPath, location);
            if (Application.platform == RuntimePlatform.Android)
            {
                WWW www = new WWW(location);
                while (!www.isDone) ;
                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.LogError("Read text from StreamingAssets failed. location=" + location);
                    Debug.LogError(www.error);
                    return "";
                }
                return www.text;
            }

            return XFile.ReadTextFile(location);
        }

        [System.Obsolete("This method has problem.")]
        public static byte[] ReadBytesFromStreamingAssets(string location)
        {
            Debug.Log("Read bytes from StreamingAssets : " + location);
            location = string.Format("{0}/{1}", Application.streamingAssetsPath, location);
            if (Application.platform == RuntimePlatform.Android)
            {
                WWW www = new WWW(location);
                while (!www.isDone) ;
                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.LogError("Read bytes from StreamingAssets failed. location=" + location);
                    Debug.LogError(www.error);
                    return null;
                }
                return www.bytes;
            }

            return XFile.ReadBytesFile(location);
        }
        #endregion

        #region Unity-Hierarchy Utilities
        public static void AttachGameObject(GameObject target, GameObject me, bool normalizeLocalT = true, bool normalizeLocalR = true, bool normalizeLocalS = true)
        {
            if (me == null)
                return;
            me.transform.parent = target != null ? target.transform : null;
            if (normalizeLocalT)
                me.transform.localPosition = Vector3.zero;
            if (normalizeLocalR)
                me.transform.localRotation = Quaternion.identity;
            if (normalizeLocalS)
                me.transform.localScale = Vector3.one;
        }

        public static void DestroyChildren(GameObject target)
        {
            if (target == null)
                return;
            foreach (Transform t in target.transform)
            {
                Object.Destroy(t.gameObject);
            }
        }

        public static void ForeachGameObjectRecursively(GameObject root, System.Action<GameObject> func)
        {
            if (root == null || func == null)
                return;
            func(root);
            foreach (Transform t in root.transform)
            {
                XUtility.ForeachGameObjectRecursively(t.gameObject, func);
            }
        }

        public static void ForeachComponentRecursively<T>(GameObject root, System.Action<T> func) where T : Component
        {
            if (root == null || func == null)
                return;
            T[] components = root.GetComponents<T>();
            foreach (var c in components)
            {
                func(c);
            }
            foreach (Transform t in root.transform)
            {
                XUtility.ForeachComponentRecursively(t.gameObject, func);
            }
        }

        public static void SetLayerRecursively(GameObject root, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            XUtility.ForeachGameObjectRecursively(root, go => go.layer = layer);
        }

        public static void SetLayerRecursively<T>(GameObject root, string layerName) where T : Component
        {
            int layer = LayerMask.NameToLayer(layerName);
            XUtility.ForeachComponentRecursively<T>(root, c => c.gameObject.layer = layer);
        }

        public static void SetRendererEnabledRecursively(GameObject root, bool enabled)
        {
            XUtility.ForeachComponentRecursively<Renderer>(root, c => c.enabled = enabled);
        }

        public static void SetColliderEnabledRecursively(GameObject root, bool enabled)
        {
            XUtility.ForeachComponentRecursively<Collider>(root, c => c.enabled = enabled);
        }

        public static void SetRenderQueueRecursively(GameObject root, int renderQueue)
        {
            XUtility.ForeachComponentRecursively<Renderer>(root,
                r =>
                {
                    foreach (Material m in r.materials)
                    {
                        if (m == null)
                            continue;
                        m.renderQueue = renderQueue;
                    }
                });
        }
        public static void ResetRenderQueueRecursively(GameObject root)
        {
            XUtility.ForeachComponentRecursively<Renderer>(root,
                r =>
                {
                    foreach (Material m in r.materials)
                    {
                        if (m == null)
                            continue;
                        m.renderQueue = m.shader.renderQueue;
                    }
                });
        }

        public static GameObject FindGameObjectInChildren(GameObject root, int instanceId)
        {
            if (root == null)
                return null;
            foreach (Transform t in root.transform)
            {
                if (t.gameObject.GetInstanceID() == instanceId)
                    return t.gameObject;
            }
            return null;
        }

        public static GameObject FindGameObjectInChildren(GameObject root, string name)
        {
            if (root == null)
                return null;
            foreach (Transform t in root.transform)
            {
                if (t.name == name)
                    return t.gameObject;
            }
            return null;
        }

        public static GameObject FindGameObjectRecursively(GameObject root, int instanceId)
        {
            if (root == null)
                return null;
            if (root.GetInstanceID() == instanceId)
                return root;
            GameObject result = null;
            foreach (Transform t in root.transform)
            {
                result = XUtility.FindGameObjectRecursively(t.gameObject, instanceId);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static GameObject FindGameObjectRecursively(GameObject root, string name)
        {
            if (root == null)
                return null;
            if (root.name == name)
                return root;
            GameObject result = null;
            foreach (Transform t in root.transform)
            {
                result = XUtility.FindGameObjectRecursively(t.gameObject, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static GameObject FindOrCreateGameObject(GameObject parent, string name)
        {
            GameObject result = null;
            if (parent == null)
            {
                result = GameObject.Find(name);
                if (result == null)
                {
                    result = new GameObject(name);
                }
            }
            else
            {
                result = XUtility.FindGameObjectInChildren(parent, name);
                if (result == null)
                {
                    result = new GameObject(name);
                    XUtility.AttachGameObject(parent, result);
                }
            }
            return result;
        }

        public static T FindOrCreateComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }
        public static T FindOrCreateComponent<T>(Component thisComp) where T : Component
        {
            T component = thisComp.GetComponent<T>();
            if (component == null)
            {
                component = thisComp.gameObject.AddComponent<T>();
            }
            return component;
        }

        public static void CopyComponent<T>(T target, T source) where T : Component
        {
            System.Type type = target.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

            try
            {
                PropertyInfo[] pinfos = type.GetProperties(flags);
                foreach (var pinfo in pinfos)
                {
                    if (!pinfo.CanWrite)
                        continue;
                    var value = pinfo.GetValue(source, null);
                    pinfo.SetValue(target, value, null);
                }

                FieldInfo[] finfos = type.GetFields(flags);
                foreach (var finfo in finfos)
                {
                    var value = finfo.GetValue(source);
                    finfo.SetValue(target, value);
                }
            }
            catch (System.NotImplementedException e)
            {
                // NotImplementedException means the property neednot be copied.
            }
            catch
            {
                throw new System.Exception("CopyComponentException: Component<" + type.Name + "> can not be copied.");
            }
        }
        #endregion

        #region Unity-Graphics Utilities
        public static Mesh CreateMesh(Vector3[] vertices, int[] triangles, Vector2[] uv, Vector3[] normals)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.normals = normals;
            return mesh;
        }

        public static List<int> Subdivision(Mesh mesh)
        {
            if (mesh == null || mesh.vertices == null || mesh.triangles == null)
                return null;

            List<Vector3> verts = new List<Vector3>(mesh.vertices);
            List<int> faces = new List<int>(mesh.triangles);
            List<int> lastLOD = XUtility.Subdivision(verts, faces);
            mesh.vertices = verts.ToArray();
            mesh.triangles = faces.ToArray();
            return lastLOD;
        }

        //      v1
        //       *
        //      / \
        //     /   \
        //  m0*-----*m1
        //   / \   / \
        //  /   \ /   \
        // *-----*-----*
        // v0    m2     v2
        public static List<int> Subdivision(List<Vector3> verts, List<int> faces)
        {
            int iCount = faces.Count;
            int fCount = faces.Count / 3;
            for (int f = 0; f < fCount; f++)
            {
                int iv0 = faces[f * 3 + 0];
                int iv1 = faces[f * 3 + 1];
                int iv2 = faces[f * 3 + 2];
                Vector3 v0 = verts[iv0];
                Vector3 v1 = verts[iv1];
                Vector3 v2 = verts[iv2];

                Vector3 m0 = (v0 + v1) * 0.5f;
                Vector3 m1 = (v1 + v2) * 0.5f;
                Vector3 m2 = (v0 + v2) * 0.5f;

                int vCount = verts.Count;
                int vIndex = 0;

                int im0 = vCount + vIndex;
                var in0 = XUtility.GetNearestVertices(verts, m0, 0.1f);
                if (in0.Count == 0)
                {
                    verts.Add(m0);
                    vIndex++;
                }
                else
                {
                    im0 = in0[0];
                }

                int im1 = vCount + vIndex;
                var in1 = XUtility.GetNearestVertices(verts, m1, 0.1f);
                if (in1.Count == 0)
                {
                    verts.Add(m1);
                    vIndex++;
                }
                else
                {
                    im1 = in1[0];
                }

                int im2 = vCount + vIndex;
                var in2 = XUtility.GetNearestVertices(verts, m2, 0.1f);
                if (in2.Count == 0)
                {
                    verts.Add(m2);
                    vIndex++;
                }
                else
                {
                    im2 = in2[0];
                }

                faces.Add(iv0); faces.Add(im0); faces.Add(im2);
                faces.Add(iv1); faces.Add(im1); faces.Add(im0);
                faces.Add(iv2); faces.Add(im2); faces.Add(im1);
                faces.Add(im0); faces.Add(im1); faces.Add(im2);
            }
            List<int> lastLOD = faces.GetRange(0, iCount);
            faces.RemoveRange(0, iCount);
            return lastLOD;
        }

        public static List<int> GetNearestVertices(List<Vector3> verts, Vector3 pos, float range)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < verts.Count; i++)
            {
                Vector3 v = verts[i];
                if ((pos - v).sqrMagnitude <= range * range)
                {
                    result.Add(i);
                }
            }
            return result;
        }

        public static Rect ComputeTextureCoordByArea(Texture2D image, Rect area)
        {
            Rect coord = new Rect(0, 0, 0, 0);
            coord.x = area.x / image.width;
            coord.y = 1.0f - (area.y + area.height) / image.height;
            coord.width = area.width / image.width;
            coord.height = area.height / image.height;
            return coord;
        }

        public static bool IsInCameraViewField(Camera camera, Bounds bounds)
        {
            if (camera == null)
                return false;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }
        public static bool IsInCameraXField(Camera camera, Bounds bounds)
        {
            if (camera == null)
                return false;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            Plane l = default(Plane);
            Plane r = default(Plane);
            foreach (var p in planes)
            {
                Vector3 pos = -p.normal * p.distance;
                if (pos.x < 0) l = p;
                if (pos.x > 0) r = p;
            }
            Plane[] vplanes = new Plane[2] { l, r };
            return GeometryUtility.TestPlanesAABB(vplanes, bounds);
        }
        public static bool IsInCameraYField(Camera camera, Bounds bounds)
        {
            if (camera == null)
                return false;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            Plane t = default(Plane);
            Plane b = default(Plane);
            foreach (var p in planes)
            {
                Vector3 pos = -p.normal * p.distance;
                if (pos.y < 0) b = p;
                if (pos.y > 0) t = p;
            }
            Plane[] vplanes = new Plane[2] { t, b };
            return GeometryUtility.TestPlanesAABB(vplanes, bounds);
        }
        public static bool IsInCameraZField(Camera camera, Bounds bounds)
        {
            if (camera == null)
                return false;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            Plane n = default(Plane);
            Plane f = default(Plane);
            foreach (var p in planes)
            {
                Vector3 pos = -p.normal * p.distance;
                if (pos.x == 0 && pos.y == 0)
                {
                    if (pos.z > camera.nearClipPlane)
                        f = p;
                    else
                        n = p;
                }
            }
            Plane[] vplanes = new Plane[2] { n, f };
            return GeometryUtility.TestPlanesAABB(vplanes, bounds);
        }

        /// <summary>
        /// Combine skinned-mesh-renderer src to dst.
        /// </summary>
        public static void CombineSkinnedMeshRenderer(SkinnedMeshRenderer result, List<Transform> skeleton, List<SkinnedMeshRenderer> parts)
        {
            List<CombineInstance> combines = new List<CombineInstance>();
            List<Transform> bones = new List<Transform>();
            List<Material> materials = new List<Material>();

            if (parts != null && parts.Count > 0)
            {
                foreach (var p in parts)
                {
                    if (p == null)
                    {
                        Debug.LogError("combination-part for [" + result.name + "] is null.");
                        continue;
                    }
                    if (p.sharedMesh == null)
                    {
                        Debug.LogError("mesh of combination-part [" + p.name + "] in [" + p.transform.root.name + "] for [" + result.name + "] is null.");
                        continue;
                    }
                    if (p.bones == null)
                    {
                        Debug.LogError("bones of combination-part [" + p.name + "] in [" + p.transform.root.name + "] for [" + result.name + "] is null.");
                        continue;
                    }

                    for (int i = 0; i < p.sharedMesh.subMeshCount; i++)
                    {
                        CombineInstance ci = new CombineInstance();
                        ci.mesh = p.sharedMesh;
                        ci.subMeshIndex = i;
                        combines.Add(ci);
                    }

                    foreach (var b in p.bones)
                    {
                        bool found = false;
                        foreach (var t in skeleton)
                        {
                            if (b.name == t.name)
                            {
                                found = true;
                                bones.Add(t);
                                break;
                            }
                        }
                        if (!found)
                        {
                            Debug.LogError("bone named [" + b.name + "] not found in skeleton.");
                        }
                    }

                    materials.AddRange(p.sharedMaterials);
                }
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combines.ToArray(), false, false);
            mesh.RecalculateBounds();
            result.sharedMesh = mesh;
            result.bones = bones.ToArray();
            result.materials = materials.ToArray();
        }
        #endregion

        #region Mecanim Utilities
        public static bool IsAnimatorPlayingClip(Animator animator, AnimationClip clip, int layerIndex)
        {
            var clipInfos = animator.GetCurrentAnimatorClipInfo(layerIndex);
            for (int i = 0; i < clipInfos.Length; i++)
            {
                var ci = clipInfos[i];
                if (ci.clip == clip)
                    return true;
            }
            return false;
        }

        public static bool IsAnimatorPlayingClip(Animator animator, string clipName, int layerIndex)
        {
            var clipInfos = animator.GetCurrentAnimatorClipInfo(layerIndex);
            for (int i = 0; i < clipInfos.Length; i++)
            {
                var ci = clipInfos[i];
                if (ci.clip.name == clipName)
                    return true;
            }
            return false;
        } 
        #endregion

        #region Unity-Physics Utilities
        public static bool Raycast(Ray ray, out RaycastHit hit, params string[] layers)
        {
            if (layers.Length == 0)
                return Physics.Raycast(ray, out hit);

            int mask = 0;
            foreach (string name in layers)
            {
                mask |= (1 << LayerMask.NameToLayer(name));
            }
            return Physics.Raycast(ray, out hit, float.MaxValue, mask);
        }

        public static Vector3 ScreenToPlanePoint(Camera camera, Plane plane, Vector3 point)
        {
            Ray ray = camera.ScreenPointToRay(point);
            float dst = 0;
            if (plane.Raycast(ray, out dst))
                return ray.GetPoint(dst);
            else
                return ray.GetPoint(float.MaxValue);
        }

        public static List<Vector3> ScreenToPlaneArea(Camera camera, Plane plane, bool containsCameraPos)
        {
            List<Vector3> result = new List<Vector3>();
            if (containsCameraPos)
            {
                Vector3 pos = camera.transform.position;
                pos.y = 0;
                result.Add(pos);
            }

            Vector3 v1 = Vector3.zero;
            Vector3 v2 = Vector3.zero; v2.Set(Screen.width, 0, 0);
            Vector3 v3 = Vector3.zero; v3.Set(0, Screen.height, 0);
            Vector3 v4 = Vector3.zero; v4.Set(Screen.width, Screen.height, 0);

            Vector3 p1 = XUtility.ScreenToPlanePoint(camera, plane, v1);
            Vector3 p2 = XUtility.ScreenToPlanePoint(camera, plane, v2);
            Vector3 p3 = XUtility.ScreenToPlanePoint(camera, plane, v3);
            Vector3 p4 = XUtility.ScreenToPlanePoint(camera, plane, v4);

            result.Add(p1);
            result.Add(p2);
            result.Add(p3);
            result.Add(p4);

            return result;
        }

        public static List<Vector3> ScreenToGroundArea(Camera camera)
        {
            Plane ground = new Plane(Vector3.up, Vector3.zero);
            return XUtility.ScreenToPlaneArea(camera, ground, true);
        }

        public static Rect ScreenToGroundAreaAlignedAxis(Camera camera)
        {
            Rect rect = new Rect(0, 0, 0, 0);
            var list = XUtility.ScreenToGroundArea(camera);
            foreach (var p in list)
            {
                if (p.x < rect.xMin) rect.xMin = p.x;
                if (p.x > rect.xMax) rect.xMax = p.x;
                if (p.z < rect.yMin) rect.yMin = p.z;
                if (p.z > rect.yMax) rect.yMax = p.z;
            }
            return rect;
        }

        public static Bounds ComputeBounds(params Vector3[] pointList)
        {
            Bounds bounds = new Bounds();
            bounds.center = pointList != null && pointList.Length > 0 ? pointList[0] : Vector3.zero;
            bounds.extents = Vector3.zero;
            foreach (var p in pointList)
            {
                bounds.Encapsulate(p);
            }
            return bounds;
        }

        public static Bounds ComputeBounds(params Bounds[] boundsList)
        {
            Bounds bounds = new Bounds();
            bounds.center = boundsList != null && boundsList.Length > 0 ? boundsList[0].center : Vector3.zero;
            bounds.extents = Vector3.zero;
            foreach (var b in boundsList)
            {
                bounds.Encapsulate(b);
            }
            return bounds;
        }
        #endregion

        #region Unity Color String
        /// <summary>
        /// Convert string to color, support alpha channel.
        /// </summary>
        /// <param name="colorString">a hex color string like #FFFFFF</param>
        /// <returns></returns>
        public static Color StringToColor(string str)
        {
            str = str.Replace("#", "");
            if (str.Length < 6)
                return Color.black;

            string strR = str.Substring(0, 2);
            string strG = str.Substring(2, 2);
            string strB = str.Substring(4, 2);
            string strA = str.Length >= 8 ? str.Substring(6, 2) : "FF";

            float r = System.Convert.ToInt32(strR, 16);
            float g = System.Convert.ToInt32(strG, 16);
            float b = System.Convert.ToInt32(strB, 16);
            float a = System.Convert.ToInt32(strA, 16);

            Color color = Color.black;
            color.r = r / 255;
            color.g = g / 255;
            color.b = b / 255;
            color.a = a / 255;
            return color;
        } 

        public static string ColorToString(Color color)
        {
            int r = (int)(color.r * 255);
            int g = (int)(color.g * 255);
            int b = (int)(color.b * 255);
            int a = (int)(color.a * 255);

            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", r, g, b, a);
        }
        #endregion

        #region Unity-Other Utilities
        public static string AddCloneMarkForName(string name)
        {
            if (name.EndsWith("(Clone)"))
                return name;
            return name + "(Clone)";
        }

        public static string CutCloneMarkForName(string name)
        {
            if (name.EndsWith("(Clone)"))
            {
                name = name.Substring(0, name.Length - 7);
            }
            name = name.Trim();
            return name;
        }
        #endregion
    }
}
