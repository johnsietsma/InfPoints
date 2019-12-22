using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace InfPoints.Octree
{
    /// <summary>
    /// Unsafe pointer algorithms. <see cref="NativeCollectionExtensions" for documentation./>
    /// </summary>
    public static unsafe class NativeCollectionUnsafe
    {
        /// <summary>
        /// <see cref="NativeCollectionExtensions.Swap"/> 
        /// </summary>
        public static void Swap<T>(void* ptr, int i, int j) 
#if CSHARP_7_3_OR_NEWER
            where T : unmanaged
#else
	    	where T : struct
#endif
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (ptr == null) throw new ArgumentNullException(nameof(ptr));
            if (i == j) throw new ArgumentException("Swap indices are the same");
#endif
            T temp = UnsafeUtility.ReadArrayElement<T>(ptr, i);
            UnsafeUtility.WriteArrayElement(ptr, i, UnsafeUtility.ReadArrayElement<T>(ptr, j));
            UnsafeUtility.WriteArrayElement(ptr, j, temp);
        }

        /// <see cref="NativeCollectionExtensions.Insert"/> 
        public static void Insert<T>(void* ptr, int index, int length, T value) 
#if CSHARP_7_3_OR_NEWER
            where T : unmanaged
#else
	    	where T : struct
#endif
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (ptr == null) throw new ArgumentNullException(nameof(ptr));
            if (index < 0 || index >= length) throw new ArgumentOutOfRangeException(nameof(index));
#endif

            void* src = (byte*) ptr + index * UnsafeUtility.SizeOf<T>();
            void* dest = (byte*) src + UnsafeUtility.SizeOf<T>();

            long moveLength = length - index;
            if (moveLength > 0) // Check for writing to the last element
            {
                UnsafeUtility.MemMove(dest, src, moveLength);
            }

            UnsafeUtility.WriteArrayElement(ptr, index, value);
        }
        
        /// <see cref="NativeCollectionExtensions.RemoveAt"/> 
        public static void RemoveAt<T>(void* ptr, int index, int length) 
#if CSHARP_7_3_OR_NEWER
            where T : unmanaged
#else
	    	where T : struct
#endif
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (ptr == null) throw new ArgumentNullException(nameof(ptr));
            if (index < 0 || index >= length) throw new ArgumentOutOfRangeException(nameof(index));
#endif

            void* dest = (byte*) ptr + index * UnsafeUtility.SizeOf<T>();
            void* src = (byte*) dest + UnsafeUtility.SizeOf<T>();

            long moveLength = length - index;
            if (moveLength > 0) // Check for removing to the last element
            {
                UnsafeUtility.MemMove(dest, src, moveLength);
            }

            UnsafeUtility.WriteArrayElement(ptr, length-1, default(T));
        }
        
        public static int BinarySearch<T>(void* ptr, T key, int startIndex, int count)
#if CSHARP_7_3_OR_NEWER
            where T : unmanaged
#else
	    	where T : struct
#endif
            , IComparable<T>
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (ptr == null) throw new ArgumentNullException(nameof(ptr));
            if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
#endif

            int min = startIndex;
            int max = startIndex + count;
            while (min <= max)
            {
                int mid = min + (max - min >> 1);
                T midValue = UnsafeUtility.ReadArrayElement<T>(ptr, mid);
                int compare = key.CompareTo(midValue);
                if (compare == 0)
                {
                    return mid;
                }
                if (compare < 0)
                {
                    min = mid + 1;
                }
                else
                {
                    max = mid - 1;
                }
            }

            return ~min;
        }
    }
    
}