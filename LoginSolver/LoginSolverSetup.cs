namespace GraphicalMirai.LoginSolver
{
    public class LoginSolverSetup
    {
        private static Lazy<LoginSolverSetup> instance = new(() => new LoginSolverSetup());
        public static LoginSolverSetup Instance => instance.Value;
        private LoginSolverSetup()
        {

        }
        public virtual void Setup()
        {
            ILoginSolver.Instance = new DummyLoginSolver();
        }
    }
}