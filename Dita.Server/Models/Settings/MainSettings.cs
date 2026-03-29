using Dita.Server.Models.Enums;

namespace Dita.Server.Models.Settings;

public class MainSettings
{
   public string ServerId { get; set; } = Guid.NewGuid().ToString();
   public string ServerName { get; set; } = "Dita 01";
   public string ServerIP { get; set; } = "127.0.0.1:5678";
   public ServerCapabilities Capabilities { get; set; } = ServerCapabilities.None;
   public bool MemberOfCluster { get; set; } = false;
   public bool AutoSync { get; set; } = false;
}