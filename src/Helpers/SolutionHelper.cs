﻿using EnvDTE;
using VCFileUtils.Model;
using Microsoft.VisualStudio.VCProjectEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace VCFileUtils.Helpers
{
    static class SolutionHelper
    {
        public static IEnumerable<VCProjectItemWrapper> GetSelectedItems(VCFileUtilsPackage package)
        {
            foreach (var item in GetSelectedUIHierarchyItems(package))
            {
                VCProjectItem vcProjectItem = null;

                if (item.Object is ProjectItem)
                    vcProjectItem = (item.Object as ProjectItem).Object as VCProjectItem;

                if (item.Object is Project)
                    vcProjectItem = (item.Object as Project).Object as VCProjectItem;

                if (vcProjectItem != null)
                    yield return WrapperFactory.FromVCProjectItem(vcProjectItem);
            }
        }

        public static IEnumerable<VCProjectWrapper> GetSelectedProjects(VCFileUtilsPackage package)
        {
            return GetSelectedItems(package)
                .Where(item => item is VCProjectWrapper)
                .Cast<VCProjectWrapper>();
        }

        public static IEnumerable<VCFileWrapper> GetSelectedFiles(VCFileUtilsPackage package)
        {
            foreach (var item in GetSelectedItems(package))
            {
                if (item is VCFileWrapper)
                    yield return (item as VCFileWrapper);

                if (item is ContainerWrapper)
                    foreach (var child in (item as ContainerWrapper).GetFilesRecursive())
                        yield return child;
            }
        }

        public static VCProjectWrapper GetProjectOfSelection(VCFileUtilsPackage package)
        {
            var projects = GetSelectedItems(package)
                .Select(item => item.ContainingProject)
                .ToList();

            if (projects.Count() == 0)
                return null;

            if (projects.Any(project => !project.Equals(projects[0])))
                return null;

            return projects[0];
        }

        public static string GetSelectedDirectory(VCFileUtilsPackage package)
        {
            var selection = GetSelectedItems(package)
                .ToList();

            if (selection.Count() != 1)
                return null;

            if (selection[0] is VCProjectWrapper)
                return (selection[0] as VCProjectWrapper).GetProjectRoot();

            if (selection[0] is VCFilterWrapper)
                return selection[0].FullPath;

            return null;
        }

        private static UIHierarchy GetSolutionExplorer(VCFileUtilsPackage package)
        {
            try
            {
                return package.IDE.ToolWindows.SolutionExplorer;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private static IEnumerable<UIHierarchyItem> GetSelectedUIHierarchyItems(VCFileUtilsPackage package)
        {
            UIHierarchy solutionExplorer = GetSolutionExplorer(package);

            if (solutionExplorer == null)
                return new UIHierarchyItem[0];

            return ((object[])solutionExplorer.SelectedItems)
                .Cast<UIHierarchyItem>();
        }
    }
}
