using System.Collections;
using AxieMixer.Unity;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

namespace Game_Turn_Based
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public enum GameState
        {
            GenerateGrid = 0,
            SpawnAttacker = 1,
            SpawnDefender = 2,
            AttackerTurn = 3,
            DefenderTurn = 4
        }
        [SerializeField] GameObject _startMsgGO;
        [SerializeField] Button _applyBtn;
        [SerializeField] InputField _idAttackerInput;
        [SerializeField] InputField _idDefenderInput;
        [SerializeField] AxieFigure _attackerFigure;
        [SerializeField] AxieFigure _defenderFigure;

        public InputField ifAttackerQuantity;
        public InputField ifDefenderQuantity;
        public Button btChangeSpeed;
        public Button btPause;
        public float gameSpeed = 1.0f;
        public bool _isPlaying = false;
        public bool needUpdateRelativePower = false;
        public bool needUpdateStatsPanel = false;
        public int AttackerCount = 0;
        public int DefenderCount = 0;
        public GameObject preStartPanel;
        public GameObject onPlayingPanel;
        public GameObject statsPanel;
        public Slider relativePowerSlider;
        public Text txtUnitName;
        public Text txtUnitHP;
        public Text txtUnitDamage;
        public Text txtUnitRandomNumber;
        public Text txtRestart;
        bool _isFetchingGenes = false;
        string _currentIDAttacker = "";
        string _currentIDDefender = "";
        float[] gameSpeeds = {1.0f,1.5f,2.0f,3.0f,5.0f};
        string[] gameSpeedsTxt = {"x1","x1.5","x2","x3","x5"};
        bool isPausing = false;
        bool hasInitGameSpeed = false;
        Unit currentUnit;

        GameState gameState;

        void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            _applyBtn.onClick.AddListener(OnApplyButtonClicked);
            btChangeSpeed.onClick.AddListener(OnClickButtonChangeSpeed);
            btPause.onClick.AddListener(OnClickButtonPause);
        }

        private void OnDisable()
        {
            _applyBtn.onClick.RemoveListener(OnApplyButtonClicked);
            btChangeSpeed.onClick.RemoveAllListeners();
            btPause.onClick.RemoveAllListeners();
        }

        // Start is called before the first frame update
        void Start()
        {
            Time.timeScale = 0f;

            Mixer.Init();

            // attacker
            string axieId = PlayerPrefs.GetString("selectingAttackerId", "4191804");
            string genes = PlayerPrefs.GetString("selectingAttackerGenes", "0x2000000000000300008100e08308000000010010088081040001000010a043020000009008004106000100100860c40200010000084081060001001410a04406");
            _idAttackerInput.text = axieId;

            if(string.IsNullOrEmpty(axieId))
            {
                _attackerFigure.SetGenes("4191804", "0x2000000000000300008100e08308000000010010088081040001000010a043020000009008004106000100100860c40200010000084081060001001410a04406");
            }
            else _attackerFigure.SetGenes(axieId, genes);

            // defender
            axieId = PlayerPrefs.GetString("selectingDefenderId", "2724598");
            genes = PlayerPrefs.GetString("selectingDefenderGenes", "0x2000000000000300008100e08308000000010010088081040001000010a043020000009008004106000100100860c40200010000084081060001001410a04406");
            _idDefenderInput.text = axieId;
            _defenderFigure.SetGenes(axieId, genes);

            _currentIDAttacker = _idAttackerInput.text;
            _currentIDDefender = _idDefenderInput.text;

            gameSpeed = PlayerPrefs.GetFloat("gameSpeed");
            for(int i = 0; i < gameSpeeds.Length; i++)
            {
                if(gameSpeed == gameSpeeds[i]) 
                {
                    hasInitGameSpeed = true;
                    btChangeSpeed.GetComponentInChildren<Text>().text = gameSpeedsTxt[i];
                    break;
                }
            }
            if(!hasInitGameSpeed) gameSpeed = 1.0f;
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isPlaying)
            {
                _startMsgGO.SetActive((Time.unscaledTime % .5 < .2));
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    preStartPanel.SetActive(false);
                    _startMsgGO.SetActive(false);
                    _isPlaying = true;
                    Time.timeScale = 1f;
                    AttackerCount = Int32.Parse(ifAttackerQuantity.text);
                    DefenderCount = Int32.Parse(ifDefenderQuantity.text);
                    GridManager.Instance.GenerateGrid();
                    // _attackerFigure.gameObject.SetActive(false);
                    // _defenderFigure.gameObject.SetActive(false);
                    // _attackerFigure.GetComponentInParent<Transform>().position = new Vector2(0, 1000);
                    // _defenderFigure.GetComponentInParent<Transform>().position = new Vector2(0, 1000);
                    // _attackerFigure.GetComponentInParent<Unit>().gameObject.SetActive(false);
                    // _defenderFigure.GetComponentInParent<Unit>().gameObject.SetActive(false);
                    Destroy(_attackerFigure.GetComponentInParent<Unit>().gameObject);
                    Destroy(_defenderFigure.GetComponentInParent<Unit>().gameObject);
                    relativePowerSlider.minValue = 0;
                    relativePowerSlider.maxValue = 1;
                    if(AttackerCount + DefenderCount != 0)
                    {
                        relativePowerSlider.value =  (AttackerCount * 1.0f / (AttackerCount + DefenderCount));
                    }
                    else relativePowerSlider.value = AttackerCount;
                    onPlayingPanel.SetActive(true);
                    Camera.main.gameObject.AddComponent<CameraMovement>();
                    Camera.main.gameObject.GetComponent<CameraMovement>().cam = Camera.main;
                }
                return;
            }
            else
            {
                
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    //Application.LoadLevel(Application.loadedLevel);
                    SceneManager.LoadScene(0);
                }

                if(needUpdateRelativePower)
                {
                    if(AttackerCount + DefenderCount != 0)
                    {
                         relativePowerSlider.value = (AttackerCount * 1.0f / (AttackerCount + DefenderCount)) ;
                    }
                    else relativePowerSlider.value = AttackerCount;
                    needUpdateRelativePower = false;
                }

                if(statsPanel.activeSelf && !isPausing)
                {
                    if(currentUnit.hp <= 0)
                    {
                        statsPanel.SetActive(false);
                    }
                    txtUnitName.text = currentUnit.unitName;
                    txtUnitHP.text = "HP: " + currentUnit.hp.ToString() + "/" + currentUnit.maxHP.ToString();
                    txtUnitDamage.text = "Attack Damage: " + currentUnit.damage.ToString();
                    txtUnitRandomNumber.text = UnityEngine.Random.Range(UInt16.MinValue,UInt16.MaxValue).ToString();
                }

                if(Input.GetMouseButtonDown(0))
                {
                    selectedUnit();
                }

                if(AttackerCount == 0 || DefenderCount == 0)
                {
                    txtRestart.gameObject.SetActive(true);
                }
            }
        }

        public void ChangeState(GameState newState)
        {
            //gameState = newState;
            switch (newState)
            {
                case GameState.GenerateGrid:
                    GridManager.Instance.GenerateGrid();
                    break;
                case GameState.SpawnAttacker:
                    UnitManager.Instance.SpawnAttackerRandomPos();
                    break;
                case GameState.SpawnDefender:
                    UnitManager.Instance.SpawnDefenderRandomPos();
                    break;
                case GameState.AttackerTurn:
                    break;
                case GameState.DefenderTurn:
                    break;
                default:
                    Debug.Log(nameof(newState));
                    break;
            }
        }

        private void OnClickButtonChangeSpeed()
        {
            switch(gameSpeed)
            {
                case 1.0f:
                    gameSpeed = gameSpeeds[1];
                    btChangeSpeed.GetComponentInChildren<Text>().text = gameSpeedsTxt[1];
                    break;
                case 1.5f:
                    gameSpeed = gameSpeeds[2];
                    btChangeSpeed.GetComponentInChildren<Text>().text = gameSpeedsTxt[2];
                    break;
                case 2.0f:
                    gameSpeed = gameSpeeds[3];      
                    btChangeSpeed.GetComponentInChildren<Text>().text = gameSpeedsTxt[3];      
                    break;
                case 3.0f:
                    gameSpeed = gameSpeeds[4];
                    btChangeSpeed.GetComponentInChildren<Text>().text = gameSpeedsTxt[4];
                    break;
                case 5.0f:
                default:
                    gameSpeed = gameSpeeds[0];
                    btChangeSpeed.GetComponentInChildren<Text>().text = gameSpeedsTxt[0];
                    break;
            }
            PlayerPrefs.SetFloat("gameSpeed",gameSpeed);
        }

        private void OnClickButtonPause()
        {
            if(!isPausing)
            {
                Time.timeScale = 0f;
                btPause.GetComponentInChildren<Text>().text = "Unpause";
                isPausing = true;
            }
            else
            {
                Time.timeScale = 1.0f;
                btPause.GetComponentInChildren<Text>().text = "Pause";
                isPausing = false;
            }
        }

        void OnApplyButtonClicked()
        {
            if (string.IsNullOrEmpty(_idAttackerInput.text) || string.IsNullOrEmpty(_idDefenderInput.text) ||_isFetchingGenes) return;
            _isFetchingGenes = true;

            Debug.Log(_currentIDAttacker);
            Debug.Log(_currentIDDefender);
            StartCoroutine(GetAxiesGenes(true, _idAttackerInput.text)); 
            StartCoroutine(GetAxiesGenes(false, _idDefenderInput.text));
            // if (!_currentIDAttacker.Equals(_idAttackerInput.text))
            // {
            //     StartCoroutine(GetAxiesGenes(true, _idAttackerInput.text));  // attacker figure needs to change image
            // }
            // if (!_currentIDDefender.Equals(_idDefenderInput.text))
            // {
            //     StartCoroutine(GetAxiesGenes(false, _idDefenderInput.text)); // defender figure needs to change image
            // }
        }

        public IEnumerator GetAxiesGenes(bool isAttacker, string axieId)
        {
            string searchString = "{ axie (axieId: \"" + axieId + "\") { id, genes, newGenes}}";
            JObject jPayload = new JObject();
            jPayload.Add(new JProperty("query", searchString));

            var wr = new UnityWebRequest("https://graphql-gateway.axieinfinity.com/graphql", "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jPayload.ToString().ToCharArray());
            wr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            wr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            wr.SetRequestHeader("Content-Type", "application/json");
            wr.timeout = 10;
            yield return wr.SendWebRequest();
            if (wr.error == null)
            {
                var result = wr.downloadHandler != null ? wr.downloadHandler.text : null;
                if (!string.IsNullOrEmpty(result))
                {
                    JObject jResult = JObject.Parse(result);
                    string genesStr = (string)jResult["data"]["axie"]["newGenes"];
                    if (isAttacker)
                    {
                        PlayerPrefs.SetString("selectingAttackerId", axieId);
                        PlayerPrefs.SetString("selectingAttackerGenes", genesStr);
                        _idAttackerInput.text = axieId;
                        _attackerFigure.SetGenes(axieId, genesStr);
                    }
                    else
                    {
                        PlayerPrefs.SetString("selectingDefenderId", axieId);
                        PlayerPrefs.SetString("selectingDefenderGenes", genesStr);
                        _idDefenderInput.text = axieId;
                        _defenderFigure.SetGenes(axieId, genesStr);
                    }

                }
            }
            _isFetchingGenes = false;
        }

        public void selectedUnit()
        {
            //Converting Mouse Pos to 2D (vector2) World Pos

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit)
            {
                if(hit.transform.gameObject.GetComponent<Unit>() != null)
                {
                    if (currentUnit != null)
                    {
                        if (!hit.transform.gameObject.GetComponent<Unit>().name.Equals(currentUnit.unitName))
                        {
                            currentUnit.outline.SetActive(false);
                            hit.transform.gameObject.GetComponent<Unit>().outline.SetActive(true);
                        }
                    }
                    currentUnit = hit.transform.gameObject.GetComponent<Unit>();
                    currentUnit.outline.SetActive(true);
                    statsPanel.SetActive(true);
                }
                else
                {
                    statsPanel.SetActive(false);
                    needUpdateStatsPanel = false;
                }
            }
            else 
            {
                statsPanel.SetActive(false);
                needUpdateStatsPanel = false;
            }
        }
    }
}
