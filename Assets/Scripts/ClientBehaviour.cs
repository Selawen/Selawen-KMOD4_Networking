using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;

public delegate void NetworkMessageHandler(object handler, NetworkConnection con, DataStreamReader stream);

public enum NetworkMessageType
{
    HANDSHAKE,
    HANDSHAKE_RESPONSE,
    CHAT_MESSAGE,
    CHAT_QUIT
}

public class ClientBehaviour : MonoBehaviour
{
    static Dictionary<NetworkMessageType, NetworkMessageHandler> networkMessageHandlers = new Dictionary<NetworkMessageType, NetworkMessageHandler> {
            { NetworkMessageType.HANDSHAKE_RESPONSE, HandleHandshakeResponse },
            { NetworkMessageType.CHAT_MESSAGE, HandleServerMessage },
            { NetworkMessageType.CHAT_QUIT, HandleServerExit },
        };


    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool Done;

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.Parse("83.85.158.101", 1511);
        //var endpoint = NetworkEndPoint.LoopbackIpv4;
        //endpoint.Port = 1511;

        m_Connection = m_Driver.Connect(endpoint);
    }

    // Update is called once per frame
    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            if (!Done)
                Debug.Log("Something went wrong during connect");
            return;
        }
        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");

                FixedString128 message = "Inger";
                DataStreamWriter writer;
                int result = m_Driver.BeginSend(m_Connection, out writer);
                if (result == 0)
                {
                    writer.WriteUInt((uint)NetworkMessageType.HANDSHAKE);
                    writer.WriteFixedString128(message);
                    m_Driver.EndSend(writer);
                }
                else
                {
                    Debug.Log("something went wrong");
                }
                //uint value = 1;
                //DataStreamWriter writer;
                //int result = m_Driver.BeginSend(m_Connection, out writer);
                //if (result == 0)
                //{
                //    writer.WriteUInt(value);
                //    m_Driver.EndSend(writer);
                //}
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                uint value = stream.ReadUInt();
                Debug.Log("Got the value = " + value + " back from the server");
                //Done = true;
                //m_Connection.Disconnect(m_Driver);
                //m_Connection = default(NetworkConnection);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection);
            }
        }
    }

    public void OnDestroy()
    {
        m_Connection.Disconnect(m_Driver);
        m_Connection = default(NetworkConnection);
        m_Driver.Dispose();
    }

    private void OnConnectedToServer()
    {
        //Debug.Log("connected");
        ////if (m_Connection.IsCreated)
        ////{
        //    FixedString128 message = "Inger";
        //    DataStreamWriter writer;
        //    int result = m_Driver.BeginSend(m_Connection, out writer);
        //    if (result == 0)
        //    {
        //        writer.WriteUInt((uint)NetworkMessageType.HANDSHAKE);
        //        writer.WriteFixedString128(message);
        //        m_Driver.EndSend(writer);
        //    } else
        //{
        //    Debug.Log("something went wrong");
        //}
        //}
    }


    public void JoinServer()
    {
        if (m_Connection == default(NetworkConnection))
        {
            m_Connection = default(NetworkConnection);

            var endpoint = NetworkEndPoint.Parse("83.85.158.101", 1511);
            //var endpoint = NetworkEndPoint.LoopbackIpv4;
            //endpoint.Port = 1511;

            m_Connection = m_Driver.Connect(endpoint);

            Done = false;
        } 
    }
    
    public new void SendMessage(string chatMessage)
    {
        FixedString128 message = chatMessage;
        DataStreamWriter writer;
        int result = m_Driver.BeginSend(m_Connection, out writer);
        if (result == 0)
        {
            writer.WriteUInt((uint)NetworkMessageType.CHAT_MESSAGE);
            writer.WriteFixedString128(message);
            m_Driver.EndSend(writer);
        }
    }

    public void LeaveServer()
    {
        DataStreamWriter writer;
        int result = m_Driver.BeginSend(m_Connection, out writer);
        if (result == 0)
        {
            writer.WriteUInt((uint)NetworkMessageType.CHAT_QUIT);
            m_Driver.EndSend(writer);
        }

        Done = true;

        m_Connection.Disconnect(m_Driver);
        m_Connection = default(NetworkConnection);
    }

    //Events

    static void HandleHandshakeResponse(object handler, NetworkConnection con, DataStreamReader stream)
    {
        
    }

    static void HandleServerMessage(object handler, NetworkConnection con, DataStreamReader stream)
    {

    }

    static void HandleServerExit(object handler, NetworkConnection con, DataStreamReader stream)
    {

    }
}
