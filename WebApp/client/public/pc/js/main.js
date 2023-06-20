import { SendVideo } from "./sendvideo.js";
import { getServerConfig, getRTCConfiguration } from "../../js/config.js";
import { createDisplayStringArray } from "../../js/stats.js";
import { RenderStreaming } from "../../module/renderstreaming.js";
import { Signaling, WebSocketSignaling } from "../../module/signaling.js";

const defaultStreamWidth = 1280;
const defaultStreamHeight = 720;
const streamSizeList = [
  { width: 640, height: 360 },
  { width: 1280, height: 720 },
  { width: 1920, height: 1080 },
  { width: 2560, height: 1440 },
  { width: 3840, height: 2160 },
  { width: 360, height: 640 },
  { width: 720, height: 1280 },
  { width: 1080, height: 1920 },
  { width: 1440, height: 2560 },
  { width: 2160, height: 3840 },
];

const localVideo = document.getElementById("localVideo");
const remoteVideo = document.getElementById("remoteVideo");
const localVideoStatsDiv = document.getElementById("localVideoStats");
const remoteVideoStatsDiv = document.getElementById("remoteVideoStats");

const connId1 = document.getElementById("connId1");
const connId2 = document.getElementById("connId2");
const connId3 = document.getElementById("connId3");
const connId4 = document.getElementById("connId4");
const connId5 = document.getElementById("connId5");

const startBtn1 = document.getElementById("startBtn1");
const startBtn2 = document.getElementById("startBtn2");
const startBtn3 = document.getElementById("startBtn3");
const startBtn4 = document.getElementById("startBtn4");
const startBtn5 = document.getElementById("startBtn5");

// textForConnectionId.value = getRandom();
// const videoSelect = document.querySelector('select#videoSource');
// const audioSelect = document.querySelector('select#audioSource');
// const videoResolutionSelect = document.querySelector('select#videoResolution');
// const cameraWidthInput = document.querySelector('input#cameraWidth');
// const cameraHeightInput = document.querySelector('input#cameraHeight');

const codecPreferences = document.getElementById("codecPreferences");
const supportsSetCodecPreferences =
  window.RTCRtpTransceiver &&
  "setCodecPreferences" in window.RTCRtpTransceiver.prototype;
const messageDiv = document.getElementById("message");
messageDiv.style.display = "none";

let useCustomResolution = false;

// setUpInputSelect();
showCodecSelect();

/** @type {SendVideo} */
let sendVideo = new SendVideo(localVideo, remoteVideo);
/** @type {RenderStreaming} */
let renderstreaming = new Array(5);
let useWebSocket;
let connectionId;

const startButton = document.getElementById("btnStartSharing");
startButton.addEventListener("click", startVideo);
// const setupButton = document.getElementById('setUpButton');
// setupButton.addEventListener('click', setUp);
// const hangUpButton = document.getElementById('hangUpButton');
// hangUpButton.addEventListener('click', hangUp);

startBtn1.addEventListener("click", function () {
  setUp(connId1.options[connId1.selectedIndex].value, 0);
});
startBtn2.addEventListener("click", function () {
  setUp(connId2.options[connId2.selectedIndex].value, 1);
});
startBtn3.addEventListener("click", function () {
  setUp(connId3.options[connId3.selectedIndex].value, 2);
});
startBtn4.addEventListener("click", function () {
  setUp(connId4.options[connId4.selectedIndex].value, 3);
});
startBtn5.addEventListener("click", function () {
  setUp(connId5.options[connId5.selectedIndex].value, 4);
});

window.addEventListener(
  "beforeunload",
  async () => {
    if (!renderstreaming) return;
    await renderstreaming.stop();
  },
  true
);

// 들어오자마자 영상 시작
window.addEventListener("load", async () => {
  await sendVideo.startLocalVideoScreen();
});

setupConfig();

async function setupConfig() {
  const res = await getServerConfig();
  useWebSocket = res.useWebSocket;
  showWarningIfNeeded(res.startupMode);
}

function showWarningIfNeeded(startupMode) {
  const warningDiv = document.getElementById("warning");
  if (startupMode == "public") {
    warningDiv.innerHTML =
      "<h4>Warning</h4> This sample is not working on Public Mode.";
    warningDiv.hidden = false;
  }
}

