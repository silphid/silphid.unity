using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Silphid.Extensions;
using Silphid.Showzup.Resolving;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Silphid.Showzup
{
    public class SolutionSet
    {
        public readonly IDictionary<Problem, Solution> Solutions = new Dictionary<Problem, Solution>();

        public SolutionSet(IDictionary<Problem, Solution> solutions)
        {
            Solutions = solutions;
        }

        public SolutionSet() {}

        public void Save(string path)
        {
            var serializer = new SerializerBuilder().Build();

            var yaml = serializer.Serialize(
                Solutions.Select(
                              solution => new SerializableSolution
                              {
                                  ProblemType = solution.Key.Type.Name,
                                  ProblemVariants = solution.Key.Variants.Select(v => new SerializableVariant(v))
                                                            .ToList(),
                                  SolutionId = solution.Value.Id,
                                  SolutionView = solution.Value.View.Name,
                                  SolutionModel = solution.Value.Model?.Name,
                                  SolutionViewModel = solution.Value.ViewModel.Name,
                                  SolutionPrefab = solution.Value.Prefab.AbsoluteUri,
                                  SolutionVariants = solution
                                                    .Value.PrefabVariants.Select(v => new SerializableVariant(v))
                                                    .ToList()
                              })
                         .ToList());

            File.WriteAllText(path, yaml);
        }

        public static SolutionSet Load(string path, TypeModelCollection typeCollection)
        {
            if (!File.Exists(path))
                return new SolutionSet();

            var input = File.ReadAllText(path);
            return LoadText(typeCollection, input);
        }

        public static SolutionSet LoadResource(string resourcePath, TypeModelCollection typeCollection)
        {
            var resource = Resources.Load<TextAsset>(resourcePath);
            if (resource == null || resource.text.IsNullOrEmpty())
                return new SolutionSet();

            return LoadText(typeCollection, resource.text);
        }

        private static SolutionSet LoadText(TypeModelCollection typeCollection, string input)
        {
            var deserializer = new DeserializerBuilder().Build();

            return new SolutionSet(
                deserializer.Deserialize<List<SerializableSolution>>(input)
                            .ToDictionary(
                                 solution => new Problem(
                                     typeCollection.GetModelFromName(solution.ProblemType),
                                     new VariantSet(solution.ProblemVariants.Select(v => v.GetVariant()))),
                                 solution =>
                                 {
                                     var solutionModel = solution.SolutionModel == null
                                                             ? null
                                                             : typeCollection.GetModelFromName(solution.SolutionModel);

                                     return new Solution(
                                         solution.SolutionId,
                                         solutionModel,
                                         typeCollection.GetModelFromName(solution.SolutionViewModel),
                                         typeCollection.GetModelFromName(solution.SolutionView),
                                         new Uri(solution.SolutionPrefab),
                                         new VariantSet(solution.SolutionVariants.Select(v => v.GetVariant())));
                                 }));
        }

        private class SerializableSolution
        {
            public string ProblemType { get; set; }
            public List<SerializableVariant> ProblemVariants { get; set; }
            public string SolutionView { get; set; }
            public string SolutionModel { get; set; }
            public string SolutionViewModel { get; set; }
            public string SolutionPrefab { get; set; }
            public Guid SolutionId { get; set; }
            public List<SerializableVariant> SolutionVariants { get; set; }
        }

        private class SerializableVariant
        {
            // ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
            public string Type { get; set; }
            public string Name { get; set; }

            public SerializableVariant(IVariant variant)
            {
                Type = variant.GetType()
                              .AssemblyQualifiedName;
                Name = variant.Name;
            }

            // ReSharper disable once UnusedMember.Local
            // required by yamldotnet deserialization 
            public SerializableVariant() {}

            public IVariant GetVariant()
            {
                var field = System.Type.GetType(Type)
                                 ?.GetField(Name, BindingFlags.Static | BindingFlags.Public);

                return (IVariant) field?.GetValue(null);
            }
        }
    }
}