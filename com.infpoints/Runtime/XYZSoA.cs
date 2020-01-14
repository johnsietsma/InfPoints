using Unity.Collections;

namespace InfPoints
{
    public struct XYZSoA<T> where T :unmanaged
    {
        public NativeArray<T> X;
        public NativeArray<T> Y;
        public NativeArray<T> Z;

    }
}