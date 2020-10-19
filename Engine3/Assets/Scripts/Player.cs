using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public class Player : MonoBehaviour
{
    // 小人跳跃时，决定远近的一个参数
    public float Factor;
    public float MaxDistance = 5;
    public GameObject Stage;
    public GameObject[] BoxTemplates;
    public Text TotalScoreText;
    public Transform Head;
    public Transform Body;
    public Text SingleScoreText;
    public Text Result;
    public GameObject panel;
    public Button RestartButton;
    private Rigidbody _rigidbody;
    private float _startTime;
    private GameObject _currentStage;
    private Vector3 _cameraRelativePosition;
    private int _score;
    private bool _isUpdateScoreAnimation;
    Vector3 _direction = new Vector3(1, 0, 0);
    private float _scoreAnimationStartTime;
    private int _lastReward = 1;
    private bool _enableInput = true;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = new Vector3(0, 0, 0);
        _currentStage = Stage;
        SpawnStage();
        _cameraRelativePosition = Camera.main.transform.position - transform.position;
        RestartButton.onClick.AddListener(() => { SceneManager.LoadScene(0); });
    }
    void Update()
    {
        if (_enableInput)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _startTime = Time.time;
            }
            if (Input.GetMouseButtonUp(0))
            {
                var elapse = Time.time - _startTime;
                OnJump(elapse);
                Body.transform.DOScale(0.1f, 0.2f);
                Head.transform.DOLocalMoveY(0.29f, 0.2f);
                _currentStage.transform.DOLocalMoveY(-0.25f, 0.2f);
                _currentStage.transform.DOScaleY(0.5f, 0.2f);
                _enableInput = false;
            }
            if (Input.GetMouseButton(0))
            {
                if (_currentStage.transform.localScale.y > 0.3)
                {
                    Body.transform.localScale += new Vector3(1, -1, 1) * 0.05f * Time.deltaTime;
                    Head.transform.localPosition += new Vector3(0, -1, 0) * 0.1f * Time.deltaTime;
                    _currentStage.transform.localScale += new Vector3(0, -1, 0) * 0.15f * Time.deltaTime;
                    _currentStage.transform.localPosition += new Vector3(0, -1, 0) * 0.15f * Time.deltaTime;
                }
            }
        }
        if (_isUpdateScoreAnimation)
            UpdateScoreAnimation();
    }
    void OnJump(float elapse)
    {
        _rigidbody.AddForce(new Vector3(0, 5f, 0) + (_direction) * elapse * Factor, ForceMode.Impulse);
        transform.DOLocalRotate(new Vector3(0, 0, -360), 0.6f, RotateMode.LocalAxisAdd);
    }
    void SpawnStage()
    {
        GameObject prefab;
        if (BoxTemplates.Length > 0)
        {
            prefab = BoxTemplates[Random.Range(0, BoxTemplates.Length)];
        }
        else
        {
            prefab = Stage;
        }
        var stage = Instantiate(prefab);
        stage.transform.position = _currentStage.transform.position + _direction * Random.Range(1.1f, MaxDistance);
        var randomScale = Random.Range(0.5f, 1);
        stage.transform.localScale = new Vector3(randomScale, 0.5f, randomScale);
        stage.GetComponent<Renderer>().material.color =
            new Color(Random.Range(0f, 1), Random.Range(0f, 1), Random.Range(0f, 1));
    }
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.name == "Ground")
        {
             ShowPanel();
        }
        else
        {
            if (_currentStage != collision.gameObject)
            {
                var contacts = collision.contacts;

                //check if player's feet on the stage
                if (contacts.Length == 1 && contacts[0].normal == Vector3.up)
                {
                    _currentStage = collision.gameObject;
                    AddScore(contacts);
                    RandomDirection();
                    SpawnStage();
                    Camera.main.transform.DOMove(transform.position + _cameraRelativePosition, 1);
                    _enableInput = true;
                }
                else // body collides with the box
                {
                    ShowPanel();
                }
            }
            else //still on the same box
            {
                var contacts = collision.contacts;

                //check if player's feet on the stage
                if (contacts.Length == 1 && contacts[0].normal == Vector3.up)
                {
                    _enableInput = true;
                }
                else // body just collides with this box
                {
                    ShowPanel();
                }
            }
        }
    }
    private void AddScore(ContactPoint[] contacts)
    {
        if (contacts.Length > 0)
        {
            var hitPoint = contacts[0].point;
            hitPoint.y = 0;

            var stagePos = _currentStage.transform.position;
            stagePos.y = 0;

            var precision = Vector3.Distance(hitPoint, stagePos);
            if (precision < 0.1)
                _lastReward *= 2;
            else
                _lastReward = 1;
            _score += _lastReward;
            TotalScoreText.text = _score.ToString();
            ShowScoreAnimation();
        }
    }
    private void ShowScoreAnimation()
    {
        _isUpdateScoreAnimation = true;
        _scoreAnimationStartTime = Time.time;
        SingleScoreText.text = "+" + _lastReward;
    }
    void UpdateScoreAnimation()
    {
        if (Time.time - _scoreAnimationStartTime > 1)
            _isUpdateScoreAnimation = false;

        var playerScreenPos =
            RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);
        SingleScoreText.transform.position = playerScreenPos +
                                             Vector2.Lerp(Vector2.zero, new Vector2(0, 200),
                                                 Time.time - _scoreAnimationStartTime);
        SingleScoreText.color = Color.Lerp(Color.black, new Color(0, 0, 0, 0), Time.time - _scoreAnimationStartTime);
    }
    void RandomDirection()
    {
        var seed = Random.Range(0, 2);
        _direction = seed == 0 ? new Vector3(1, 0, 0) : new Vector3(0, 0, 1);
        transform.right = _direction;
    }
    void ShowPanel()
    {
        Result.text = TotalScoreText.text;
        panel.SetActive(true);
    }
}