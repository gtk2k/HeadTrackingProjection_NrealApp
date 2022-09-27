using System;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

public class WebSocketManager
{
    public event Action OnOpen;

    private WebSocket _ws;
    private SynchronizationContext _ctx;

    public enum MessageType : byte
    {
        None = 0,
        ProjectorAppReset = 1,
        MarkerPose = 2,
        PlayerPose = 3
    }

    public WebSocketManager(string url)
    {
        _ctx = SynchronizationContext.Current;
        _ws = new WebSocket(url);
        _ws.OnOpen += Ws_OnOpen;
        _ws.OnClose += Ws_OnClose;
        _ws.OnError += Ws_OnError;
    }

    private void Ws_OnOpen(object sender, System.EventArgs e)
    {
        _ctx.Post(_ =>
        {
            Debug.Log($"WS OnOpen");
            OnOpen?.Invoke();
        }, null);
    }

    private void Ws_OnClose(object sender, CloseEventArgs e)
    {
        _ctx.Post(_ =>
        {
            Debug.Log($"WS OnClose > code: {e.Code}, reason:{e.Reason}");
        }, null);
    }

    private void Ws_OnError(object sender, ErrorEventArgs e)
    {
        _ctx.Post(_ =>
        {
            Debug.LogError($"WS OnClose > {e.Message}");
        }, null);
    }

    public void Connect()
    {
        _ws.Connect();
    }

    public void Close()
    {
        if (_ws != null)
        {
            _ws.Close();
        }
        _ws = null;
    }

    public void SendAppReset()
    {
        Debug.Log($"SendAppReset()");
        var data = new byte[1];
        data[0] = (byte)MessageType.ProjectorAppReset;
        _ws.Send(data);
    }

    public void SendPose(MessageType type, Vector3 position, Quaternion rotation)
    {
        var px = BitConverter.GetBytes(position.x);
        var py = BitConverter.GetBytes(position.y);
        var pz = BitConverter.GetBytes(position.z);
        var rx = BitConverter.GetBytes(rotation.x);
        var ry = BitConverter.GetBytes(rotation.y);
        var rz = BitConverter.GetBytes(rotation.z);
        var rw = BitConverter.GetBytes(rotation.w);
        //var sx = BitConverter.GetBytes(scale.x);
        //var sy = BitConverter.GetBytes(scale.y);
        //var sz = BitConverter.GetBytes(scale.z);
        var data = new byte[7 * 4 + 1];
        data[0] = (byte)type;
        Buffer.BlockCopy(px, 0, data, 1, 4);
        Buffer.BlockCopy(py, 0, data, 5, 4);
        Buffer.BlockCopy(pz, 0, data, 9, 4);
        Buffer.BlockCopy(rx, 0, data, 13, 4);
        Buffer.BlockCopy(ry, 0, data, 17, 4);
        Buffer.BlockCopy(rz, 0, data, 21, 4);
        Buffer.BlockCopy(rw, 0, data, 25, 4);
        //Buffer.BlockCopy(sx, 0, data, 29, 4);
        //Buffer.BlockCopy(sy, 0, data, 33, 4);
        //Buffer.BlockCopy(sz, 0, data, 37, 4);

        _ws.Send(data);
    }
}
