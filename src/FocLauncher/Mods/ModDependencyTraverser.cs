using System.Collections.Generic;
using System.Linq;

namespace FocLauncher.Mods
{
    public class ModDependencyTraverser
    {
        private readonly IMod _mod;

        public ModDependencyTraverser(IMod mod)
        {
            _mod = mod;
        }

        public bool HasDependencyCycles()
        {
            try
            {
                CheckForCycles();
            }
            catch (PetroglyphModException)
            {
                return true;
            }
            return false;
        }

        public IList<IMod> Traverse(bool throwOnCycle = true)
        {
            if (throwOnCycle)
                CheckForCycles();
            return TraverseCore().Distinct().ToList();
        }

        private void CheckForCycles()
        {
            CheckForCycles(_mod, new HashSet<IMod>());
        }

        private static void CheckForCycles(IMod current, ISet<IMod> seenNodes)
        {
            if (!seenNodes.Add(current))
                throw new PetroglyphModException();

            foreach (var before in current.Dependencies) 
                CheckForCycles(before, seenNodes);
            seenNodes.Remove(current);
        }

        private IEnumerable<IMod> TraverseCore()
        {
            var stack = new Queue<IMod>();
            var visited = new HashSet<IMod>();
            stack.Enqueue(_mod);
            while (stack.Count > 0)
            {
                var current = stack.Dequeue();
                if (!visited.Add(current))
                    continue;
                yield return current;

                var neighbors = current.Dependencies.Where(n => !visited.Contains(n));

                foreach (var child in neighbors)
                    stack.Enqueue(child);
            }
        }
    }
}