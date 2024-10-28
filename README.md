# PacketSender

#### 介绍
A packet sender which is used to send packet to target IP & port

#### 使用说明

The format of command

#### send packet

```send packet#```

#### update parameter

```set packet count#config-json```

config json model:

```c#
/// <summary>
/// ConfigModel
/// </summary>
public class ConfigModel
{
	/// <summary>
	/// Packet count
	/// </summary>
    public int PacketCount { get; set; }

    /// <summary>
    /// Packet interval
    /// </summary>
    public int PacketInterval { get; set; }
}
```



