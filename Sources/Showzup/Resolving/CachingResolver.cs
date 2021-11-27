using log4net;

namespace Silphid.Showzup.Resolving
{
    public class CachingResolver : IResolver
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Resolver));

        private readonly IResolver _resolver;
        private readonly SolutionSet _solutionSet;

        public CachingResolver(IResolver resolver, SolutionSet solutionSet)
        {
            _resolver = resolver;
            _solutionSet = solutionSet;
        }

        public Solution Resolve(Problem problem)
        {
            if (problem.Type == null)
                return null;

            if (_solutionSet.Solutions.TryGetValue(problem, out var solution))
            {
                if (Log.IsDebugEnabled)
                    Log.Debug($"Problem {problem} was cached, using cached solution {solution}");

                if (solution.View.Type == null)
                    throw new ResolveException(
                        problem.Type,
                        problem.Variants,
                        $"Cached solution view type was not found {solution.View.Name}");

                if (solution.ViewModel.Type == null)
                    throw new ResolveException(
                        problem.Type,
                        problem.Variants,
                        $"Cached solution view model type was not found {solution.ViewModel.Name}");

                return solution;
            }

            solution = _resolver.Resolve(problem);
            _solutionSet.Solutions.Add(problem, solution);
            return solution;
        }
    }
}