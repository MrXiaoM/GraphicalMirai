using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicalMirai.LoginSolver
{
    public interface ILoginSolver
    {
        private static ILoginSolver? instance;
        public static ILoginSolver? Instance {
            get => instance;
            set
            {
                if (instance != null) return;
                instance = value;
            }
        }
    }

    public class DummyLoginSolver : ILoginSolver { }
}
