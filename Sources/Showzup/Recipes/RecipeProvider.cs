using System;
using System.Linq;
using Silphid.Extensions;
using Silphid.Showzup.Resolving;
using UniRx;

namespace Silphid.Showzup.Recipes
{
    public class RecipeProvider : IRecipeProvider
    {
        private readonly IResolver _resolver;
        private readonly TypeModelCollection _typeModelCollection;
        private readonly IVariantProvider _variantProvider;

        public RecipeProvider(IResolver resolver, TypeModelCollection typeModelCollection, IVariantProvider variantProvider)
        {
            _resolver = resolver;
            _typeModelCollection = typeModelCollection;
            _variantProvider = variantProvider;
        }

        public IObservable<Recipe> GetRecipe(object input, IOptions options)
        {
            if (input is Type)
                throw new NotSupportedException("Input cannot be of type Type");

            var problem = new Problem(
                _typeModelCollection.GetModelFromType(input?.GetType()),
                GetRequestedVariants(options.GetVariants()));

            var solution = _resolver.Resolve(problem);
            var recipe = solution != null
                ? new Recipe
                {
                    Model = input,
                    ModelType = solution.Model?.Type ?? solution.ViewModel.Type,
                    ViewModelType = solution.ViewModel.Type,
                    ViewType = solution.View.Type,
                    PrefabUri = solution.Prefab,
                    Variants = solution.PrefabVariants, //TODO: Retrieve prefab variant some other way
                    Parameters = options.GetParameters(),
                    Options = options
                }
                : Recipe.Null;
            return Observable.Return(recipe);
        }

        private VariantSet GetRequestedVariants(VariantSet variants)
        {
            var requestedVariants = variants.UnionWith(_variantProvider.GlobalVariants.Value);
            if (requestedVariants.Distinct(x => x.Group)
                                 .Count() != requestedVariants.Count())
                throw new InvalidOperationException(
                    $"Cannot request more than one variant per group: {requestedVariants}");

            return requestedVariants;
        }
    }
}