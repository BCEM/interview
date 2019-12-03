using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InterviewTask
{
    public class Items<TKey>
    {
        private int time = 0;
        public int Now => Interlocked.Increment(ref time);       

        private class Item
        {
            public TKey Id { get; set; }

            public bool IsDeleted { get; set; }

            public int UpdatedAt { get; set; }
        }

        private readonly Dictionary<TKey, Item> items = new Dictionary<TKey, Item>();
        

        public bool Add(TKey id)
        {
            if (items.ContainsKey(id))
                return false;

            items.Add(id, new Item
            {
                Id = id,
                UpdatedAt = Now
            });
            return true;
        }

        public void Update(TKey id)
        {
            if (!items.ContainsKey(id))
                throw new Exception($"Element with key '{id}' not found");
            
            var item = items[id];

            if (item.IsDeleted)
            {
                item.IsDeleted = false;
                item.UpdatedAt = Now;
            }
        }

        public void Delete(TKey id)
        {
            if (!items.ContainsKey(id))
                throw new Exception($"Element with key '{id}' not found");

            var item = items[id];

            if (!item.IsDeleted)
            {
                item.IsDeleted = true;
                item.UpdatedAt = Now;
            }
        }

        public IEnumerable<TKey> GetActiveItems()
        {
            return items.Values
                .Where(i => !i.IsDeleted)
                .OrderByDescending(i => i.Id)
                .Select(i => i.Id);
        }

        public IEnumerable<TKey> GetDeletedItems()
        {
            return items.Values.Where(i => i.IsDeleted).OrderBy(i => i.UpdatedAt).Select(i => i.Id);
        }

        public void SetActiveItems(IEnumerable<TKey> ids)
        {            
            foreach (var id in ids)
            {
                if (items.ContainsKey(id))
                {
                    Update(id);
                }
                else
                {
                    Add(id);
                }
            }

            foreach (var id in items.Keys.Except(ids))
            {
                Delete(id);
            }            
        }
    }

    class ItemsTest
    {
        static void Main(String[] args)
        {
            try
            {
                var items = new Items<int>();
                items.Add(0);
                items.Add(1);
                items.Add(2);
                items.Delete(2);
                items.Add(3);
                items.Delete(3);

                items.SetActiveItems(new[] { 1, 2, 4 });

                items.Add(5);

                var expected = new[] { 5, 4, 2, 1 };
                var actual = items.GetActiveItems();
                if (!expected.SequenceEqual(actual))
                    throw new Exception($"Incorrect answer. Expected '{string.Join(", ", expected)}'. Was '{string.Join(", ", actual)}'");

                expected = new[] { 3, 0 };
                actual = items.GetDeletedItems();
                if (!expected.SequenceEqual(actual))
                    throw new Exception($"Incorrect answer. Expected '{string.Join(", ", expected)}'. Was '{string.Join(", ", actual)}'");

                var items2 = new Items<int>();
                for (var i = 0; i < 10000; i++)
                    items2.Add(i);

                if (Task.Run(() => items2.SetActiveItems(Enumerable.Range(5000, 15000).OrderBy(a => Guid.NewGuid())))
                    .Wait(TimeSpan.FromSeconds(1)))
                {
                    if (!Enumerable.Range(5000, 15000).SequenceEqual(items2.GetActiveItems().OrderBy(x => x)))
                        throw new Exception($"Incorrect answer");
                    else
                        Console.WriteLine("Task solved");
                }
                else
                    throw new Exception("Task is not solved optimally");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Task not solved: {e.Message}!");
            }

            Console.ReadKey();
        }
    }
}
