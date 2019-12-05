namespace InfPoints.Common
{
    using System;
    using System.Diagnostics;

    public class AssertionException : System.Exception
    {
        public AssertionException(string message) : base(message) { }
    }

    public static class Assert
    {
        [Conditional("DEBUG")]
        public static void IsTrue(bool value, string message = null)
        {
            if (!value) DoAssertion(message ?? "Assertion failed.");
        }

        public static void IsFalse(bool value, string message = null)
        {
            if (value) DoAssertion(message ?? "Assertion failed.");
        
        }

        public static void IsEqual<T>(T value1, T value2, string message = null)
        {
            if (value1.Equals(value2)) DoAssertion(message ?? $"{value1} does not equal {value2}");
        }

        public static void IsNotEqual<T>(T value1, T value2, string message = null)
        {
            if (!value1.Equals(value2)) DoAssertion(message ?? $"{value1} equals {value2}");
        }

        public static void IsGreaterThan<T>(T value1, T value2, string message = null) where T: IComparable
        {
            if (value1.CompareTo(value2)>0) DoAssertion(message ?? $"{value1} is not greater than {value2}");
        }

        public static void IsGreaterThanOrEqualTo<T>(T value1, T value2, string message = null) where T: IComparable
        {
            if (value1.CompareTo(value2)>=0) DoAssertion(message ?? $"{value1} is not greater than or equal to {value2}");
        }

        public static void IsLessThan<T>(T value1, T value2, string message = null) where T: IComparable
        {
            if (value1.CompareTo(value2)<0) DoAssertion(message ?? $"{value1} is not less than {value2}");
        }

        public static void IsLessThanOrEqualTo<T>(T value1, T value2, string message = null) where T: IComparable
        {
            if (value1.CompareTo(value2)>=0) DoAssertion(message ?? $"{value1} is not less than or equal to {value2}");
        }

        private static void DoAssertion(string message)
        {
            throw new AssertionException(message);
        }
    }

}