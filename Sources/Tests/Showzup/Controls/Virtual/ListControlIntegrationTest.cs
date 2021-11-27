using System;
using System.Collections.Generic;
using Silphid.Extensions;
using Silphid.Injexit;
using Silphid.Loadzup;
using Silphid.Requests;
using Silphid.Showzup.Recipes;
using Silphid.Showzup.Virtual.Layout;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Silphid.Showzup.Test.Controls.Virtual
{
    public class ListControlIntegrationTest : MonoBehaviour, IRecipeProvider, ILoader, IContainerProvider,
                                              IVariantProvider, IRequestHandler
    {
        private static readonly System.Random _random = new System.Random();

        private static readonly string[] _words =
        {
            "Lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit", "sed", "do", "eiusmod",
            "tempor", "incididunt", "ut", "labore", "et", "dolore", "magna", "aliqua"
        };

        private static readonly Color[] _colors =
        {
            new Color(0.49f, 0.69f, 0.45f), new Color(0.56f, 0.69f, 0.33f), new Color(0.62f, 0.69f, 0.46f),
            new Color(0.69f, 0.58f, 0.42f), new Color(0.69f, 0.47f, 0.36f), new Color(0.69f, 0.37f, 0.39f),
            new Color(0.69f, 0.39f, 0.53f), new Color(0.69f, 0.39f, 0.69f), new Color(0.57f, 0.38f, 0.69f),
            new Color(0.37f, 0.31f, 0.69f), new Color(0.33f, 0.38f, 0.69f), new Color(0.37f, 0.49f, 0.69f),
            new Color(0.35f, 0.69f, 0.64f), new Color(0.36f, 0.69f, 0.49f)
        };

        public int InitialCount = 20;
        public GameObject ViewPrefab;
        public float LoadDelayMin = 0.001f;
        public float LoadDelayMax = 0.4f;
        public Showzup.Virtual.ListControl ListControl;
        public Button PresentButton;
        public Button StressButton;
        public ScrollRectEx ScrollRectEx;

        private ReactiveCollection<object> _models;

        public ListControlIntegrationTest()
        {
            Container = new Container(new Reflector(), _ => true);
            Container.BindInstance<IRecipeProvider>(this);
            Container.BindInstance<ILoader>(this);
            Container.BindInstance<IVariantProvider>(this);
            Container.Bind<IViewLoader, ViewLoader>()
                     .AsSingle();
            Container.BindToSelf<TestViewModel>();
        }

        private void Start()
        {
            Container.Inject(ListControl);

            PresentButton.OnClickAsObservable()
                         .Subscribe(
                              _ =>
                              {
                                  _models = new ReactiveCollection<object>(CreateRandomModels(InitialCount));

                                  ScrollRectEx.horizontal =
                                      ListControl.LayoutInfo.Orientation == Orientation.Horizontal;
                                  ScrollRectEx.vertical = ListControl.LayoutInfo.Orientation == Orientation.Vertical;

                                  ListControl.Present(_models)
                                             .Subscribe(() => Debug.Log("Finished presenting models"));
                              });

            StressButton.OnClickAsObservable()
                        .SelectMany(
                             _ => Observable.Interval(TimeSpan.FromMilliseconds(20))
                                            .Take(50))
                        .Subscribe(
                             x =>
                             {
                                 if (x >= 1)
                                     _models.RemoveAt(4);

                                 var model = CreateRandomModel((int) x, "Stress!!!");
                                 _models.Insert(4, model);
                             });
        }

        private IEnumerable<object> CreateRandomModels(int count)
        {
            for (int i = 0; i < count; i++)
                yield return CreateRandomModel(i);
        }

        private TestModel CreateRandomModel(int index, string text = null, int? delay = null) =>
            new TestModel
            {
                Index = index,
                Text = text ?? _words[_random.Next(_words.Length)],
                Color = _colors[_random.Next(_colors.Length)],
                Width = _random.NextFloat(150, 400),
                Height = _random.NextFloat(100, 300),
                LoadDelay = delay ?? Random.Range(LoadDelayMin, LoadDelayMax)
            };

        public IObservable<Recipe> GetRecipe(object input, IOptions options) =>
            Observable.Return(
                new Recipe
                {
                    Model = input,
                    ModelType = typeof(TestModel),
                    ViewModelType = typeof(TestViewModel),
                    ViewType = typeof(TestView),
                    Options = options,
                    PrefabUri = new Uri("dummy://")
                });

        public IContainer Container { get; }

        public bool Supports<T>(Uri uri) => true;

        public IObservable<T> Load<T>(Uri uri, Loadzup.IOptions options = null) =>
            Observable.Return((T) (object) ViewPrefab);

        public List<IVariantGroup> AllVariantGroups => new List<IVariantGroup>();
        public ReactiveProperty<VariantSet> GlobalVariants => new ReactiveProperty<VariantSet>(new VariantSet());

        public bool Handle(IRequest request)
        {
            if (request is AddBeforeRequest addBeforeRequest)
            {
                var index = _models.IndexOf(addBeforeRequest.Model);
                _models.Insert(index, CreateRandomModel(index));
                return true;
            }

            if (request is AddAfterRequest addAfterRequest)
            {
                var index = _models.IndexOf(addAfterRequest.Model) + 1;
                _models.Insert(index, CreateRandomModel(index));
                return true;
            }

            if (request is RemoveRequest removeRequest)
            {
                _models.Remove(removeRequest.Model);
                return true;
            }

            return false;
        }
    }
}