using System;
using System.Reflection.Emit;

namespace Sample
{
    public class Class1
    {
        private static void ForceGc()
        {
            for (var i = 0; i <= GC.MaxGeneration; i++)
                GC.Collect(i, GCCollectionMode.Forced, true);
            GC.Collect();
            GC.WaitForPendingFinalizers();

            new DynamicMethod()
        }
    }
}
