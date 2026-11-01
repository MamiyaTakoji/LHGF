using System.Collections.Generic;
using UnityEngine;


namespace TexDrawLib
{
    public class FillHelper : IFlushable
    {
        // The specs that used in TEXDraw are ...
        public List<Vector3> m_Positions = new List<Vector3>(); // Character (verts) position (XYZ)
        public List<Color32> m_Colors = new List<Color32>();    // Character colors (RGBA)
        public List<Vector2> m_Uv0S = new List<Vector2>();      // primary map to Font Texture (UV1)
        public List<Vector2> m_Uv1S = new List<Vector2>();      // Additional metadata (UV2)
        public List<int> m_Indicies = new List<int>(); // Usual triangle list data (List of submeshes)
        public int m_Font;

        public void Clear()
        {
            m_Positions.Clear();
            m_Colors.Clear();
            m_Uv0S.Clear();
            m_Uv1S.Clear();
            m_Indicies.Clear();
            m_Font = 0;
        }

        public void FillMesh(Mesh mesh, bool normals)
        {
            mesh.Clear();

            if (m_Positions.Count >= 65000)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            else
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;

            mesh.SetVertices(m_Positions);
            if (m_Colors.Count == m_Positions.Count)
                mesh.SetColors(m_Colors);
            if (m_Uv0S.Count == m_Positions.Count)
                mesh.SetUVs(0, m_Uv0S);
            if (m_Uv1S.Count == m_Positions.Count)
                mesh.SetUVs(1, m_Uv1S);
            mesh.SetTriangles(m_Indicies, 0, true);

            if (normals)
            {
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
            }
        }

        public void AddVert(Vector3 position, Color32 color, Vector2 uv0)
        {
            m_Positions.Add(position);
            m_Colors.Add(color);
            m_Uv0S.Add(uv0);
        }

        public void AddTriangle(int idx0, int idx1, int idx2)
        {
            m_Indicies.Add(idx0);
            m_Indicies.Add(idx1);
            m_Indicies.Add(idx2);
        }

        public void AddQuad()
        {
            var n = m_Positions.Count;
            AddTriangle(n - 2, n - 3, n - 4);
            AddTriangle(n - 1, n - 2, n - 4);
        }

        public static FillHelper Get()
        {
            return ObjPool<FillHelper>.Get();
        }

        public void Flush()
        {
            Clear();
            ObjPool<FillHelper>.Release(this);
        }

        public bool IsFlushed { get; set; }
    }
}
