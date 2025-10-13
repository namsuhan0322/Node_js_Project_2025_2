using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthUI : MonoBehaviour
{
    [Header("InputField")]
    public InputField usernameInput;
    public InputField passwordInput;

    public Button registerButton;
    public Button loginButton;

    public Text statusText;

    private AuthManager _authManager;

    void Start()
    {
        _authManager = GetComponent<AuthManager>();
        registerButton.onClick.AddListener(OnRegisterClick);
        loginButton.onClick.AddListener(OnLoginClick);
    }

    private void OnRegisterClick()
    {
        StartCoroutine(RegisterCoroutine());
    }

    private void OnLoginClick()
    {
        StartCoroutine(LoginCoroutine());
    }

    private IEnumerator RegisterCoroutine()
    {
        statusText.text = "ȸ�� ���� ��....";
        yield return StartCoroutine(_authManager.Register(usernameInput.text, passwordInput.text));
        statusText.text = "ȸ�� ���� ����, �α��� ���ּ���";
    }

    private IEnumerator LoginCoroutine()
    {
        statusText.text = "�α��� ��....";
        yield return StartCoroutine(_authManager.Login(usernameInput.text, passwordInput.text));
        statusText.text = "�α��� ����";
    }
}