async function startVideo() {
  // START
  await sendVideo.startLocalVideoScreen();
  startButton.removeEventListener("click", startVideo);

  // SETUP
  // connectionId = textForConnectionId.value;
  // const signaling1 = useWebSocket ? new WebSocketSignaling() : new Signaling();
  // const config1 = getRTCConfiguration();
  // renderstreaming1 = new RenderStreaming(signaling1, config1);
  // renderstreaming1.onConnect = () => {
  //   const tracks = sendVideo.getLocalTracks();
  //   for (const track of tracks) {
  //     renderstreaming1.addTransceiver(track, { direction: 'sendonly' });
  //   }
  //   setCodecPreferences();
  //   showStatsMessage();
  // };
  // renderstreaming1.onDisconnect = () => {
  //   hangUp();
  // };
  // renderstreaming1.onTrackEvent = (data) => {
  //   const direction = data.transceiver.direction;
  //   if (direction == "sendrecv" || direction == "recvonly") {
  //     sendVideo.addRemoteTrack(data.track);
  //   }
  // };

  // await renderstreaming1.start();
  // await renderstreaming1.createConnection("11111");

  // const signaling2 = useWebSocket ? new WebSocketSignaling() : new Signaling();
  // const config2 = getRTCConfiguration();
  // renderstreaming2 = new RenderStreaming(signaling2, config2);
  // renderstreaming2.onConnect = () => {
  //   const tracks = sendVideo.getLocalTracks();
  //   for (const track of tracks) {
  //     renderstreaming2.addTransceiver(track, { direction: 'sendonly' });
  //   }
  //   setCodecPreferences();
  //   showStatsMessage();
  // };
  // renderstreaming2.onDisconnect = () => {
  //   hangUp();
  // };
  // renderstreaming2.onTrackEvent = (data) => {
  //   const direction = data.transceiver.direction;
  //   if (direction == "sendrecv" || direction == "recvonly") {
  //     sendVideo.addRemoteTrack(data.track);
  //   }
  // };

  // await renderstreaming2.start();
  // await renderstreaming2.createConnection("11112");
}

async function setUp(connectionId, index) {
  if (connectionId === "") {
    alert("User ID를 선택하세요.");
    return;
  }

  let interval;
  const signaling = useWebSocket ? new WebSocketSignaling() : new Signaling();
  const config = getRTCConfiguration();
  renderstreaming[index] = new RenderStreaming(signaling, config);
  renderstreaming[index].onConnect = () => {
    const tracks = sendVideo.getLocalTracks();
    for (const track of tracks) {
      renderstreaming[index].addTransceiver(track, { direction: "sendonly" });
    }
    setCodecPreferences(index);
    // showStatsMessage();
  };
  renderstreaming[index].onDisconnect = () => {
    clearInterval(interval);
    hangUp();
  };
  renderstreaming[index].onTrackEvent = (data) => {
    const direction = data.transceiver.direction;
    if (direction == "sendrecv" || direction == "recvonly") {
      sendVideo.addRemoteTrack(data.track);
    }
  };

  await renderstreaming[index].start();
  await renderstreaming[index].createConnection(connectionId);

  // 연결 상태 체크
  interval = setInterval(async () => {
    const stats = await renderstreaming[index].getStats();
    const array = createDisplayStringArray(stats, lastStats);
    lastStats = stats;

    if (array.join("<br>").includes("undefined")) console.log("로딩중");
    else console.log("연결 완료");
  }, 1000);
}

// async function setUp() {
//   // setupButton.disabled = true;
//   // hangUpButton.disabled = false;
//   // connectionId = textForConnectionId.value;
//   // codecPreferences.disabled = true;

//   connectionId = textForConnectionId.value;

//   const signaling = useWebSocket ? new WebSocketSignaling() : new Signaling();
//   const config = getRTCConfiguration();
//   renderstreaming = new RenderStreaming(signaling, config);
//   renderstreaming.onConnect = () => {
//     const tracks = sendVideo.getLocalTracks();
//     for (const track of tracks) {
//       renderstreaming.addTransceiver(track, { direction: 'sendonly' });
//     }
//     setCodecPreferences();
//     showStatsMessage();
//   };
//   renderstreaming.onDisconnect = () => {
//     hangUp();
//   };
//   renderstreaming.onTrackEvent = (data) => {
//     const direction = data.transceiver.direction;
//     if (direction == "sendrecv" || direction == "recvonly") {
//       sendVideo.addRemoteTrack(data.track);
//     }
//   };

//   await renderstreaming.start();
//   await renderstreaming.createConnection(connectionId);
// }

function setCodecPreferences(index) {
  /** @type {RTCRtpCodecCapability[] | null} */
  let selectedCodecs = null;
  if (supportsSetCodecPreferences) {
    const preferredCodec =
      codecPreferences.options[codecPreferences.selectedIndex];
    if (preferredCodec.value !== "") {
      const [mimeType, sdpFmtpLine] = preferredCodec.value.split(" ");
      const { codecs } = RTCRtpSender.getCapabilities("video");
      const selectedCodecIndex = codecs.findIndex(
        (c) => c.mimeType === mimeType && c.sdpFmtpLine === sdpFmtpLine
      );
      const selectCodec = codecs[selectedCodecIndex];
      selectedCodecs = [selectCodec];
    }
  }

  if (selectedCodecs == null) {
    return;
  }
  const transceivers = renderstreaming[index]
    .getTransceivers()
    .filter((t) => t.receiver.track.kind == "video");
  if (transceivers && transceivers.length > 0) {
    transceivers.forEach((t) => t.setCodecPreferences(selectedCodecs));
  }
}

