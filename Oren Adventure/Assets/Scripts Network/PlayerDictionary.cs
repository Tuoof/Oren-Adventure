using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace oren_Network
{
    public class PlayerDictionary : NetworkBehaviour
    {
        public static PlayerDictionary Singleton { get; set; }
        public Dictionary<ulong, bool> m_Clients = new Dictionary<ulong, bool>();

        // Start is called before the first frame update
        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

