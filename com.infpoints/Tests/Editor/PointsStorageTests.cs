using NUnit.Framework;
using Unity.Collections;

namespace InfPoints.Tests.Editor
{
    public class PointsStorageTests
    {
        [Test]
        public void StorageIsFull()
        {
            using (var pointsStorage = new PointsStorage(2, Allocator.TempJob))
            using( var points = new XYZSoA<float>(1, Allocator.TempJob))
            {
                Assert.That(pointsStorage.IsFull, Is.False);
                pointsStorage.AddPoints(points);
                Assert.That(pointsStorage.IsFull, Is.False);
                pointsStorage.AddPoints(points);
                Assert.That(pointsStorage.IsFull, Is.True);
            }
        }
    }
}