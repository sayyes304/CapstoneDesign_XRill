"use strict";

// #users => 은닉화. 외부에서 데이터 접근 불가능

const db = require("../config/db");

class UserStorage {
  // ...변수명 : 배열 형태로 반환
  // reduce : 반복해서 순회

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
        if (err) reject(`이미 존재하는 학번입니다. `);
        resolve({ success: true });
      });
    });
  }
}

module.exports = UserStorage;
