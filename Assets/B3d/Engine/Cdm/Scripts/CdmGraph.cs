using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B3d.Engine.Cdm
{
   /// <summary>
   /// Graph data, format closely follows GraphML
   /// </summary>
   public abstract class CdmGraph
   {
      public string GraphId;
      public string EdgeDefault = "directed";
      protected List<CdmNode> _nodes;
      protected List<CdmEdge> _edges;

      public CdmGraph()
      {
         _nodes = new List<CdmNode>();
         _edges = new List<CdmEdge>();
      }

      public virtual CdmNode FindNodeById(string nodeId)
      {
         return _nodes.Find(c => c.NodeId == nodeId);
      }

      /// <summary>
      /// Find the node at the end of edge edgeId, that has node nodeId at the other end
      /// Returns null if nothing found
      /// </summary>
      /// <param name="nodeId">node at one end of edge edgeId</param>
      /// <param name="edgeId"></param>
      /// <returns>node at the end of edge edgeId, that has node nodeId at the other end</returns>
      public virtual CdmNode FindNodeAtEndOfEdge(string nodeId, string edgeId)
      {
         CdmEdge e = _edges.Find(c => (c.EdgeId == edgeId));
         if (e != null)
         {
            if (nodeId == e.NodeSourceId)
            {
               // return opposite end
               return FindNodeById(e.NodeTargetId);
            }
            else if (nodeId == e.NodeTargetId)
            {
               // return opposite end
               return FindNodeById(e.NodeSourceId);
            }
            else
            {
               return null;
            }
         }
         else
         {
            return null;
         }
      }

      public virtual CdmEdge FindEdgeById(string edgeId)
      {
         return _edges.Find(c => c.EdgeId == edgeId);
      }

      public virtual CdmEdge FindEdgeByNodeSourceAndTarget(string nodeIdSource, string nodeIdTarget)
      {
         // Edge can be stored in the Cdm either way around
         CdmEdge e1 = _edges.Find(c => (c.NodeSourceId == nodeIdSource && c.NodeTargetId == nodeIdTarget));
         if (e1 != null)
         {
            return e1;
         }
         else
         {
            CdmEdge e2 = _edges.Find(c => (c.NodeTargetId == nodeIdSource && c.NodeSourceId == nodeIdTarget));
            if (e2 != null)
            {
               return e2;
            }
            else
            {
               return null;
            }
         }
      }

      public virtual CdmEdge FindEdgeByNodeAndNumber(string nodeId, int edgeNumber)
      {
         // Edge can be stored in the Cdm either way around
         CdmEdge e1 = _edges.Find(c => (c.NodeSourceId == nodeId && c.EdgeNumberInSource == edgeNumber));
         if (e1 != null)
         {
            return e1;
         }
         else
         {
            CdmEdge e2 = _edges.Find(c => (c.NodeTargetId == nodeId && c.EdgeNumberInTarget == edgeNumber));
            if (e2 != null)
            {
               return e2;
            }
            else
            {
               return null;
            }
         }
      }

      public virtual void AddNode(CdmNode n)
      {
         // Only add if not already there
         CdmNode existing = FindNodeById(n.NodeId);
         if (existing == null)
         {
            _nodes.Add(n);
         }
      }

      public virtual void AddEdge(CdmEdge e)
      {
         // Only add if not already there
         CdmEdge existing = FindEdgeById(e.EdgeId);
         if (existing == null)
         {
            _edges.Add(e);
         }
      }

      public virtual void AddNodeRange(List<CdmNode> nodes)
      {
         foreach (CdmNode n in nodes)
         {
            AddNode(n);
         }
      }

      public virtual void AddEdgeRange(List<CdmEdge> edges)
      {
         foreach (CdmEdge e in edges)
         {
            AddEdge(e);
         }
      }

      public virtual List<CdmNode> GetAllNodes()
      {
         return _nodes;
      }

      public virtual List<CdmEdge> GetAllEdges()
      {
         return _edges;
      }

      public virtual void SetAllEdgesAndNodesAsSent()
      {
         foreach (CdmNode n in _nodes)
         {
            n.IsSentToView = true;
         }

         foreach (CdmEdge e in _edges)
         {
            e.IsSentToView = true;
         }
      }
   }
}
