/*
* Copyright (c) OpenSim project, http://sim.opensecondlife.org/
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the <organization> nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY <copyright holder> ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL <copyright holder> BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
* 
*/

using System;
using System.Collections.Generic;
using libsecondlife;
using System.Collections;
using libsecondlife.Packets;
using libsecondlife.AssetSystem;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace OpenSimLite
{
	/// <summary>
	/// Manages Viewer connections
	/// </summary>
	/*public class ViewerServer
	{
		public ViewerServer()
		{
		}
	}
	
	public class ViewerNetInfo
	{
		public ViewerNetInfo()
		{
			
		}
	}*/
	
	 //for now though, we will use a adapted version of the server
	 // class from version 0.1
	 public class Server
	 {
	 	/// <summary>A public reference to the client that this Simulator object
	 	/// is attached to</summary>
	 	//public SecondLife Client;

	 	/// <summary>The Region class that this Simulator wraps</summary>
	 	// public Region Region;

	 	private object _sendPacketSync = new object();
	 	/// <summary>
	 	/// Used internally to track sim disconnections, do not modify this
	 	/// variable
	 	/// </summary>
	 	public bool DisconnectCandidate = false;

	 	/// <summary>
	 	/// The ID number associated with this particular connection to the
	 	/// simulator, used to emulate TCP connections. This is used
	 	/// internally for packets that have a CircuitCode field
	 	/// </summary>
	 	public uint CircuitCode
	 	{
	 		get { return circuitCode; }
	 		set { circuitCode = value; }
	 	}

	 	/// <summary>
	 	/// The IP address and port of the server
	 	/// </summary>
	 	public IPEndPoint IPEndPoint
	 	{
	 		get { return ipEndPoint; }
	 	}

	 	/// <summary>
	 	/// A boolean representing whether there is a working connection to the
	 	/// simulator or not
	 	/// </summary>
	 	public bool Connected
	 	{
	 		get { return connected; }
	 	}
	 	
	 	private uint Sequence = 0;
	 	private object SequenceLock = new object();
	 	private byte[] RecvBuffer = new byte[4096];
	 	private byte[] ZeroBuffer = new byte[8192];
	 	private byte[] ZeroOutBuffer = new byte[4096];
	 	private Socket Connection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
	 	private AsyncCallback ReceivedData;
	 	private bool connected = false;
	 	private uint circuitCode;
	 	private IPEndPoint ipEndPoint;
	 	private EndPoint endPoint;
	 	private IPEndPoint ipeSender;
	 	private EndPoint epSender;
	 	private System.Timers.Timer AckTimer;
	 	private Server_Settings Settings=new Server_Settings();
	 	public ArrayList User_agents=new ArrayList();

	 	/// <summary>
	 	/// 
	 	/// </summary>
	 	public Server()
	 	{
	 		
	 	}
	 	
	 	/// <summary>
	 	/// 
	 	/// </summary>
	 	public void Startup()
	 	{
	 		AckTimer = new System.Timers.Timer(Settings.NETWORK_TICK_LENGTH);
	 		AckTimer.Elapsed += new ElapsedEventHandler(AckTimer_Elapsed);

	 		// Initialize the callback for receiving a new packet
	 		ReceivedData = new AsyncCallback(this.OnReceivedData);

	 		// Client.Log("Connecting to " + ip.ToString() + ":" + port, Helpers.LogLevel.Info);

	 		try
	 		{
	 			// Create an endpoint that we will be communicating with (need it in two
	 			// types due to .NET weirdness)
	 			// ipEndPoint = new IPEndPoint(ip, port);
	 			ipEndPoint = new IPEndPoint(IPAddress.Any, Globals.Instance.SimPort);
	 			endPoint = (EndPoint)ipEndPoint;

	 			// Associate this simulator's socket with the given ip/port and start listening
	 			Connection.Bind(endPoint);
	 			ipeSender = new IPEndPoint(IPAddress.Any, 0);
	 			//The epSender identifies the incoming clients
	 			epSender = (EndPoint) ipeSender;
	 			Connection.BeginReceiveFrom(RecvBuffer, 0, RecvBuffer.Length, SocketFlags.None, ref epSender, ReceivedData, null);

	 			// Start the ACK timer
	 			AckTimer.Start();
	 		}
	 		catch (Exception e)
	 		{	 			
	 			Console.WriteLine(e.Message);
				Console.WriteLine( e.StackTrace );
	 		}
	 	}

	 	/// <summary>
	 	/// Disconnect a Simulator
	 	/// </summary>
	 	public void Disconnect()
	 	{
	 		if (connected)
	 		{
	 			connected = false;
	 			AckTimer.Stop();

	 			// Send the CloseCircuit notice
	 			CloseCircuitPacket close = new CloseCircuitPacket();

	 			if (Connection.Connected)
	 			{
	 				try
	 				{
	 					//  Connection.Send(close.ToBytes());
	 				}
	 				catch (SocketException)
	 				{
	 					// There's a high probability of this failing if the network is
	 					// disconnecting, so don't even bother logging the error
	 				}
	 			}

	 			try
	 			{
	 				// Shut the socket communication down
	 				//  Connection.Shutdown(SocketShutdown.Both);
	 			}
	 			catch (SocketException)
	 			{
	 			}
	 		}
	 	}

	 	/// <summary>
	 	/// Sends a packet
	 	/// </summary>
	 	/// <param name="packet">Packet to be sent</param>
	 	/// <param name="incrementSequence">Increment sequence number?</param>
	 	public void SendPacket(Packet packet, bool incrementSequence, NetworkInfo User_info)
	 	{
	 		lock(this._sendPacketSync)
	 		{
	 			byte[] buffer;
	 			int bytes;

	 			if (!connected && packet.Type != PacketType.UseCircuitCode)
	 			{
	 				// Client.Log("Trying to send a " + packet.Type.ToString() + " packet when the socket is closed",
	 				//      Helpers.LogLevel.Info);

	 				return;
	 			}

	 			if (packet.Header.AckList.Length > 0)
	 			{
	 				// Scrub any appended ACKs since all of the ACK handling is done here
	 				packet.Header.AckList = new uint[0];
	 			}
	 			packet.Header.AppendedAcks = false;

	 			// Keep track of when this packet was sent out
	 			packet.TickCount = Environment.TickCount;

	 			if (incrementSequence)
	 			{
	 				// Set the sequence number
	 				lock (SequenceLock)
	 				{
	 					if (Sequence > Settings.MAX_SEQUENCE)
	 						Sequence = 1;
	 					else
	 						Sequence++;
	 					packet.Header.Sequence = Sequence;
	 				}

	 				if (packet.Header.Reliable)
	 				{
	 					lock (User_info.NeedAck)
	 					{
	 						if (!User_info.NeedAck.ContainsKey(packet.Header.Sequence))
	 						{
	 							User_info.NeedAck.Add(packet.Header.Sequence, packet);
	 						}
	 						else
	 						{
	 							//  Client.Log("Attempted to add a duplicate sequence number (" +
	 							//     packet.Header.Sequence + ") to the NeedAck dictionary for packet type " +
	 							//      packet.Type.ToString(), Helpers.LogLevel.Warning);
	 						}
	 					}

	 					// Don't append ACKs to resent packets, in case that's what was causing the
	 					// delivery to fail
	 					if (!packet.Header.Resent)
	 					{
	 						// Append any ACKs that need to be sent out to this packet
	 						lock (User_info.PendingAcks)
	 						{
	 							if (User_info.PendingAcks.Count > 0 && User_info.PendingAcks.Count < Settings.MAX_APPENDED_ACKS &&
	 							    packet.Type != PacketType.PacketAck &&
	 							    packet.Type != PacketType.LogoutRequest)
	 							{
	 								packet.Header.AckList = new uint[User_info.PendingAcks.Count];
	 								int i = 0;
	 								
	 								foreach (uint ack in User_info.PendingAcks.Values)
	 								{
	 									packet.Header.AckList[i] = ack;
	 									i++;
	 								}

	 								User_info.PendingAcks.Clear();
	 								packet.Header.AppendedAcks = true;
	 							}
	 						}
	 					}
	 				}
	 			}

	 			// Serialize the packet
	 			buffer = packet.ToBytes();
	 			bytes = buffer.Length;

	 			try
	 			{
	 				// Zerocode if needed
	 				if (packet.Header.Zerocoded)
	 				{
	 					lock (ZeroOutBuffer)
	 					{
	 						bytes = Helpers.ZeroEncode(buffer, bytes, ZeroOutBuffer);
	 						Connection.SendTo(ZeroOutBuffer, bytes, SocketFlags.None,User_info.endpoint);
	 					}
	 				}
	 				else
	 				{
	 					
	 					Connection.SendTo(buffer, bytes, SocketFlags.None,User_info.endpoint);
	 				}
	 			}
	 			catch (SocketException)
	 			{
	 				//Client.Log("Tried to send a " + packet.Type.ToString() + " on a closed socket",
	 				//   Helpers.LogLevel.Warning);

	 				Disconnect();
	 			}
	 		}
	 	}

	 	/// <summary>
	 	/// Send a raw byte array payload as a packet
	 	/// </summary>
	 	/// <param name="payload">The packet payload</param>
	 	/// <param name="setSequence">Whether the second, third, and fourth bytes
	 	/// should be modified to the current stream sequence number</param>
	 	/// <summary>
	 	/// Returns Simulator Name as a String
	 	/// </summary>
	 	/// <returns></returns>
	 	public override string ToString()
	 	{
	 		return( " (" + ipEndPoint.ToString() + ")");
	 	}

	 	/// <summary>
	 	/// Sends out pending acknowledgements
	 	/// </summary>
	 	private void SendAcks(NetworkInfo User_info)
	 	{
	 		lock (User_info.PendingAcks)
	 		{
	 			if (connected && User_info.PendingAcks.Count > 0)
	 			{
	 				if (User_info.PendingAcks.Count > 250)
	 				{
	 					// FIXME: Handle the odd case where we have too many pending ACKs queued up
	 					//Client.Log("Too many ACKs queued up!", Helpers.LogLevel.Error);
	 					return;
	 				}

	 				int i = 0;
	 				PacketAckPacket acks = new PacketAckPacket();
	 				acks.Packets = new PacketAckPacket.PacketsBlock[User_info.PendingAcks.Count];

	 				foreach (uint ack in User_info.PendingAcks.Values)
	 				{
	 					acks.Packets[i] = new PacketAckPacket.PacketsBlock();
	 					acks.Packets[i].ID = ack;
	 					i++;
	 				}

	 				acks.Header.Reliable = false;
	 				SendPacket(acks, true,User_info);

	 				User_info.PendingAcks.Clear();
	 			}
	 		}
	 	}
	 	/// <summary>
	 	/// Resend unacknowledged packets
	 	/// </summary>
	 	private void ResendUnacked(NetworkInfo User_info)
	 	{
	 		if (connected)
	 		{
	 			int now = Environment.TickCount;

	 			lock (User_info.NeedAck)
	 			{
	 				foreach (Packet packet in User_info.NeedAck.Values)
	 				{
	 					if (now - packet.TickCount > Settings.RESEND_TIMEOUT)
	 					{
	 						//  Client.Log("Resending " + packet.Type.ToString() + " packet, " +
	 						//     (now - packet.TickCount) + "ms have passed", Helpers.LogLevel.Info);

	 						packet.Header.Resent = true;
	 						SendPacket(packet, false,User_info);
	 					}
	 				}
	 			}
	 		}
	 	}
	 	/// <summary>
	 	/// Callback handler for incomming data
	 	/// </summary>
	 	/// <param name="result"></param>
	 	private void OnReceivedData(IAsyncResult result)
	 	{
	 		
	 		ipeSender = new IPEndPoint(IPAddress.Any, 0);
	 		epSender = (EndPoint)ipeSender;
	 		Packet packet = null;
	 		int numBytes;

	 		// If we're receiving data the sim connection is open
	 		connected = true;

	 		// Update the disconnect flag so this sim doesn't time out
	 		DisconnectCandidate = false;
	 		NetworkInfo User_info=null;

	 		lock (RecvBuffer)
	 		{
	 			// Retrieve the incoming packet
	 			try
	 			{
	 				numBytes = Connection.EndReceiveFrom(result, ref epSender);
	 				
	 				//find user_agent_info
	 				
	 				int packetEnd = numBytes - 1;
	 				packet = Packet.BuildPacket(RecvBuffer, ref packetEnd, ZeroBuffer);
	 				
	 				
	 				//should check if login/useconnection packet first
	 				if (packet.Type == PacketType.UseCircuitCode)
	 				{
	 					//new connection
	 					//TODO check that this circuit and session is expected
	 					UseCircuitCodePacket cir_pack=(UseCircuitCodePacket)packet;
	 					NetworkInfo new_user=new NetworkInfo();
	 					new_user.CircuitCode=cir_pack.CircuitCode.Code;
	 					new_user.User.AgentID=cir_pack.CircuitCode.ID;
	 					new_user.User.SessionID=cir_pack.CircuitCode.SessionID;
	 					new_user.endpoint=epSender;
	 					new_user.Inbox = new Queue<uint>(Settings.INBOX_SIZE);
	 					new_user.Connection = new ClientConnection();
	 					new_user.Connection.NetInfo = new_user;
	 					new_user.Connection.Start();
	 					
	 					//this.CallbackObject.NewUserCallback(new_user);
	 					this.User_agents.Add(new_user);
	 					
	 				}
	 				
	 				NetworkInfo temp_agent=null;
	 				IPEndPoint send_ip=(IPEndPoint)epSender;
	 				//	this.callback_object.error("incoming: address is "+send_ip.Address +"port number is: "+send_ip.Port.ToString());
	 				
	 				for(int ii=0; ii<this.User_agents.Count ; ii++)
	 				{
	 					temp_agent=(NetworkInfo)this.User_agents[ii];
	 					IPEndPoint ag_ip=(IPEndPoint)temp_agent.endpoint;
	 					//this.callback_object.error("searching: address is "+ag_ip.Address +"port number is: "+ag_ip.Port.ToString());
	 					
	 					if((ag_ip.Address.ToString()==send_ip.Address.ToString()) && (ag_ip.Port.ToString()==send_ip.Port.ToString()))
	 					{
	 						//this.callback_object.error("found user");
	 						User_info=temp_agent;
	 						break;
	 					}
	 				}
	 				
	 				Connection.BeginReceiveFrom(RecvBuffer, 0, RecvBuffer.Length, SocketFlags.None, ref epSender, ReceivedData, null);
	 			}
	 			catch (SocketException)
	 			{
	 				// Client.Log(endPoint.ToString() + " socket is closed, shutting down " + this.Region.Name,
	 				//     Helpers.LogLevel.Info);

	 				connected = false;
	 				//Network.DisconnectSim(this);
	 				return;
	 			}
	 		}
	 		if(User_info==null)
	 		{
	 			
	 			//error finding agent
	 			//this.CallbackObject.ErrorCallback("no user found");
	 			return;
	 		}

	 		// Fail-safe check
	 		if (packet == null)
	 		{
	 			//this.CallbackObject.ErrorCallback("couldn't build packet");
	 			// Client.Log("Couldn't build a message from the incoming data", Helpers.LogLevel.Warning);
	 			return;
	 		}
	 		//this.callback_object.error("past tests");
	 		// Track the sequence number for this packet if it's marked as reliable
	 		if (packet.Header.Reliable)
	 		{
	 			if (User_info.PendingAcks.Count > Settings.MAX_PENDING_ACKS)
	 			{
	 				SendAcks(User_info);
	 			}

	 			// Check if we already received this packet
	 			if (User_info.Inbox.Contains(packet.Header.Sequence))
	 			{
	 				//Client.Log("Received a duplicate " + packet.Type.ToString() + ", sequence=" +
	 				//    packet.Header.Sequence + ", resent=" + ((packet.Header.Resent) ? "Yes" : "No") +
	 				//    ", Inbox.Count=" + Inbox.Count + ", NeedAck.Count=" + NeedAck.Count,
	 				//    Helpers.LogLevel.Info);

	 				// Send an ACK for this packet immediately
	 				//SendAck(packet.Header.Sequence);

	 				// TESTING: Try just queuing up ACKs for resent packets instead of immediately triggering an ACK
	 				lock (User_info.PendingAcks)
	 				{
	 					uint sequence = (uint)packet.Header.Sequence;
	 					if (!User_info.PendingAcks.ContainsKey(sequence)) { User_info.PendingAcks[sequence] = sequence; }
	 				}

	 				// Avoid firing a callback twice for the same packet
	 				// this.callback_object.error("avoiding callback");
	 				return;
	 			}
	 			else
	 			{
	 				lock (User_info.PendingAcks)
	 				{
	 					uint sequence = (uint)packet.Header.Sequence;
	 					if (!User_info.PendingAcks.ContainsKey(sequence)) { User_info.PendingAcks[sequence] = sequence; }
	 				}
	 			}
	 		}

	 		// Add this packet to our inbox
	 		lock (User_info.Inbox)
	 		{
	 			while (User_info.Inbox.Count >= Settings.INBOX_SIZE)
	 			{
	 				User_info.Inbox.Dequeue();
	 			}
	 			User_info.Inbox.Enqueue(packet.Header.Sequence);
	 		}

	 		// Handle appended ACKs
	 		if (packet.Header.AppendedAcks)
	 		{
	 			lock (User_info.NeedAck)
	 			{
	 				foreach (uint ack in packet.Header.AckList)
	 				{
	 					User_info.NeedAck.Remove(ack);
	 				}
	 			}
	 		}

	 		// Handle PacketAck packets
	 		if (packet.Type == PacketType.PacketAck)
	 		{
	 			PacketAckPacket ackPacket = (PacketAckPacket)packet;

	 			lock (User_info.NeedAck)
	 			{
	 				foreach (PacketAckPacket.PacketsBlock block in ackPacket.Packets)
	 				{
	 					User_info.NeedAck.Remove(block.ID);
	 				}
	 			}
	 		}
	 		
	 		//if it is a ping check send return
	 		if( ( packet.Type == PacketType.StartPingCheck ) ) {
	 			//reply to pingcheck
	 			libsecondlife.Packets.StartPingCheckPacket startPing = (libsecondlife.Packets.StartPingCheckPacket)packet;
	 			libsecondlife.Packets.CompletePingCheckPacket endPing = new CompletePingCheckPacket();
	 			endPing.PingID.PingID = startPing.PingID.PingID;
	 			SendPacket(endPing, true, User_info );
	 		}
	 		else if(packet.Type != PacketType.PacketAck)
	 		{
	 			User_info.Connection.InQueue.Enqueue(packet);
	 		}
	 		
	 	}

	 	private void AckTimer_Elapsed(object sender, ElapsedEventArgs ea)
	 	{
	 		if (connected)
	 		{
	 			
	 			//TODO for each user_agent_info
	 			for(int i=0; i<this.User_agents.Count; i++)
	 			{
	 				NetworkInfo user=(NetworkInfo)this.User_agents[i];
	 				
	 				SendAcks(user);
	 				ResendUnacked(user);
	 			}
	 		}
	 	}
	 }
	  
	public class Server_Settings
    {
       
        /// <summary>Millisecond interval between ticks, where all ACKs are 
        /// sent out and the age of unACKed packets is checked</summary>
        public readonly int NETWORK_TICK_LENGTH = 500;
        /// <summary>The maximum value of a packet sequence number. After that 
        /// we assume the sequence number just rolls over? Or maybe the 
        /// protocol isn't able to sustain a connection past that</summary>
        public readonly int MAX_SEQUENCE = 0xFFFFFF;
        /// <summary>Number of milliseconds before a teleport attempt will time
        /// out</summary>
        public readonly int TELEPORT_TIMEOUT = 18 * 1000;
		/// <summary>Number of milliseconds before NetworkManager.Logout() will time out</summary>
		public int LOGOUT_TIMEOUT = 5 * 1000;
		/// <summary>Number of milliseconds for xml-rpc to timeout</summary>
		public int LOGIN_TIMEOUT = 30 * 1000;
        /// <summary>The maximum size of the sequence number inbox, used to
        /// check for resent and/or duplicate packets</summary>
        public int INBOX_SIZE = 100;
        /// <summary>Milliseconds before a packet is assumed lost and resent</summary>
        public int RESEND_TIMEOUT = 4000;
        /// <summary>Milliseconds before the connection to a simulator is 
        /// assumed lost</summary>
        public int SIMULATOR_TIMEOUT = 15000;
        /// <summary>Maximum number of queued ACKs to be sent before SendAcks()
        /// is forced</summary>
        public int MAX_PENDING_ACKS = 10;
        /// <summary>Maximum number of ACKs to append to a packet</summary>
        public int MAX_APPENDED_ACKS = 10;
        /// <summary>Cost of uploading an asset</summary>

        public Server_Settings()
        {
        	
        }
    }
	 
	public class NetworkInfo
    {
    	public EndPoint endpoint;
    	public uint CircuitCode;
        public CircuitConnection Connection;  //should be taken out?
        public UserInfo User;
        
        public Dictionary<uint, Packet> NeedAck = new Dictionary<uint, Packet>();
        // Sequence numbers of packets we've received from the simulator
        public Queue<uint> Inbox;
        // ACKs that are queued up to be sent to the simulator
        public Dictionary<uint, uint> PendingAcks = new Dictionary<uint, uint>();
      
    	public NetworkInfo()
    	{
    		this.User = new UserInfo();
    	}
    }

	public class UserInfo
	{
		public LLUUID AgentID;
        public LLUUID SessionID;
        public string FullName;
        public uint AvatarLocalID;
        public string FirstName;
        public string LastName;
        
		public UserInfo()
		{
			
		}
	}
}


