using B3d.Engine.Cdm;
using B3d.Engine.FrontEnd;
using UnityEngine;

public class MinimalGraphFactory : MonoBehaviour, IGraphFactory
{
   void IGraphFactory.CreateOrUpdateEdge(CdmEdge edgeNew)
   {
      // edgeNew contains all the interesting data about an edge
      Debug.Log("Edge received: " + edgeNew.EdgeIdFriendly);
   }

   void IGraphFactory.CreateOrUpdateNode(CdmNode nodeNew)
   {
      // nodeNew contains all the interesting data about a node (an address or a transaction)
      Debug.Log("Node received: " + nodeNew.NodeType + " " + nodeNew.NodeId);
   }

   void Start()
   {
      // Request data for an address
      Debug.Log("Asking for data for address...");
      FrontEndController.Instance.GetAddressData("17mjNZWa3LVXnpiewKae3VkWyDCqKwt7PV", 1);

      // Request data for a transaction
      //Debug.Log("Asking for data for transaction...");
      //FrontEndController.Instance.GetTransactionData("00c3b434effcb7a9f267ccc1f3c199694fef85491c3491ef6b29dec2fb2f8592", 1);
   }
}
