using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using UnityEngine.SceneManagement;

using Unity.Collections;
using Unity.Networking.Transport;

namespace Network
{
    public delegate void NetworkMessageHandler(object handler, NetworkConnection con, DataStreamReader stream);

    public enum NetworkMessageType
    {
        HANDSHAKE,
        HANDSHAKE_RESPONSE,
        GAME_START,
        NOTE_SPAWN,
        CHAT_MESSAGE,
        CHAT_QUIT
    }

    public class ClientBehaviour : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;

        public bool connected = false;

        public string ipAddress = "0.0.0.0";
        static Dictionary<NetworkMessageType, NetworkMessageHandler> networkMessageHandlers = new Dictionary<NetworkMessageType, NetworkMessageHandler> {
            { NetworkMessageType.HANDSHAKE_RESPONSE, HandleHandshakeResponse },
            { NetworkMessageType.GAME_START, HandleGameStart },
            { NetworkMessageType.NOTE_SPAWN, HandleNoteSpawn},
            { NetworkMessageType.CHAT_MESSAGE, HandleServerMessage },
            { NetworkMessageType.CHAT_QUIT, HandleServerExit },
        };


        public NetworkDriver m_Driver;
        public NetworkConnection m_Connection;
        public bool Done;

        public int playerNumber;

        // Start is called before the first frame update
        void Start()
        {
            DontDestroyOnLoad(this.gameObject);

            m_Driver = NetworkDriver.Create();
            m_Connection = default(NetworkConnection);

            var endpoint = NetworkEndPoint.Parse(ipAddress, 1511);
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
                    connected = true;
                    Debug.Log("We are now connected to the server");
                    FixedString128 message = nameText.text;
                    if (nameText.text == "" || nameText.text == null || nameText.text.Length <= 1)
                        message = "Anonymous";
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
                    NetworkMessageType msgType = (NetworkMessageType)stream.ReadUInt();

                    if (networkMessageHandlers.ContainsKey(msgType))
                    {
                        networkMessageHandlers[msgType].Invoke(this, m_Connection, stream);
                    }
                    else
                    {
                        Debug.LogWarning($"Unsupported message type received: {msgType}", this);
                    }
                    //FixedString128 value = stream.ReadFixedString128();
                    //Debug.Log("Got the value = " + value.ToString() + " back from the server");
                    //Done = true;
                    //m_Connection.Disconnect(m_Driver);
                    //m_Connection = default(NetworkConnection);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    connected = false;
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
            //Debug.Log("attempting to join server");
            if (!(m_Connection == default(NetworkConnection)))
            {
                m_Connection = default(NetworkConnection);

                var endpoint = NetworkEndPoint.Parse(ipAddress, 1511); ;
                if (ipAddress == "0.0.0.0")
                {
                endpoint = NetworkEndPoint.LoopbackIpv4;
                endpoint.Port = 1511;
                }

                m_Connection = m_Driver.Connect(endpoint);

                Done = false;
            }
        }

        public void JoinServer(string ip)
        {
            ipAddress = ip;
            //Debug.Log("attempting to join server");
            if (!(m_Connection == default(NetworkConnection)))
            {
                m_Connection = default(NetworkConnection);

                var endpoint = NetworkEndPoint.Parse(ipAddress, 1511); ;
                if (ipAddress == "0.0.0.0")
                {
                    endpoint = NetworkEndPoint.LoopbackIpv4;
                    endpoint.Port = 1511;
                }

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

        public void StartGame()
        {
            DataStreamWriter writer;
            int result = m_Driver.BeginSend(m_Connection, out writer);
            if (result == 0)
            {
                writer.WriteUInt((uint)NetworkMessageType.GAME_START);
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
            connected = false;

            m_Connection.Disconnect(m_Driver);
            m_Connection = default(NetworkConnection);
        }

        //Events

        static void HandleHandshakeResponse(object handler, NetworkConnection con, DataStreamReader stream)
        {
            // Pop name
            FixedString128 str = stream.ReadFixedString128();
            Debug.Log(str.ToString());
            UpdateMessage(str.ToString());
        }

        static void HandleServerMessage(object handler, NetworkConnection con, DataStreamReader stream)
        {

        }

        static void HandleGameStart(object handler, NetworkConnection con, DataStreamReader stream)
        {
            ClientBehaviour client = handler as ClientBehaviour;

            Debug.Log("start game");

            client.playerNumber = (int)stream.ReadUInt();
            Debug.Log(client.playerNumber);
            SceneManager.LoadScene(1);
            FindObjectOfType<GameManager>().SetPlayerColour(client.playerNumber);
        }        
        
        static void HandleNoteSpawn(object handler, NetworkConnection con, DataStreamReader stream)
        {
            ClientBehaviour client = handler as ClientBehaviour;
            FindObjectOfType<GameManager>().SetPlayerColour(client.playerNumber);

            int pos = (int)stream.ReadUInt();
            int colour = (int)stream.ReadUInt();

            GameManager manager = FindObjectOfType<GameManager>();
            manager.SpawnNote(pos, colour);
        }

        static void HandleServerExit(object handler, NetworkConnection con, DataStreamReader stream)
        {

        }

        public static void UpdateMessage(string message)
        {
            GameObject.Find("MessageTxt").GetComponent<TextMeshProUGUI>().text = message;
        }
    }
}
