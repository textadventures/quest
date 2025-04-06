using Microsoft.AspNetCore.Components.Server.Circuits;

namespace QuestViva.WebPlayer;

public class DisconnectionHandler : CircuitHandler
{
    public event Func<Task>? OnCircuitDisconnected;

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        return OnCircuitDisconnected?.Invoke() ?? Task.CompletedTask;
    }
}