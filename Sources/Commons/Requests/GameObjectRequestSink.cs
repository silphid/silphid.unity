using UnityEngine;

namespace Silphid.Requests
{
    public class GameObjectRequestSink : IRequestSink
    {
        private readonly GameObject _gameObject;

        public GameObjectRequestSink(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public void Send(IRequest request) =>
            _gameObject.Send(request);
    }
}