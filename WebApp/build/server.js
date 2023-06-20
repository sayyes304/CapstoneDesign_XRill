"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.createServer = void 0;
var express = require("express");
var path = require("path");
var fs = require("fs");
var morgan = require("morgan");
var signaling_1 = require("./signaling");
var log_1 = require("./log");
var httphandler_1 = require("./class/httphandler");
var cors = require("cors");
var cookieParser = require("cookie-parser");
var fs = require("fs");

const mysql = require("mysql");

// 추가한 코드
const db = mysql.createConnection({
  host: "localhost",
  user: "root",
  password: "ghost7409",
  database: "oshare",
  port: "3306",
});

db.connect();
class UserStorage {
  static getUserInfo(id) {
    return new Promise((resolve, reject) => {
      const query = "SELECT * FROM users WHERE ID = ?;";
      db.query(query, [id], (err, data) => {
        if (err) reject(err);
        resolve(data[0]);
      });
    });
  }

  static async save(userInfo) {
    return new Promise((resolve, reject) => {
      const query = "INSERT INTO users(id, name, pw) VALUES(?,?,?);";
      db.query(query, [userInfo.id, userInfo.name, userInfo.pw], (err) => {
        if (err) reject(`이미 존재하는 아이디입니다. `);
        resolve({ success: true });
      });
    });
  }
}

class User {
  constructor(body) {
    this.body = body;
  }

  async login() {
    const client = this.body;
    try {
      const user = await UserStorage.getUserInfo(client.id);
      if (user) {
        if (user.id === client.id && user.pw === client.pw) {
          return {
            success: true,
            name: user.name,
            connectoinid: user.connectionID,
          };
        }
        return { success: false, msg: "비밀번호가 틀렸습니다." };
      }
      return { success: false, msg: "아이디가 존재하지 않습니다." };
    } catch (err) {
      return { success: false, msg: err };
    }
  }

  async register() {
    const client = this.body;
    try {
      const response = await UserStorage.save(client);
      return response;
    } catch (err) {
      return { success: false, msg: err };
    }
  }
}

var createServer = function (config) {
  var app = express();
  (0, httphandler_1.reset)(config.mode);
  // logging http access
  if (config.logging != "none") {
    app.use(morgan(config.logging));
  }
  // const signal = require('./signaling');
  app.use(cors({ origin: "*" }));
  app.use(express.urlencoded({ extended: true }));
  app.use(express.json());
  app.get("/config", function (req, res) {
    const query = "SELECT * FROM users";
    db.query(query, (error, results) => {
      if (error) console.error("Error executing MySQL query");

      var string = JSON.stringify(results);
      var jsonstring = '{ "Array" :' + JSON.stringify(results) + "}";
      const file = "client/public/test.txt";
      fs.open(file, "w", (err, fd) => {
        if (err) throw err;
        console.log("file open");
      });
      fs.writeFileSync(file, string, "utf-8", (err) => {
        console.log("write end");
      });
    });

    return res.json({
      useWebSocket: config.type == "websocket",
      startupMode: config.mode,
      logging: config.logging,
    });
  });

  app.use(express.static(path.join(__dirname, "../client/public/pc/js")));

  app.use("/signaling", signaling_1.default);
  app.use(express.static(path.join(__dirname, "../client/public")));
  app.use("/module", express.static(path.join(__dirname, "../client/src")));
  app.get("/", function (req, res) {
    var indexPagePath = path.join(__dirname, "../client/public/index.html");
    fs.access(indexPagePath, function (err) {
      if (err) {
        (0, log_1.log)(
          log_1.LogLevel.warn,
          "Can't find file ' ".concat(indexPagePath)
        );
        res.status(404).send("Can't find file ".concat(indexPagePath));
      } else {
        res.sendFile(indexPagePath);
      }
    });
  });

  app.set("views", "./client/public/views");
  app.set("view engine", "ejs");
  app.use(cookieParser());

  app.get("/login", (req, res) => {
    res.render("home/login");
  });

  app.post("/login", async (req, res) => {
    const user = new User(req.body);
    const response = await user.login();
    if (response.success) {
      res.cookie("connectionID", response.connectoinid, {
        expires: new Date(Date.now() + 900000),
        // httpOnly: true,
      });
      res.cookie("user", response.name, {
        expires: new Date(Date.now() + 900000),
        // httpOnly: true,
      });
    }
    return res.json(response);
  });

  app.get("/register", (req, res) => {
    res.render("home/register");
  });

  app.post("/register", async (req, res) => {
    const user = new User(req.body);
    const response = await user.register();
    return res.json(response);
  });

  app.get("/logout", (req, res) => {
    res.clearCookie("connectionID");
    res.clearCookie("user");

    res.redirect("/");
  });

  // app.get("/json", (req, res) => {
  //   const query = "SELECT * FROM users";
  //   db.query(query, (error, results) => {
  //     if (error) console.error("Error executing MySQL query");

  //     var string = JSON.stringify(results);

  //     const file = "client/public/test.txt";
  //     fs.open(file, "w", (err, fd) => {
  //       if (err) throw err;
  //       console.log("file open");
  //     });
  //     fs.writeFile(file, string, "utf-8", (err) => {
  //       console.log("write end");
  //     });
  //   });
  // });

  return app;
};
exports.createServer = createServer;
//# sourceMappingURL=server.js.map
