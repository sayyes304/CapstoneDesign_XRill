"use strict";

const mysql = require("mysql");

const db = mysql.createConnection({
  host: "localhost",
  user: "root",
  password: "ghost7409",
  database: "oshare",
  port: "3306",
});

db.connect();

module.exports = db;
