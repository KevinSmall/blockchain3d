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
using System.Collections.Generic;
using UnityEngine;

namespace B3d.Engine.Adaptors
{
   /// <summary>
   /// Stateless helpers for adaptors 
   /// </summary>
   class AdaptorHelpers
   {
      public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
      {
         // Unix timestamp is seconds past epoch
         System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
         dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
         return dtDateTime;
      }

      public static string FormatEdgeIdFriendly(JSONNode itemOne, JSONNode itemTwo)
      {
         return itemOne.ToString().Substring(1, 5) + "--" + itemTwo.ToString().Substring(1, 5);
      }

      public static string FormatEdgeId(JSONNode itemOne, JSONNode itemTwo, EdgeType edgeType)
      {
         //return itemOne.ToString() + "--" + itemTwo.ToString() + "--" + edgeType.ToString();
         return itemOne + "--" + itemTwo + "--" + edgeType.ToString();
      }

      public static float GetLinkValueForAddressFromInputs(string addr, JSONNode inputs)
      {
         for (int i = 0; i < inputs.Count; i++)
         {
            var inputAddr = inputs[i]["prev_out"]["addr"];
            var inputValue = inputs[i]["prev_out"]["value"];
            float inputValF = (float)inputValue / 100000f; // mBTC

            if (inputAddr == addr)
            {
               return inputValF;
            }
         }
         return 0f;
      }  
   
      public static float GetLinkValueForAddressFromOutputs(string addr, JSONNode outputs)
      {
         for (int i = 0; i < outputs.Count; i++)
         {
            var outputAddr = outputs[i]["addr"];
            var outputValue = outputs[i]["value"];
            //float outputValF = (float)Convert.ToDouble(outputValue) / 100000f; // mBTC
            float outputValF = (float)outputValue / 100000f; // mBTC
  
            if (outputAddr == addr)
            {
               return outputValF;
            }
         }
         return 0f;
      }

      /// <summary>
      /// Take the incoming tx and split it into a list of paged partial tx.
      /// Inbound tx is in API tx format.   Outbound list is also in API tx format, with header info
      /// repeated on every entry.  Note in pages form, it is possible for a tx entry to have only inputs
      /// or only outputs, with the remainder appearing in later pages.
      /// </summary>
      /// <param name="tx">Inbound tx is in API tx format</param>
      /// <param name="pageSize">number of tx in a page</param>
      /// <returns>List of paged tx also in API tx format</returns>
      public static List<JSONNode> ConvertTxToPagedTx(JSONNode tx, int pageSize)
      {
         bool deepLogging = false;
         if (deepLogging)
         {
            Msg.Log("ConvertTxToPagedTx: Node In: " + tx.ToString());
         }

         List<JSONNode> txPages = new List<JSONNode>();
         
         // Make a master copy (convert tx to string)
         string masterStr = tx.ToString();
         int masterInputCount = tx["inputs"].Count;
         int masterOutputCount = tx["out"].Count;
         int masterTxCount = masterInputCount + masterOutputCount;

         // How many loops/pages?
         int loops = masterTxCount / pageSize;
         int remainder = masterTxCount % pageSize;
         if (remainder > 0)
         {
            loops++;
         }
         if (deepLogging)
         {
            Msg.Log("ConvertTxToPagedTx: Inputs: " + masterInputCount + " Outputs: " + masterOutputCount + " PageSize: " + pageSize + " Pages: " + loops);
         }

         // pi is short for pageIndex (index with page granularity)
         for (int pi = 0; pi < loops; pi++)
         {
            // Make our copy of the master
            var N = JSON.Parse(masterStr);
            // Now N is a copy of tx

            //------------------------------------------------------------
            // Removal logic 
            // Page | Removal index
            // 0    | 0-19
            // 1    | 20-39 <<<[eg for page 1 keep only these indices]
            // 2    | 40-59
            // 3    | 60-79
            // remove where index < pi * pageSize
            //    or where index > (pi + 1) * pageSize
            // The removal index works through inputs and outputs
            //------------------------------------------------------------         
            var inputs = N["inputs"];
            var outputs = N["out"];

            // ri is removal index (index with lowest level granularity, ie a single list entry in input or output)
            for (int ri = 0; ri < masterTxCount; ri++)
            {
               if (ri < masterInputCount)
               {
                  // Input removal - are we eligible for removal?
                  if (ri < (pi * pageSize) || (ri >= (pi + 1) * pageSize))
                  {
                     inputs[ri]["script"].Value = "DELETEME";
                  }
               }
               else if (ri < (masterInputCount + masterOutputCount))
               {
                  // Output removal - eligible for removal? same logic as above
                  if (ri < (pi * pageSize) || (ri >= (pi + 1) * pageSize))
                  {
                     int localRi = ri - masterInputCount;
                     outputs[localRi]["script"].Value = "DELETEME";
                  }
               }
               else
               {
                  Debug.LogWarning("SimpleJSONBitStream: removal index higher than expected!");
               }
            }

            // Ditch the DELETEME nodes in inputs or outputs
            RemoveTheDeleteMeNodes(N);

            // Add it to the list
            txPages.Add(N);
         }

         if (deepLogging)
         {
            Debug.Log("ConvertTxToPagedTx: Node List Out: Count: " + txPages.Count);
            foreach (JSONNode j in txPages)
            {
               Debug.Log("ConvertTxToPagedTx: Node List Item: " + j.ToString());
            }
         }
         return txPages;
      }

