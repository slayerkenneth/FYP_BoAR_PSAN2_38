using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Graphs
{
    /// <summary>
    /// Represents a single edge between two vertices inside a graph.
    /// </summary>
    public class Edge
    {
        private Pair<Vertex> _vertices;

        public Pair<Vertex> Vertices
        {
            get
            {
                return _vertices;
            }
            set
            {
                _vertices = Vertices;
            }
        }

        public Edge(Vertex a, Vertex b)
        {
            _vertices = new Pair<Vertex>(a, b);
        }
        public Edge(Pair<Vector2> p)
        {
            _vertices = new Pair<Vertex>(new Vertex(p.First), new Vertex(p.Last));
        }

        #region Overrides
        public override string ToString()
        {
            return _vertices.First + " - " + _vertices.Last;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is Edge))
            {
                return false;
            }
            
            return (_vertices.First.Equals(((Edge)obj)._vertices.First) && _vertices.Last.Equals(((Edge)obj)._vertices.Last))||(_vertices.First.Equals(((Edge)obj)._vertices.Last) && _vertices.Last.Equals(((Edge)obj)._vertices.First));
        }

        public override int GetHashCode()
        {
            return _vertices.First.GetHashCode() * 23 + _vertices.Last.GetHashCode();
        }
        #endregion
    }
}
