// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using System;
using SocketIO;

public interface ISocketManager
{
	void CloseHandler (SocketIOEvent e);
	
	SocketIOComponent createSocket (string host, string port);

}


