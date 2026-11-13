using System.IO.Compression;
using System.Reflection;

namespace QuestViva.Cli;

// The WasmPlayer bundle that `qv preview` serves: the AppBundle a `dotnet publish` of
// src/WasmPlayer makes (index.html, wasm-player.js, _framework/, ...).
//
// A release build of qv embeds it as player.zip (Cli.csproj's EmbedPlayerBundle, given the
// QvPlayerBundle property), so the player is the same version as the engine qv plays games with
// headlessly. A build without it - an ordinary `dotnet build` in a checkout - looks for the
// checkout's own WasmPlayer build instead, or is given a folder with --player.
internal abstract class PlayerBundle
{
    public abstract string Description { get; }

    // Whether it's the player built into qv, which only a release build has.
    public virtual bool BuiltIn => false;

    // The file at path, relative to the bundle's root with forward slashes, or null.
    public abstract byte[]? Read(string path);

    public static PlayerBundle? Embedded()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("player.zip");
        return stream == null ? null : new ZipBundle(stream);
    }

    public static PlayerBundle FromFolder(DirectoryInfo folder) => new FolderBundle(folder);

    // The checkout's WasmPlayer AppBundle, when qv runs from its own build output
    // (src/Cli/bin/<config>/net11.0/qv.dll): the more recently built of Debug and Release.
    public static PlayerBundle? FromCheckout()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var wasmPlayer = Path.Combine(dir.FullName, "src", "WasmPlayer", "bin");
            if (!Directory.Exists(wasmPlayer))
            {
                continue;
            }

            var newest = new[] { "Debug", "Release" }
                .Select(config => new FileInfo(Path.Combine(wasmPlayer, config, "net11.0", "browser-wasm", "AppBundle",
                    "_framework", "dotnet.js")))
                .Where(f => f.Exists)
                .MaxBy(f => f.LastWriteTimeUtc);
            return newest == null ? null : new FolderBundle(newest.Directory!.Parent!);
        }

        return null;
    }

    private sealed class ZipBundle : PlayerBundle
    {
        private readonly Dictionary<string, byte[]> _files = new(StringComparer.Ordinal);

        public ZipBundle(Stream stream)
        {
            // About 50 files and a few MB once a published bundle is unzipped: read it all once.
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
            foreach (var entry in zip.Entries.Where(e => e.Name.Length > 0))
            {
                using var entryStream = entry.Open();
                using var bytes = new MemoryStream();
                entryStream.CopyTo(bytes);
                _files[entry.FullName.Replace('\\', '/')] = bytes.ToArray();
            }
        }

        public override string Description => "the player built into qv";

        public override bool BuiltIn => true;

        public override byte[]? Read(string path) => _files.GetValueOrDefault(path);
    }

    private sealed class FolderBundle(DirectoryInfo folder) : PlayerBundle
    {
        private readonly string _root = Path.GetFullPath(folder.FullName) + Path.DirectorySeparatorChar;

        public override string Description => _root;

        public override byte[]? Read(string path)
        {
            var full = Path.GetFullPath(Path.Combine(_root, path));
            return full.StartsWith(_root, StringComparison.Ordinal) && File.Exists(full) ? File.ReadAllBytes(full) : null;
        }
    }
}
