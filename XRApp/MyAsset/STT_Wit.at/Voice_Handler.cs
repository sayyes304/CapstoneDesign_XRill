using UnityEngine;
using Meta.WitAi;
using UnityEngine.UI;
using TMPro;
using Meta.WitAi.Json;


namespace Oculus.Voice { 
    public class Voice_Handler : MonoBehaviour
    {
        [SerializeField] private string DefaultText = "  입력하세요. ";

            // public TextMeshProUGUI BtnText;

            // public GameObject KeyboardTxt;
            // public GameObject VoiceTxt;

            public Image icon;


        [Header("UI")]
        [SerializeField] public TextMeshProUGUI textArea;
        [SerializeField] private bool showJson;

        [Header("Voice")]
        [SerializeField] private AppVoiceExperience appVoiceExperience;

        // Whether voice is activated
        public bool IsActive => _active;
        public bool _active = false;


        private void OnEnable()
        {
            //textArea.text = DefaultText;
            appVoiceExperience.VoiceEvents.OnRequestCreated.AddListener(OnRequestStarted);
            appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnRequestTranscript);
            appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnRequestTranscript);
            appVoiceExperience.VoiceEvents.OnStartListening.AddListener(OnListenStart);
            appVoiceExperience.VoiceEvents.OnStoppedListening.AddListener(OnListenStop);
            appVoiceExperience.VoiceEvents.OnStoppedListeningDueToDeactivation.AddListener(OnListenForcedStop);
            appVoiceExperience.VoiceEvents.OnStoppedListeningDueToInactivity.AddListener(OnListenForcedStop);
            appVoiceExperience.VoiceEvents.OnResponse.AddListener(OnRequestResponse);
            appVoiceExperience.VoiceEvents.OnError.AddListener(OnRequestError);
        }
        // Remove delegates
        private void OnDisable()
        {
            appVoiceExperience.VoiceEvents.OnRequestCreated.RemoveListener(OnRequestStarted);
            appVoiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(OnRequestTranscript);
            appVoiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnRequestTranscript);
            appVoiceExperience.VoiceEvents.OnStartListening.RemoveListener(OnListenStart);
            appVoiceExperience.VoiceEvents.OnStoppedListening.RemoveListener(OnListenStop);
            appVoiceExperience.VoiceEvents.OnStoppedListeningDueToDeactivation.RemoveListener(OnListenForcedStop);
            appVoiceExperience.VoiceEvents.OnStoppedListeningDueToInactivity.RemoveListener(OnListenForcedStop);
            appVoiceExperience.VoiceEvents.OnResponse.RemoveListener(OnRequestResponse);
            appVoiceExperience.VoiceEvents.OnError.RemoveListener(OnRequestError);
        }

        // Request began
        private void OnRequestStarted(WitRequest r)
        {
                // Store json on completion
            if (showJson)
                    r.onRawResponse = (response) =>
                        {
                            textArea.text = response;
                        };
            // Begin
            _active = true;

            
        }


        // 실시간으로 음성 입력 값 response
        private void OnRequestTranscript(string transcript)
        {
            textArea.text = transcript;
        }


        // Listen start
        private void OnListenStart()
        {
            textArea.text = " 듣는 중...";
        }
        // Listen stop
        private void OnListenStop()
        {
            textArea.text = "Processing...";
        }

        // activate를 하지 않고 
        private void OnListenForcedStop()
        {
            Debug.Log("잘 안 됨");

            if (!showJson)
            {
                textArea.text = DefaultText;

            }
            OnRequestComplete();
        }

        // Request response
        private void OnRequestResponse(WitResponseNode response)
        {
            if (!showJson)
            {
                if (!string.IsNullOrEmpty(response["text"]))    // 
                    {
                    textArea.text = response["text"];
                        //    BtnText.text = "  음성 입력";

                    if (GetComponent<VirtualObjectNet>() != null) // only VR mode
                    {
                        Debug.Log("Called");
                        GetComponent<VirtualObjectNet>().RPC_note(response["text"]);
                    }
                }
                else
                {
                    textArea.text = DefaultText;
                }
            }
            OnRequestComplete();
        }
        // Request error
        private void OnRequestError(string error, string message)
        {
            if (!showJson)
            {
                textArea.text = $"<color=\"red\">Error: {error}\n\n{message}</color>";
            }
            OnRequestComplete();
        }
        // Deactivate
        private void OnRequestComplete()
        {
            _active = false;
        }

        // Toggle activation
        public void ToggleActivation()
        {
               // KeyboardTxt.SetActive(false);
              //  VoiceTxt.SetActive(true);

            SetActivation(!_active);

        }
        // Set activation
        public void SetActivation(bool toActivated)
        {
            if (_active != toActivated)
            {
                _active = toActivated;
                if (_active)
                {
                    appVoiceExperience.Activate();
                    //  BtnText.text = "  녹음중 ...";
                    
                }
                else
                {
                    appVoiceExperience.Deactivate();
                }
            }
        }

        private void Update()
        {
            if (_active)
                icon.color = new Color(0.91f, 0.32f, 0.5f, 1);
            else
                icon.color = Color.gray;
        }
    }

    
}
