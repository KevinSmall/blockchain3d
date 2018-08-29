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

namespace B3d.Engine.Cdm
{
   /// <summary>
   /// Application type
   /// Note Bch (Bitcoin cash) is treated as family Btc (Bitcoin) since technically fields are the same (I think, for now).
   /// </summary>
   public enum Family { Btc, Eth, Erc20, Twitter, Etc }

   /// <summary>
   /// Node type, covering all possible blockchains and use cases. For example:
   /// Addr = Address (used by Btc, Eth etc)
   /// Tx = Transaction (used by Btc, Eth etc)
   /// Acct = Account (used by some other family of data)
   /// This is used by Adaptors (to know what sort of node is requested) and Cdm (to store data) and frontends (to know how to represent that data).
   /// </summary>
   public enum NodeType { Addr, Tx, Acct, Etc }

   /// <summary>
   /// Edge type, covering all possible blockchains and use cases. For example:   
   /// Input = Btc Tx Input
   /// Output = Btc Tx Output   
   /// Mixed = Input and Output
   /// can ad others for Ethereum etc
   /// </summary>
   public enum EdgeType { Input, Output, Mixed, Unknown }   
}
