using System;
namespace GVMetting.CommonServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceCore.Instance.StartListener();
            Console.Read();
            ServiceCore.Instance.StopListener();
        }
    }
}
