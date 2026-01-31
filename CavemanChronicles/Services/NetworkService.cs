// Services/NetworkService.cs
public interface INetworkService
{
    Task<bool> ConnectViaBluetooth(string deviceId);
    Task<bool> ConnectViaIP(string ipAddress, int port);
    Task SendBattleData(BattleData data);
    Task<BattleData> ReceiveBattleData();
}