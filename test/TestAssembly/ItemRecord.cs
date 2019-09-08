using System;

namespace TestAssembly
{
    public class ItemRecord
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int CurrentTicks { get; set; }

        public ItemRecord(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public void UpdateTick(int ticks)
        {
            CurrentTicks += ticks;
        }
    }
}
