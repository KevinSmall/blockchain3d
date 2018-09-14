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

   void IGraphFactory.CreateOrUpdateNode(CdmNode nodeNew, Vector3 location)
   {
      // nodeNew contains all the interesting data about a node (an address or a transaction)
      Debug.Log("Node received: " + nodeNew.NodeType + " " + nodeNew.NodeId + " to be created near: " + location);
   }

   void Start()
   {
      // Example one
      // Check a bitcoin address exists, supply callbacks which will be called when result is known
      Debug.Log("Checking address...");
      FrontEndController.Instance.CheckAddressExists("17mjNZWa3LVXnpiewKae3VkWyDCqKwt7PV", OnCheckNodeOk, OnCheckNodeFailed);
      // Similarly can use CheckTransactionExists()

      // Example two
      // Request bitcoin data for an address
      // The methods IGraphFactory.CreateOrUpdateEdge and IGraphFactory.CreateOrUpdateNode will get called if address is good
      Debug.Log("Asking for data for address...");
      FrontEndController.Instance.GetAddressData("17mjNZWa3LVXnpiewKae3VkWyDCqKwt7PV", 1);

      // Similarly could request data for a transaction
      //Debug.Log("Asking for data for transaction...");
      //FrontEndController.Instance.GetTransactionData("00c3b434effcb7a9f267ccc1f3c199694fef85491c3491ef6b29dec2fb2f8592", 1);
   }

   private void OnCheckNodeOk(string id)
   {
      Debug.Log("Node OK:" + id);
   }

   private void OnCheckNodeFailed(string id)
   {
      Debug.Log("Node failed:" + id);
   }

}
