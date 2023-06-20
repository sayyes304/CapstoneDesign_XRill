import * as Config from "./config.js";

const startupDiv = document.getElementById("startup");
startupDiv.innerHTML = "<h2>연결 정보</h2>";

// 접속 기기가 모바일 디바이스라면 화면 공유 버튼 OFF
const pcButton = document.getElementById("PC");
let isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
if(isMobile) pcButton.style.display = 'none';

const displayConfig = async () => {
  const res = await Config.getServerConfig();
  if (res.useWebSocket) {
    startupDiv.innerHTML += "<li>Signaling Protocol : <strong>WebSocket</strong></li>";
  } else {
    startupDiv.innerHTML += "<li>Signaling Protocol : <strong>HTTP</strong></li>";
  }

  const mode = res.startupMode.replace(/^./, res.startupMode[0].toUpperCase());
  startupDiv.innerHTML += `<li>Signaling Mode : <strong>${mode}</strong></li>`;
};

displayConfig();