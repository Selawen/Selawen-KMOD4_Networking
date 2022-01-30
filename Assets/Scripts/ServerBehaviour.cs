using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

using Unity.Collections;
using Unity.Networking.Transport;

namespace Network
{
    public class ServerBehaviour : MonoBehaviour
    {
        static Dictionary<NetworkMessageType, NetworkMessageHandler> networkMessageHandlers = new Dictionary<NetworkMessageType, NetworkMessageHandler> {
            { NetworkMessageType.HANDSHAKE, HandleClientHandshake },
            { NetworkMessageType.GAME_START, HandleGameStart },
            { NetworkMessageType.CHAT_MESSAGE, HandleClientMessage },
            { NetworkMessageType.CHAT_QUIT, HandleClientExit },
        };

        public NetworkDriver m_Driver;
        public NetworkPipeline m_Pipeline;
        private NativeList<NetworkConnection> m_Connections;

        private Dictionary<NetworkConnection, string> nameList = new Dictionary<NetworkConnection, string>();

        private bool gameStarted = false;
        [SerializeField] private ServerGameManager manager;
        public int playercount;

        [SerializeField] private TextMeshProUGUI messageText;

        // Start is called before the first frame update
        void Start()
        {
            manager = GetComponent<ServerGameManager>();
            manager.enabled = false;
            m_Driver = NetworkDriver.Create();
            m_Pipeline = m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

            var endpoint = NetworkEndPoint.AnyIpv4;
            endpoint.Port = 1511;
            if (m_Driver.Bind(endpoint) != 0)
                Debug.Log("Failed to bind to port 1511");
            else
            {
                Debug.Log("ip address: " + endpoint.Address);
                m_Driver.Listen();
            }

            m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        }

