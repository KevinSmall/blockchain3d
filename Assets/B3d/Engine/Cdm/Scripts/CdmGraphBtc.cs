// Blockchain 3D and VR Explorer: Blockchain Technology Visualization
// Copyright (C) 2018 Kevin Small email:contactweb@blockchain3d.info
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

using B3d.Tools;

namespace B3d.Engine.Cdm
{
   /// <summary>
   /// Bitcoin specific graph data handling. It is in this class the BTC specific overrides (eg for merging Edges) are handled.
   /// </summary>
   public class CdmGraphBtc : CdmGraph
   {
      /// <summary>
      /// Adding an edge to a Btc graph can do the following if the edge already exists:
      ///   1) not add the new edge, instead upgrade existing edge to type mixed ( == input and output)
      ///   2) not add the new edge, instead update the existing edge's edge counts if any are zero (means unknown)
      /// </summary>
      public override void AddEdge(CdmEdge e)
      {
         #region Data Quality Checks
         CdmEdgeBtc ebNew = e as CdmEdgeBtc;
         if (ebNew == null)
         {
            Msg.LogError("CdmGraphBtc.AddEdge cannot cast edge to CdmEdgeBtc");
            return;
         }

         // Check edge has been handed to us in correct format, that is Transaction in the source slot
         CdmNode n = FindNodeAtEndOfEdge(ebNew.NodeTargetId, ebNew.EdgeId);
         if (n != null)
         {
            // Above may seem a bit lax, but we dont know order things will be added to the graph, maybe node is not known yet
            if (n.NodeType != NodeType.Tx)
            {
               Msg.LogError("CdmGraphBtc.AddEdge all edges must have a transaction as their source");
               return;
            }
         }
         #endregion

         CdmEdgeBtc ebExisting = FindEdgeByNodeSourceAndTarget(ebNew.NodeSourceId, ebNew.NodeTargetId) as CdmEdgeBtc;
         if (ebExisting == null)
         {
            // New edge, just add to list
            _edges.Add(ebNew);
         }
         else
         {
            // Edge exists in graph already, upgrade it to type mixed if needed
            MergeExistingEdge(ebNew, ebExisting);
         }
      }

      /// <summary>
      /// Merge data from incoming edge ebNew into existing edge ebExisting, updating the existing edge and changing its type to mixed if needed.
      /// </summary>
      private void MergeExistingEdge(CdmEdgeBtc ebNew, CdmEdgeBtc ebExisting)
      {
         // Edge types. Anything can override Unknown, otherwise merging two different types promotes to Mixed
         if (ebNew.EdgeType != ebExisting.EdgeType)
         {
            if (ebNew.EdgeType == EdgeType.Unknown)
            {
               // no change to existing edge type
            }
            else
            {
               // New edge type is known 
               if (ebExisting.EdgeType == EdgeType.Unknown)
               {
                  ebExisting.EdgeType = ebNew.EdgeType;
               }
               else
               {
                  // Both existing and new edge types are known
                  if (ebExisting.EdgeType == ebNew.EdgeType)
                  {
                     // they are the same anyway, no change
                  }
                  else
                  {
                     ebExisting.EdgeType = EdgeType.Mixed;
                  }
               }
            }
         }

         // Edge values for mixed edges
         float fsmall = 0.000001f;
         if (ebExisting.EdgeType == EdgeType.Mixed)
         {
            if (ebExisting.ValueInSource <= fsmall && ebNew.ValueInSource > fsmall)
            {
               ebExisting.ValueInSource = ebNew.ValueInSource;
            }

            if (ebExisting.ValueInTarget <= fsmall && ebNew.ValueInTarget > fsmall)
            {
               ebExisting.ValueInTarget = ebNew.ValueInTarget;
            }
         }

         // Edge exists in graph already, check to see if we have more info about edge numbering
         // Source Edge Number
         if (ebExisting.EdgeNumberInSource == 0)
         {
            if (ebNew.NodeSourceId == ebExisting.NodeSourceId)
            {
               // edge in graph already is stored same way around as new edge, so source node in graph == source node in new
               if (ebNew.EdgeNumberInSource > 0)
               {
                  ebExisting.EdgeNumberInSource = ebNew.EdgeNumberInSource;
               }
            }
            else
            {
               // edge in graph already is stored other way around as new edge, so source node in graph == target node in new
               if (ebNew.EdgeNumberInTarget > 0)
               {
                  ebExisting.EdgeNumberInSource = ebNew.EdgeNumberInTarget;
               }
            }
         }

         // Target Edge Number
         if (ebExisting.EdgeNumberInTarget == 0)
         {
            if (ebNew.NodeSourceId == ebExisting.NodeSourceId)
            {
               // edge in graph already is stored same way around as new edge, so source node in graph == source node in new
               if (ebNew.EdgeNumberInTarget > 0)
               {
                  ebExisting.EdgeNumberInTarget = ebNew.EdgeNumberInTarget;
               }
            }
            else
            {
               // edge in graph already is stored other way around as new edge, so source node in graph == target node in new
               if (ebNew.EdgeNumberInSource > 0)
               {
                  ebExisting.EdgeNumberInTarget = ebNew.EdgeNumberInSource;
               }
            }
         }
      }
   }
}
