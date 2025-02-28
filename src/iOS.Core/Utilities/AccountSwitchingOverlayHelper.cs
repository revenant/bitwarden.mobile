﻿using System.Threading.Tasks;
using Bit.App.Controls;
using Bit.Core.Abstractions;
using Bit.Core.Utilities;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Bit.iOS.Core.Utilities
{
    public class AccountSwitchingOverlayHelper
    {
        IStateService _stateService;
        IMessagingService _messagingService;
        ILogger _logger;

        public AccountSwitchingOverlayHelper()
        {
            _stateService = ServiceContainer.Resolve<IStateService>("stateService");
            _messagingService = ServiceContainer.Resolve<IMessagingService>("messagingService");
            _logger = ServiceContainer.Resolve<ILogger>("logger");
        }

        public async Task<UIImage> CreateAvatarImageAsync()
        {
            var avatarImageSource = new AvatarImageSource(await _stateService.GetNameAsync(), await _stateService.GetEmailAsync());
            var avatarUIImage = await avatarImageSource.GetNativeImageAsync();
            return avatarUIImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
        }

        public AccountSwitchingOverlayView CreateAccountSwitchingOverlayView(UIView containerView)
        {
            var overlay = new AccountSwitchingOverlayView()
            {
                LongPressAccountEnabled = false,
                AfterHide = () =>
                {
                    if (containerView != null)
                    {
                        containerView.Hidden = true;
                    }
                }
            };

            var vm = new AccountSwitchingOverlayViewModel(_stateService, _messagingService, _logger)
            {
                FromIOSExtension = true
            };
            overlay.BindingContext = vm;
            overlay.IsVisible = false;

            var renderer = Platform.CreateRenderer(overlay.Content);
            renderer.SetElementSize(new Size(containerView.Frame.Size.Width, containerView.Frame.Size.Height));

            var view = renderer.NativeView;
            view.TranslatesAutoresizingMaskIntoConstraints = false;

            containerView.AddSubview(view);
            containerView.AddConstraints(new NSLayoutConstraint[]
            {
                    NSLayoutConstraint.Create(containerView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, view, NSLayoutAttribute.Trailing, 1f, 0f),
                    NSLayoutConstraint.Create(containerView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, view, NSLayoutAttribute.Leading, 1f, 0f),
                    NSLayoutConstraint.Create(containerView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, view, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(containerView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, view, NSLayoutAttribute.Bottom, 1f, 0f)
            });
            containerView.Hidden = true;

            return overlay;
        }

        public void OnToolbarItemActivated(AccountSwitchingOverlayView accountSwitchingOverlayView, UIView containerView)
        {
            var overlayVisible = accountSwitchingOverlayView.IsVisible;
            if (!overlayVisible)
            {
                // So that the animation doesn't break we only care about showing it
                // and the hiding if done through AccountSwitchingOverlayView -> AfterHide
                containerView.Hidden = false;
            }
            accountSwitchingOverlayView.ToggleVisibililtyCommand.Execute(null);
            containerView.UserInteractionEnabled = !overlayVisible;
            containerView.Subviews[0].UserInteractionEnabled = !overlayVisible;
        }
    }
}
