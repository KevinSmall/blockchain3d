using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace B3d.Engine.Cdm
{
   /// <summary>
   /// Edge data, format closely follows GraphML
   /// </summary>
   //[DebuggerDisplay("EdgeId = {EdgeId}")]
   [DebuggerDisplay("{DebuggerDisplay,nq}")]
   [Serializable]
   public class CdmEdge
   {
      /// <summary>
      /// Edge ID. Must be globally unique
      /// </summary>
      public string EdgeId;
      /// <summary>
      /// Friendly edge name, doesnt have to be globally unique
      /// </summary>
      public string EdgeIdFriendly;

      public EdgeType EdgeType;

      public string NodeSourceId;
      public string NodeTargetId;
 
      /// <summary>
      /// Edge number as counted from source node's view, used in paging, zero means not yet known.
      /// </summary>
      public int EdgeNumberInSource;
      /// <summary>
      /// Edge number as counted from target node's view, used in paging, zero means not yet known.
      /// </summary>
      public int EdgeNumberInTarget;
     
      /// <summary>
      /// A value associated with the edge
      /// </summary> 
      public float Value;
      /// <summary>
      /// A value associated with the edge from its source node
      /// </summary> 
      public float ValueInSource;
      /// <summary>
      /// A value associated with the edge from its target node
      /// </summary> 
      public float ValueInTarget;

      private string DebuggerDisplay
      {
         get
         {
            return string.Format("Edge: Source: {0}... {1}, Target: {2}... {3}",
              NodeSourceId.Substring(0, 5), EdgeNumberInSource,
              NodeTargetId.Substring(0, 5), EdgeNumberInTarget);
         }
      }
   }
}
