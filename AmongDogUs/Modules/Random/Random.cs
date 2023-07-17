namespace AmongDogUs.Modules;

internal class MersenneTwister
{
    private const int N = 624;
    private const int M = 397;
    private const uint MatrixA = 0x9908b0df;
    private const uint UpperMask = 0x80000000;
    private const uint LowerMask = 0x7fffffff;
    private const uint TemperingMaskB = 0x9d2c5680;
    private const uint TemperingMaskC = 0xefc60000;

    private static readonly uint[] Mag01 = { 0x0, MatrixA };

    private readonly uint[] _mt = new uint[N];
    private int _mtIndex;

    internal ulong Seed { get; set; }

    internal MersenneTwister() : this((ulong)Guid.NewGuid().GetHashCode()) { }

    internal MersenneTwister(ulong seed)
    {
        Seed = seed;
        Init();
    }

    private void Init()
    {
        _mt[0] = (uint)Seed;
        for (int i = 1; i < N; i++)
        {
            _mt[i] = (uint)(1812433253U * (_mt[i - 1] ^ (_mt[i - 1] >> 30)) + i);
        }
    }

    private uint Generate()
    {
        if (_mtIndex >= N) Twist();

        uint y = _mt[_mtIndex++];
        y ^= y >> 11;
        y ^= (y << 7) & TemperingMaskB;
        y ^= (y << 15) & TemperingMaskC;
        y ^= y >> 18;

        return y;
    }

    private void Twist()
    {
        for (int i = 0; i < N; i++)
        {
            uint y = (_mt[i] & UpperMask) | (_mt[(i + 1) % N] & LowerMask);
            _mt[i] = _mt[(i + M) % N] ^ (y >> 1) ^ Mag01[y & 0x1];
        }

        _mtIndex = 0;
    }

    internal int Next(int min, int max)
    {
        if (min < 0 || max < 0) throw new ArgumentException("Each of the values must be bigger than 0.");
        else if (min > max) throw new ArgumentException("Max value must be bigger than min value.");
        else if (min == max) return min;

        return (int)(min + (Generate() % (max - min)));
    }
    internal int Next(int max) => Next(0, max);
    internal int Next() => Next(0, int.MaxValue);
}