using System;

namespace Silphid.Showzup.Recipes
{
    public interface IRecipeProvider
    {
        IObservable<Recipe> GetRecipe(object input, IOptions options);
    }
}