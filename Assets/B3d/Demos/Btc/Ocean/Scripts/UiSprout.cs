using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B3d.Tools;

namespace B3d.Demos
{
   /// <summary>
   /// Graph grows by itself, sprouting new nodes and edges
   /// </summary>
   public class Sprout : MonoBehaviour
   {
      [Tooltip("Parent folder to look inside to find nodes")]
      public GameObject NodeFolder;

      [Tooltip("Delay in seconds between sprouts")]
      public float SecondsBetweenSprouts = 3f;

      private List<GraphNodeBrain> _nodesSproutable;
      private float _timer;

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
         //if (!ControlPanel.main.BitStreamRuntime.IsSproutingActive)
         //   return;

         if (NodeFolder == null)
            return;

         // Build list if sproutable nodes
         _nodesSproutable = new List<GraphNodeBrain>();

         //foreach (Transform child in NodeFolder.transform)
         //{
         //      // might be a node, might be a link
         //      GraphNodeBrain bnb = child.GetComponent<GraphNodeBrain>();
         //   if (bnb != null)
         //   {
         //      if (bnb.BitNodeType == GraphNodeBrain.AddrIncomplete || bnb.BitNodeType == BitNodeType.TxIncomplete)
         //      {
         //         // We are a sproutable node
         //         _nodesSproutable.Add(bnb);
         //      }
         //   }
         //}

         //   // Randomly do a sprout
         //   GraphNodeBrain node = _nodesSproutable.GetRandomEntry();
         //if (node != null)
         //{
         //   // do the equivalent of the player tapping or clicking the node 
         //   node.OnGazeTrigger();
         //}
      }
   }
}
