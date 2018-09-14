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

using B3d.Engine.Cdm;
using B3d.Tools;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace B3d.Engine.Adaptors
{
   /// <summary>
   /// Implementation of a Btc Adaptor that gets its data from Blockchain.Info
   /// </summary>
   public class AdaptorBtcDotInfo : Adaptor, IAdaptorBtc
   {
      [Header("AdaptorBtcDotInfo Config")]
      // BLOCKCHAIN.info
      [Tooltip("API key for blockchain.info")]
      public string ApiKey = "enter API key here";
      [Tooltip("Root URI for address data")]
      public string ApiUriRootForAddr = "https://blockchain.info/rawaddr/";
      [Tooltip("Root URI for transaction data")]
      public string ApiUriRootForTx = "https://blockchain.info/rawtx/";

      [Header("Record Results of API calls")]
      [Tooltip("Set true to record raw results of API calls to file")]
      public bool RecordToFileOn = false;
      [Tooltip("Files are stored in this folder for Addresses (use trailing /)")]
      public string RecordToFileFolderAddr = @"Assets/B3d/Engine/Adaptors/Resources/BtcAddress/";
      [Tooltip("Files are stored in this folder for Tx (use trailing /)")]
      public string RecordToFileFolderTx = @"Assets/B3d/Engine/Adaptors/Resources/BtcTx/";

      Family IAdaptor.GetFamily()
      {
         return Family.Btc;
      }

      void IAdaptor.CheckConnectionPossible(Action callbackOnSuccess, Action callbackOnFail)
      {
         CheckConnectionPossible(callbackOnSuccess, callbackOnFail);
      }

      /// <summary>
      /// Check existence of a node, specify nodeType
      /// The nodeId is passed back as a string in the callbacks
      /// </summary>
      void IAdaptor.CheckNodeExists(string nodeId, NodeType nodeType, Action<string> callbackOnSuccess, Action<string> callbackOnFail)
      {
         if (nodeType == NodeType.Tx)
         {
            Msg.Log("AdaptorBtc checking tx with uwr...");
            // Check tx with unity web request
            CdmRequest r = new CdmRequest() { NodeId = nodeId, NodeType = nodeType, EdgeCountFrom = 1, EdgeCountTo = 1 };
            StartCoroutine(ProcessUwr(r, PayloadCheckTx, callbackOnSuccess, callbackOnFail));
         }
         else if (nodeType == NodeType.Addr)
         {
            Msg.Log("AdaptorBtc checking addr with uwr...");
            // Check addr with unity web request         
            CdmRequest r = new CdmRequest() { NodeId = nodeId, NodeType = nodeType, EdgeCountFrom = 1, EdgeCountTo = 1 };
            StartCoroutine(ProcessUwr(r, PayloadCheckAddr, callbackOnSuccess, callbackOnFail));
         }
         else
         {
            callbackOnFail("AdaptorBtcDotInfo.CheckNodeExists: Node Type not supported");
         }
      }

      /// <summary>
      /// Get a graph fragment: the node requested, the edges requested, and the nodes at the end of each edge, and push them to Cdm
      /// Success means data pushed to Cdm and it will be raising events, the callback on fail is not essential, if 
      /// anything fails then just nothing new will appear in the Cdm
      /// </summary>
      void IAdaptor.GetGraphFragment(CdmRequest r, Action<string> callbackOnFail)
      {
         if (CdmCore == null)
         {
            Msg.LogError("AdaptorBtcDotInfo.GetGraphFragment CdmCore is null");
            return;
         }
         Msg.Log("AdaptorBtcDotInfo is getting data for node " + r.NodeId);

         // The payload methods do the work of creating graph fragment and pushing it to Cdm
         if (r.NodeType == NodeType.Addr)
         {
            Msg.Log("AdaptorBtcDotInfo.GetGraphFragment getting addr with uwr...");
            // Get addr with unity web request                 
            StartCoroutine(ProcessUwr(r, PayloadGetAddr, CallBackDummy, callbackOnFail));
         }
         else if (r.NodeType == NodeType.Tx)
         {
            Msg.Log("AdaptorBtcDotInfo.GetGraphFragment getting tx with uwr...");
            // Get tx with unity web request
            StartCoroutine(ProcessUwr(r, PayloadGetTx, CallBackDummy, callbackOnFail));
         }
         else
         {
            Msg.LogError("AdaptorBtcDotInfo.GetGraphFragment unknown node type");
         }
      }

      private string BuildUrl(NodeType nodeType, string nodeId, int edgeCountFrom, int edgeCountTo)
      {
         int limit = edgeCountTo - edgeCountFrom + 1;
         int offset = edgeCountFrom - 1;

         string u = null;
         if (nodeType == NodeType.Addr)
         {
            // https://blockchain.info/rawaddr/3CD1QW6fjgTwKq3Pj97nty28WZAVkziNom?cors=true&limit=20&offset=0&api_code=
            u = ApiUriRootForAddr;
            u = u + nodeId + "?cors=true";
            //u = u + "&limit=1" + "&offset=0";
            u = u + "&limit=" + limit.ToString() + "&offset=" + offset.ToString(); // blockchain.info supports limit and offset for addresses
         }
         else if (nodeType == NodeType.Tx)
         {
            // https://blockchain.info/rawtx/6aae5a6707326ed618a1932d27c38b36e83ee820683743a8649c19c56a170e83?cors=true&api_code=
            u = ApiUriRootForTx;
            u = u + nodeId + "?cors=true";
            // u = u + "&limit=1" + "&offset=0"; // For blockchain.info the limit and offset is not supported anyway for tx
         }
         else
         {
            Msg.Log("AdaptorBtcDotInfo.BuildUrl unknown node type: " + nodeType);
            return null;
         }

         // common stuff in url
         if (!String.IsNullOrEmpty(ApiKey))
         {
            u = u + "&api_code=" + ApiKey;
         }
         Msg.Log("AdaptorBtcDotInfo.BuildUrl built url for: " + nodeType + " " + u);
         return u;
      }

      private bool PayloadGetAddr(CdmRequest r, JSONNode n)
      {
         CdmGraph g = new CdmGraphBtc() { GraphId = "Graph for " + r.NodeType + " " + r.NodeId };
         var N = n;
         string addr = N["address"];                //.ToString(); adds a doublequote "         
         var finBal = N["final_balance"];           // address current balance         
         float finBalF = (float)finBal / 100000f;   // (float)Convert.ToDouble(finBal) / 100000f; // mBTC         
         var addrTxsRaw = N["n_tx"];                // address total tx         
         int addrTxs = (int)addrTxsRaw;             // int addrTxs = (int)Convert.ToInt32(addrTxsRaw);

         //------------------------------------------------------------------------------------------------
         // The address itself         
         //------------------------------------------------------------------------------------------------
         g.AddNode(new CdmNodeBtc()
         {
            NodeId = addr,
            NodeType = NodeType.Addr,
            NodeEdgeCountTotal = addrTxs,
            FinalBalance = finBalF,
            TotalReceived = 0,
            TotalSent = 0
         });

         //------------------------------------------------------------------------------------------------
         // Now create transactions
         //------------------------------------------------------------------------------------------------
         // address tx in this batch received
         var txs = N["txs"];
         Msg.Log("AdaptorBtcDotInfo.PayloadGetAddr has found:" + txs.Count + " transactions attached to the address " + addr);

         int edgeCounter = r.EdgeCountFrom;

         for (int i = 0; i < txs.Count; i++)
         {
            var txBody = txs[i];

            // Tx can contain multiple inputs (and outputs?) from the SAME source address (basically each UTXO from the source address)
            // we have to aggregate across these unspent tx outputs
            AdaptorHelpers.CompressTxInputs(txBody);
            AdaptorHelpers.CompressTxOutputs(txBody);

            var txId = txs[i]["hash"];
            var inputs = txs[i]["inputs"];
            var outputs = txs[i]["out"];
            var unixTime = txs[i]["time"];
            var blockHeight = txs[i]["block_height"];
            var relayedBy = txs[i]["relayed_by"];
            var vinSize = txs[i]["vin_sz"];
            var voutSize = txs[i]["vout_sz"];
            int totalInputsOutputs = 0;
            // defensively calc total inputs p lus outputs
            try
            {
               totalInputsOutputs = (int)vinSize + (int)voutSize;
            }
            catch
            {
               totalInputsOutputs = 0;
            }

            // defensively get date
            double unixTimeD = 0;
            DateTime unixTimeDT = DateTime.Now;
            try
            {
               unixTimeD = (double)unixTime;
               unixTimeDT = AdaptorHelpers.UnixTimeStampToDateTime(unixTimeD);
            }
            catch
            {
               unixTimeDT = DateTime.Now;
            }

            // Create tx
            g.AddNode(new CdmNodeBtc()
            {
               NodeId = txId,
               NodeType = NodeType.Tx,
               NodeEdgeCountTotal = totalInputsOutputs,
               CreateDate = unixTimeDT,
               CreateDateStr = unixTimeDT.ToLongDateString(),
               BlockHeight = blockHeight,
               RelayedBy = relayedBy,
               VoutSize = voutSize,
               VinSize = vinSize
            });

            //------------------------------------------------------------------------------------------------
            // Edges of all sorts:
            // Edge tx -<--- address == type input
            // Edge tx --->- address == type output
            // Edge tx -<->- address == type mixed
            //------------------------------------------------------------------------------------------------
            // Inside either the inputs or outputs, we'll find the addrObj -> get the value from it
            float linkValInp = AdaptorHelpers.GetLinkValueForAddressFromInputs(addr, inputs);
            float linkValOut = AdaptorHelpers.GetLinkValueForAddressFromOutputs(addr, outputs);

            // Edge type
            EdgeType edgeType = EdgeType.Unknown;

            if (linkValInp > 0f && linkValOut > 0f)
            {
               // Mixed edge from address to tx
               edgeType = EdgeType.Mixed;
            }
            else if (linkValOut > 0f)
            {
               // Output
               edgeType = EdgeType.Output;
            }
            else if (linkValInp > 0f)
            {
               // Input
               edgeType = EdgeType.Input;
            }
            else
            {
               // In paged cases may not have any knowledge of value - yet (or ever, until all pages retrieved) because
               // the remainder of the address information will come in a later page.
               edgeType = EdgeType.Unknown;
            }

            // Edge
            g.AddEdge(new CdmEdgeBtc()
            {
               EdgeId = AdaptorHelpers.FormatEdgeId(txId, addr, edgeType),
               EdgeIdFriendly = AdaptorHelpers.FormatEdgeIdFriendly(txId, addr),
               NodeSourceId = txId,
               NodeTargetId = addr,
               EdgeNumberInSource = 0,
               EdgeNumberInTarget = edgeCounter,
               ValueInSource = linkValInp,
               ValueInTarget = linkValOut,
               EdgeType = edgeType,
            });
            edgeCounter++;
         }

         CdmCore.IngestCdmGraphFragment(r, g);
         return true;
      }

      private bool PayloadGetTx(CdmRequest r, JSONNode n)
      {
         CdmGraph g = new CdmGraphBtc() { GraphId = "Graph for " + r.NodeType + " " + r.NodeId };

         // Extract JSON
         var N = n;
         // Tx can contain multiple inputs or outputs from the SAME source address (basically each UTXO from the source address)
         // we have to aggregate across these unspent txoutputs
         AdaptorHelpers.CompressTxInputs(N);
         AdaptorHelpers.CompressTxOutputs(N);

         // these var are all JSONNodes
         var txHash = N["hash"];
         var inputs = N["inputs"];
         var outputs = N["out"];
         var unixTime = N["time"];
         var blockHeight = N["block_height"];
         var relayedBy = N["relayed_by"];
         var vinSize = N["vin_sz"];
         var voutSize = N["vout_sz"];

         int totalInputsOutputs = 0;
         try
         {
            totalInputsOutputs = (int)vinSize + (int)voutSize;
         }
         catch
         {
            totalInputsOutputs = 0;
         }


         double unixTimeD = 0;
         DateTime unixTimeDT = DateTime.Now;
         try
         {
            //unixTimeD = Convert.ToDouble(unixTime);
            unixTimeD = (double)unixTime;
            unixTimeDT = AdaptorHelpers.UnixTimeStampToDateTime(unixTimeD);
         }
         catch
         {
            unixTimeDT = DateTime.Now;
         }

         // The tx itself
         g.AddNode(new CdmNodeBtc()
         {
            NodeId = txHash,
            NodeType = NodeType.Tx,
            NodeEdgeCountTotal = totalInputsOutputs,

            CreateDate = unixTimeDT,
            CreateDateStr = unixTimeDT.ToLongDateString(),
            BlockHeight = blockHeight,
            RelayedBy = relayedBy,
            VoutSize = voutSize,
            VinSize = vinSize
         });

         int edgeCounter = r.EdgeCountFrom;

         //------------------------------------------------------------------------------------------------
         // inputs (create "from addresses" and links from there to the tx)
         //------------------------------------------------------------------------------------------------
         // from: inputs[i].prev_out.addr
         // to    txHash (which already exists)
         // value inputs[i].prev_out.value
         for (int i = 0; i < inputs.Count; i++)
         {
            var inputAddr = inputs[i]["prev_out"]["addr"];
            var inputValue = inputs[i]["prev_out"]["value"];
            float inputValF = (float)inputValue / 100000f; // mBTC

            // Address at end of the edge
            g.AddNode(new CdmNodeBtc()
            {
               NodeId = inputAddr,
               NodeType = NodeType.Addr,
               NodeEdgeCountTotal = 0, // not known
               FinalBalance = 0,       // none of this stuff known yet
               TotalReceived = 0,
               TotalSent = 0
            });

            // Edge tx -<--- address
            g.AddEdge(new CdmEdgeBtc()
            {
               EdgeId = AdaptorHelpers.FormatEdgeId(txHash, inputAddr, EdgeType.Input),
               EdgeIdFriendly = AdaptorHelpers.FormatEdgeIdFriendly(txHash, inputAddr),
               NodeSourceId = txHash,
               NodeTargetId = inputAddr,
               EdgeNumberInSource = edgeCounter,
               EdgeNumberInTarget = 0,   // not knwon yet
               ValueInSource = inputValF,
               EdgeType = EdgeType.Input
            });
            edgeCounter++;
         }

         //------------------------------------------------------------------------------------------------
         // outputs (create "to addresses" and links from tx to there
         //------------------------------------------------------------------------------------------------
         // from: txHash (which already exists)
         // to    output[i].addr
         // value output[i].value   

         for (int i = 0; i < outputs.Count; i++)
         {
            var outputAddr = outputs[i]["addr"];
            var outputValue = outputs[i]["value"];
            float outputValF = (float)outputValue / 100000f; // mBTC

            // Address at end of the edge
            g.AddNode(new CdmNodeBtc()
            {
               NodeId = outputAddr,
               NodeType = NodeType.Addr,
               NodeEdgeCountTotal = 0, // not known
               FinalBalance = 0,       // none of this stuff known yet
               TotalReceived = 0,
               TotalSent = 0
            });

            // Edge tx --->- address
            g.AddEdge(new CdmEdgeBtc()
            {
               EdgeId = AdaptorHelpers.FormatEdgeId(txHash, outputAddr, EdgeType.Output),
               EdgeIdFriendly = AdaptorHelpers.FormatEdgeIdFriendly(txHash, outputAddr),
               NodeSourceId = txHash,
               NodeTargetId = outputAddr,
               EdgeNumberInSource = edgeCounter,
               EdgeNumberInTarget = 0,    // not known yet
               ValueInTarget = outputValF,
               EdgeType = EdgeType.Output
            });
            edgeCounter++;

         }

         CdmCore.IngestCdmGraphFragment(r, g);
         return true;
      }

      private bool PayloadCheckAddr(CdmRequest r, JSONNode n)
      {
         string addrActual = n["address"];
         if (addrActual == null)
         {
            return false;
         }
         else
         {
            return true;
         }
      }

      private bool PayloadCheckTx(CdmRequest r, JSONNode n)
      {
         string txActual = n["hash"];
         if (txActual == null)
         {
            return false;
         }
         else
         {
            return true;
         }
      }

      /// <summary>
      /// Process a Unity Web Request. Given a node id, node type, paging information (edges from and to), a payload method (used to
      /// do the actual work) and some callbacks, this method prepares a URL and sends it to blockchain.info, then processes the results
      /// with the payload.
      /// </summary>
      /// <param name="r">Request for a graph fragment, details nodeid, node type and edge counts from and to (as counted by nodeid)</param>
      /// <param name="payload">This method will be executed for the request r against the JSONNode returned by blockchain.info</param>
      /// <param name="callbackOnSuccess">Called with string filled with nodeId</param>
      /// <param name="callbackOnFail">Called with string filled with nodeId</param>
      /// <returns></returns>      
      private IEnumerator ProcessUwr(CdmRequest r, Func<CdmRequest, JSONNode, bool> payload, Action<string> callbackOnSuccess, Action<string> callbackOnFail)
      {
         string u = BuildUrl(r.NodeType, r.NodeId, r.EdgeCountFrom, r.EdgeCountTo);
         UnityWebRequest uwr = UnityWebRequest.Get(u);
         yield return uwr.SendWebRequest();

         if (uwr.isNetworkError || uwr.isHttpError)
         {
            // Fail         
            Msg.LogWarning("AdaptorBtcDotInfo.ProcessUwr had network or HTTP error");
            Msg.Log(uwr.error);
            callbackOnFail(r.NodeId);
         }
         else
         {
            // Request was OK, what is the response like?
            string s = uwr.downloadHandler.text;
            // Msg.Log("AdaptorBtcDotInfo.ProcessUwr call returned: " + s.Substring(0,60) + "...";);
            var N = JSON.Parse(s);
            if (N == null)
            {
               Debug.LogWarning("AdaptorBtcDotInfo.ProcessUwr got back null data, maybe asked for bad data?");
               callbackOnFail(r.NodeId);
            }
            else
            {
               RecordToFile(r, s);

               // For the request r, launch the payload against the JSON Node N that we got back from the blockchain.info API
               bool payloadProcessingWasOk = payload(r, N);
               if (payloadProcessingWasOk)
               {
                  callbackOnSuccess(r.NodeId);
               }
               else
               {
                  Debug.LogWarning("AdaptorBtcDotInfo.ProcessUwr got back data, but payload could not process it");
                  callbackOnFail(r.NodeId);
               }
            }
         }
      }

      private void RecordToFile(CdmRequest r, string s)
      {
         if (!RecordToFileOn)
         {
            return;
         }
         
         string filepath;         
         if (r.NodeType == NodeType.Addr)
         {
            // Addr can be paged
            filepath = RecordToFileFolderAddr + AdaptorHelpers.GetFilenameForAddressRequest(r);
         }
         else
         {
            // Tx are never paged, we get all of it in one go from blockchain.info
            filepath = RecordToFileFolderTx + r.NodeId + @".txt";
         }
         
         using (StreamWriter wr = new StreamWriter(filepath))
         {
            wr.WriteLine(s);
         }
      }

      /// <summary>
      /// Empty call back
      /// </summary>
      private void CallBackDummy(string s) { }
   }
}