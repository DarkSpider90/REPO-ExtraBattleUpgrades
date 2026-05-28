using Photon.Pun;
using UnityEngine;

namespace ExtraBattleUpgrades.Visuals;

internal sealed class ShockGripVisualRelay : MonoBehaviour
{
    private PlayerAvatar _player;
    private PhotonView _photonView;

    private void Awake()
    {
        _player = GetComponent<PlayerAvatar>();
        _photonView = GetComponent<PhotonView>();
    }

    internal static void Ensure(PlayerAvatar player)
    {
        if (player == null)
        {
            return;
        }

        if (player.GetComponent<ShockGripVisualRelay>() == null)
        {
            player.gameObject.AddComponent<ShockGripVisualRelay>();
        }
    }

    internal static void Broadcast(PlayerAvatar holder)
    {
        if (holder == null)
        {
            return;
        }

        Ensure(holder);

        PhotonView photonView = holder.photonView;
        if (photonView == null)
        {
            return;
        }

        if (GameManager.Multiplayer())
        {
            photonView.RPC(nameof(ShockGripVisualRPC), RpcTarget.All);
        }
        else
        {
            ShockGripVisuals.PlayLocal(holder);
        }
    }

    [PunRPC]
    private void ShockGripVisualRPC(PhotonMessageInfo info = default)
    {
        if (_player == null)
        {
            _player = GetComponent<PlayerAvatar>();
        }

        ShockGripVisuals.PlayLocal(_player);
    }
}