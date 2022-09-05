using AxieMixer.Unity;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Game_Turn_Based
{
    public class AxieFigure : MonoBehaviour
    {
        [SerializeField] private string stringID;
        private SkeletonAnimation skeletonAnimation;

        [SerializeField] private bool _flipX = false;
        public bool flipX
        {
            get
            {
                return _flipX;
            }
            set
            {
                _flipX = value;
                if (skeletonAnimation != null)
                {
                    skeletonAnimation.skeleton.ScaleX = (_flipX ? -1 : 1) * Mathf.Abs(skeletonAnimation.skeleton.ScaleX);
                }
            }
        }

        public GameObject target;
        public bool isAttacker;

        private void Awake()
        {
            skeletonAnimation = gameObject.GetComponent<SkeletonAnimation>();
        }

        private void Start()
        {
            Mixer.Init();

            string axieId = "";
            string genes = "";
            if(isAttacker)
            {
                axieId = PlayerPrefs.GetString("selectingAttackerId", "4191804");
                genes = PlayerPrefs.GetString("selectingAttackerGenes", "0x2000000000000300008100e08308000000010010088081040001000010a043020000009008004106000100100860c40200010000084081060001001410a04406");
                
            }
            else
            {
                axieId = PlayerPrefs.GetString("selectingDefenderId", "2724598");
                genes = PlayerPrefs.GetString("selectingDefenderGenes", "0x2000000000000300008100e08308000000010010088081040001000010a043020000009008004106000100100860c40200010000084081060001001410a04406");
            }
            SetGenes(axieId, genes);
        }

        void Update()
        {
            
        }

        public void SetGenes(string id, string genes)
        {
            if (string.IsNullOrEmpty(genes)) return;

            if (skeletonAnimation != null && skeletonAnimation.state != null)
            {
                skeletonAnimation.state.End -= SpineEndHandler;
            }
            Mixer.SpawnSkeletonAnimation(skeletonAnimation, id, genes);

            skeletonAnimation.transform.localPosition = new Vector3(0f, -0.32f, 0f);
            //skeletonAnimation.transform.SetParent(transform, false);
            skeletonAnimation.transform.localScale = new Vector3(1, 1, 1);
            skeletonAnimation.skeleton.ScaleX = (_flipX ? -1 : 1) * Mathf.Abs(skeletonAnimation.skeleton.ScaleX);
            skeletonAnimation.timeScale = 0.5f;
            skeletonAnimation.skeleton.FindSlot("shadow").Attachment = null;
            skeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);
            skeletonAnimation.state.End += SpineEndHandler;
        }

        private void OnDisable()
        {
            if (skeletonAnimation != null)
            {
                if(skeletonAnimation.state != null)
                {
                    skeletonAnimation.state.End -= SpineEndHandler;
                }
            }
        }

        public void PlayMoveAnim()
        {
            skeletonAnimation.timeScale = 1f;
            skeletonAnimation.AnimationState.SetAnimation(0, "action/move-forward", false);
        }

        public void PlayVictoryAnim()
        {
            skeletonAnimation.timeScale = 1f;
            skeletonAnimation.AnimationState.SetAnimation(0, "activity/victory-pose-back-flip", true);
        }

        public void StopCurrentAnim()
        {
            skeletonAnimation.timeScale = 0.5f;
            skeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);
        }

        public void PlayAttackAnim()
        {
            skeletonAnimation.timeScale = 1f;
            skeletonAnimation.AnimationState.SetAnimation(0, "attack/melee/normal-attack", false);
        }

        public void PlayDieAnim()
        {
            skeletonAnimation.timeScale = 1f;
            skeletonAnimation.AnimationState.SetAnimation(0, "battle/get-debuff", false);
        }

        private void SpineEndHandler(TrackEntry trackEntry)
        {
            string animation = trackEntry.Animation.Name;
            if (animation == "action/move-forward")
            {
                skeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);
                skeletonAnimation.timeScale = 0.5f;
            }
        }
    }
}
