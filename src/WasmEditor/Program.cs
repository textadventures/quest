using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("browser")]

await JSHost.ImportAsync("quest-editor.js", "../quest-editor.js");
