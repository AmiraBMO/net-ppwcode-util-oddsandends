﻿// Copyright 2014 by PeopleWare n.v..
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace PPWCode.Util.OddsAndEnds.II.AssemblyHelpers
{
    /// <summary>
    ///     Helper class for Assembly.
    /// </summary>
    public static class AssemblyHelper
    {
        private static readonly object s_LoadedAssembliesLock = new object();
        private static readonly Dictionary<string, Assembly> s_LoadedAssemblies = new Dictionary<string, Assembly>();

        private static readonly object s_ClassNamesLock = new object();
        private static readonly Dictionary<Assembly, Dictionary<string, Type>> s_ClassNames = new Dictionary<Assembly, Dictionary<string, Type>>();

        /// <summary>
        ///     Loads the assembly.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly.</param>
        /// <returns>The assembly.</returns>
        public static Assembly LoadAssembly(string assemblyName)
        {
            Contract.Requires(!string.IsNullOrEmpty(assemblyName));

            lock (s_LoadedAssembliesLock)
            {
                Assembly result;
                if (!s_LoadedAssemblies.TryGetValue(assemblyName, out result))
                {
                    result = Assembly.LoadFrom(assemblyName);
                    if (result == null)
                    {
                        throw new ArgumentException(string.Format("assembly {0} not found", assemblyName));
                    }

                    s_LoadedAssemblies.Add(assemblyName, result);
                }

                return result;
            }
        }

        /// <summary>
        ///     Creates an instance of the Type of the class using the default constructor.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="className">The class name.</param>
        /// <returns>An object.</returns>
        public static object CreateInstanceOf(Assembly assembly, string className)
        {
            Contract.Requires(assembly != null);
            Contract.Requires(!string.IsNullOrEmpty(className));

            lock (s_ClassNamesLock)
            {
                Dictionary<string, Type> classes;
                if (!s_ClassNames.TryGetValue(assembly, out classes))
                {
                    classes = new Dictionary<string, Type>();
                    // ReSharper disable AssignNullToNotNullAttribute
                    assembly
                        .GetTypes()
                        .Where(o => o.IsClass)
                        .ToList()
                        .ForEach(o => classes.Add(o.FullName, o));
                    // ReSharper restore AssignNullToNotNullAttribute
                    s_ClassNames.Add(assembly, classes);
                }

                Type classType;
                if (!classes.TryGetValue(className, out classType))
                {
                    throw new ArgumentException(string.Format("className {0} not found in assembly {1}", className, assembly.FullName));
                }

                return Activator.CreateInstance(classType);
            }
        }
    }
}