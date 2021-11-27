using System;
using Silphid.Machina;
using Silphid.Requests;

namespace Silphid.Showzup.Flows
{
    public abstract class Flow : Machine<IView>, IFlow
    {
        private readonly IRequestSink _requestSink;
        private readonly IRequest _initialRequest;
        private bool _isConfigured;
        protected IFlowFactory FlowFactory { get; }

        protected Flow(IFlowFactory flowFactory, IRequestSink requestSink, IRequest initialRequest = null)
            : base(initialState: null, disposeOnCompleted: true)
        {
            _requestSink = requestSink;
            _initialRequest = initialRequest;
            FlowFactory = flowFactory;
        }

        protected virtual void Configure()
        {
            Always()
               .Handle<ISubFlowRequest>(x => StartFlow(x.FlowType, x.Options));

            WhenSubFlow()
               .Handle<CompleteSubFlowRequest>(_ => ExitState());
        }

        protected void StartFlow(Type flowType, IFlowOptions options = null)
        {
            Enter(FlowFactory.Create(flowType, options));
        }

        protected void StartFlow<TFlow>(IFlowOptions options = null)
        {
            StartFlow(typeof(TFlow), options);
        }

        protected void CompleteTopFlow()
        {
            Send<CompleteTopFlowRequest>();
        }

        protected void CompleteSubFlow()
        {
            Send<CompleteSubFlowRequest>();
        }

        protected override void OnStarting(object _)
        {
            if (!_isConfigured)
            {
                Configure();
                _isConfigured = true;
            }

            base.OnStarting(null);

            if (_initialRequest != null)
                _requestSink.Send(_initialRequest);
        }

        public override bool Handle(IRequest request)
        {
            if (base.Handle(request))
                return true;

            if (request is ViewChangedRequest viewChangedRequest)
            {
                Enter(viewChangedRequest.View);
                return true;
            }

            return false;
        }

        protected new IRule When<TView>() where TView : IView =>
            base.When<TView>();

        public IRule WhenSubFlow() =>
            WhenSubMachine<IFlow>();

        public IRule WhenSubFlow<TFlow>() where TFlow : IFlow =>
            WhenSubMachine<TFlow>();

        public IRule WhenSubFlow<TFlow>(TFlow state) where TFlow : IFlow =>
            WhenSubMachine(state);

        public IRule WhenSubFlow<TFlow>(Predicate<TFlow> predicate) where TFlow : IFlow =>
            WhenSubMachine(predicate);

        protected void Send(IRequest request) =>
            _requestSink.Send(request);

        protected void Send(Exception exception) =>
            _requestSink.Send(exception);

        protected void Send<TRequest>() where TRequest : IRequest, new() =>
            Send(new TRequest());
    }
}