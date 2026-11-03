using System.Collections;
using System.Diagnostics.CodeAnalysis;
using QuestViva.Common;

namespace QuestViva.Cli;

internal static class GameFiles
{
    // An .aslx file can include libraries and reference resources that sit next to it, so files
    // in its folder are made available to the game, read when the game asks for them.
    public static GameData Load(FileInfo file)
    {
        if (!file.Exists)
        {
            throw new FileNotFoundException($"File not found: {file.FullName}");
        }

        var bytes = File.ReadAllBytes(file.FullName);
        return new GameData(bytes, file.FullName, new FolderFiles(file.DirectoryName!));
    }

    // Read-only view of the files under a folder, keyed by path relative to it. Only lookups are
    // needed; enumerating would mean reading the whole folder, so it yields nothing.
    private sealed class FolderFiles(string folder) : IReadOnlyDictionary<string, byte[]>
    {
        private readonly string _folder = Path.GetFullPath(folder) + Path.DirectorySeparatorChar;

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out byte[] value)
        {
            var path = Path.GetFullPath(Path.Combine(_folder, key));
            // A game can't read files outside its own folder.
            if (!path.StartsWith(_folder, StringComparison.Ordinal) || !File.Exists(path))
            {
                value = null;
                return false;
            }

            value = File.ReadAllBytes(path);
            return true;
        }

        public bool ContainsKey(string key) => TryGetValue(key, out _);

        public byte[] this[string key] =>
            TryGetValue(key, out var value) ? value : throw new KeyNotFoundException(key);

        public IEnumerable<string> Keys => [];
        public IEnumerable<byte[]> Values => [];
        public int Count => 0;
        public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator() => Enumerable.Empty<KeyValuePair<string, byte[]>>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
