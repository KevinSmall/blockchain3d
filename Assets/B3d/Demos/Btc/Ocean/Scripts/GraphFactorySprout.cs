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
      [Tooltip("The start node defaults to showing 1 layer of edges deeper at startup. Increase this to have graph grow automatically. 3 works well, 5 may cause poor performance.")]
      [RangeAttribute(1, 5)]
      public int AutoSproutDepth = 1;

      public static GraphFactorySprout Instance;

      [Tooltip("Runtime flag if auto grow is switched on or not. The default at startup is always on.")]
      /// <summary>
      /// Runtime flag if auto grow is switched on or not. The default at startup is always on.
      /// </summary>
      public bool IsAutoGrowActive = true;

      /// <summary>
      /// Some nodes have 100s or 1000s of edges. If automatic sprouting is on, we don't necessarily want to display all, just enough
      /// to get a feel for the shape. This number of edges is deemed "enough" in this case.
      /// </summary>
      [Tooltip("Some nodes have 100s or 1000s of edges. If automatic sprouting is on, we don't necessarily want to display all, just enough to get a feel for the shape.")]
      public int EnoughEdges = 48;

      [Tooltip("Delay in seconds between sprouts")]
      public float SecondsBetweenSprouts = 2f;

      [Tooltip("Auto sprout will switch itself off in offline demo mode")]
      public float AutoSproutMaxLifeInOfflineDemo = 30f;

      private List<GraphNodeBrain> _nodesSproutable;
      private float _timer;
      private Transform _nodeFolder;

      private void Awake()
      {
         // Singleton
         if (Instance == null)
         {
            Msg.Log("GraphFactorySprout created");
            Instance = this;
            //DontDestroyOnLoad(gameObject);
         }
         else
         {
            Msg.LogWarning("GraphFactorySprout re-creation attempted, destroying the new one");
            Destroy(gameObject);
         }

         // Find graph factory
         GraphFactoryBtc gfb = GetComponent<GraphFactoryBtc>();
         if (gfb == null)
         {
            Msg.LogWarning("GraphFactorySprout could not find GraphFactory component - no nodes will sprout");
            IsAutoGrowActive = false;
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
         IsAutoGrowActive = true;
      }

      // Update is called once per frame
      void Update()
      {
         if (!IsAutoGrowActive)
         {
            return;
         }

         // Delay between sprouts
         _timer -= Time.deltaTime;
         if (_timer < 0f)
         {
            _timer = SecondsBetweenSprouts;
            DoSprout();
         }

         // In offline demos we want to force sprout off
         if (GlobalData.Instance.OfflineTransactionDataRequested == true && GlobalData.Instance.OfflineAddressDataRequested == true)
         {
            AutoSproutMaxLifeInOfflineDemo -= Time.deltaTime;
            if (AutoSproutMaxLifeInOfflineDemo < 0f)
            {
               Msg.Log("GraphFactorySprout is switching itself off as we are in an offline demo and max time has passed.");
               IsAutoGrowActive = false;
            }
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
            Msg.Log("GraphFactorySprout is switching itself off, there are no sproutable nodes left.");
            IsAutoGrowActive = false;
         }
      }
   }
}
