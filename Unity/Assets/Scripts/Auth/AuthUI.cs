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
        statusText.text = "회원 가입 중....";
        yield return StartCoroutine(_authManager.Register(usernameInput.text, passwordInput.text));
        statusText.text = "회원 가입 성공, 로그인 해주세요";
    }

    private IEnumerator LoginCoroutine()
    {
        statusText.text = "로그인 중....";
        yield return StartCoroutine(_authManager.Login(usernameInput.text, passwordInput.text));
        statusText.text = "로그인 성공";
    }
}
