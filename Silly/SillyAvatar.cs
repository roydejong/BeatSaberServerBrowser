using System.Linq;
using BeatSaber.BeatAvatarAdapter;
using BeatSaber.BeatAvatarAdapter.AvatarEditor;
using BeatSaber.BeatAvatarSDK;
using ServerBrowser.Util;
using UnityEngine;
using Zenject;

namespace ServerBrowser.Silly
{
    public class SillyAvatar : MonoBehaviour
    {
        private static BeatAvatarEditorFlowCoordinator? _avatarEditorFlowCoordinator = null;

        public static SillyAvatar? TryCreate(DiContainer container)
        {
            if (_avatarEditorFlowCoordinator == null)
                // BeatAvatar is not in the menu DI container, it uses a separate async loaded one
                // ...so we need to do this dirty old thing:
                _avatarEditorFlowCoordinator = Resources.FindObjectsOfTypeAll<BeatAvatarEditorFlowCoordinator>()
                    .FirstOrDefault();

            if (_avatarEditorFlowCoordinator == null || _avatarEditorFlowCoordinator._avatarContainerGameObject == null)
                return null;


            var basePlayerAvatar = _avatarEditorFlowCoordinator._avatarContainerGameObject;
            
            var gameObject = Instantiate(basePlayerAvatar);
            gameObject.name = "SillyAvatar";
            gameObject.TryRemoveComponent<DeactivateAnimatorOnInputFocusCapture>();
            gameObject.TryRemoveComponent<ZenjectStateMachineBehaviourAutoInjecter>();
            var obj = gameObject.AddComponent<SillyAvatar>();
            obj.Inject(container);
            return obj;
        }

        private readonly AvatarTweenController _tweenController;
        private readonly BeatAvatarVisualController _visualController;
        private readonly BeatAvatarPoseController _poseController;

        private SillyAvatar()
        {
            _tweenController = GetComponent<AvatarTweenController>();
            _visualController = GetComponentInChildren<BeatAvatarVisualController>();
            _poseController = GetComponentInChildren<BeatAvatarPoseController>();

            transform.position = new Vector3(-1.25f, -0.05f, 4.05f);
            transform.localScale = new Vector3(0.33f, 0.33f, 0.33f);
        }

        private void Inject(DiContainer container) => container.InjectGameObject(gameObject);

        private static uint AvatarSystemHash => BeatAvatarSystem.kAvatarSystemTypeIdentifier.hash;

        public void SetAvatarData(MultiplayerAvatarsData multiplayerAvatarsData)
        {
            var beatAvatarData = multiplayerAvatarsData.multiplayerAvatarsData
                .FirstOrDefault(x => x.avatarTypeIdentifierHash == AvatarSystemHash);
            _visualController.UpdateAvatarVisual(beatAvatarData.CreateAvatarData());
            _tweenController.PopAll();
        }

        public void SetActive(bool active)
        {
            if (active)
                _tweenController.PresentAvatar();
            else
                _tweenController.HideAvatar();
        }
    }
}