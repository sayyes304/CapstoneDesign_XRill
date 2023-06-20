console.log("COOKIE !");

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

const ID_cookie = getCookie("connectionID");
const connectionID = "1000" + ID_cookie;
console.log(ID_cookie);
const id_cookieDiplay = document.querySelector(".connection_cookie");

if (ID_cookie != null) id_cookieDiplay.value = connectionID;

fetch("test.json")
  .then((response) => response.json())
  .then((jsonData) => {
    console.log(jsonData);
    const names = jsonData.map((obj) => obj.id);

    const selection1 = document.getElementById("connId1");
    const selection2 = document.getElementById("connId2");
    const selection3 = document.getElementById("connId3");
    const selection4 = document.getElementById("connId4");
    const selection5 = document.getElementById("connId5");

    for (let i = 0; i < names.length; i++) {
      const optionElement1 = document.createElement("option");
      const optionElement2 = document.createElement("option");
      const optionElement3 = document.createElement("option");
      const optionElement4 = document.createElement("option");
      const optionElement5 = document.createElement("option");

      optionElement1.value = names[i];
      optionElement1.textContent = names[i];

      optionElement2.value = names[i];
      optionElement2.textContent = names[i];

      optionElement3.value = names[i];
      optionElement3.textContent = names[i];

      optionElement4.value = names[i];
      optionElement4.textContent = names[i];

      optionElement5.value = names[i];
      optionElement5.textContent = names[i];

      selection1.appendChild(optionElement1);
      selection2.appendChild(optionElement2);
      selection3.appendChild(optionElement3);
      selection4.appendChild(optionElement4);
      selection5.appendChild(optionElement5);
    }
    console.log("NAME : " + names[0]);
  })
  .catch((error) => {
    console.error(error);
  });
