using System.Collections.Generic;

namespace Xer.Worker
{
    public class WorkGroup : IWork
    {
        private readonly IEnumerable<IWork> _workList;

        public WorkGroup(IEnumerable<IWork> work)
        {
            _workList = work;
        }

        public void Execute()
        {
            foreach(IWork work in _workList)
            {
                work.Execute();
            }
        }
    }
}
