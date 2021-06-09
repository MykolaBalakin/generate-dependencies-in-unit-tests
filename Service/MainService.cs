namespace Service
{
    public class MainService : IMainService
    {
        private readonly IImportantDependency _importantDependency;
        private readonly INotImportantDependency _notImportantDependency;

        public MainService(IImportantDependency importantDependency, INotImportantDependency notImportantDependency)
        {
            _importantDependency = importantDependency;
            _notImportantDependency = notImportantDependency;
        }

        public void DoWork(bool useNotImportantDependency)
        {
            _importantDependency.GetData();

            if (useNotImportantDependency)
            {
                _notImportantDependency.GetData();
            }
        }
    }
}