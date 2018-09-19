using B3d.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace B3d.Demos
{
   /// <summary>
   /// Graph grows by itself, sprouting new nodes and edges
   /// </summary>
   [RequireComponent(typeof(GraphFactoryBtc))]
   public class GraphFactorySprout : MonoBehaviour
   {
      [Info("This component automatically sprouts new nodes until the sprout depth is reached. It relies on GraphNodeBrain having an IsNodeSproutable method.")]
      /// <summary>
      /// The start node defaults to showing 1 layer of edges deeper at startup. Increase this to have graph grow automatically.
      /// </summary>
      [Tooltip("The start node defaults to showing 1 layer of edges deeper at startup. Increase this to have graph grow automatically.")]
      [RangeAttribute(1, 3)]
      public int AutoSproutDepth = 1;

      /// <summary>
      /// Some nodes have 100s or 1000s of edges. If automatic sprouting is on, we don't necessarily want to display all, just enough
      /// to get a feel for the shape. This number of edges is deemed "enough" in this case.
      /// </summary>
      [Tooltip("Some nodes have 100s or 1000s of edges. If automatic sprouting is on, we don't necessarily want to display all, just enough to get a feel for the shape.")]
      public int EnoughEdges = 32;

      [Tooltip("Delay in seconds between sprouts")]
      public float SecondsBetweenSprouts = 2f;

      private List<GraphNodeBrain> _nodesSproutable;
      private float _timer;
      private Transform _nodeFolder;

      private void Awake()
      {
         GraphFactoryBtc gfb = GetComponent<GraphFactoryBtc>();
         if (gfb == null)
         {
            Msg.LogWarning("GraphFactorySprout could not find GraphFactory component - no nodes will sprout");
         }
         else
         {
            _nodeFolder = gfb.ParentFolder;
         }
         Msg.Log("GraphFactorySprout is awake");
      }

      // Use this for initialization
      void Start()
      {
         _timer = SecondsBetweenSprouts;
      }

      // Update is called once per frame
      void Update()
      {
         _timer -= Time.deltaTime;
         if (_timer < 0f)
         {
            _timer = SecondsBetweenSprouts;
            DoSprout();
         }
      }

      private void DoSprout()
      {
         if (_nodeFolder == null)
         {
            return;
         }

         bool didSproutHappen = false;
         foreach (Transform child in _nodeFolder.transform)
         {
            // might be a node, might be an edge
            GraphNodeBrain gnb = child.GetComponent<GraphNodeBrain>();
            if (gnb != null)
            {
               if (gnb.IsSproutable(AutoSproutDepth, EnoughEdges))
               {
                  // trigger a tap/click on the node
                  gnb.OnGazeTrigger();
                  didSproutHappen = true;
                  break;
               }
            }
         }

         if (!didSproutHappen)
         {
            // We are done, can switch off sprouting
            // TODO could disable component?
            Msg.Log("GraphFactorySprout is switching itself off, there are no sproutable nodes left.");
            SecondsBetweenSprouts = 3600f;
         }
      }
   }
}
