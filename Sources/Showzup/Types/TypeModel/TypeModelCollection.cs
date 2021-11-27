using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Silphid.Showzup
{
    public class TypeModelCollection
    {
        private readonly IDictionary<string, TypeModel> _typeModels = new Dictionary<string, TypeModel>();

        public void Save(string path)
        {
            var serializer = new SerializerBuilder().Build();

            var yaml = serializer.Serialize(
                _typeModels.Select(
                    t => new SerializableTypeModel
                    {
                        Type = t.Key,
                        IsInterface = t.Value is InterfaceModel,
                        Parent = t.Value is ClassModel classModel
                                     ? classModel.ParentName
                                     : null,
                        Interfaces = t.Value.InterfaceNames?.ToList()
                    }));

            File.WriteAllText(path, yaml);
        }

        public static TypeModelCollection Load(string path)
        {
            if (!File.Exists(path))
                return new TypeModelCollection();

            return LoadText(File.ReadAllText(path));
        }

        public static TypeModelCollection LoadResource(string resourcePath)
        {
            var resource = Resources.Load<TextAsset>(resourcePath);
            if (resource == null)
                return new TypeModelCollection();

            return LoadText(resource.text);
        }

        private static TypeModelCollection LoadText(string input)
        {
            var types = new DeserializerBuilder().Build()
                                                 .Deserialize<List<SerializableTypeModel>>(input);

            var typeModelCollection = CreateModelCollection(types);
            UpdateTypeParentsAndInterfaces(typeModelCollection._typeModels);

            return typeModelCollection;
        }

        public TypeModel GetModelFromName(string assemblyQualifiedName)
        {
            if (_typeModels.TryGetValue(assemblyQualifiedName, out var typeModel))
                return typeModel;

            var type = Type.GetType(assemblyQualifiedName);
            return type?.AssemblyQualifiedName == null
                       ? null
                       : AddTypeModel(type);
        }

        public TypeModel GetModelFromType(Type type)
        {
            if (type?.AssemblyQualifiedName == null)
                return null;

            if (_typeModels.TryGetValue(type.AssemblyQualifiedName, out var typeModel))
                return typeModel;

            return AddTypeModel(type);
        }

        private TypeModel AddTypeModel(Type type)
        {
            var typeModel = type.IsInterface
                                ? new InterfaceModel(type.AssemblyQualifiedName)
                                : AddClassModel(type);

            AddInterfacesInfo(type, typeModel);
            _typeModels.Add(type.AssemblyQualifiedName, typeModel);

            return typeModel;
        }

        private TypeModel AddClassModel(Type type)
        {
            var classModel = new ClassModel(type.AssemblyQualifiedName);

            if (type.BaseType != null)
            {
                classModel.Parent = GetModelFromType(type.BaseType) as ClassModel;
                classModel.ParentName = type.BaseType.AssemblyQualifiedName;
            }

            return classModel;
        }

        private void AddInterfacesInfo(Type type, TypeModel classModel)
        {
            var interfaces = type.GetInterfaces();

            var directInterfaces = interfaces.Except(
                                                  interfaces.SelectMany(i => i.GetInterfaces())
                                                            .Union(
                                                                 type.BaseType?.GetInterfaces() ??
                                                                 Enumerable.Empty<Type>()))
                                             .ToList();

            if (directInterfaces.Any())
            {
                classModel.InterfaceNames = directInterfaces.Select(i => i.AssemblyQualifiedName)
                                                            .ToList();
                classModel.Interfaces = directInterfaces.Select(i => GetModelFromType(i) as InterfaceModel)
                                                        .ToList();
            }
        }

        private static TypeModelCollection CreateModelCollection(IEnumerable<SerializableTypeModel> types)
        {
            var typeModelCollection = new TypeModelCollection();

            foreach (var type in types)
            {
                TypeModel typeModel;
                if (type.IsInterface)
                    typeModel = new InterfaceModel(type.Type);
                else
                {
                    typeModel = new ClassModel(type.Type) { ParentName = type.Parent };
                }

                typeModel.InterfaceNames = type.Interfaces;

                typeModelCollection._typeModels.Add(type.Type, typeModel);
            }

            return typeModelCollection;
        }

        private static void UpdateTypeParentsAndInterfaces(IDictionary<string, TypeModel> typeModels)
        {
            foreach (var (_, typeModel) in typeModels)
            {
                if (typeModel.InterfaceNames != null)
                    typeModel.Interfaces = typeModel.InterfaceNames.Select(
                                                         x =>
                                                         {
                                                             if (!typeModels.TryGetValue(x, out var interfaceModel))
                                                                 throw new KeyNotFoundException(
                                                                     $"The interface {x} of {typeModel} was not found in the TypeModelCollection." +
                                                                     $"Try deleting or fixing {typeModel} from the TypeModelCollection resource file.");

                                                             return interfaceModel as InterfaceModel;
                                                         })
                                                    .ToList();

                if (typeModel is ClassModel classModel && classModel.ParentName != null)
                {
                    if (!typeModels.TryGetValue(classModel.ParentName, out var parent))
                    {
                        throw new KeyNotFoundException(
                            $"The base type {classModel.ParentName} of {typeModel} was not found in the TypeModelCollection. " +
                            $"Try deleting or fixing {typeModel} from the TypeModelCollection resource file.");
                    }

                    classModel.Parent = parent as ClassModel;
                }
            }
        }

        private class SerializableTypeModel
        {
            public string Type { get; set; }
            public bool IsInterface { get; set; }
            public string Parent { get; set; }
            public List<string> Interfaces { get; set; }
        }
    }
}