        // Update is called once per frame
        void Update()
        {
            m_Driver.ScheduleUpdate().Complete();

            // Clean up connections
            for (int i = 0; i < m_Connections.Length; i++)
            {
                if (nameList.ContainsKey(m_Connections[i]))
                {
                    //chat.NewMessage($"{ nameList[m_Connections[i]]} has disconnected.", ChatCanvas.leaveColor);
                    //nameList.Remove(m_Connections[i]);
                }

                if (!m_Connections[i].IsCreated)
                {
                    m_Connections.RemoveAtSwapBack(i);
                    --i;
                }
            }

            // Accept new connections
            NetworkConnection c;
            while ((c = m_Driver.Accept()) != default(NetworkConnection))
            {
                m_Connections.Add(c);
                messageText.text = messageText.text + "\n Accepted a connection";
                Debug.Log("Accepted a connection");
            }

            DataStreamReader stream;
            for (int i = 0; i < m_Connections.Length; i++)
            {
                if (!m_Connections[i].IsCreated)
                    continue;
                NetworkEvent.Type cmd;
                while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        NetworkMessageType msgType = (NetworkMessageType)stream.ReadUInt();

                        if (networkMessageHandlers.ContainsKey(msgType))
                        {
                            messageText.text = messageText.text + "\n message received: " + msgType;
                            networkMessageHandlers[msgType].Invoke(this, m_Connections[i], stream);
                        }
                        else
                        {
                            Debug.LogWarning($"Unsupported message type received: {msgType}", this);
                            messageText.text = messageText.text + "\n Unsupported message type received";
                        }
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        messageText.text = messageText.text + "\n Client disconnected from server";
                        Debug.Log("Client disconnected from server");
                        m_Connections[i] = default(NetworkConnection);
                    }
                }
            }
        }


        public void OnDestroy()
        {
            m_Driver.Dispose();
            m_Connections.Dispose();
        }

        public void NoteSpawn(int pos, int player)
        {
            messageText.text = messageText.text + "\n Spawn note";

            uint position = (uint)pos;
            uint forPlayer = (uint)player;

            DataStreamWriter writer;
            for (int i = 0; i < m_Connections.Length; i++)
            {
                if (!m_Connections[i].IsCreated)
                    continue;
                int result = m_Driver.BeginSend(m_Connections[i], out writer);
                if (result == 0)
                {
                    writer.WriteUInt((uint)NetworkMessageType.NOTE_SPAWN);
                    writer.WriteUInt(position);
                    writer.WriteUInt(forPlayer);
                    m_Driver.EndSend(writer);
                }
            }
        }


        static void HandleClientHandshake(object handler, NetworkConnection connection, DataStreamReader stream)
        {
            // Pop name
            FixedString128 str = stream.ReadFixedString128();

            ServerBehaviour serv = handler as ServerBehaviour;

            // Add to list
            serv.nameList.Add(connection, str.ToString());
            serv.messageText.text = serv.messageText.text + "\n" + str.ToString() + " has joined the chat";
            //serv.chat.NewMessage($"{str.ToString()} has joined the chat.", ChatCanvas.joinColor);

            // Send message back
            DataStreamWriter writer;
            int result = serv.m_Driver.BeginSend(NetworkPipeline.Null, connection, out writer);

            // non-0 is an error code
            if (result == 0)
            {
                writer.WriteUInt((uint)NetworkMessageType.HANDSHAKE_RESPONSE);
                writer.WriteFixedString128(new FixedString128($"Welcome {str.ToString()}!"));

                serv.m_Driver.EndSend(writer);
            }
            else
            {
                Debug.LogError($"Could not write message to driver: {result}", serv);
            }
        }

        static void HandleClientMessage(object handler, NetworkConnection connection, DataStreamReader stream)
        {
            // Pop message
            FixedString128 str = stream.ReadFixedString128();

            ServerBehaviour serv = handler as ServerBehaviour;

            if (serv.nameList.ContainsKey(connection))
            {
                //serv.chat.NewMessage($"{serv.nameList[connection]}: {str.ToString()}", ChatCanvas.chatColor);
            }
            else
            {
                Debug.LogError($"Received message from unlisted connection: {str}");
            }
        }        
        
        static void HandleGameStart(object handler, NetworkConnection connection, DataStreamReader stream)
        {
            ServerBehaviour serv = handler as ServerBehaviour;

            if (serv.nameList.ContainsKey(connection))
            {
                serv.messageText.text = serv.messageText.text + "\n Game start";
                serv.gameStarted = true;

                serv.playercount = 0;
                for (int i = 0; i < serv.m_Connections.Length; i++)
                {
                    if (!serv.m_Connections[i].IsCreated || serv.playercount >= 3)
                        continue;

                    // Send message back
                    DataStreamWriter writer;
                    int result = serv.m_Driver.BeginSend(NetworkPipeline.Null, connection, out writer);

                    // non-0 is an error code
                    if (result == 0)
                    {
                        writer.WriteUInt((uint)NetworkMessageType.GAME_START);
                        writer.WriteUInt((uint)serv.playercount);

                        serv.m_Driver.EndSend(writer);
                    }
                    else
                    {
                        Debug.LogError($"Could not write message to driver: {result}", serv);
                    }
                    serv.playercount++;
                    //serv.chat.NewMessage($"{serv.nameList[connection]}: {str.ToString()}", ChatCanvas.chatColor);
                }
                serv.manager.enabled = true;
                serv.messageText.text = serv.messageText.text + "\n players in game: " + serv.playercount;

            }
            else
            {
                serv.messageText.text = serv.messageText.text + "\n Received message from unlisted connection";
                Debug.LogError($"Received message from unlisted connection");
            }
        }

        static void HandleClientExit(object handler, NetworkConnection connection, DataStreamReader stream)
        {
            ServerBehaviour serv = handler as ServerBehaviour;

            if (serv.nameList.ContainsKey(connection))
            {
                //serv.chat.NewMessage($"{serv.nameList[connection]} has left the chat.", ChatCanvas.leaveColor);
                connection.Disconnect(serv.m_Driver);
            }
            else
            {
                Debug.LogError("Received exit from unlisted connection");
            }
        }
    }
}
