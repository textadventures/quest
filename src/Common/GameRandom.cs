namespace QuestViva.Common;

/// <summary>
/// A game's random numbers. Every game has a known seed, so a run can be repeated, and the count of
/// samples drawn so far, so a saved game can store its place in the sequence and a loaded one can
/// carry on from it.
/// </summary>
/// <remarks>
/// This wraps a seeded <see cref="Random"/>, which .NET keeps on its original algorithm for
/// compatibility, so a seed gives the same numbers it always has. Each call below draws one sample
/// from it, except <see cref="Next"/> over a range wider than <see cref="int.MaxValue"/>, which draws
/// two. <see cref="Restore"/> catches up by drawing that many samples with <see cref="Random.Next()"/>,
/// which also draws one.
/// </remarks>
public sealed class GameRandom
{
    private Random _random;

    public GameRandom() : this(Random.Shared.Next())
    {
    }

    public GameRandom(int seed)
    {
        Seed = seed;
        _random = new Random(seed);
    }

    public int Seed { get; private set; }

    /// <summary>How many samples have been drawn since the game started with <see cref="Seed"/>.</summary>
    public long Samples { get; private set; }

    /// <summary>Starts again from the beginning of <paramref name="seed"/>'s sequence.</summary>
    public void Reseed(int seed) => Restore(seed, 0);

    /// <summary>Goes to the point in <paramref name="seed"/>'s sequence after <paramref name="samples"/> samples.</summary>
    public void Restore(int seed, long samples)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(samples);
        var random = new Random(seed);
        for (long i = 0; i < samples; i++)
        {
            random.Next();
        }

        (_random, Seed, Samples) = (random, seed, samples);
    }

    /// <summary>A number from <paramref name="minValue"/> up to, but not including, <paramref name="maxValue"/>.</summary>
    public int Next(int minValue, int maxValue)
    {
        var result = _random.Next(minValue, maxValue);
        Samples += (long)maxValue - minValue > int.MaxValue ? 2 : 1;
        return result;
    }

    public double NextDouble()
    {
        var result = _random.NextDouble();
        Samples++;
        return result;
    }
}
