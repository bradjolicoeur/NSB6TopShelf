using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSBWindowsService
{
    public interface IDependency
    {
        void Write();
    }

    public class Dependency : IDependency
    {
        public void Write()
        {
            Console.WriteLine("Line from Service dependency");
        }
    }
}
