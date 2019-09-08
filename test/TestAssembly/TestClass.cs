using System;

namespace TestAssembly
{
    public class TestClass
    {
        public void RunTest(string name, string description, int ticksToBe)
        {
            var itemRecord = new ItemRecord(name, description);

            if (itemRecord.CurrentTicks != 0)
                throw new Exception("Un expected state");

            for (var i = 0; i < ticksToBe; i++)
            {
                itemRecord.UpdateTick(1);
            }

            if (itemRecord.CurrentTicks != ticksToBe)
                throw new Exception("Un expected state");
        }
    }
}
