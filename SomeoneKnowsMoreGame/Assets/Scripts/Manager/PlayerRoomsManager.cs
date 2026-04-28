using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerRoom
{
    public uint roomNumber;
    public Transform spawnPos;
    public DoorSystem roomDoor;
}

[Serializable]
public struct PlayerRoomsData
{
    public uint netId;
    public uint roomNumber;
    public Vector3 spawnPosition;
    public uint doorNetId;
}

public class PlayerRoomsManager : NetworkBehaviour
{
    [SerializeField] private List<PlayerRoom> playerRooms;
    public List<PlayerRoom> PlayerRooms => playerRooms;
    public static PlayerRoomsManager Instance { get; private set; }

    private Dictionary<uint, PlayerRoom> occupiedRoomsByPlayer = new Dictionary<uint, PlayerRoom>();
    private HashSet<uint> occupiedRoomNumbers = new HashSet<uint>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Server]
    public void SetPlayerRoomsData(uint netId)
    {
        PlayerRoom room = FindRandomEmptyRoomAndReserve(netId);
        if (room == null)
        {
            Debug.LogError($"No empty rooms available for player {netId}");
            return;
        }

        NetworkIdentity doorIdentity = room.roomDoor.GetComponentInParent<NetworkIdentity>();
        if (doorIdentity == null)
        {
            Debug.LogError($"Door {room.roomDoor.name} has no NetworkIdentity!");
            occupiedRoomNumbers.Remove(room.roomNumber);
            occupiedRoomsByPlayer.Remove(netId);
            return;
        }

        room.roomDoor.ServerAddPlayerAccess(netId);

        PlayerRoomsData newPlayer = new PlayerRoomsData
        {
            netId = netId,
            roomNumber = room.roomNumber,
            spawnPosition = room.spawnPos.position,
            doorNetId = doorIdentity.netId
        };

        if (NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity networkIdentity))
        {
            GameObject playerObject = networkIdentity.gameObject;
            PlayerInfo playerInfo = playerObject.GetComponent<PlayerComponentData>().PlayerInfo;

            if (playerInfo != null)
                playerInfo.ServerSetPlayerRoomsData(newPlayer);
            else
                Debug.LogError($"Player {netId} has no PlayerInfo component");
        }
    }

    [Server]
    public void RemoveOccupiedRoom(uint netId)
    {
        if (occupiedRoomsByPlayer.TryGetValue(netId, out PlayerRoom room))
        {
            if (room.roomDoor != null)
                room.roomDoor.ServerRemovePlayerAccess(netId);
            occupiedRoomNumbers.Remove(room.roomNumber);
            occupiedRoomsByPlayer.Remove(netId);
        }
    }

    private PlayerRoom FindRandomEmptyRoomAndReserve(uint netId)
    {
        List<PlayerRoom> freeRooms = new List<PlayerRoom>();
        foreach (var room in playerRooms)
        {
            if (!occupiedRoomNumbers.Contains(room.roomNumber))
                freeRooms.Add(room);
        }

        if (freeRooms.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, freeRooms.Count);
        PlayerRoom selectedRoom = freeRooms[randomIndex];
        occupiedRoomNumbers.Add(selectedRoom.roomNumber);
        occupiedRoomsByPlayer[netId] = selectedRoom;
        return selectedRoom;
    }
}