﻿using System;
using System.Collections.Generic;
using System.Linq;
using Repository.Graph;

namespace Repository.Model
{
    public class Repository : INode<Repository>
    {
        private Namespace? _namespace;
        public Namespace Namespace => _namespace ?? throw new Exception("Namespace not initialized.");
        public IEnumerable<Include> Includes { get; }
        public IEnumerable<Repository> Dependencies => Includes.Select(x => x.ResolvedRepository);

        public Repository(IEnumerable<Include> includes)
        {
            Includes = includes;
        }

        internal void SetNamespace(Namespace @namespace)
        {
            this._namespace = @namespace;
        }
    }
}
