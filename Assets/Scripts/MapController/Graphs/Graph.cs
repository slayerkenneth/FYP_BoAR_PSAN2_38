using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Graphs
{
    /// <summary>
    /// A class that handles all algorithms and data structures needed when using graphs.
    /// </summary>
    public class Graph
    {

        #region Instance Variables

        //vertices should have quick access through usage of a hashmap
        private Dictionary<Vertex, bool> _vertices;
        private Dictionary<Edge, bool> _edges;

        public Vertex[] Vertices
        {
            get
            {
                return _vertices.Keys.ToArray<Vertex>();
            }
        }
        public Edge[] Edges
        {
            get
            {
                return _edges.Keys.ToArray<Edge>();
            }
        }

        #endregion


        #region Constructors
        /// <summary>
        /// Creates an empty Graph.
        /// </summary>
        public Graph()
        {
            _vertices = new Dictionary<Vertex, bool>();
            _edges = new Dictionary<Edge, bool>();
        }

        public Graph(Vertex[] vertices, Edge[] edges)
        {
            _vertices = new Dictionary<Vertex, bool>();
            _edges = new Dictionary<Edge, bool>();

            foreach(Vertex v in vertices)
            {
                AddNewVertex(v);
            }
            foreach (Edge e in edges)
            {
                AddNewEdge(e);
            }
        }

        public Graph(Vector2[] vertices, Edge[] edges)
        {
            _vertices = new Dictionary<Vertex, bool>();
            _edges = new Dictionary<Edge, bool>();

            foreach(Vector2 v in vertices)
            {
                AddNewVertex(v);
            }
            foreach(Edge e in edges)
            {
                AddNewEdge(e);
            }
        }

        public Graph(Vertex[] vertices, Pair<Vector2>[] edges)
        {
            _vertices = new Dictionary<Vertex, bool>();
            _edges = new Dictionary<Edge, bool>();

            foreach(Vertex v in vertices)
            {
                AddNewVertex(v);
            }
            foreach(Pair<Vector2> e in edges)
            {
                AddNewEdge(e);
            }
        }

        public Graph(Vector2[] vertices, Pair<Vector2>[] edges)
        {
            _vertices = new Dictionary<Vertex, bool>();
            _edges = new Dictionary<Edge, bool>();

            foreach (Vector2 v in vertices)
            {
                AddNewVertex(v);
            }
            foreach (Pair<Vector2> e in edges)
            {
                AddNewEdge(e);
            }
        }

        #endregion


        #region Add/Remove-Methods

        /// <summary>
        /// Adds a new Vertex with the specified name.
        /// Returns false when Vertex already existed.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool AddNewVertex(Vector2 n)
        {
            return AddNewVertex(new Vertex(n));
        }

        /// <summary>
        /// Adds the specified Vertex to the Graph.
        /// Returns false when Vertex already existed.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool AddNewVertex(Vertex v)
        {
            if (!_vertices.ContainsKey(v))
            {
                _vertices.Add(v, true);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a new Edge between the two vertices defined by the strings.
        /// Returns false when edge already existed.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool AddNewEdge(Vector2 a, Vector2 b)
        {
            return AddNewEdge(new Pair<Vector2>(a, b));
        }

        /// <summary>
        /// Adds a new Edge between the two vertices defined by the string-pair.
        /// Returns false when edge already existed.
        /// </summary>
        /// <param name="stringpair"></param>
        /// <returns></returns>
        public bool AddNewEdge(Pair<Vector2> p)
        {
            return AddNewEdge(new Edge(p));
        }

        /// <summary>
        /// Adds the specified Edge to the Graph.
        /// Returns false when edge already existed.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool AddNewEdge(Edge newEdge)
        {
            if (!_edges.ContainsKey(newEdge))
            {
                Vertex firstV = newEdge.Vertices.First;
                Vertex lastV = newEdge.Vertices.Last;
                if (!_vertices.ContainsKey(firstV))
                {
                    AddNewVertex(firstV);
                }
                if (!_vertices.ContainsKey(lastV))
                {
                    AddNewVertex(lastV);
                }
                _edges.Add(newEdge, true);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the defined Edge from the Graph.
        /// Returns false when the Edge did not exist.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool RemoveEdge(Edge edge)
        {
            if (_edges.ContainsKey(edge))
            {
                _edges.Remove(edge);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the defined Vertex and all Edges containing it from the Graph.
        /// Returns false when the Vertex did not exist.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool RemoveVertex(Vertex vertex)
        {
            if (!_vertices.ContainsKey(vertex))
            {
                return false;
            }
            var temp = GraphHelper.FindAdjacentEdges(this, vertex);
            for (int i = 0; i < temp.Length; i++) {
                _edges.Remove(temp[i]);
            }
            _vertices.Remove(vertex);
            return true;
        }

        #endregion
        public void clear()
        {
            _edges.Clear();
            _vertices.Clear();
        }
    }
}
