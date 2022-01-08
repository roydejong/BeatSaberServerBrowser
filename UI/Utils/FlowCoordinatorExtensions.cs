using System;
using HMUI;
using IPA.Utilities;

namespace ServerBrowser.UI.Utils
{
    internal static class FlowCoordinatorExtensions
    {
        internal static void ReplaceChildFlowCoordinator(this FlowCoordinator parentFlowCoordinator,
            FlowCoordinator childFlowCoordinator, Action? finishedCallback = null,
            ViewController.AnimationDirection animationDirection = ViewController.AnimationDirection.Horizontal,
            bool immediately = false)
        {
            parentFlowCoordinator.InvokeMethod<object, FlowCoordinator>("ReplaceChildFlowCoordinator", 
                childFlowCoordinator, finishedCallback, animationDirection, immediately);
        }
    }
}