async function hangUp() {
  clearStatsMessage();
  messageDiv.style.display = "block";
  messageDiv.innerText = `Disconnect peer on ${connectionId}.`;

  hangUpButton.disabled = true;
  setupButton.disabled = false;
  await renderstreaming.deleteConnection();
  await renderstreaming.stop();
  renderstreaming = null;
  remoteVideo.srcObject = null;

  // textForConnectionId.value = getRandom();
  connectionId = null;
  if (supportsSetCodecPreferences) {
    codecPreferences.disabled = false;
  }
}

// Connection ID
function getRandom() {
  // const max = 99999;
  // const length = String(max).length;
  // const number = Math.floor(Math.random() * max);
  // return (Array(length).join('0') + number).slice(-length);
  return 11111;
}

// async function setUpInputSelect() {
//   const deviceInfos = await navigator.mediaDevices.enumerateDevices();

//   for (let i = 0; i !== deviceInfos.length; ++i) {
//     const deviceInfo = deviceInfos[i];
//     if (deviceInfo.kind === 'videoinput') {
//       const option = document.createElement('option');
//       option.value = deviceInfo.deviceId;
//       option.text = deviceInfo.label || `camera ${videoSelect.length + 1}`;
//       videoSelect.appendChild(option);
//     } else if (deviceInfo.kind === 'audioinput') {
//       const option = document.createElement('option');
//       option.value = deviceInfo.deviceId;
//       option.text = deviceInfo.label || `mic ${audioSelect.length + 1}`;
//       audioSelect.appendChild(option);
//     }
//   }

//   // pc 화면 공유를 위한 옵션 추가
//   const pcScreen = document.createElement('option');
//   pcScreen.value = 'Screen Sharing';
//   pcScreen.text = 'Screen Sharing';
//   videoSelect.appendChild(pcScreen);

//   for (let i = 0; i < streamSizeList.length; i++) {
//     const streamSize = streamSizeList[i];
//     const option = document.createElement('option');
//     option.value = i;
//     option.text = `${streamSize.width} x ${streamSize.height}`;
//     videoResolutionSelect.appendChild(option);
//   }

//   const option = document.createElement('option');
//   option.value = streamSizeList.length;
//   option.text = 'Custom';
//   videoResolutionSelect.appendChild(option);
//   videoResolutionSelect.value = 1; // default select index (1280 x 720)

//   videoResolutionSelect.addEventListener('change', (event) => {
//     const isCustom = event.target.value >= streamSizeList.length;
//     cameraWidthInput.disabled = !isCustom;
//     cameraHeightInput.disabled = !isCustom;
//     useCustomResolution = isCustom;
//   });
// }

function showCodecSelect() {
  if (!supportsSetCodecPreferences) {
    messageDiv.style.display = "block";
    messageDiv.innerHTML = `Current Browser does not support <a href="https://developer.mozilla.org/en-US/docs/Web/API/RTCRtpTransceiver/setCodecPreferences">RTCRtpTransceiver.setCodecPreferences</a>.`;
    return;
  }

  const codecs = RTCRtpSender.getCapabilities("video").codecs;
  codecs.forEach((codec) => {
    if (["video/red", "video/ulpfec", "video/rtx"].includes(codec.mimeType)) {
      return;
    }
    const option = document.createElement("option");
    option.value = (codec.mimeType + " " + (codec.sdpFmtpLine || "")).trim();
    option.innerText = option.value;
    codecPreferences.appendChild(option);
  });
  codecPreferences.disabled = false;
}

let lastStats;
let intervalId;

function showStatus(index) {
  setInterval(async () => {
    const stats = await renderstreaming[index].getStats();
    const array = createDisplayStringArray(stats, lastStats);
    lastStats = stats;

    if (array.join("<br>").includes("undefined")) console.log("로딩중");
    else console.log("연결 완료");
  }, 1000);
}

function showStatsMessage() {
  intervalId = setInterval(async () => {
    if (localVideo.videoWidth) {
      localVideoStatsDiv.innerHTML = `<em>Sending resolution:</em> ${localVideo.videoWidth} x ${localVideo.videoHeight} px`;
    }
    // if (remoteVideo.videoWidth) {
    //   remoteVideoStatsDiv.innerHTML = `<strong>Receiving resolution:</strong> ${remoteVideo.videoWidth} x ${remoteVideo.videoHeight} px`;
    // }

    if (renderstreaming == null || connectionId == null) {
      return;
    }

    const stats = await renderstreaming.getStats();
    if (stats == null) {
      return;
    }

    const array = createDisplayStringArray(stats, lastStats);
    if (array.length) {
      messageDiv.style.display = "block";
      messageDiv.innerHTML = array.join("<br>");
    }
    lastStats = stats;
  }, 1000);
}

function clearStatsMessage() {
  if (intervalId) {
    clearInterval(intervalId);
  }
  lastStats = null;
  intervalId = null;
  localVideoStatsDiv.innerHTML = "";
  remoteVideoStatsDiv.innerHTML = "";
  messageDiv.style.display = "none";
  messageDiv.innerHTML = "";
}
