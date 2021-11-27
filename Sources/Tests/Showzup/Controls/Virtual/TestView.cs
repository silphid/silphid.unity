using System;
using TMPro;
using UniRx;
using UnityEngine.UI;

namespace Silphid.Showzup.Test.Controls.Virtual
{
    public class TestView : View<TestViewModel>
    {
        public TMP_Text Text;
        public Image Image;
        public LayoutElement LayoutElement;
        public Button DeleteButton;
        public Button AddBeforeButton;
        public Button AddAfterButton;
        
        protected TestModel Model => ViewModel.Model;

        public override ICompletable Load()
        {
            Text.text = $"{Model.Index} - {Model.Text}";
            Image.color = Model.Color;
            LayoutElement.preferredWidth = Model.Width;
            LayoutElement.preferredHeight = Model.Height;

            AddBeforeButton.OnClickAsObservable()
                           .Subscribe(_ => Send(new AddBeforeRequest(Model)));
            AddAfterButton.OnClickAsObservable()
                           .Subscribe(_ => Send(new AddAfterRequest(Model)));
            DeleteButton.OnClickAsObservable()
                           .Subscribe(_ => Send(new RemoveRequest(Model)));
            
            return Completable.Timer(TimeSpan.FromSeconds(Model.LoadDelay));
        }
    }
}