using Microsoft.AspNetCore.Components;

namespace WebPlayer.Components.Pages;

public partial class Quest : ComponentBase
{
    private readonly List<string> output = [];
    private string input = string.Empty;

    private void Submit()
    {
        if (input.Length == 0) return;
        
        output.Add(input);
        input = "";
    }
}