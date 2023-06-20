function getCookie(name) {
  var cookieName = name + "=";
  var decodedCookie = decodeURIComponent(document.cookie);
  var cookieArray = decodedCookie.split(";");
  for (var i = 0; i < cookieArray.length; i++) {
    var cookie = cookieArray[i];
    while (cookie.charAt(0) === " ") {
      cookie = cookie.substring(1);
    }
    if (cookie.indexOf(cookieName) === 0) {
      return cookie.substring(cookieName.length, cookie.length);
    }
  }
  return null;
}

// 쿠키에서 값을 가져와서 <h2>에 표시하기
var user_cookieValue = getCookie("user");
console.log("USER " + getCookie("user"));

var greetingMessage = "안녕하세요, " + user_cookieValue + "님!";
var user_cookieDisplay = document.getElementById("user_cookieValue");

// login, logout button hidden
const loginBtn = document.getElementById("loginBtn");
const RegisterBtn = document.getElementById("RegisterBtn");
const logoutBtn = document.getElementById("LogoutBtn");

if (user_cookieValue != null) {
  user_cookieDisplay.textContent = greetingMessage;
  loginBtn.style.display = "none";
  RegisterBtn.style.display = "none";
} else {
  logoutBtn.style.display = "none";
}
