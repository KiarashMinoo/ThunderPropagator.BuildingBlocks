using ThunderPropagator.BuildingBlocks.Application.Collections;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.Collections
{
    public
#if !DEBUG
        sealed
#endif
        class LinkedArrayTests
    {
        [Fact]
        public void CreateLinkedArray()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            Assert.Equal(array.Length, linkedArray.Count);
            Assert.Equal(array, linkedArray.ToArray());
        }

        [Fact]
        public void EmptyLinkedArray()
        {
            var linkedArray = LinkedArray<int>.Empty;

            Assert.Empty(linkedArray);
            Assert.Empty(linkedArray.ToArray());
        }

        [Fact]
        public void ForEachAction()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            var sum = 0;
            linkedArray.ForEach(item => sum += item);

            Assert.Equal(array.Sum(), sum);
        }

        [Fact]
        public void ForEachActionWithIndex()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            var sum = 0;
            linkedArray.ForEach((index, item) => sum += index * item);

            Assert.Equal(0 + 2 + 6 + 12 + 20, sum);
        }

        [Fact]
        public void ForEachFunc()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            var squaredArray = linkedArray.ForEach(item => item * item);

            Assert.Equal(array.Select(x => x * x), squaredArray);
        }

        [Fact]
        public void ForEachFuncWithIndex()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            var squaredArray = linkedArray.ForEach((index, item) => index * item * item);

            Assert.Equal(array.Select((x, index) => index * x * x), squaredArray);
        }

        [Fact]
        public void IndexOf()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            Assert.Equal(2, linkedArray.IndexOf(3));
            Assert.Equal(-1, linkedArray.IndexOf(10));
        }

        [Fact]
        public void Insert()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            linkedArray.Insert(2, 10);

            Assert.Equal(6, linkedArray.Count);
            Assert.Equal(new[] { 1, 2, 10, 3, 4, 5 }, linkedArray.ToArray());
        }

        [Fact]
        public void RemoveAt()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            linkedArray.RemoveAt(2);

            Assert.Equal(4, linkedArray.Count);
            Assert.Equal(new[] { 1, 2, 4, 5 }, linkedArray.ToArray());
        }

        [Fact]
        public void Add()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            linkedArray.Add(6);

            Assert.Equal(6, linkedArray.Count);
            Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, linkedArray.ToArray());
        }

        [Fact]
        public void Contains()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            Assert.True(linkedArray.Contains(3));
            Assert.False(linkedArray.Contains(10));
        }

        [Fact]
        public void Remove()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            linkedArray.Remove(3);

            Assert.Equal(4, linkedArray.Count);
            Assert.Equal(new[] { 1, 2, 3, 5 }, linkedArray.ToArray());
        }

        [Fact]
        public void Clear()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            linkedArray.Clear();

            Assert.Empty(linkedArray);
            Assert.Empty(linkedArray.ToArray());
        }

        [Fact]
        public void ReadOnlyProperty()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            Assert.False(linkedArray.IsReadOnly);
        }

        [Fact]
        public void SetItemAllowsNewItem()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            linkedArray[2] = 10;

            Assert.Equal(10, linkedArray[2]);
            Assert.Equal(new[] { 1, 2, 10, 4, 5 }, linkedArray.ToArray());
        }

        [Fact]
        public void LinkedArrayEmpty()
        {
            var linkedArray = LinkedArray<int>.Empty;

            Assert.Empty(linkedArray);
            Assert.Empty(linkedArray.ToArray());
        }

        [Fact]
        public void Empty_ShouldReturnNewInstance_OnEachAccess()
        {
            var first = LinkedArray<int>.Empty;
            first.Add(1);

            var second = LinkedArray<int>.Empty;

            Assert.NotSame(first, second);
            Assert.Empty(second);
        }

        [Fact]
        public void ContainsInvalidIndexReturnsFalse()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            Assert.False(linkedArray.Contains(10));
            Assert.False(linkedArray.Contains(-1));
        }

        [Fact]
        public void RemoveInvalidIndexReturnsFalse()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var linkedArray = new LinkedArray<int>(array);

            Assert.False(linkedArray.Remove(10));
            Assert.False(linkedArray.Remove(-1));
        }
    }
}