      /// <summary>
      /// A tx can contain multiple inputs or outputs from the SAME source address (basically each unspent transaction
      /// output UTXO from the source address). This method aggregates across these unspent tx outputs, summarising the tx
      /// to just have one input per address.
      /// Critically, this method must see an ENTIRE tx in one go (no paging), but that is fine because eg blockchain.info always
      /// sends tx in one go, it is only addresses that it pages.
      /// </summary>
      /// <param name="N">A whole tx in JSONNode format, this gets changed in-situ</param>
      public static void CompressTxInputs(JSONNode N)
      {
         var inputs = N["inputs"];
         //var outputs = N["out"];         
         if (inputs.Count == 0)
         {
            return;
         }
         
         // Dict to hold the aggregation
         Dictionary<string, float> aggr;         
         aggr = new Dictionary<string, float>();
         float valueSoFar = 0f;
         
         // loop inputs
         for (int i = 0; i < inputs.Count; i++)
         {  
            var inputAddr = inputs[i]["prev_out"]["addr"];
            var inputValue = inputs[i]["prev_out"]["value"];
            float inputValueF = (float)inputValue; // note satoshis // / 100000f; // mBTC
                                   
            if (aggr.TryGetValue(inputAddr, out valueSoFar))
            {
               // aggregate the value there already
               aggr[inputAddr] = valueSoFar + inputValueF;
               // flag this entry for deletion
               inputs[i]["script"].Value = "DELETEME";
            }
            else
            {
               // create a new dict entry
               aggr.Add(inputAddr, inputValueF);
               // note dont flag this for deletion, we store final aggr value here soon
            }          
         }

         // write back the aggregated values
         float valueAggr = 0f;
         for (int i = 0; i < inputs.Count; i++)
         {
            if (inputs[i]["script"].Value != "DELETEME")
            {
               var inputAddr = inputs[i]["prev_out"]["addr"];
               if (aggr.TryGetValue(inputAddr, out valueAggr))
               {
                  inputs[i]["prev_out"]["value"] = valueAggr;
               }
            }
         }        

         // delete the DELETME inputs
         RemoveTheDeleteMeNodes(N);

         N["vin_sz"] = inputs.Count;
      }

      /// <summary>
      /// See comment for CompressTxInputs - I dont think this applies to outputs, but there is no harm in checking.
      /// </summary>
      /// <param name="N"></param>
      public static void CompressTxOutputs(JSONNode N)
      {
         var outputs = N["out"];
         if (outputs.Count == 0)
         {
            return;
         }

         // Dict to hold the aggregation
         Dictionary<string, float> aggr;
         aggr = new Dictionary<string, float>();
         float valueSoFar = 0f;

         // loop outputs
         for (int i = 0; i < outputs.Count; i++)
         {
            var outputAddr = outputs[i]["addr"];
            var outputValue = outputs[i]["value"];
            float outputValueF = (float)outputValue; // note satoshis // / 100000f; // mBTC

            if (aggr.TryGetValue(outputAddr, out valueSoFar))
            {
               // aggregate the value there already
               aggr[outputAddr] = valueSoFar + outputValueF;
               // flag this entry for deletion
               outputs[i]["script"].Value = "DELETEME";
            }
            else
            {
               // create a new dict entry
               aggr.Add(outputAddr, outputValueF);
               // note dont flag this for deletion, we store final aggr value here soon
            }
         }

         // write back aggregated values
         float valueAggr = 0f;
         for (int i = 0; i < outputs.Count; i++)
         {
            if (outputs[i]["script"].Value != "DELETEME")
            {
               var outputAddr = outputs[i]["addr"];
               if (aggr.TryGetValue(outputAddr, out valueAggr))
               {
                  outputs[i]["value"] = valueAggr;
               }
            }
         }         

         // delete the DELETME inputs
         RemoveTheDeleteMeNodes(N);

         N["vout_sz"] = outputs.Count;
      }

      /// <summary>
      /// Remove the input and output address entries that have script = DELETEME
      /// </summary>
      /// <param name="N"></param>
      private static void RemoveTheDeleteMeNodes(JSONNode N)
      {
         var inputs = N["inputs"];
         var outputs = N["out"];
         int masterInputCount = inputs.Count;
         int masterOutputCount = outputs.Count;

         bool okToContinue = true;
         int i = 0;

         // cleanse inputs
         do
         {
            if (inputs.Count == 0 || i >= inputs.Count)
            {
               okToContinue = false;
               continue;
            }

            if (inputs[i]["script"].Value == "DELETEME")
            {
               inputs.Remove(i);
               i = 0;
               continue;
            }
            else
            {
               i++;
            }

         } while (okToContinue);

         // cleanse outputs
         okToContinue = true;
         i = 0;
         do
         {
            if (outputs.Count == 0 || i >= outputs.Count)
            {
               okToContinue = false;
               continue;
            }

            if (outputs[i]["script"].Value == "DELETEME")
            {
               outputs.Remove(i);
               i = 0;
               continue;
            }
            else
            {
               i++;
            }

         } while (okToContinue);
      }

      /* Below method produces this output:
   {
       "someString": "stringValue",
       "subclass": {
           "substring": "another string",
           "subint": 5
       },
       "A": {
           "B": {
               "C": [{
                   "val": "Hello world"
               }, "string"]
           }
       },
       "anotherSubClass": {
           "someValue": "value"
       }
   }     
       */
      public static string TestStr()
      {
         JSONNode N = new JSONObject(); // Start with JSONArray or JSONClass

         N["someString"] = "stringValue";
         N["subclass"]["substring"] = "another string";
         N["subclass"]["subint"].AsInt = 5;
         N["A"]["B"]["C"][-1]["val"] = "Hello world";
         N["A"]["B"]["C"][-1] = "string";

         // Even this would work
         JSONNode sub = N["anotherSubClass"];
         sub["someValue"] = "value";

         string s = N.ToString();
         return s;
      }
   }
}